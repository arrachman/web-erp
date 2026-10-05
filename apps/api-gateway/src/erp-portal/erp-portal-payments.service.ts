import {
  BadRequestException, Injectable, Logger, NotFoundException, UnauthorizedException,
} from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { createHmac, randomBytes, timingSafeEqual } from 'crypto';
import { PrismaService } from '../prisma/prisma.service';
import { ErpFinArReceiptsService } from '../erp-fin-ar-receipts/erp-fin-ar-receipts.service';
import type { ErpPortalAccount, ErpPortalPayment } from '@prisma/client';

/**
 * W5 — Pembayaran portal (Fase 3 G2).
 * Keputusan user 2026-10-05 (plan §2 #5): provider = **TRANSFER MANUAL
 * dengan konfirmasi admin** (bukan payment gateway). Alur:
 *   PENDING → (pengguna klik "Saya Sudah Transfer") MENUNGGU_KONFIRMASI
 *   → (admin verifikasi mutasi & konfirmasi) PAID — saat itu AR Receipt
 *   (IP) dibuat + SUBMIT/APPROVE/POST otomatis → invoice lunas;
 *   admin juga bisa menolak (DITOLAK, pengguna dapat mengklaim ulang).
 * Instruksi transfer dibaca dari rekening primer perusahaan
 * (sys_bank_accounts is_primary) — data resmi diatur admin di ERP,
 * tidak dikarang di kode. Jalur webhook HMAC di bawah dipertahankan
 * sebagai kode legacy provider 'SIMULASI' (tidak dipakai pembayaran baru).
 */
export const PORTAL_PAYMENT_PROVIDER = 'MANUAL';
const LEGACY_SIM_PROVIDER = 'SIMULASI';
const PROVISIONAL_WEBHOOK_SECRET = 'provisional-dev-secret-ganti-di-produksi';

type PaymentMeta = {
  claimedAt?: string; claimNote?: string;
  rejectedReason?: string; rejectedAt?: string;
  [k: string]: unknown;
};

@Injectable()
export class ErpPortalPaymentsService {
  private readonly logger = new Logger(ErpPortalPaymentsService.name);

  constructor(
    private readonly prisma: PrismaService,
    private readonly receipts: ErpFinArReceiptsService,
  ) {}

  // ── Webhook legacy (provider SIMULASI) ──────────────────────────────────
  private webhookSecret(): string {
    return process.env.PORTAL_PAYMENT_WEBHOOK_SECRET ?? PROVISIONAL_WEBHOOK_SECRET;
  }

  private verifySignature(ref: string, amount: string, status: string, signature?: string): boolean {
    if (!signature) return false;
    const expected = createHmac('sha256', this.webhookSecret())
      .update(`${ref}.${amount}.${status}`).digest('hex');
    const a = Buffer.from(expected);
    const b = Buffer.from(signature);
    return a.length === b.length && timingSafeEqual(a, b);
  }

  // ── Util ────────────────────────────────────────────────────────────────
  private async outstanding(invoiceId: bigint, grandTotal: Prisma.Decimal): Promise<Prisma.Decimal> {
    const sum = await this.prisma.erpFinSettlementAllocation.aggregate({
      where: { invoiceRef: invoiceId.toString() },
      _sum: { amount: true },
    });
    return new Prisma.Decimal(grandTotal).sub(new Prisma.Decimal(sum._sum.amount ?? 0));
  }

  /** Rekening tujuan transfer: rekening primer perusahaan di sys_bank_accounts. */
  async transferTarget() {
    const acc = await this.prisma.erpBankAccount.findFirst({
      where: { isPrimary: true, isActive: true, deletedAt: null },
    }) ?? await this.prisma.erpBankAccount.findFirst({
      where: { isActive: true, deletedAt: null },
      orderBy: { id: 'asc' },
    });
    if (!acc) return null;
    return {
      bankName: acc.bankName,
      accountNumber: acc.accountNumber,
      accountHolder: acc.accountHolder,
      accountName: acc.name,
    };
  }

  private view(p: ErpPortalPayment, invoiceNumber?: string, partnerName?: string) {
    const meta = (p.metadata ?? {}) as PaymentMeta;
    return {
      id: p.id.toString(),
      invoiceId: p.invoiceId.toString(),
      invoiceNumber: invoiceNumber ?? null,
      partnerId: p.partnerId.toString(),
      partnerName: partnerName ?? null,
      provider: p.provider,
      providerRef: p.providerRef,
      vaNumber: p.vaNumber,
      amount: p.amount.toString(),
      status: p.status,
      expiresAt: p.expiresAt,
      paidAt: p.paidAt,
      receiptId: p.receiptId?.toString() ?? null,
      claimedAt: meta.claimedAt ?? null,
      claimNote: meta.claimNote ?? null,
      rejectedReason: meta.rejectedReason ?? null,
      createdAt: p.createdAt,
    };
  }

