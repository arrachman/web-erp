import { Injectable, Logger } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { ErpWhatsappService } from '../erp-whatsapp/erp-whatsapp.service';

export type NotificationEvent = 'ORDER_DITERIMA' | 'BARANG_DIKIRIM' | 'TAGIHAN_TERBIT';

/** Nama template di sys_wa_templates (dikelola modul erp-whatsapp). */
const EVENT_TEMPLATE: Record<NotificationEvent, string> = {
  ORDER_DITERIMA: 'order_diterima',
  BARANG_DIKIRIM: 'order_terkirim',
  TAGIHAN_TERBIT: 'tagihan_terbit',
};

type DispatchResult = { success: boolean; skipped?: string; logId?: string; status?: string };

function fmtRp(value: Prisma.Decimal | number | string): string {
  return `Rp${Number(value).toLocaleString('id-ID')}`;
}

function fmtDate(d: Date | null | undefined): string {
  if (!d) return '-';
  return new Date(d).toLocaleDateString('id-ID', { day: 'numeric', month: 'long', year: 'numeric' });
}

/**
 * W6 — Notifikasi WhatsApp (Fase 3 G2).
 * Pengiriman lewat **wa-gateway self-hosted** (`apps/wa-gateway`, kompatibel
 * Fonnte, device "WA Bahtera Madani") melalui fasad ErpWhatsappService —
 * sesuai keputusan user 2026-10-05 (bukan BSP komersial). Service ini
 * mengorkestrasi peristiwa ERP (order dibuat / DO POST / invoice POST) dan
 * mencatat hasilnya ke sys_notification_logs; render template + antrean +
 * log pengiriman rinci hidup di modul erp-whatsapp (sys_wa_templates /
 * sys_wa_logs). Saklar aktivasi: sys_settings WHATSAPP SEND_ENABLED —
 * bila nonaktif, dispatch di-skip dan tercatat SKIPPED di sini (tidak ada
 * pengiriman diam-diam). Dedupe dengan notifier pemindai erp-whatsapp
 * lewat keberadaan baris sys_wa_logs untuk referensi+template yang sama.
 * Service ini TIDAK PERNAH melempar error ke pemanggil.
 */
@Injectable()
export class ErpOutboundNotificationsService {
  private readonly logger = new Logger(ErpOutboundNotificationsService.name);

  constructor(
    private readonly prisma: PrismaService,
    private readonly wa: ErpWhatsappService,
  ) {}

  private async writeLog(params: {
    event: NotificationEvent;
    partnerId: bigint | null;
    portalAccountId: bigint | null;
    recipientPhone: string | null;
    recipientName: string | null;
    docId: bigint;
    docNumber: string;
    status: string;
    error?: string | null;
    waLogId?: string;
  }): Promise<void> {
    let message: string | null = null;
    let providerMessageId: string | null = null;
    if (params.waLogId) {
      const waLog = await this.prisma.erpWaLog.findUnique({
        where: { id: BigInt(params.waLogId) },
        select: { body: true, messageId: true, errorReason: true },
      }).catch(() => null);
      message = waLog?.body ?? null;
      providerMessageId = waLog?.messageId ?? null;
      if (!params.error && waLog?.errorReason) params.error = waLog.errorReason;
    }
    await this.prisma.erpNotificationLog.create({
      data: {
        channel: 'WHATSAPP',
        event: params.event,
        templateCode: EVENT_TEMPLATE[params.event],
        recipientPhone: params.recipientPhone,
        recipientName: params.recipientName,
        partnerId: params.partnerId,
        portalAccountId: params.portalAccountId,
        relatedDocType: 'sls_orders',
        relatedDocId: params.docId,
        relatedDocNumber: params.docNumber,
        message,
        status: params.status,
        provider: 'WA-GATEWAY',
        providerMessageId,
        error: params.error ?? null,
        metadata: params.waLogId ? { waLogId: params.waLogId } : undefined,
      },
    }).catch((err) => this.logger.warn(`tulis log notifikasi gagal: ${(err as Error).message}`));
  }

