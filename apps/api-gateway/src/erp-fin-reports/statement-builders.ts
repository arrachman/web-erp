import { PrismaService } from '../prisma/prisma.service';
import { ReportColumn, ReportDocument, ReportRow } from './report-types';
import {
  baseLedgerWhere,
  loadPostableAccounts,
  metaCabang,
  movementsByAccount,
  num,
  printedMeta,
  ZERO,
} from './report-helpers';

// ---------------------------------------------------------------------------
// Neraca Mutasi (Trial Balance with movement)
// Worksheet view per akun: Saldo Awal · Debit · Kredit · Saldo Akhir.
// Saldo awal/akhir = konvensi debit-positif (positif = saldo debit,
// negatif = saldo kredit) → total tiap kolom ≈ 0 bila buku seimbang.
// ---------------------------------------------------------------------------

const MOVEMENT_COLUMNS: ReportColumn[] = [
  { key: 'Kode', label: 'Kode', type: 'text', width: 12 },
  { key: 'Akun', label: 'Akun', type: 'text', width: 34 },
  { key: 'SaldoAwal', label: 'Saldo Awal', type: 'number', align: 'right', width: 18 },
  { key: 'Debit', label: 'Debit', type: 'number', align: 'right', width: 16 },
  { key: 'Kredit', label: 'Kredit', type: 'number', align: 'right', width: 16 },
  { key: 'SaldoAkhir', label: 'Saldo Akhir', type: 'number', align: 'right', width: 18 },
];

export async function buildMovementBalance(
  prisma: PrismaService,
  from: string,
  to: string,
  branchId?: string,
): Promise<ReportDocument> {
  const accounts = await loadPostableAccounts(prisma);

  const beforeWhere = baseLedgerWhere(branchId);
  beforeWhere.entryDate = { lt: new Date(from) };
  const before = await movementsByAccount(prisma, beforeWhere);

  const periodWhere = baseLedgerWhere(branchId);
  periodWhere.entryDate = { gte: new Date(from), lte: new Date(to) };
  const period = await movementsByAccount(prisma, periodWhere);

  const rows: ReportRow[] = [];
  let totOpen = ZERO;
  let totDebit = ZERO;
  let totCredit = ZERO;
  let totClose = ZERO;

  for (const acc of accounts) {
    const b = before.get(acc.id.toString()) ?? { debit: ZERO, credit: ZERO };
    const p = period.get(acc.id.toString()) ?? { debit: ZERO, credit: ZERO };

    // openingBalance disimpan normal-side positif → konversi ke debit-positif.
    const openingSeed =
      acc.normalBalance === 'DEBIT' ? acc.openingBalance : acc.openingBalance.neg();
    const opening = openingSeed.add(b.debit).sub(b.credit);
    const closing = opening.add(p.debit).sub(p.credit);

    const hasActivity =
      !opening.equals(ZERO) ||
      !p.debit.equals(ZERO) ||
      !p.credit.equals(ZERO) ||
      !closing.equals(ZERO);
    if (!hasActivity) continue;

    totOpen = totOpen.add(opening);
    totDebit = totDebit.add(p.debit);
    totCredit = totCredit.add(p.credit);
    totClose = totClose.add(closing);

    rows.push({
      cells: {
        Kode: acc.code,
        Akun: acc.name,
        SaldoAwal: num(opening),
        Debit: num(p.debit),
        Kredit: num(p.credit),
        SaldoAkhir: num(closing),
      },
    });
  }

  return {
    key: 'movement-balance',
    title: 'Neraca Mutasi',
    subtitle: `Periode ${from} s/d ${to}`,
    meta: [
      { label: 'Periode', value: `${from} s/d ${to}` },
      ...metaCabang(branchId),
      printedMeta(),
    ],
    columns: MOVEMENT_COLUMNS,
    sections: [{ rows }],
    grandTotal: {
      cells: {
        Kode: '',
        Akun: 'TOTAL',
        SaldoAwal: num(totOpen),
        Debit: num(totDebit),
        Kredit: num(totCredit),
        SaldoAkhir: num(totClose),
      },
      bold: true,
    },
  };
}

