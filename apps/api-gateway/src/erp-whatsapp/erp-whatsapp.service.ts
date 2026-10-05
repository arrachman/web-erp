import { BadRequestException, Injectable, Logger, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { ErpWhatsappConfigService } from './erp-whatsapp-config.service';
import { ErpWhatsappProvider } from './erp-whatsapp-provider.service';
import { ErpWaTemplatesService } from './erp-wa-templates.service';
import { normalizePhoneId, phoneLookupVariants } from './wa-phone.util';
import { renderTemplate } from './wa-template.util';
import { gatewayPost } from './wa-gateway-http.util';
import { QueryWaLogDto, SendWaTestDto } from './dto/erp-whatsapp.dto';

export interface WaWebhookDto {
  id?: string;
  sender?: string;
  status?: string;
  state?: string;
  device?: string;
  reason?: string;
}

const rupiah = (n: number | string) =>
  new Intl.NumberFormat('id-ID').format(typeof n === 'string' ? Number(n) : n);

/**
 * Inti pengiriman WhatsApp ERP: log pengiriman, kirim (tes + event), webhook
 * status, dan kesehatan koneksi. Pengiriman sinkron (volume transaksional
 * rendah); setiap kirim tercatat di `sys_wa_logs` dengan status
 * queued → terkirim → sampai → dibaca / gagal.
 *
 * Kill-switch: pengiriman OTOMATIS (event) hanya jalan bila SEND_ENABLED=true
 * di pengaturan; kirim tes & kirim ulang manual selalu jalan.
 */
@Injectable()
export class ErpWhatsappService {
  private readonly logger = new Logger(ErpWhatsappService.name);

  constructor(
    private readonly prisma: PrismaService,
    private readonly provider: ErpWhatsappProvider,
    private readonly settings: ErpWhatsappConfigService,
    private readonly templates: ErpWaTemplatesService,
  ) {}

  // ----- Log & statistik -----

  async findAllLogs(query: QueryWaLogDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 50;
    const where: Prisma.ErpWaLogWhereInput = {};
    if (query.status) where.status = query.status;
    if (query.recipientPhone) where.recipientPhone = { contains: query.recipientPhone };
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpWaLog.findMany({
        where,
        include: { template: { select: { id: true, name: true, category: true } } },
        orderBy: [{ id: 'desc' }],
        skip: (page - 1) * limit,
        take: limit,
      }),
      this.prisma.erpWaLog.count({ where }),
    ]);
    return {
      success: true,
      data: items.map((l) => ({
        id: l.id.toString(),
        templateId: l.templateId?.toString() ?? null,
        templateName: l.template?.name ?? null,
        recipientType: l.recipientType,
        recipientPhone: l.recipientPhone,
        body: l.body,
        messageId: l.messageId,
        status: l.status,
        errorReason: l.errorReason,
        referenceType: l.referenceType,
        referenceId: l.referenceId?.toString() ?? null,
        sentAt: l.sentAt,
        deliveredAt: l.deliveredAt,
        readAt: l.readAt,
        failedAt: l.failedAt,
        createdAt: l.createdAt,
      })),
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) },
    };
  }

  async getStats() {
    const start = new Date();
    start.setHours(0, 0, 0, 0);
    const end = new Date();
    end.setHours(23, 59, 59, 999);
    const where: Prisma.ErpWaLogWhereInput = { createdAt: { gte: start, lte: end } };
    const [total, read, failed] = await this.prisma.$transaction([
      this.prisma.erpWaLog.count({ where }),
      this.prisma.erpWaLog.count({ where: { ...where, status: 'dibaca' } }),
      this.prisma.erpWaLog.count({ where: { ...where, status: 'gagal' } }),
    ]);
    return {
      success: true,
      data: {
        sentToday: total,
        readToday: read,
        failedToday: failed,
        readRate: total > 0 ? Math.round((read / total) * 100) : 0,
      },
    };
  }

  // ----- Kirim -----

  async sendTest(dto: SendWaTestDto, actorId?: string) {
    let body = dto.body ?? '';
    let templateId: bigint | null = null;
    if (dto.templateId) {
      const tpl = await this.templates.findOne(dto.templateId);
      body = renderTemplate(tpl.data.body, dto.variables ?? {});
      templateId = BigInt(tpl.data.id);
    } else if (dto.body && dto.variables) {
      body = renderTemplate(dto.body, dto.variables);
    }
    if (!body.trim()) {
      throw new BadRequestException('Isi pesan kosong — pilih template atau isi pesan manual.');
    }
    return this.dispatchRaw({
      templateId,
      recipientType: 'tes',
      recipientPhone: dto.phone,
      body,
      metadata: { test: true, actor: actorId },
    });
  }

  /** Kirim ulang isi log lama apa adanya (sebagai log baru). */
  async resendLog(id: string, actorId?: string) {
    const log = await this.prisma.erpWaLog.findUnique({ where: { id: BigInt(id) } });
    if (!log) throw new NotFoundException(`Log ${id} tidak ditemukan`);
    return this.dispatchRaw({
      templateId: log.templateId,
      recipientType: log.recipientType,
      recipientPhone: log.recipientPhone,
      body: log.body,
      referenceType: log.referenceType,
      referenceId: log.referenceId,
      metadata: { resendOf: id, actor: actorId },
    });
  }

  /**
   * Dispatch event-driven berdasarkan nama template. Menghormati kill-switch
   * SEND_ENABLED. Tidak melempar — aman dipanggil dari alur transaksi lain
   * (kegagalan WA tidak boleh menggagalkan transaksi utama).
   */
  async dispatch(params: {
    templateName: string;
    recipientType: string;
    recipientPhone: string;
    variables: Record<string, string | number>;
    referenceType?: string;
    referenceId?: bigint;
  }): Promise<{ success: boolean; skipped?: string; logId?: string; status?: string }> {
    if (!(await this.settings.sendEnabled())) {
      this.logger.log(`WA dispatch dilewati (SEND_ENABLED=false): ${params.templateName}`);
      return { success: false, skipped: 'disabled' };
    }
    const tpl = await this.prisma.erpWaTemplate.findFirst({
      where: { name: params.templateName, isActive: true, deletedAt: null },
    });
    if (!tpl) {
      this.logger.warn(`Template '${params.templateName}' tidak ada/nonaktif — dispatch dilewati`);
      return { success: false, skipped: 'template_not_found' };
    }
    const body = renderTemplate(tpl.body, params.variables);
    const res = await this.dispatchRaw({
      templateId: tpl.id,
      recipientType: params.recipientType,
      recipientPhone: params.recipientPhone,
      body,
      referenceType: params.referenceType,
      referenceId: params.referenceId,
      metadata: { event: tpl.triggerEvent, variables: params.variables },
    });
    return { success: res.success, logId: res.data.logId, status: res.data.status };
  }

  private async dispatchRaw(args: {
    templateId: bigint | null;
    recipientType: string;
    recipientPhone: string;
    body: string;
    referenceType?: string | null;
    referenceId?: bigint | null;
    metadata: Record<string, unknown>;
  }) {
    const normalizedPhone = normalizePhoneId(args.recipientPhone) ?? args.recipientPhone;
    const log = await this.prisma.erpWaLog.create({
      data: {
        templateId: args.templateId,
        recipientType: args.recipientType,
        recipientPhone: normalizedPhone,
        body: args.body,
        referenceType: args.referenceType ?? null,
        referenceId: args.referenceId ?? null,
        metadata: args.metadata as Prisma.InputJsonValue,
        status: 'queued',
      },
    });
    const result = await this.provider.send(normalizedPhone, args.body);
    const status = result.status === 'sent' ? 'terkirim' : 'gagal';
    await this.prisma.erpWaLog.update({
      where: { id: log.id },
      data: {
        messageId: result.messageId,
        status,
        sentAt: result.status === 'sent' ? new Date() : null,
        failedAt: result.status === 'failed' ? new Date() : null,
        errorReason: result.errorReason ?? null,
      },
    });
    return {
      success: result.status === 'sent',
      data: { logId: log.id.toString(), status, messageId: result.messageId },
      message: result.errorReason,
    };
  }

  // ----- Notifikasi peristiwa ERP (dipakai modul lain) -----

  /** Nomor WA tujuan sekolah: akun portal aktif → kontak partner. */
  async resolvePartnerPhone(partnerId: bigint): Promise<string | null> {
    const account = await this.prisma.erpPortalAccount.findFirst({
      where: { partnerId, status: 'ACTIVE', phone: { not: null } },
      orderBy: { id: 'desc' },
      select: { phone: true },
    });
    if (account?.phone) return account.phone;
    const contact = await this.prisma.erpPartnerContact.findFirst({
      where: { partnerId, phone: { not: null } },
      orderBy: { id: 'asc' },
      select: { phone: true },
    });
    return contact?.phone ?? null;
  }

  async notifyOrderReceived(params: {
    partnerId: bigint;
    schoolName: string;
    docNumber: string;
    grandTotal: string | number;
    orderId: bigint;
  }) {
    const phone = await this.resolvePartnerPhone(params.partnerId);
    if (!phone) return { success: false, skipped: 'no_phone' as const };
    return this.dispatch({
      templateName: 'order_diterima',
      recipientType: 'sekolah',
      recipientPhone: phone,
      variables: {
        sekolah: params.schoolName,
        nomor_order: params.docNumber,
        total: rupiah(params.grandTotal),
      },
      referenceType: 'ORDER',
      referenceId: params.orderId,
    });
  }

  async notifyOrderShipped(params: {
    partnerId: bigint;
    schoolName: string;
    docNumber: string;
    doNumber: string;
    orderId: bigint;
  }) {
    const phone = await this.resolvePartnerPhone(params.partnerId);
    if (!phone) return { success: false, skipped: 'no_phone' as const };
    return this.dispatch({
      templateName: 'order_terkirim',
      recipientType: 'sekolah',
      recipientPhone: phone,
      variables: {
        sekolah: params.schoolName,
        nomor_order: params.docNumber,
        nomor_do: params.doNumber,
      },
      referenceType: 'ORDER',
      referenceId: params.orderId,
    });
  }

  async notifyInvoiceIssued(params: {
    partnerId: bigint;
    schoolName: string;
    invoiceNumber: string;
    orderNumber: string;
    grandTotal: string | number;
    dueDate: string;
    invoiceId: bigint;
  }) {
    const phone = await this.resolvePartnerPhone(params.partnerId);
    if (!phone) return { success: false, skipped: 'no_phone' as const };
    return this.dispatch({
      templateName: 'tagihan_terbit',
      recipientType: 'sekolah',
      recipientPhone: phone,
      variables: {
        sekolah: params.schoolName,
        nomor_invoice: params.invoiceNumber,
        nomor_order: params.orderNumber,
        total: rupiah(params.grandTotal),
        jatuh_tempo: params.dueDate,
      },
      referenceType: 'INVOICE',
      referenceId: params.invoiceId,
    });
  }

  // ----- Webhook & kesehatan -----

  async handleWebhook(dto: WaWebhookDto) {
    if (!dto.id && !dto.sender) return { success: false, error: 'missing_identifier' };
    let log = dto.id
      ? await this.prisma.erpWaLog.findFirst({ where: { messageId: dto.id } })
      : null;
    if (!log && dto.sender) {
      log = await this.prisma.erpWaLog.findFirst({
        where: {
          recipientPhone: { in: phoneLookupVariants(dto.sender) },
          status: { in: ['terkirim', 'queued'] },
        },
        orderBy: { id: 'desc' },
      });
    }
    if (!log) {
      this.logger.warn(`Webhook WA tidak match log — id=${dto.id} sender=${dto.sender}`);
      return { success: false, error: 'log_not_found' };
    }
    const statusMap: Record<string, string> = {
      sent: 'terkirim',
      delivered: 'sampai',
      read: 'dibaca',
      failed: 'gagal',
    };
    const FINAL = new Set(['delivered', 'read', 'failed']);
    const raw = FINAL.has(dto.status ?? '') ? dto.status : (dto.state ?? dto.status);
    const newStatus = (raw && statusMap[raw]) || raw || log.status;
    const data: Prisma.ErpWaLogUpdateInput = { status: newStatus };
    if (newStatus === 'sampai') data.deliveredAt = new Date();
    if (newStatus === 'dibaca') data.readAt = new Date();
    if (newStatus === 'gagal') {
      data.failedAt = new Date();
      data.errorReason = dto.reason ?? null;
    }
    await this.prisma.erpWaLog.update({ where: { id: log.id }, data });
    return { success: true, data: { logId: log.id.toString(), status: newStatus } };
  }

  async connectionHealth() {
    const summary = await this.settings.summary();
    let gateway: { up: boolean; degraded?: boolean; devices?: number } = { up: false };
    try {
      const res = await fetch(`${summary.gatewayUrl}/health`, {
        signal: AbortSignal.timeout(3000),
      });
      const json = (await res.json()) as { degraded?: boolean; devices?: unknown[] };
      gateway = { up: res.ok, degraded: json.degraded, devices: json.devices?.length ?? 0 };
    } catch {
      gateway = { up: false };
    }
    let activeDevice: { connected: boolean; devicePhone?: string; deviceName?: string } = {
      connected: false,
    };
    const token = await this.settings.activeDeviceToken();
    if (token && gateway.up) {
      try {
        const { json } = await gatewayPost(summary.gatewayUrl, '/device', token, {});
        activeDevice = {
          connected: json.status === true && json.device_status === 'connect',
          devicePhone: typeof json.device === 'string' ? json.device : undefined,
          deviceName: typeof json.name === 'string' ? json.name : undefined,
        };
      } catch {
        activeDevice = { connected: false };
      }
    }
    return { success: true, data: { ...summary, gateway, activeDevice } };
  }
}
