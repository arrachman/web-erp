import { Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { ReportColumn, ReportDocument, ReportRow, ReportSection } from './report-types';
import { buildGeneralLedger } from './general-ledger.builder';
import { buildEquityChanges, buildMovementBalance } from './statement-builders';
import {
  AccountType,
  PostableAccount,
  ZERO,
  baseLedgerWhere,
  loadPostableAccounts,
  metaCabang,
  movementsByAccount,
  num,
  printedMeta,
} from './report-helpers';

@Injectable()
export class ErpFinReportsService {
  constructor(private readonly prisma: PrismaService) {}

  // ---------------------------------------------------------------------------
  // 1. Trial Balance
  // ---------------------------------------------------------------------------
  async buildTrialBalance(from: string, to: string, branchId?: string): Promise<ReportDocument> {
    const accounts = await loadPostableAccounts(this.prisma);
    const where = baseLedgerWhere(branchId);
    where.entryDate = { gte: new Date(from), lte: new Date(to) };
    const moves = await movementsByAccount(this.prisma, where);

    const columns: ReportColumn[] = [
      { key: 'Kode', label: 'Kode', type: 'text', width: 14 },
      { key: 'Akun', label: 'Akun', type: 'text', width: 40 },
      { key: 'Debit', label: 'Debit', type: 'number', align: 'right', width: 18 },
      { key: 'Kredit', label: 'Kredit', type: 'number', align: 'right', width: 18 },
    ];

    const rows: ReportRow[] = [];
    let totalDebit = ZERO;
    let totalCredit = ZERO;
    for (const acc of accounts) {
      const m = moves.get(acc.id.toString());
      if (!m) continue;
      totalDebit = totalDebit.add(m.debit);
      totalCredit = totalCredit.add(m.credit);
      rows.push({
        cells: { Kode: acc.code, Akun: acc.name, Debit: num(m.debit), Kredit: num(m.credit) },
      });
    }

    return {
      key: 'trial-balance',
      title: 'Neraca Saldo',
      subtitle: `Periode ${from} s/d ${to}`,
      meta: [
        { label: 'Periode', value: `${from} s/d ${to}` },
        ...metaCabang(branchId),
        printedMeta(),
      ],
      columns,
      sections: [{ rows }],
      grandTotal: {
        cells: { Kode: '', Akun: 'TOTAL', Debit: num(totalDebit), Kredit: num(totalCredit) },
        bold: true,
      },
    };
  }

  // ---------------------------------------------------------------------------
  // 2. Income Statement
  // ---------------------------------------------------------------------------
  async buildIncomeStatement(
    from: string,
    to: string,
    branchId?: string,
  ): Promise<ReportDocument> {
    const accounts = await loadPostableAccounts(this.prisma);
    const where = baseLedgerWhere(branchId);
    where.entryDate = { gte: new Date(from), lte: new Date(to) };
    const moves = await movementsByAccount(this.prisma, where);

    const columns: ReportColumn[] = [
      { key: 'Kode', label: 'Kode', type: 'text', width: 14 },
      { key: 'Akun', label: 'Akun', type: 'text', width: 40 },
      { key: 'Jumlah', label: 'Jumlah', type: 'number', align: 'right', width: 20 },
    ];

    const buildSection = (
      heading: string,
      type: AccountType,
      creditNormal: boolean,
      subtotalLabel: string,
    ): { section: ReportSection; total: Prisma.Decimal } => {
      const rows: ReportRow[] = [];
      let total = ZERO;
      for (const acc of accounts) {
        if (acc.type !== type) continue;
        const m = moves.get(acc.id.toString());
        if (!m) continue;
        const amount = creditNormal ? m.credit.sub(m.debit) : m.debit.sub(m.credit);
        total = total.add(amount);
        rows.push({ cells: { Kode: acc.code, Akun: acc.name, Jumlah: num(amount) } });
      }
      return {
        section: {
          heading,
          rows,
          subtotal: { cells: { Kode: '', Akun: subtotalLabel, Jumlah: num(total) }, bold: true },
        },
        total,
      };
    };

    const rev = buildSection('PENDAPATAN', 'REVENUE', true, 'Total Pendapatan');
    const exp = buildSection('BEBAN', 'EXPENSE', false, 'Total Beban');
    const net = rev.total.sub(exp.total);

    return {
      key: 'income-statement',
      title: 'Laba Rugi',
      subtitle: `Periode ${from} s/d ${to}`,
      meta: [
        { label: 'Periode', value: `${from} s/d ${to}` },
        ...metaCabang(branchId),
        printedMeta(),
      ],
      columns,
      sections: [rev.section, exp.section],
      grandTotal: {
        cells: { Kode: '', Akun: 'LABA / (RUGI) BERSIH', Jumlah: num(net) },
        bold: true,
      },
    };
  }

  // ---------------------------------------------------------------------------
  // 3. Balance Sheet
  // ---------------------------------------------------------------------------
  async buildBalanceSheet(asOf: string, branchId?: string): Promise<ReportDocument> {
    const accounts = await loadPostableAccounts(this.prisma);
    const where = baseLedgerWhere(branchId);
    where.entryDate = { lte: new Date(asOf) };
    const moves = await movementsByAccount(this.prisma, where);

    const columns: ReportColumn[] = [
      { key: 'Kode', label: 'Kode', type: 'text', width: 14 },
      { key: 'Akun', label: 'Akun', type: 'text', width: 40 },
      { key: 'Saldo', label: 'Saldo', type: 'number', align: 'right', width: 20 },
    ];

    const balanceOf = (acc: PostableAccount): Prisma.Decimal => {
      const m = moves.get(acc.id.toString()) ?? { debit: ZERO, credit: ZERO };
      const movement =
        acc.normalBalance === 'DEBIT' ? m.debit.sub(m.credit) : m.credit.sub(m.debit);
      return acc.openingBalance.add(movement);
    };

    const buildSection = (
      heading: string,
      type: AccountType,
      subtotalLabel: string,
      extraRow?: ReportRow,
      extraAmount?: Prisma.Decimal,
    ): { section: ReportSection; total: Prisma.Decimal } => {
      const rows: ReportRow[] = [];
      let total = ZERO;
      for (const acc of accounts) {
        if (acc.type !== type) continue;
        const hasMove = moves.has(acc.id.toString());
        if (!hasMove && acc.openingBalance.equals(ZERO)) continue;
        const bal = balanceOf(acc);
        total = total.add(bal);
        rows.push({ cells: { Kode: acc.code, Akun: acc.name, Saldo: num(bal) } });
      }
      if (extraRow) {
        rows.push(extraRow);
        total = total.add(extraAmount ?? ZERO);
      }
      return {
        section: {
          heading,
          rows,
          subtotal: { cells: { Kode: '', Akun: subtotalLabel, Saldo: num(total) }, bold: true },
        },
        total,
      };
    };

    // Net income as-of: revenue (credit-debit) - expense (debit-credit)
    let netIncome = ZERO;
    for (const acc of accounts) {
      const m = moves.get(acc.id.toString());
      if (!m) continue;
      if (acc.type === 'REVENUE') netIncome = netIncome.add(m.credit.sub(m.debit));
      else if (acc.type === 'EXPENSE') netIncome = netIncome.sub(m.debit.sub(m.credit));
    }

    const aset = buildSection('ASET', 'ASSET', 'Total Aset');
    const kewajiban = buildSection('KEWAJIBAN', 'LIABILITY', 'Total Kewajiban');
    const ekuitas = buildSection(
      'EKUITAS',
      'EQUITY',
      'Total Ekuitas',
      { cells: { Kode: '', Akun: 'Laba/(Rugi) Berjalan', Saldo: num(netIncome) } },
      netIncome,
    );

    return {
      key: 'balance-sheet',
      title: 'Neraca',
      subtitle: `Per ${asOf}`,
      meta: [{ label: 'Per', value: asOf }, ...metaCabang(branchId), printedMeta()],
      columns,
      sections: [aset.section, kewajiban.section, ekuitas.section],
      grandTotal: {
        cells: {
          Kode: '',
          Akun: 'TOTAL KEWAJIBAN + EKUITAS',
          Saldo: num(kewajiban.total.add(ekuitas.total)),
        },
        bold: true,
      },
    };
  }

  // ---------------------------------------------------------------------------
  // 4. General Ledger (delegated to keep this file small)
  // ---------------------------------------------------------------------------
  async buildGeneralLedger(
    from: string,
    to: string,
    accountId?: string,
    branchId?: string,
  ): Promise<ReportDocument> {
    return buildGeneralLedger(this.prisma, from, to, accountId, branchId);
  }

  // ---------------------------------------------------------------------------
  // 5. Neraca Mutasi (Trial Balance with movement)
  // ---------------------------------------------------------------------------
  async buildMovementBalance(from: string, to: string, branchId?: string): Promise<ReportDocument> {
    return buildMovementBalance(this.prisma, from, to, branchId);
  }

  // ---------------------------------------------------------------------------
  // 6. Perubahan Modal (Statement of Changes in Equity)
  // ---------------------------------------------------------------------------
  async buildEquityChanges(from: string, to: string, branchId?: string): Promise<ReportDocument> {
    return buildEquityChanges(this.prisma, from, to, branchId);
  }
}
