import {
  BadRequestException, Injectable, Logger, NotFoundException, UnauthorizedException,
} from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { createHmac, randomBytes, timingSafeEqual } from 'crypto';
import { PrismaService } from '../prisma/prisma.service';
import { ErpFinArReceiptsService } from '../erp-fin-ar-receipts/erp-fin-ar-receipts.service';
import type { ErpPortalAccount } from '@prisma/client';

/**
 * W5 — Pembayaran portal (Fase 3 G2).
 * Provider payment gateway BELUM dipilih (plan §2 #5), jadi v1 memakai
 * adapter 'SIMULASI': intent pembayaran menghasilkan nomor VA dan
 * penyelesaian terjadi lewat webhook ber-HMAC (kontrak yang sama seperti
 * provider nyata). Saat provider dipilih, adapter diganti di service ini
 * saja — tabel, endpoint portal, dan alur AR Receipt tidak berubah.
 * Uang sungguhan TIDAK bisa mengalir lewat provider SIMULASI.
 * PROVISIONAL (CLAUDE.md §6): webhook secret default dev di bawah hanya
 * untuk pengujian; produksi wajib PORTAL_PAYMENT_WEBHOOK_SECRET di env.
 */
export const PORTAL_PAYMENT_PROVIDER = 'SIMULASI';
const PROVISIONAL_WEBHOOK_SECRET = 'provisional-dev-secret-ganti-di-produksi';

@Injectable()
export class ErpPortalPaymentsService {
  private readonly logger = new Logger(ErpPortalPaymentsService.name);

  constructor(
    private readonly prisma: PrismaService,
    private readonly receipts: ErpFinArReceiptsService,
  ) {}

  private webhookSecret(): string {
    return process.env.PORTAL_PAYMENT_WEBHOOK_SECRET ?? PROVISIONAL_WEBHOOK_SECRET;
  }

  /** Signature kontrak provider simulasi: HMAC-SHA256 atas "ref.amount.status". */
  signPayload(ref: string, amount: string, status: string): string {
    return createHmac('sha256', this.webhookSecret()).update(`${ref}.${amount}.${status}`).digest('hex');
  }

  private verifySignature(ref: string, amount: string, status: string, signature?: string): boolean {
    if (!signature) return false;
    const expected = this.signPayload(ref, amount, status);
    const a = Buffer.from(expected);
    const b = Buffer.from(signature);
    return a.length === b.length && timingSafeEqual(a, b);
  }

  private async outstanding(invoiceId: bigint, grandTotal: Prisma.Decimal): Promise<Prisma.Decimal> {
    const sum = await this.prisma.erpFinSettlementAllocation.aggregate({
      where: { invoiceRef: invoiceId.toString() },
      _sum: { amount: true },
    });
    return new Prisma.Decimal(grandTotal).sub(new Prisma.Decimal(sum._sum.amount ?? 0));
  }

  private view(p: {
    id: bigint; invoiceId: bigint; provider: string; providerRef: string;
    vaNumber: string | null; amount: Prisma.Decimal; status: string;
    expiresAt: Date | null; paidAt: Date | null; receiptId: bigint | null;
  }, invoiceNumber?: string) {
    return {
      id: p.id.toString(),
      invoiceId: p.invoiceId.toString(),
      invoiceNumber: invoiceNumber ?? null,
      provider: p.provider,
      providerRef: p.providerRef,
      vaNumber: p.vaNumber,
      amount: p.amount.toString(),
      status: p.status,
      expiresAt: p.expiresAt,
      paidAt: p.paidAt,
      receiptId: p.receiptId?.toString() ?? null,
    };
  }

  /** Buat (atau pakai ulang) intent pembayaran untuk invoice milik akun. */
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