// ---------------------------------------------------------------------------
// Perubahan Modal (Statement of Changes in Equity)
// Per akun ekuitas: Saldo Awal · Mutasi · Saldo Akhir, ditambah baris
// Laba/(Rugi) Berjalan periode (kontribusi ke ekuitas akhir).
// ---------------------------------------------------------------------------

const EQUITY_COLUMNS: ReportColumn[] = [
  { key: 'Kode', label: 'Kode', type: 'text', width: 12 },
  { key: 'Akun', label: 'Akun', type: 'text', width: 40 },
  { key: 'SaldoAwal', label: 'Saldo Awal', type: 'number', align: 'right', width: 18 },
  { key: 'Mutasi', label: 'Mutasi', type: 'number', align: 'right', width: 18 },
  { key: 'SaldoAkhir', label: 'Saldo Akhir', type: 'number', align: 'right', width: 18 },
];

export async function buildEquityChanges(
  prisma: PrismaService,
  from: string,
  to: string,
  branchId?: string,
): Promise<ReportDocument> {
  const accounts = await loadPostableAccounts(prisma);

  const beforeWhere = baseLedgerWhere(branchId);
  beforeWhere.entryDate = { lt: new Date(from) };
  const before = await movementsByAccount(prisma, beforeWhere);

  const periodWhere = baseLedgerWhere(branchId);
  periodWhere.entryDate = { gte: new Date(from), lte: new Date(to) };
  const period = await movementsByAccount(prisma, periodWhere);

  const rows: ReportRow[] = [];
  let totOpen = ZERO;
  let totMove = ZERO;
  let totClose = ZERO;

  for (const acc of accounts) {
    if (acc.type !== 'EQUITY') continue;
    const b = before.get(acc.id.toString()) ?? { debit: ZERO, credit: ZERO };
    const p = period.get(acc.id.toString()) ?? { debit: ZERO, credit: ZERO };

    // Ekuitas normal kredit → saldo = kredit - debit (positif = normal).
    const opening = acc.openingBalance.add(b.credit).sub(b.debit);
    const movement = p.credit.sub(p.debit);
    const closing = opening.add(movement);

    if (opening.equals(ZERO) && movement.equals(ZERO) && closing.equals(ZERO)) continue;

    totOpen = totOpen.add(opening);
    totMove = totMove.add(movement);
    totClose = totClose.add(closing);

    rows.push({
      cells: {
        Kode: acc.code,
        Akun: acc.name,
        SaldoAwal: num(opening),
        Mutasi: num(movement),
        SaldoAkhir: num(closing),
      },
    });
  }

  // Laba/(rugi) berjalan periode = pendapatan (kredit-debit) - beban (debit-kredit).
  let netIncome = ZERO;
  for (const acc of accounts) {
    const p = period.get(acc.id.toString());
    if (!p) continue;
    if (acc.type === 'REVENUE') netIncome = netIncome.add(p.credit.sub(p.debit));
    else if (acc.type === 'EXPENSE') netIncome = netIncome.sub(p.debit.sub(p.credit));
  }
  if (!netIncome.equals(ZERO)) {
    totMove = totMove.add(netIncome);
    totClose = totClose.add(netIncome);
    rows.push({
      cells: {
        Kode: '',
        Akun: 'Laba/(Rugi) Tahun Berjalan',
        SaldoAwal: num(ZERO),
        Mutasi: num(netIncome),
        SaldoAkhir: num(netIncome),
      },
    });
  }

  return {
    key: 'equity-changes',
    title: 'Laporan Perubahan Modal',
    subtitle: `Periode ${from} s/d ${to}`,
    meta: [
      { label: 'Periode', value: `${from} s/d ${to}` },
      ...metaCabang(branchId),
      printedMeta(),
    ],
    columns: EQUITY_COLUMNS,
    sections: [{ rows }],
    grandTotal: {
      cells: {
        Kode: '',
        Akun: 'TOTAL EKUITAS',
        SaldoAwal: num(totOpen),
        Mutasi: num(totMove),
        SaldoAkhir: num(totClose),
      },
      bold: true,
    },
  };
}