  private async viewForUser(p: ErpPortalPayment, invoiceNumber?: string) {
    const target = await this.transferTarget();
    return {
      ...this.view(p, invoiceNumber),
      instructions: target
        ? { ...target, amount: p.amount.toString(), reference: p.providerRef }
        : null,
    };
  }

  /** Buat (atau pakai ulang) intent pembayaran transfer manual untuk invoice milik akun. */
  async createForInvoice(account: ErpPortalAccount, invoiceIdRaw: string) {
    const invoiceId = BigInt(invoiceIdRaw);
    const invoice = await this.prisma.erpSlsInvoice.findFirst({
      where: { id: invoiceId, deletedAt: null },
      select: {
        id: true, docNumber: true, customerId: true, orderId: true, status: true,
        settlementStatus: true, grandTotal: true,
      },
    });
    if (!invoice) throw new NotFoundException('Tagihan tidak ditemukan.');
    const partnerForPayment = invoice.customerId ?? account.partnerId;
    if (!partnerForPayment) throw new NotFoundException('Tagihan tidak ditemukan.');
    if (account.role === 'ORANG_TUA') {
      const order = invoice.orderId
        ? await this.prisma.erpSlsOrder.findUnique({
            where: { id: invoice.orderId }, select: { customFields: true },
          })
        : null;
      const cf = (order?.customFields ?? {}) as { portalParent?: { accountId?: string } };
      if (cf.portalParent?.accountId !== account.id.toString()) {
        throw new NotFoundException('Tagihan tidak ditemukan.');
      }
    } else if (!account.partnerId || invoice.customerId !== account.partnerId) {
      throw new NotFoundException('Tagihan tidak ditemukan.');
    }
    if (invoice.status !== 'POSTED') {
      throw new BadRequestException('Tagihan belum terbit (belum diposting).');
    }
    if (invoice.settlementStatus === 'PAID') {
      throw new BadRequestException('Tagihan sudah lunas.');
    }
    const due = await this.outstanding(invoice.id, invoice.grandTotal);
    if (due.lte(0)) throw new BadRequestException('Tagihan sudah lunas.');

    const existing = await this.prisma.erpPortalPayment.findFirst({
      where: {
        invoiceId: invoice.id, deletedAt: null,
        status: { in: ['PENDING', 'MENUNGGU_KONFIRMASI'] },
      },
      orderBy: { id: 'desc' },
    });
    if (existing) return this.viewForUser(existing, invoice.docNumber);

    const created = await this.prisma.erpPortalPayment.create({
      data: {
        invoiceId: invoice.id,
        partnerId: partnerForPayment,
        portalAccountId: account.id,
        provider: PORTAL_PAYMENT_PROVIDER,
        providerRef: `MAN-${invoice.id}-${randomBytes(4).toString('hex')}`,
        amount: due,
        status: 'PENDING',
        metadata: { origin: 'portal-pay', method: 'TRANSFER_MANUAL' },
      },
    });
    return this.viewForUser(created, invoice.docNumber);
  }

  /** Riwayat pembayaran akun (sekolah: semua pembayaran partner; orang tua: miliknya). */
  async listForAccount(account: ErpPortalAccount) {
    const where = account.role === 'ORANG_TUA'
      ? { portalAccountId: account.id, deletedAt: null }
      : { partnerId: account.partnerId ?? BigInt(-1), deletedAt: null };
    const rows = await this.prisma.erpPortalPayment.findMany({
      where, orderBy: { id: 'desc' }, take: 100,
    });
    const invIds = [...new Set(rows.map((r) => r.invoiceId))];
    const invoices = invIds.length
      ? await this.prisma.erpSlsInvoice.findMany({
          where: { id: { in: invIds } }, select: { id: true, docNumber: true },
        })
      : [];
    const numById = new Map(invoices.map((i) => [i.id.toString(), i.docNumber]));
    return Promise.all(rows.map((r) => this.viewForUser(r, numById.get(r.invoiceId.toString()))));
  }

