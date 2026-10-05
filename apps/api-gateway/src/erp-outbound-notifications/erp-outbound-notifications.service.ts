import { Injectable, Logger } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';

export type NotificationEvent = 'ORDER_DITERIMA' | 'BARANG_DIKIRIM' | 'TAGIHAN_TERBIT';

const TEMPLATES: Record<NotificationEvent, { code: string; render: (v: Record<string, string>) => string }> = {
  ORDER_DITERIMA: {
    code: 'WA_ORDER_DITERIMA',
    render: (v) =>
      `Halo ${v.nama}, pesanan ${v.docNumber} sebesar Rp${v.total} sudah kami terima dan sedang diproses. Terima kasih — CV Bahtera Madani.`,
  },
  BARANG_DIKIRIM: {
    code: 'WA_BARANG_DIKIRIM',
    render: (v) =>
      `Halo ${v.nama}, pesanan ${v.docNumber} sudah dikirim (Surat Jalan ${v.doNumber}) dan sedang dalam perjalanan ke ${v.sekolah}. — CV Bahtera Madani.`,
  },
  TAGIHAN_TERBIT: {
    code: 'WA_TAGIHAN_TERBIT',
    render: (v) =>
      `Halo ${v.nama}, tagihan ${v.invNumber} sebesar Rp${v.total} untuk pesanan ${v.docNumber} sudah terbit. Jatuh tempo ${v.dueDate}. Silakan buka portal untuk membayar. — CV Bahtera Madani.`,
  },
};

function fmtRp(value: Prisma.Decimal | number | string): string {
  const n = Number(value);
  return n.toLocaleString('id-ID');
}

function fmtDate(d: Date | null | undefined): string {
  if (!d) return '-';
  return new Date(d).toLocaleDateString('id-ID', { day: 'numeric', month: 'long', year: 'numeric' });
}

/**
 * W6 — Notifikasi WhatsApp (Fase 3 G2).
 * BSP WhatsApp resmi BELUM dipilih (plan §2 #6), jadi pengirim v1 adalah
 * adapter 'LOG': pesan dirender dari template, dicatat ke
 * sys_notification_logs dengan status SENT (tercatat) — bukan terkirim ke
 * WhatsApp sungguhan. Saat BSP dipilih, hanya method send() yang diganti
 * adapter nyata; template, hook peristiwa, dan log tidak berubah.
 * Service ini TIDAK PERNAH melempar error ke pemanggil: kegagalan
 * notifikasi tidak boleh menggagalkan transaksi bisnis.
 */
@Injectable()
export class ErpOutboundNotificationsService {
  private readonly logger = new Logger(ErpOutboundNotificationsService.name);

  constructor(private readonly prisma: PrismaService) {}

  /** Pengirim v1 (LOG). Adapter BSP menggantikan isi method ini saja. */
  private async send(_phone: string, _message: string): Promise<{ providerMessageId: string | null }> {
    return { providerMessageId: null };
  }

  private async dispatchToAccount(params: {
    event: NotificationEvent;
    account: { id: bigint; fullName: string; phone: string | null; partnerId: bigint | null };
    vars: Record<string, string>;
    docType: string;
    docId: bigint;
    docNumber: string;
  }): Promise<void> {
    const tpl = TEMPLATES[params.event];
    const message = tpl.render(params.vars);
    const base = {
      channel: 'WHATSAPP',
      event: params.event,
      templateCode: tpl.code,
      recipientPhone: params.account.phone,
      recipientName: params.account.fullName,
      partnerId: params.account.partnerId,
      portalAccountId: params.account.id,
      relatedDocType: params.docType,
      relatedDocId: params.docId,
      relatedDocNumber: params.docNumber,
      message,
      provider: 'LOG',
    };
    if (!params.account.phone) {
      await this.prisma.erpNotificationLog.create({
        data: { ...base, status: 'SKIPPED', error: 'Akun portal tidak punya nomor telepon.' },
      });
      return;
    }
    try {
      const sent = await this.send(params.account.phone, message);
      await this.prisma.erpNotificationLog.create({
        data: { ...base, status: 'SENT', providerMessageId: sent.providerMessageId },
      });
    } catch (err) {
      this.logger.warn(`Notifikasi ${params.event} gagal: ${(err as Error).message}`);
      await this.prisma.erpNotificationLog.create({
        data: { ...base, status: 'FAILED', error: (err as Error).message },
      }).catch(() => undefined);
    }
  }

  /** Akun portal penerima untuk sebuah order: orang tua pemesan saja, atau semua akun aktif sekolah. */
  private async recipientsForOrder(order: {
    id: bigint;
    customerId: bigint | null;
    customFields: unknown;
  }): Promise<{ id: bigint; fullName: string; phone: string | null; partnerId: bigint | null }[]> {
    const cf = (order.customFields ?? {}) as { portalParent?: { accountId?: string } };
    if (cf.portalParent?.accountId) {
      const acc = await this.prisma.erpPortalAccount.findFirst({
        where: { id: BigInt(cf.portalParent.accountId), deletedAt: null, status: 'ACTIVE' },
        select: { id: true, fullName: true, phone: true, partnerId: true },
      });
      return acc ? [acc] : [];
    }
    return this.prisma.erpPortalAccount.findMany({
      where: { partnerId: order.customerId, deletedAt: null, status: 'ACTIVE' },
      select: { id: true, fullName: true, phone: true, partnerId: true },
      take: 20,
    });
  }

  private async notifyForOrder(
    orderId: bigint,
    event: NotificationEvent,
    extra: { doNumber?: string; invNumber?: string; dueDate?: Date | null; totalOverride?: Prisma.Decimal },
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
      const recipients = await this.recipientsForOrder(order);
      if (recipients.length === 0) return;
      const vars: Record<string, string> = {
        nama: '',
        docNumber: order.docNumber,
        total: fmtRp(extra.totalOverride ?? order.grandTotal),
        sekolah: partner?.name ?? '',
        doNumber: extra.doNumber ?? '',
        invNumber: extra.invNumber ?? '',
        dueDate: fmtDate(extra.dueDate),
      };
      for (const acc of recipients) {
        vars.nama = acc.fullName;
        await this.dispatchToAccount({
          event, account: acc, vars,
          docType: 'sls_orders', docId: order.id, docNumber: order.docNumber,
        });
      }
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
        invNumber: inv.docNumber, dueDate: inv.dueDate, totalOverride: inv.grandTotal,
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