  private mapResult(result: DispatchResult): { status: string; error?: string } {
    if (result.success) return { status: 'SENT' };
    switch (result.skipped) {
      case 'disabled':
        return { status: 'SKIPPED', error: 'Pengiriman WA nonaktif (SEND_ENABLED=false di Pengaturan WhatsApp).' };
      case 'no_phone':
        return { status: 'SKIPPED', error: 'Tidak ada nomor WA tujuan (akun portal/kontak partner kosong).' };
      case 'template_not_found':
        return { status: 'SKIPPED', error: 'Template WA tidak ada/nonaktif di sys_wa_templates.' };
      default:
        return { status: 'FAILED' }; // alasan spesifik diambil dari sys_wa_logs di writeLog
    }
  }

  /** Sudah ada log WA untuk referensi+template ini? (dedupe vs notifier pemindai) */
  private async alreadySentViaWa(referenceType: string, referenceId: bigint, event: NotificationEvent): Promise<boolean> {
    const tpl = await this.prisma.erpWaTemplate.findFirst({
      where: { name: EVENT_TEMPLATE[event] },
      select: { id: true },
    });
    if (!tpl) return false;
    const count = await this.prisma.erpWaLog.count({
      where: { referenceType, referenceId, templateId: tpl.id },
    });
    return count > 0;
  }

  private async notifyForOrder(
    orderId: bigint,
    event: NotificationEvent,
    extra: { doNumber?: string; invNumber?: string; invoiceId?: bigint; dueDate?: Date | null; totalOverride?: Prisma.Decimal },
  ): Promise<void> {
    try {
      const order = await this.prisma.erpSlsOrder.findFirst({
        where: { id: orderId, deletedAt: null },
        select: {
          id: true, docNumber: true, customerId: true, channel: true,
          grandTotal: true, customFields: true,
        },
      });
      if (!order || !order.customerId || !String(order.channel).startsWith('PORTAL')) return;
      const partner = await this.prisma.erpPartner.findUnique({
        where: { id: order.customerId }, select: { name: true },
      });
      const schoolName = partner?.name ?? '';
      const cf = (order.customFields ?? {}) as { portalParent?: { accountId?: string } };
      const referenceType = event === 'TAGIHAN_TERBIT' ? 'INVOICE' : 'ORDER';
      const referenceId = event === 'TAGIHAN_TERBIT' && extra.invoiceId ? extra.invoiceId : order.id;
      const total = extra.totalOverride ?? order.grandTotal;

      // ── Orang tua: kirim ke nomor akun orang tua pemesan saja ──
      if (cf.portalParent?.accountId) {
        const account = await this.prisma.erpPortalAccount.findFirst({
          where: { id: BigInt(cf.portalParent.accountId), deletedAt: null },
          select: { id: true, fullName: true, phone: true },
        });
        if (!account?.phone) {
          await this.writeLog({
            event, partnerId: order.customerId, portalAccountId: account?.id ?? null,
            recipientPhone: account?.phone ?? null, recipientName: account?.fullName ?? null,
            docId: order.id, docNumber: order.docNumber,
            status: 'SKIPPED', error: 'Akun orang tua tidak punya nomor telepon.',
          });
          return;
        }
        const variables: Record<string, string> = {
          sekolah: schoolName,
          nomor_order: order.docNumber,
          total: fmtRp(total),
          nomor_do: extra.doNumber ?? '',
          nomor_invoice: extra.invNumber ?? '',
          jatuh_tempo: fmtDate(extra.dueDate),
        };
        const result = await this.wa.dispatch({
          templateName: EVENT_TEMPLATE[event],
          recipientType: 'orang_tua',
          recipientPhone: account.phone,
          variables,
          referenceType,
          referenceId,
        }) as DispatchResult;
        const mapped = this.mapResult(result);
        await this.writeLog({
          event, partnerId: order.customerId, portalAccountId: account.id,
          recipientPhone: account.phone, recipientName: account.fullName,
          docId: order.id, docNumber: order.docNumber,
          status: mapped.status, error: mapped.error, waLogId: result.logId,
        });
        return;
      }

      // ── Sekolah: fasad notify* (resolusi nomor: akun portal → kontak) ──
      if (await this.alreadySentViaWa(referenceType, referenceId, event)) {
        await this.writeLog({
          event, partnerId: order.customerId, portalAccountId: null,
          recipientPhone: null, recipientName: schoolName,
          docId: order.id, docNumber: order.docNumber,
          status: 'SKIPPED', error: 'Sudah diberitahu via notifier WA (dedupe sys_wa_logs).',
        });
        return;
      }
      const phone = await this.wa.resolvePartnerPhone(order.customerId);
      let result: DispatchResult;
      if (event === 'ORDER_DITERIMA') {
        result = await this.wa.notifyOrderReceived({
          partnerId: order.customerId, schoolName,
          docNumber: order.docNumber, grandTotal: order.grandTotal.toString(), orderId: order.id,
        }) as DispatchResult;
      } else if (event === 'BARANG_DIKIRIM') {
        result = await this.wa.notifyOrderShipped({
          partnerId: order.customerId, schoolName,
          docNumber: order.docNumber, doNumber: extra.doNumber ?? '', orderId: order.id,
        }) as DispatchResult;
      } else {
        result = await this.wa.notifyInvoiceIssued({
          partnerId: order.customerId, schoolName,
          invoiceNumber: extra.invNumber ?? '', orderNumber: order.docNumber,
          grandTotal: total.toString(), dueDate: fmtDate(extra.dueDate),
          invoiceId: extra.invoiceId ?? BigInt(0),
        }) as DispatchResult;
      }
      const mapped = this.mapResult(result);
      await this.writeLog({
        event, partnerId: order.customerId, portalAccountId: null,
        recipientPhone: phone, recipientName: schoolName,
        docId: order.id, docNumber: order.docNumber,
        status: mapped.status, error: mapped.error, waLogId: result.logId,
      });
    } catch (err) {
      this.logger.warn(`notifyForOrder ${event} order ${orderId}: ${(err as Error).message}`);
    }
  }