    const now = new Date();
    const existing = await this.prisma.erpPortalPayment.findFirst({
      where: { invoiceId: invoice.id, status: 'PENDING', deletedAt: null },
      orderBy: { id: 'desc' },
    });
    if (existing && existing.expiresAt && existing.expiresAt > now) {
      return this.view(existing, invoice.docNumber);
    }
    if (existing) {
      await this.prisma.erpPortalPayment.update({
        where: { id: existing.id }, data: { status: 'EXPIRED' },
      });
    }
    const expiresAt = new Date(now.getTime() + 24 * 3600 * 1000);
    const created = await this.prisma.erpPortalPayment.create({
      data: {
        invoiceId: invoice.id,
        partnerId: partnerForPayment,
        portalAccountId: account.id,
        provider: PORTAL_PAYMENT_PROVIDER,
        providerRef: `SIM-${invoice.id}-${randomBytes(4).toString('hex')}`,
        amount: due,
        status: 'PENDING',
        expiresAt,
        metadata: { origin: 'portal-pay', provisional: true },
      },
    });
    const vaNumber = `8808${String(created.id).padStart(12, '0')}`;
    const withVa = await this.prisma.erpPortalPayment.update({
      where: { id: created.id }, data: { vaNumber },
    });
    return this.view(withVa, invoice.docNumber);
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
    return rows.map((r) => this.view(r, numById.get(r.invoiceId.toString())));
  }

  /** Akun kas/bank penerimaan pembayaran portal (Bank BCA default BM). */
  private async settlementBankAccountId(): Promise<bigint> {
    const acc = await this.prisma.erpAccount.findFirst({
      where: { deletedAt: null, code: { startsWith: '1110.01' } },
      orderBy: { code: 'asc' }, select: { id: true },
    });
    if (!acc) throw new BadRequestException('Akun bank penerimaan belum tersedia.');
    return acc.id;
  }

  /**
   * Webhook provider: verifikasi HMAC → tandai PAID → buat AR Receipt
   * (IP) SUBMIT/APPROVE/POST sehingga invoice lunas TANPA input manual.
   * Idempoten: webhook ganda untuk pembayaran yang sudah PAID = no-op.
   */
  async handleWebhook(provider: string, signature: string | undefined, body: { ref?: string; status?: string; amount?: string }) {
    if (provider !== PORTAL_PAYMENT_PROVIDER) throw new NotFoundException('Provider tidak dikenal.');
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
    if (status !== 'PAID') {
      await this.prisma.erpPortalPayment.update({
        where: { id: payment.id },
        data: { metadata: { ...(payment.metadata as object ?? {}), lastWebhook: body } as Prisma.InputJsonValue },
      });
      return { ok: true, settled: false };
    }
    if (!new Prisma.Decimal(amount).equals(payment.amount)) {
      throw new BadRequestException('Nominal webhook tidak sama dengan nominal tagihan.');
    }
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
      await this.prisma.erpPortalPayment.update({
        where: { id: payment.id },
        data: { status: 'PAID', paidAt: new Date() },
      });
      return { ok: true, alreadySettled: true };
    }
    const amountStr = due.toString();
    const bankAccountId = await this.settlementBankAccountId();
    const created: any = await this.receipts.create({
      branchId: invoice.branchId.toString(),
      transactionDate: new Date().toISOString().slice(0, 10),
      partnerId: customerId.toString(),
      description: `Pembayaran portal ${ref} untuk ${invoice.docNumber}`,
      currencyId: invoice.currencyId.toString(),
      exchangeRate: invoice.exchangeRate.toString(),
      source: 'PORTAL',
      instruments: [{
        method: 'TRANSFER' as never, bankAccountId: bankAccountId.toString(),
        amount: amountStr, lineNo: 1,
      }],
      allocations: [{ invoiceId: invoice.id.toString(), amount: amountStr, lineNo: 1 }],
    });
    const receiptId = BigInt((created?.data ?? created).id);
    for (const action of ['SUBMIT', 'APPROVE', 'POST'] as const) {
      await this.receipts.transition(receiptId, { action } as never);
    }
    const done = await this.prisma.erpPortalPayment.update({
      where: { id: payment.id },
      data: {
        status: 'PAID', paidAt: new Date(), receiptId,
        metadata: { ...(payment.metadata as object ?? {}), webhook: body } as Prisma.InputJsonValue,
      },
    });
    this.logger.log(`Pembayaran ${ref} lunas → receipt ${receiptId}`);
    return { ok: true, settled: true, payment: this.view(done, invoice.docNumber) };
  }
}
