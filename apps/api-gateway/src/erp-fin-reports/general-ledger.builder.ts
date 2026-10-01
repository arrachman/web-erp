import { PrismaService } from '../prisma/prisma.service';
import { ReportColumn, ReportDocument, ReportRow, ReportSection } from './report-types';
import {
  baseLedgerWhere,
  loadPostableAccounts,
  metaCabang,
  movementsByAccount,
  num,
  printedMeta,
  ZERO,
} from './report-helpers';

const GL_COLUMNS: ReportColumn[] = [
  { key: 'Tanggal', label: 'Tanggal', type: 'date', width: 14 },
  { key: 'Dokumen', label: 'No. Dokumen', type: 'text', width: 18 },
  { key: 'Keterangan', label: 'Keterangan', type: 'text', width: 34 },
  { key: 'Debit', label: 'Debit', type: 'number', align: 'right', width: 16 },
  { key: 'Kredit', label: 'Kredit', type: 'number', align: 'right', width: 16 },
  { key: 'Saldo', label: 'Saldo', type: 'number', align: 'right', width: 18 },
];

export async function buildGeneralLedger(
  prisma: PrismaService,
  from: string,
  to: string,
  accountId?: string,
  branchId?: string,
): Promise<ReportDocument> {
  const all = await loadPostableAccounts(prisma);
  const accounts = accountId ? all.filter((a) => a.id.toString() === accountId) : all;

  const fromDate = new Date(from);
  const toDate = new Date(to);
  const where = baseLedgerWhere(branchId);

  // Opening balances: openingBalance + signed movement for entryDate < from
  const openMoves = await movementsByAccount(prisma, {
    ...where,
    entryDate: { lt: fromDate },
  });

  // Determine which accounts have entries in range (non-filtered mode), cap at 200.
  let candidates = accounts;
  let capped = false;
  if (!accountId) {
    const inRange = await prisma.erpFinLedgerEntry.groupBy({
      by: ['accountId'],
      where: { ...where, entryDate: { gte: fromDate, lte: toDate } },
    });
    const withEntries = new Set(inRange.map((g) => g.accountId.toString()));
    candidates = accounts.filter((a) => withEntries.has(a.id.toString()));
    if (candidates.length > 200) {
      capped = true;
      candidates = candidates.slice(0, 200);
    }
  }

  const sections: ReportSection[] = [];
  for (const acc of candidates) {
    const open = openMoves.get(acc.id.toString()) ?? { debit: ZERO, credit: ZERO };
    const openingMovement =
      acc.normalBalance === 'DEBIT' ? open.debit.sub(open.credit) : open.credit.sub(open.debit);
    let running = acc.openingBalance.add(openingMovement);

    const entries = await prisma.erpFinLedgerEntry.findMany({
      where: { ...where, accountId: acc.id, entryDate: { gte: fromDate, lte: toDate } },
      orderBy: [{ entryDate: 'asc' }, { id: 'asc' }],
      select: { entryDate: true, docNumber: true, description: true, debit: true, credit: true },
    });

    const rows: ReportRow[] = [
      {
        cells: {
          Tanggal: null,
          Dokumen: '',
          Keterangan: 'Saldo Awal',
          Debit: null,
          Kredit: null,
          Saldo: num(running),
        },
      },
    ];

    let sumDebit = ZERO;
    let sumCredit = ZERO;
    for (const e of entries) {
      sumDebit = sumDebit.add(e.debit);
      sumCredit = sumCredit.add(e.credit);
      const delta =
        acc.normalBalance === 'DEBIT' ? e.debit.sub(e.credit) : e.credit.sub(e.debit);
      running = running.add(delta);
      rows.push({
        cells: {
          Tanggal: e.entryDate.toISOString().slice(0, 10),
          Dokumen: e.docNumber,
          Keterangan: e.description ?? '',
          Debit: num(e.debit),
          Kredit: num(e.credit),
          Saldo: num(running),
        },
      });
    }

    sections.push({
      heading: `${acc.code} — ${acc.name}`,
      rows,
      subtotal: {
        cells: {
          Tanggal: null,
          Dokumen: '',
          Keterangan: 'Saldo Akhir',
          Debit: num(sumDebit),
          Kredit: num(sumCredit),
          Saldo: num(running),
        },
        bold: true,
      },
    });
  }

  const meta = [
    { label: 'Periode', value: `${from} s/d ${to}` },
    ...metaCabang(branchId),
    printedMeta(),
  ];
  if (capped) meta.push({ label: 'Catatan', value: 'Dibatasi 200 akun' });

  return {
    key: 'general-ledger',
    title: 'Buku Besar',
    subtitle: `Periode ${from} s/d ${to}`,
    meta,
    columns: GL_COLUMNS,
    sections,
  };
}