  /** Pengguna menyatakan sudah transfer → menunggu konfirmasi admin. */
  async confirmSent(account: ErpPortalAccount, paymentIdRaw: string, note?: string) {
    const payment = await this.prisma.erpPortalPayment.findFirst({
      where: { id: BigInt(paymentIdRaw), deletedAt: null },
    });
    if (!payment) throw new NotFoundException('Pembayaran tidak ditemukan.');
    const own = account.role === 'ORANG_TUA'
      ? payment.portalAccountId === account.id
      : payment.partnerId === account.partnerId;
    if (!own) throw new NotFoundException('Pembayaran tidak ditemukan.');
    if (payment.status === 'PAID') throw new BadRequestException('Pembayaran sudah lunas.');
    if (payment.status === 'MENUNGGU_KONFIRMASI') {
      return this.viewForUser(payment);
    }
    if (payment.status !== 'PENDING' && payment.status !== 'DITOLAK') {
      throw new BadRequestException(`Status pembayaran ${payment.status} tidak bisa dikonfirmasi.`);
    }
    const meta = (payment.metadata ?? {}) as PaymentMeta;
    const updated = await this.prisma.erpPortalPayment.update({
      where: { id: payment.id },
      data: {
        status: 'MENUNGGU_KONFIRMASI',
        metadata: {
          ...meta, claimedAt: new Date().toISOString(), claimNote: note ?? null,
          rejectedReason: undefined, rejectedAt: undefined,
        } as Prisma.InputJsonValue,
      },
    });
    return this.viewForUser(updated);
  }

  // ── Admin ERP: antrean konfirmasi ───────────────────────────────────────
  async listAdmin(status?: string) {
    const rows = await this.prisma.erpPortalPayment.findMany({
      where: { deletedAt: null, ...(status ? { status } : {}) },
      orderBy: { id: 'desc' }, take: 200,
    });
    const invIds = [...new Set(rows.map((r) => r.invoiceId))];
    const partnerIds = [...new Set(rows.map((r) => r.partnerId))];
    const [invoices, partners] = await Promise.all([
      invIds.length
        ? this.prisma.erpSlsInvoice.findMany({
            where: { id: { in: invIds } }, select: { id: true, docNumber: true },
          })
        : [],
      partnerIds.length
        ? this.prisma.erpPartner.findMany({
            where: { id: { in: partnerIds } }, select: { id: true, name: true },
          })
        : [],
    ]);
    const invById = new Map(invoices.map((i) => [i.id.toString(), i.docNumber]));
    const parById = new Map(partners.map((p) => [p.id.toString(), p.name]));
    return rows.map((r) => this.view(
      r, invById.get(r.invoiceId.toString()), parById.get(r.partnerId.toString()),
    ));
  }

  /** Akun GL kas/bank penerimaan: GL rekening primer bila terhubung, else BCA 1110.01. */
  private async settlementBankAccountId(): Promise<bigint> {
    const bank = await this.prisma.erpBankAccount.findFirst({
      where: { isPrimary: true, isActive: true, deletedAt: null, glAccountId: { not: null } },
      select: { glAccountId: true },
    });
    if (bank?.glAccountId) return bank.glAccountId;
    const acc = await this.prisma.erpAccount.findFirst({
      where: { deletedAt: null, code: { startsWith: '1110.01' } },
      orderBy: { code: 'asc' }, select: { id: true },
    });
    if (!acc) throw new BadRequestException('Akun bank penerimaan belum tersedia.');
    return acc.id;
  }

  /**
   * Penyelesaian inti: buat AR Receipt (IP) + SUBMIT/APPROVE/POST atas
   * sisa tagihan invoice, lalu tandai pembayaran PAID. Dipakai konfirmasi
   * admin (jalur utama) dan webhook legacy.
   */
  private async settlePayment(payment: ErpPortalPayment, actorId?: string): Promise<ErpPortalPayment> {
    const invoice = await this.prisma.erpSlsInvoice.findUnique({
      where: { id: payment.invoiceId },
      select: {
        id: true, docNumber: true, branchId: true, customerId: true,
        currencyId: true, exchangeRate: true, grandTotal: true,
      },
    });
    if (!invoice) throw new NotFoundException('Tagihan tidak ditemukan.');
    const customerId = invoice.customerId;
    if (!customerId) throw new BadRequestException('Tagihan tidak punya pelanggan.');
    const due = await this.outstanding(invoice.id, invoice.grandTotal);
    if (due.lte(0)) {
      return this.prisma.erpPortalPayment.update({
        where: { id: payment.id },
        data: { status: 'PAID', paidAt: new Date() },
      });
    }
    const amountStr = due.toString();
    const bankAccountId = await this.settlementBankAccountId();
    const created: any = await this.receipts.create({
      branchId: invoice.branchId.toString(),
      transactionDate: new Date().toISOString().slice(0, 10),
      partnerId: customerId.toString(),
      description: `Pembayaran portal ${payment.providerRef} untuk ${invoice.docNumber}`,
      currencyId: invoice.currencyId.toString(),
      exchangeRate: invoice.exchangeRate.toString(),
      source: 'PORTAL',
      instruments: [{
        method: 'TRANSFER' as never, bankAccountId: bankAccountId.toString(),
        amount: amountStr, lineNo: 1,
      }],
      allocations: [{ invoiceId: invoice.id.toString(), amount: amountStr, lineNo: 1 }],
    }, actorId);
    const receiptId = BigInt((created?.data ?? created).id);
    for (const action of ['SUBMIT', 'APPROVE', 'POST'] as const) {
      await this.receipts.transition(receiptId, { action } as never, actorId);
    }
    const done = await this.prisma.erpPortalPayment.update({
      where: { id: payment.id },
      data: { status: 'PAID', paidAt: new Date(), receiptId },
    });
    this.logger.log(`Pembayaran ${payment.providerRef} lunas → receipt ${receiptId}`);
    return done;
  }

