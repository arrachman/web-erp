/**
 * One-off: generate scheduled dummy cash/bank transactions (POSTED + GL ledger)
 * on the 1st & 15th of every month from 2025-01-01 up to today, 20 txns per day
 * (5 each of the 4 directions, so ledger stays balanced).
 *
 * Run: npx ts-node prisma/seed-erp-cashbank-scheduled.ts
 * Appends rows; numbering sequence advanced so re-runs don't collide. Mirrors
 * CashBankPostingService posting logic. Marker source='DUMMY_SEED'.
 */
import { PrismaClient, Prisma } from '@prisma/client';

const prisma = new PrismaClient();

const CURRENCY_IDR = 1n;
const BRANCHES = [1n, 1015n, 1016n, 1017n, 1018n];
const USERS = [1n, 2n, 3n];
const CASH_ACCOUNTS = [183n, 184n];
const BANK_ACCOUNTS = [185n, 186n, 187n, 188n, 189n];
const REVENUE_CONTRA = [259n, 260n, 261n, 264n, 268n, 269n];
const EXPENSE_CONTRA = [
  276n, 277n, 281n, 283n, 288n, 289n, 293n, 294n,
  295n, 296n, 297n, 302n, 303n, 309n,
];
const RECEIPT_DESC = [
  'Penerimaan penjualan tunai', 'Penerimaan jasa pengolahan', 'Penjualan bahan sisa',
  'Pendapatan bunga bank', 'Penerimaan sewa', 'Pendapatan lain-lain',
  'Setoran pelanggan', 'Penerimaan komisi',
];
const DISBURSE_DESC = [
  'Pembayaran biaya operasional', 'Pembayaran listrik & air', 'Pembayaran gaji',
  'Biaya promosi', 'Biaya transportasi', 'Pembayaran sewa kantor',
  'Beban administrasi bank', 'Pembelian ATK', 'Biaya pemeliharaan',
];

const SOURCE = 'CASH_BANK';
const SOURCE_DOC_TYPE = 'fin_cash_bank_transactions';

const START_YEAR = 2025;
const START_MONTH = 0; // Jan (0-based)
const TODAY = new Date('2026-06-07T00:00:00Z');
const DAYS_OF_MONTH = [1, 15];
const PER_KIND_PER_DAY = 5; // 5 × 4 directions = 20 txns/day

const rand = <T>(arr: T[]): T => arr[Math.floor(Math.random() * arr.length)];
const randInt = (min: number, max: number) =>
  Math.floor(Math.random() * (max - min + 1)) + min;
const randAmount = () => new Prisma.Decimal(randInt(500, 50_000) * 1000);

type Dir = 'RECEIPT' | 'DISBURSEMENT';
type Kind = 'CASH' | 'BANK';
const PLANS: { kind: Kind; dir: Dir; docCode: string }[] = [
  { kind: 'CASH', dir: 'RECEIPT', docCode: 'CASH_RECEIPT' },
  { kind: 'CASH', dir: 'DISBURSEMENT', docCode: 'CASH_DISBURSEMENT' },
  { kind: 'BANK', dir: 'RECEIPT', docCode: 'BANK_RECEIPT' },
  { kind: 'BANK', dir: 'DISBURSEMENT', docCode: 'BANK_DISBURSEMENT' },
];

function buildDates(): Date[] {
  const dates: Date[] = [];
  let y = START_YEAR;
  let m = START_MONTH;
  while (true) {
    let stop = false;
    for (const day of DAYS_OF_MONTH) {
      const d = new Date(Date.UTC(y, m, day));
      if (d > TODAY) { stop = true; break; }
      dates.push(d);
    }
    if (stop) break;
    m += 1;
    if (m > 11) { m = 0; y += 1; }
    if (y > TODAY.getUTCFullYear() + 1) break;
  }
  return dates;
}