  /** Hook: order portal dibuat (checkout portal). */
  async notifyOrderCreated(orderId: bigint): Promise<void> {
    await this.notifyForOrder(orderId, 'ORDER_DITERIMA', {});
  }

  /** Hook: DO untuk order terkait di-POST. */
  async notifyDeliveryPosted(deliveryOrderId: bigint): Promise<void> {
    try {
      const doRow = await this.prisma.erpSlsDeliveryOrder.findFirst({
        where: { id: deliveryOrderId, deletedAt: null },
        select: { orderId: true, docNumber: true },
      });
      if (!doRow?.orderId) return;
      await this.notifyForOrder(doRow.orderId, 'BARANG_DIKIRIM', { doNumber: doRow.docNumber });
    } catch (err) {
      this.logger.warn(`notifyDeliveryPosted ${deliveryOrderId}: ${(err as Error).message}`);
    }
  }

  /** Hook: invoice untuk order terkait di-POST. */
  async notifyInvoicePosted(invoiceId: bigint): Promise<void> {
    try {
      const inv = await this.prisma.erpSlsInvoice.findFirst({
        where: { id: invoiceId, deletedAt: null },
        select: { orderId: true, docNumber: true, grandTotal: true, dueDate: true },
      });
      if (!inv?.orderId) return;
      await this.notifyForOrder(inv.orderId, 'TAGIHAN_TERBIT', {
        invNumber: inv.docNumber, invoiceId, dueDate: inv.dueDate, totalOverride: inv.grandTotal,
      });
    } catch (err) {
      this.logger.warn(`notifyInvoicePosted ${invoiceId}: ${(err as Error).message}`);
    }
  }

  /** Daftar log untuk admin ERP. */
  async listLogs(query: { event?: string; status?: string; partnerId?: string; page?: number; limit?: number }) {
    const page = query.page ?? 1;
    const limit = Math.min(query.limit ?? 50, 200);
    const where: Prisma.ErpNotificationLogWhereInput = { deletedAt: null };
    if (query.event) where.event = query.event;
    if (query.status) where.status = query.status;
    if (query.partnerId) where.partnerId = BigInt(query.partnerId);
    const [rows, total] = await Promise.all([
      this.prisma.erpNotificationLog.findMany({
        where, orderBy: { id: 'desc' }, skip: (page - 1) * limit, take: limit,
      }),
      this.prisma.erpNotificationLog.count({ where }),
    ]);
    return {
      data: rows.map((r) => ({
        ...r,
        id: r.id.toString(),
        partnerId: r.partnerId?.toString() ?? null,
        portalAccountId: r.portalAccountId?.toString() ?? null,
        relatedDocId: r.relatedDocId?.toString() ?? null,
      })),
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) },
    };
  }
}
