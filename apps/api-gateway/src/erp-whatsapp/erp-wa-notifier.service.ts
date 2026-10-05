import { Injectable, Logger, OnModuleDestroy, OnModuleInit } from '@nestjs/common';
import { PrismaService } from '../prisma/prisma.service';
import { ErpWhatsappConfigService } from './erp-whatsapp-config.service';
import { ErpWhatsappService } from './erp-whatsapp.service';

const WINDOW_MS = 7 * 24 * 3600 * 1000;
const TICK_MS = 60_000;

/**
 * Notifier peristiwa ERP → WhatsApp (Fase 3 W6), tanpa mengait ke service
 * transaksi lain: memindai dokumen baru secara berkala dan mengirim lewat
 * ErpWhatsappService. Dedupe memakai sys_wa_logs (referenceType+referenceId+
 * template), jadi tiap peristiwa terkirim sekali.
 *
 * Keamanan peluncuran: pada tick pertama, penanda NOTIFY_SINCE dipasang ke
 * waktu sekarang — dokumen lama tidak pernah diberitahu surut.
 */
@Injectable()
export class ErpWaNotifierService implements OnModuleInit, OnModuleDestroy {
  private readonly logger = new Logger(ErpWaNotifierService.name);
  private timer?: ReturnType<typeof setInterval>;
  private running = false;

  constructor(
    private readonly prisma: PrismaService,
    private readonly wa: ErpWhatsappService,
    private readonly settings: ErpWhatsappConfigService,
  ) {}

  onModuleInit() {
    this.timer = setInterval(() => void this.tick(), TICK_MS);
    this.timer.unref?.();
  }

  onModuleDestroy() {
    if (this.timer) clearInterval(this.timer);
  }

  private async tick(): Promise<void> {
    if (this.running) return;
    this.running = true;
    try {
      let sinceRaw = await this.settings.watchValue('NOTIFY_SINCE');
      if (!sinceRaw) {
        await this.settings.setWatchValue('NOTIFY_SINCE', new Date().toISOString());
        this.logger.log('WA notifier: NOTIFY_SINCE dipasang (dokumen lama tidak diberitahu).');
        return;
      }
      const since = new Date(sinceRaw);
      const floor = new Date(Math.max(since.getTime(), Date.now() - WINDOW_MS));
      await this.scanOrders(floor);
      await this.scanDeliveries(floor);
      await this.scanInvoices(floor);
    } catch (err) {
      this.logger.warn(`WA notifier tick gagal: ${err instanceof Error ? err.message : err}`);
    } finally {
      this.running = false;
    }
  }

  private async alreadyLogged(
    referenceType: string,
    referenceId: bigint,
    templateName: string,
  ): Promise<boolean> {
    const tpl = await this.prisma.erpWaTemplate.findFirst({
      where: { name: templateName },
      select: { id: true },
    });
    if (!tpl) return true; // template belum ada → jangan spam percobaan
    const count = await this.prisma.erpWaLog.count({
      where: { referenceType, referenceId, templateId: tpl.id },
    });
    return count > 0;
  }

  /** Catat penanda "tidak ada nomor" supaya tidak dicoba berulang & terlihat admin. */
  private async markNoPhone(
    templateName: string,
    referenceType: string,
    referenceId: bigint,
    partnerName: string,
  ): Promise<void> {
    const tpl = await this.prisma.erpWaTemplate.findFirst({
      where: { name: templateName },
      select: { id: true },
    });
    await this.prisma.erpWaLog.create({
      data: {
        templateId: tpl?.id ?? null,
        recipientType: 'sekolah',
        recipientPhone: '-',
        body: `(tidak terkirim — ${partnerName} tidak punya nomor WA tercatat)`,
        status: 'gagal',
        errorReason: 'Tidak ada nomor WA tujuan (akun portal/kontak partner kosong)',
        referenceType,
        referenceId,
        metadata: { skipped: 'no_phone' },
      },
    });
  }

  private async partnerName(partnerId: bigint): Promise<string> {
    const p = await this.prisma.erpPartner.findUnique({
      where: { id: partnerId },
      select: { name: true },
    });
    return p?.name ?? 'Bapak/Ibu';
  }

  private async scanOrders(floor: Date): Promise<void> {
    const orders = await this.prisma.erpSlsOrder.findMany({
      where: { createdAt: { gte: floor }, deletedAt: null },
      orderBy: { id: 'asc' },
      take: 20,
      select: { id: true, docNumber: true, grandTotal: true, customerId: true },
    });
    for (const o of orders) {
      if (!o.customerId) continue;
      if (await this.alreadyLogged('ORDER', o.id, 'order_diterima')) continue;
      const schoolName = await this.partnerName(o.customerId);
      const res = await this.wa.notifyOrderReceived({
        partnerId: o.customerId,
        schoolName,
        docNumber: o.docNumber,
        grandTotal: o.grandTotal.toString(),
        orderId: o.id,
      });
      if (!res.success && res.skipped === 'no_phone') {
        await this.markNoPhone('order_diterima', 'ORDER', o.id, schoolName);
      }
    }
  }

  private async scanDeliveries(floor: Date): Promise<void> {
    const dos = await this.prisma.erpSlsDeliveryOrder.findMany({
      where: { status: 'POSTED', updatedAt: { gte: floor }, deletedAt: null },
      orderBy: { id: 'asc' },
      take: 20,
      select: {
        id: true,
        docNumber: true,
        orderId: true,
        order: { select: { docNumber: true, customerId: true } },
      },
    });
    for (const d of dos) {
      if (!d.orderId || !d.order?.customerId) continue;
      if (await this.alreadyLogged('ORDER', d.orderId, 'order_terkirim')) continue;
      const schoolName = await this.partnerName(d.order.customerId);
      const res = await this.wa.notifyOrderShipped({
        partnerId: d.order.customerId,
        schoolName,
        docNumber: d.order.docNumber,
        doNumber: d.docNumber,
        orderId: d.orderId,
      });
      if (!res.success && res.skipped === 'no_phone') {
        await this.markNoPhone('order_terkirim', 'ORDER', d.orderId, schoolName);
      }
    }
  }

  private async scanInvoices(floor: Date): Promise<void> {
    const invoices = await this.prisma.erpSlsInvoice.findMany({
      where: { status: 'POSTED', updatedAt: { gte: floor }, deletedAt: null },
      orderBy: { id: 'asc' },
      take: 20,
      select: {
        id: true,
        docNumber: true,
        grandTotal: true,
        dueDate: true,
        customerId: true,
        orderId: true,
        order: { select: { docNumber: true } },
      },
    });
    for (const inv of invoices) {
      if (!inv.customerId) continue;
      if (await this.alreadyLogged('INVOICE', inv.id, 'tagihan_terbit')) continue;
      const schoolName = await this.partnerName(inv.customerId);
      const res = await this.wa.notifyInvoiceIssued({
        partnerId: inv.customerId,
        schoolName,
        invoiceNumber: inv.docNumber,
        orderNumber: inv.order?.docNumber ?? '-',
        grandTotal: inv.grandTotal.toString(),
        dueDate: inv.dueDate
          ? inv.dueDate.toLocaleDateString('id-ID', {
              day: 'numeric',
              month: 'long',
              year: 'numeric',
            })
          : '-',
        invoiceId: inv.id,
      });
      if (!res.success && res.skipped === 'no_phone') {
        await this.markNoPhone('tagihan_terbit', 'INVOICE', inv.id, schoolName);
      }
    }
  }
}