async function main() {
  const dates = buildDates();
  console.log(`→ ${dates.length} tanggal × 20 txn = ${dates.length * 20} transaksi`);

  const periods = await prisma.erpFiscalPeriod.findMany({
    where: { deletedAt: null },
    select: { id: true, startDate: true, endDate: true },
  });
  const periodFor = (d: Date): bigint => {
    const p = periods.find((x) => x.startDate <= d && x.endDate >= d);
    if (!p) throw new Error(`No fiscal period covers ${d.toISOString()}`);
    return p.id;
  };

  const numberings = new Map<string, { id: bigint; prefix: string; next: number; digits: number }>();
  for (const plan of PLANS) {
    const n = await prisma.erpDocumentNumbering.findFirst({
      where: { documentCode: plan.docCode, deletedAt: null },
    });
    if (!n) throw new Error(`No numbering for ${plan.docCode}`);
    numberings.set(plan.docCode, { id: n.id, prefix: n.prefix, next: n.nextNumber, digits: n.digitCount });
  }

  const ledgerRows: Prisma.ErpFinLedgerEntryCreateManyInput[] = [];
  let made = 0;
  const now = new Date();

  for (const date of dates) {
    const fiscalPeriodId = periodFor(date);
    for (const plan of PLANS) {
      const seq = numberings.get(plan.docCode)!;
      const isReceipt = plan.dir === 'RECEIPT';
      const headerAccts = plan.kind === 'CASH' ? CASH_ACCOUNTS : BANK_ACCOUNTS;
      const contraAccts = isReceipt ? REVENUE_CONTRA : EXPENSE_CONTRA;
      const descPool = isReceipt ? RECEIPT_DESC : DISBURSE_DESC;

      for (let i = 0; i < PER_KIND_PER_DAY; i++) {
        const branchId = rand(BRANCHES);
        const bankAccountId = rand(headerAccts);
        const contraId = rand(contraAccts);
        const actor = rand(USERS);
        const amount = randAmount();
        const description = rand(descPool);
        const docNumber = `${seq.prefix}${String(seq.next).padStart(seq.digits, '0')}`;
        seq.next += 1;

        const txn = await prisma.erpFinCashBankTransaction.create({
          data: {
            docNumber, autoNumber: docNumber,
            direction: plan.dir as never, kind: plan.kind as never,
            paymentMethod: (plan.kind === 'BANK' ? 'TRANSFER' : 'CASH') as never,
            branchId, source: 'DUMMY_SEED', transactionDate: date, fiscalPeriodId,
            bankAccountId, description, currencyId: CURRENCY_IDR,
            exchangeRate: new Prisma.Decimal(1), amount,
            status: 'POSTED' as never, previousStatus: 'APPROVED' as never,
            postingStatus: 'POSTED' as never, postedAt: now,
            createdById: actor, updatedById: actor,
            lines: { create: [{
              accountId: contraId, currencyId: CURRENCY_IDR,
              exchangeRate: new Prisma.Decimal(1), amount, notes: description, lineNo: 1,
            }] },
          },
          select: { id: true },
        });

        const zero = new Prisma.Decimal(0);
        const base = {
          branchId, source: SOURCE, sourceDocType: SOURCE_DOC_TYPE, sourceId: txn.id,
          docNumber, entryDate: date, fiscalPeriodId, currencyId: CURRENCY_IDR,
          exchangeRate: new Prisma.Decimal(1), reconciliationStatus: 'UNRECONCILED' as const,
          status: 'POSTED' as const, postingStatus: 'POSTED' as const, postedAt: now,
          createdById: actor, updatedById: actor,
        };
        ledgerRows.push({
          ...base, accountId: bankAccountId, description,
          debit: isReceipt ? amount : zero, credit: isReceipt ? zero : amount, lineNo: 1,
        });
        ledgerRows.push({
          ...base, accountId: contraId, description, notes: description,
          debit: isReceipt ? zero : amount, credit: isReceipt ? amount : zero, lineNo: 2,
        });
        made++;
      }
    }
    console.log(`  ${date.toISOString().slice(0, 10)} → ${made} total`);
  }

  console.log(`→ Inserting ${ledgerRows.length} ledger entries…`);
  for (let i = 0; i < ledgerRows.length; i += 500) {
    await prisma.erpFinLedgerEntry.createMany({ data: ledgerRows.slice(i, i + 500) });
  }

  for (const [, n] of numberings) {
    await prisma.erpDocumentNumbering.update({ where: { id: n.id }, data: { nextNumber: n.next } });
  }

  console.log(`✓ Done: ${made} transactions + ${ledgerRows.length} ledger entries.`);
}

main()
  .catch((e) => { console.error(e); process.exit(1); })
  .finally(() => prisma.$disconnect());
