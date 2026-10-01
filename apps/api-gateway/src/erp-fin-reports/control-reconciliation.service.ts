import { Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';

const ZERO = new Prisma.Decimal(0);
const TOLERANCE = new Prisma.Decimal('0.01');

export interface ControlReconciliation {
  area: 'AR' | 'AP';
  glBalance: string;
  subledgerBalance: string;
  difference: string;
  isBalanced: boolean;
  controlAccounts: { accountId: string; code: string; name: string; glBalance: string }[];
  components: { label: string; amount: string }[];
}

export interface ControlReconciliationReport {
  asOf: string;
  ar: ControlReconciliation;
  ap: ControlReconciliation;
}

const sumOf = (rows: { grandTotal: Prisma.Decimal }[]) => rows.reduce((s, r) => s.add(r.grandTotal), ZERO);

/**
 * Laporan kontrol PRD ("Rekonsiliasi subledger"): saldo subledger dari dokumen
 * sumber vs saldo GL akun kontrol AR/AP per tanggal. Akun kontrol = akun piutang/
 * utang yang dipakai dokumen & master partner. Selisih ≠ 0 → ada posting ganda,
 * jurnal manual ke akun kontrol, atau dokumen yang tidak terposting.
 */
@Injectable()
export class ControlReconciliationService {
  constructor(private readonly prisma: PrismaService) {}

  async build(asOfStr?: string, branchId?: string): Promise<ControlReconciliationReport> {
    const asOf = asOfStr ? new Date(asOfStr) : new Date();
    const branch = branchId ? { branchId: BigInt(branchId) } : {};
    return {
      asOf: asOf.toISOString().slice(0, 10),
      ar: await this.receivable(asOf, branch),
      ap: await this.payable(asOf, branch),
    };
  }

  private async receivable(asOf: Date, branch: { branchId?: bigint }): Promise<ControlReconciliation> {
    const posted = { deletedAt: null, postingStatus: 'POSTED' as const, docDate: { lte: asOf }, ...branch };
    const invoices = await this.prisma.erpSlsInvoice.findMany({
      where: posted,
      select: { grandTotal: true, advanceId: true, advanceAmount: true, receivableAccountId: true },
    });
    const returns = await this.prisma.erpSlsReturn.findMany({
      where: posted,
      select: { grandTotal: true, receivableAccountId: true },
    });
    const allocated = await this.prisma.erpFinSettlementAllocation.aggregate({
      where: { arReceipt: { deletedAt: null, postingStatus: 'POSTED', transactionDate: { lte: asOf }, ...branch } },
      _sum: { amount: true },
    });
    const advanceApplied = invoices.reduce((s, i) => s.add(i.advanceId ? (i.advanceAmount ?? ZERO) : ZERO), ZERO);
    const partnerAccounts = await this.prisma.erpPartner.findMany({
      where: { receivableAccountId: { not: null }, deletedAt: null },
      select: { receivableAccountId: true },
    });
    const accountIds = [
      ...invoices.map((i) => i.receivableAccountId),
      ...returns.map((r) => r.receivableAccountId),
      ...partnerAccounts.map((p) => p.receivableAccountId),
    ];
    const components = [
      { label: 'Σ Sales Invoice POSTED', amount: sumOf(invoices) },
      { label: '− Potongan uang muka (AS) pada SI', amount: advanceApplied.neg() },
      { label: '− Σ Sales Return POSTED', amount: sumOf(returns).neg() },
      { label: '− Σ Alokasi AR Receipt POSTED', amount: new Prisma.Decimal(allocated._sum.amount ?? 0).neg() },
    ];
    return this.assemble('AR', accountIds, components, asOf, branch, 'debit-credit');
  }

  private async payable(asOf: Date, branch: { branchId?: bigint }): Promise<ControlReconciliation> {
    const posted = { deletedAt: null, postingStatus: 'POSTED' as const, docDate: { lte: asOf }, ...branch };
    const invoices = await this.prisma.erpPurInvoice.findMany({
      where: posted,
      select: { grandTotal: true, payableAccountId: true },
    });
    const returns = await this.prisma.erpPurReturn.findMany({
      where: posted,
      select: { grandTotal: true, payableAccountId: true },
    });
    const allocated = await this.prisma.erpFinSettlementAllocation.aggregate({
      where: { apPayment: { deletedAt: null, postingStatus: 'POSTED', transactionDate: { lte: asOf }, ...branch } },
      _sum: { amount: true },
    });
    const partnerAccounts = await this.prisma.erpPartner.findMany({
      where: { payableAccountId: { not: null }, deletedAt: null },
      select: { payableAccountId: true },
    });
    const accountIds = [
      ...invoices.map((i) => i.payableAccountId),
      ...returns.map((r) => r.payableAccountId),
      ...partnerAccounts.map((p) => p.payableAccountId),
    ];
    const components = [
      { label: 'Σ Purchase Invoice POSTED', amount: sumOf(invoices) },
      { label: '− Σ Purchase Return POSTED', amount: sumOf(returns).neg() },
      { label: '− Σ Alokasi AP Payment POSTED', amount: new Prisma.Decimal(allocated._sum.amount ?? 0).neg() },
    ];
    return this.assemble('AP', accountIds, components, asOf, branch, 'credit-debit');
  }

  private async assemble(
    area: 'AR' | 'AP',
    rawAccountIds: (bigint | null)[],
    components: { label: string; amount: Prisma.Decimal }[],
    asOf: Date,
    branch: { branchId?: bigint },
    direction: 'debit-credit' | 'credit-debit',
  ): Promise<ControlReconciliation> {
    const ids = [...new Set(rawAccountIds.filter((v): v is bigint => v != null).map(String))].map(BigInt);
    const accounts = ids.length
      ? await this.prisma.erpAccount.findMany({ where: { id: { in: ids } }, select: { id: true, code: true, name: true } })
      : [];
    const groups = ids.length
      ? await this.prisma.erpFinLedgerEntry.groupBy({
          by: ['accountId'],
          where: { accountId: { in: ids }, entryDate: { lte: asOf }, deletedAt: null, ...branch },
          _sum: { debit: true, credit: true },
        })
      : [];
    const balanceOf = (accountId: bigint) => {
      const g = groups.find((x) => x.accountId === accountId);
      const d = new Prisma.Decimal(g?._sum.debit ?? 0);
      const c = new Prisma.Decimal(g?._sum.credit ?? 0);
      return direction === 'debit-credit' ? d.sub(c) : c.sub(d);
    };
    const controlAccounts = accounts.map((a) => ({
      accountId: a.id.toString(), code: a.code, name: a.name, glBalance: balanceOf(a.id).toFixed(2),
    }));
    const gl = accounts.reduce((s, a) => s.add(balanceOf(a.id)), ZERO);
    const sub = components.reduce((s, c) => s.add(c.amount), ZERO);
    const diff = gl.sub(sub);
    return {
      area,
      glBalance: gl.toFixed(2),
      subledgerBalance: sub.toFixed(2),
      difference: diff.toFixed(2),
      isBalanced: diff.abs().lte(TOLERANCE),
      controlAccounts,
      components: components.map((c) => ({ label: c.label, amount: c.amount.toFixed(2) })),
    };
  }
}
