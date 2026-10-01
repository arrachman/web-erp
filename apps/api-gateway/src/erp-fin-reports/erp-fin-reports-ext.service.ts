import { Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { ReportColumn, ReportDocument, ReportRow, ReportSection } from './report-types';
import { ZERO, metaCabang, num, printedMeta } from './report-helpers';
import {
  accountMap,
  buildApAging,
  buildApCard,
  buildArAging,
  buildArCard,
  partnerMap,
} from './ar-ap-report.builder';

@Injectable()
export class ErpFinReportsExtService {
  constructor(private readonly prisma: PrismaService) {}

  // ---------------------------------------------------------------------------
  // 1. Cash Flow
  // ---------------------------------------------------------------------------
  async buildCashFlow(from: string, to: string, branchId?: string): Promise<ReportDocument> {
    const where: Prisma.ErpFinCashBankTransactionWhereInput = {
      deletedAt: null,
      transactionDate: { gte: new Date(from), lte: new Date(to) },
    };
    if (branchId) where.branchId = BigInt(branchId);

    const txns = await this.prisma.erpFinCashBankTransaction.findMany({
      where,
      select: { kind: true, direction: true, amount: true },
    });

    const columns: ReportColumn[] = [
      { key: 'Keterangan', label: 'Keterangan', type: 'text', width: 30 },
      { key: 'Kas', label: 'Kas (CASH)', type: 'number', align: 'right', width: 18 },
      { key: 'Bank', label: 'Bank', type: 'number', align: 'right', width: 18 },
      { key: 'Total', label: 'Total', type: 'number', align: 'right', width: 18 },
    ];

    const sumMap: Record<string, Record<string, Prisma.Decimal>> = {
      RECEIPT: { CASH: ZERO, BANK: ZERO },
      DISBURSEMENT: { CASH: ZERO, BANK: ZERO },
    };
    for (const t of txns) {
      const dir = t.direction as string;
      const kind = t.kind as string;
      if (sumMap[dir]?.[kind] !== undefined) {
        sumMap[dir][kind] = sumMap[dir][kind].add(t.amount);
      }
    }

    const makeRow = (dir: string, label: string): ReportRow => {
      const cash = sumMap[dir].CASH;
      const bank = sumMap[dir].BANK;
      return { cells: { Keterangan: label, Kas: num(cash), Bank: num(bank), Total: num(cash.add(bank)) } };
    };

    const netCash = sumMap.RECEIPT.CASH.sub(sumMap.DISBURSEMENT.CASH);
    const netBank = sumMap.RECEIPT.BANK.sub(sumMap.DISBURSEMENT.BANK);

    return {
      key: 'cash-flow',
      title: 'Laporan Arus Kas',
      subtitle: `Periode ${from} s/d ${to}`,
      meta: [{ label: 'Periode', value: `${from} s/d ${to}` }, ...metaCabang(branchId), printedMeta()],
      columns,
      sections: [
        { heading: 'Penerimaan', rows: [makeRow('RECEIPT', 'Total Penerimaan')] },
        { heading: 'Pengeluaran', rows: [makeRow('DISBURSEMENT', 'Total Pengeluaran')] },
      ],
      grandTotal: {
        cells: {
          Keterangan: 'Net Arus Kas',
          Kas: num(netCash),
          Bank: num(netBank),
          Total: num(netCash.add(netBank)),
        },
        bold: true,
      },
    };
  }

  // ---------------------------------------------------------------------------
  // 2. Daily Cash Bank
  // ---------------------------------------------------------------------------
  async buildDailyCashBank(from: string, to: string, branchId?: string): Promise<ReportDocument> {
    const where: Prisma.ErpFinCashBankTransactionWhereInput = {
      deletedAt: null,
      transactionDate: { gte: new Date(from), lte: new Date(to) },
    };
    if (branchId) where.branchId = BigInt(branchId);

    const txns = await this.prisma.erpFinCashBankTransaction.findMany({
      where,
      select: { kind: true, direction: true, amount: true, transactionDate: true },
      orderBy: { transactionDate: 'asc' },
    });

    const columns: ReportColumn[] = [
      { key: 'Tanggal', label: 'Tanggal', type: 'date', width: 14 },
      { key: 'Penerimaan', label: 'Penerimaan', type: 'number', align: 'right', width: 18 },
      { key: 'Pengeluaran', label: 'Pengeluaran', type: 'number', align: 'right', width: 18 },
      { key: 'Neto', label: 'Neto', type: 'number', align: 'right', width: 18 },
    ];

    type DayEntry = { receipt: Prisma.Decimal; disbursement: Prisma.Decimal };
    const byKind: Record<string, Record<string, DayEntry>> = { CASH: {}, BANK: {} };

    for (const t of txns) {
      const kind = t.kind as string;
      if (!byKind[kind]) continue;
      const dateKey = t.transactionDate.toISOString().slice(0, 10);
      if (!byKind[kind][dateKey]) byKind[kind][dateKey] = { receipt: ZERO, disbursement: ZERO };
      if ((t.direction as string) === 'RECEIPT') {
        byKind[kind][dateKey].receipt = byKind[kind][dateKey].receipt.add(t.amount);
      } else {
        byKind[kind][dateKey].disbursement = byKind[kind][dateKey].disbursement.add(t.amount);
      }
    }

    const sections: ReportSection[] = [];
    for (const kind of ['CASH', 'BANK']) {
      const daily = byKind[kind];
      let totRec = ZERO;
      let totDis = ZERO;
      const rows: ReportRow[] = Object.entries(daily).map(([date, v]) => {
        totRec = totRec.add(v.receipt);
        totDis = totDis.add(v.disbursement);
        return {
          cells: {
            Tanggal: date,
            Penerimaan: num(v.receipt),
            Pengeluaran: num(v.disbursement),
            Neto: num(v.receipt.sub(v.disbursement)),
          },
        };
      });
      sections.push({
        heading: kind === 'CASH' ? 'Kas' : 'Bank',
        rows,
        subtotal: {
          cells: {
            Tanggal: 'Subtotal',
            Penerimaan: num(totRec),
            Pengeluaran: num(totDis),
            Neto: num(totRec.sub(totDis)),
          },
          bold: true,
        },
      });
    }

    return {
      key: 'daily-cash-bank',
      title: 'Kas Bank Harian',
      subtitle: `Periode ${from} s/d ${to}`,
      meta: [{ label: 'Periode', value: `${from} s/d ${to}` }, ...metaCabang(branchId), printedMeta()],
      columns,
      sections,
    };
  }

  // ---------------------------------------------------------------------------
  // 3. AR Card — delegated to builder
  // ---------------------------------------------------------------------------
  async buildArCard(from: string, to: string, partnerId?: string, branchId?: string): Promise<ReportDocument> {
    return buildArCard(this.prisma, from, to, partnerId, branchId);
  }

  // ---------------------------------------------------------------------------
  // 4. AR Aging — delegated to builder
  // ---------------------------------------------------------------------------
  async buildArAging(asOf?: string): Promise<ReportDocument> {
    return buildArAging(this.prisma, asOf);
  }

  // ---------------------------------------------------------------------------
  // 5. AP Card — delegated to builder
  // ---------------------------------------------------------------------------
  async buildApCard(from: string, to: string, partnerId?: string, branchId?: string): Promise<ReportDocument> {
    return buildApCard(this.prisma, from, to, partnerId, branchId);
  }

  // ---------------------------------------------------------------------------
  // 6. AP Aging — delegated to builder
  // ---------------------------------------------------------------------------
  async buildApAging(asOf?: string): Promise<ReportDocument> {
    return buildApAging(this.prisma, asOf);
  }

  // ---------------------------------------------------------------------------
  // 7. Giro Maturity
  // ---------------------------------------------------------------------------
  async buildGiroMaturity(from?: string, to?: string): Promise<ReportDocument> {
    const refDate = from ? new Date(from) : new Date();
    const endDate = to ? new Date(to) : new Date(refDate.getTime() + 90 * 86400000);
    const refStr = refDate.toISOString().slice(0, 10);
    const endStr = endDate.toISOString().slice(0, 10);

    const giros = await this.prisma.erpFinGiro.findMany({
      where: { deletedAt: null, status: 'OUTSTANDING' },
      orderBy: { dueDate: 'asc' },
    });
    const partnerIds = giros.filter((g) => g.partnerId !== null).map((g) => g.partnerId as bigint);
    const pMap = await partnerMap(this.prisma, partnerIds);

    const columns: ReportColumn[] = [
      { key: 'No', label: 'No Giro', type: 'text', width: 18 },
      { key: 'Jenis', label: 'Jenis', type: 'text', width: 12 },
      { key: 'Partner', label: 'Partner', type: 'text', width: 30 },
      { key: 'Jumlah', label: 'Jumlah', type: 'number', align: 'right', width: 16 },
      { key: 'JatuhTempo', label: 'Jatuh Tempo', type: 'date', width: 14 },
      { key: 'Bucket', label: 'Bucket', type: 'text', width: 22 },
    ];

    const bNames = ['Jatuh Tempo (0-7 hari)', 'Jatuh Tempo (8-30 hari)', 'Lebih dari 30 hari'];
    const byBucket: Record<string, ReportRow[]> = Object.fromEntries(bNames.map((b) => [b, []]));

    for (const g of giros) {
      const days = Math.floor((g.dueDate.getTime() - refDate.getTime()) / 86400000);
      const bucket = days <= 7 ? bNames[0] : days <= 30 ? bNames[1] : bNames[2];
      byBucket[bucket].push({
        cells: {
          No: g.giroNumber,
          Jenis: g.type as string,
          Partner: g.partnerId ? (pMap.get(g.partnerId.toString()) ?? '') : '',
          Jumlah: num(g.amount),
          JatuhTempo: g.dueDate.toISOString().slice(0, 10),
          Bucket: bucket,
        },
      });
    }

    return {
      key: 'giro-maturity',
      title: 'Jatuh Tempo Giro',
      subtitle: `Dari ${refStr} s/d ${endStr}`,
      meta: [{ label: 'Periode', value: `${refStr} s/d ${endStr}` }, printedMeta()],
      columns,
      sections: bNames.map((b) => ({ heading: b, rows: byBucket[b] })),
    };
  }

  // ---------------------------------------------------------------------------
  // 8. Budget Realization
  // ---------------------------------------------------------------------------
  async buildBudgetRealization(from?: string, to?: string): Promise<ReportDocument> {
    const periodWhere: Prisma.ErpFiscalPeriodWhereInput = { deletedAt: null };
    if (from) periodWhere.startDate = { gte: new Date(from) };
    if (to) periodWhere.endDate = { lte: new Date(to) };

    const periods = await this.prisma.erpFiscalPeriod.findMany({ where: periodWhere, select: { id: true } });
    const periodIds = periods.map((p) => p.id);

    const brWhere: Prisma.ErpFinBudgetRealizationWhereInput = { deletedAt: null };
    if (periodIds.length > 0) brWhere.fiscalPeriodId = { in: periodIds };

    const items = await this.prisma.erpFinBudgetRealization.findMany({
      where: brWhere,
      orderBy: { accountId: 'asc' },
    });
    const aMap = await accountMap(this.prisma, items.map((i) => i.accountId));

    const columns: ReportColumn[] = [
      { key: 'KodeAkun', label: 'Kode Akun', type: 'text', width: 14 },
      { key: 'NamaAkun', label: 'Nama Akun', type: 'text', width: 34 },
      { key: 'Anggaran', label: 'Anggaran', type: 'number', align: 'right', width: 16 },
      { key: 'Realisasi', label: 'Realisasi', type: 'number', align: 'right', width: 16 },
      { key: 'Selisih', label: 'Selisih', type: 'number', align: 'right', width: 16 },
      { key: 'Pct', label: '%', type: 'number', align: 'right', width: 10 },
    ];

    const reportRows: ReportRow[] = items.map((r) => {
      const acc = aMap.get(r.accountId.toString());
      const isExpense = acc?.type === 'EXPENSE';
      const realization = isExpense ? r.debitTotal.sub(r.creditTotal) : r.creditTotal.sub(r.debitTotal);
      const variance = r.budgetAmount.sub(realization);
      const pct = r.budgetAmount.isZero()
        ? 0
        : Math.round(num(realization.div(r.budgetAmount).mul(new Prisma.Decimal(100))) * 100) / 100;
      return {
        cells: {
          KodeAkun: acc?.code ?? '',
          NamaAkun: acc?.name ?? '',
          Anggaran: num(r.budgetAmount),
          Realisasi: num(realization),
          Selisih: num(variance),
          Pct: pct,
        },
      };
    });

    const subtitle = from && to ? `Periode ${from} s/d ${to}` : 'Semua Periode';
    return {
      key: 'budget-realization',
      title: 'Realisasi Anggaran',
      subtitle,
      meta: [{ label: 'Periode', value: subtitle }, printedMeta()],
      columns,
      sections: [{ rows: reportRows }],
    };
  }
}