  /** Admin mengonfirmasi dana diterima → lunasi. */
  async confirmByAdmin(paymentIdRaw: string, actorId?: string) {
    const payment = await this.prisma.erpPortalPayment.findFirst({
      where: { id: BigInt(paymentIdRaw), deletedAt: null },
    });
    if (!payment) throw new NotFoundException('Pembayaran tidak ditemukan.');
    if (payment.status === 'PAID') {
      throw new BadRequestException('Pembayaran sudah lunas.');
    }
    if (payment.status !== 'MENUNGGU_KONFIRMASI' && payment.status !== 'PENDING') {
      throw new BadRequestException(`Status ${payment.status} tidak bisa dikonfirmasi.`);
    }
    const done = await this.settlePayment(payment, actorId);
    const invoice = await this.prisma.erpSlsInvoice.findUnique({
      where: { id: done.invoiceId }, select: { docNumber: true },
    });
    const partner = await this.prisma.erpPartner.findUnique({
      where: { id: done.partnerId }, select: { name: true },
    });
    return this.view(done, invoice?.docNumber, partner?.name);
  }

  /** Admin menolak klaim transfer (dana tidak ditemukan) → pengguna bisa klaim ulang. */
  async rejectByAdmin(paymentIdRaw: string, reason: string, actorId?: string) {
    const payment = await this.prisma.erpPortalPayment.findFirst({
      where: { id: BigInt(paymentIdRaw), deletedAt: null },
    });
    if (!payment) throw new NotFoundException('Pembayaran tidak ditemukan.');
    if (payment.status === 'PAID') throw new BadRequestException('Pembayaran sudah lunas.');
    const meta = (payment.metadata ?? {}) as PaymentMeta;
    const updated = await this.prisma.erpPortalPayment.update({
      where: { id: payment.id },
      data: {
        status: 'DITOLAK',
        updatedById: actorId ? BigInt(actorId) : null,
        metadata: {
          ...meta, rejectedReason: reason, rejectedAt: new Date().toISOString(),
        } as Prisma.InputJsonValue,
      },
    });
    return this.view(updated);
  }

  /** Webhook legacy provider SIMULASI (tidak dipakai pembayaran MANUAL). */
  async handleWebhook(provider: string, signature: string | undefined, body: { ref?: string; status?: string; amount?: string }) {
    if (provider !== LEGACY_SIM_PROVIDER) throw new NotFoundException('Provider tidak dikenal.');
    const ref = String(body?.ref ?? '');
    const status = String(body?.status ?? '');
    const amount = String(body?.amount ?? '');
    if (!ref || !this.verifySignature(ref, amount, status, signature)) {
      throw new UnauthorizedException('Signature webhook tidak valid.');
    }
    const payment = await this.prisma.erpPortalPayment.findFirst({
      where: { providerRef: ref, deletedAt: null },
    });
    if (!payment) throw new NotFoundException('Pembayaran tidak ditemukan.');
    if (payment.status === 'PAID') return { ok: true, alreadyPaid: true };
    if (status !== 'PAID') return { ok: true, settled: false };
    if (!new Prisma.Decimal(amount).equals(payment.amount)) {
      throw new BadRequestException('Nominal webhook tidak sama dengan nominal tagihan.');
    }
    const done = await this.settlePayment(payment);
    return { ok: true, settled: true, payment: this.view(done) };
  }
}
