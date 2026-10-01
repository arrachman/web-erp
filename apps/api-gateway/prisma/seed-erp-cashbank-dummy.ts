/**
 * One-off: generate 1000 dummy cash/bank transactions (POSTED + GL ledger),
 * backdated 2025-01-01 .. 2026-06-07, evenly split across the 4 directions:
 *   Kas Masuk (CASH/RECEIPT), Kas Keluar (CASH/DISBURSEMENT),
 *   Bank Masuk (BANK/RECEIPT), Bank Keluar (BANK/DISBURSEMENT).
 *
 * Each txn = 1 header + 1 contra line + 2 balanced ledger entries.
 * Doc numbers drawn from sys_document_numberings (sequence advanced at the end).
 *
 * Run: npx ts-node prisma/seed-erp-cashbank-dummy.ts
 * Idempotent-ish: appends rows; numbering sequence is advanced so re-runs don't
 * collide on doc_number. Mirrors CashBankPostingService posting logic exactly.
 */
import { PrismaClient, Prisma } from '@prisma/client';

const prisma = new PrismaClient();

// ── reference master data (resolved from live DB 2026-06-07) ───────────────────
const CURRENCY_IDR = 1n;
const BRANCHES = [1n, 1015n, 1016n, 1017n, 1018n]; // HQ + 4 pabrik/gudang
const USERS = [1n, 2n, 3n];

const CASH_ACCOUNTS = [183n, 184n]; // Kas Besar, Kas Kecil
const BANK_ACCOUNTS = [185n, 186n, 187n, 188n, 189n]; // BCA/Mandiri/BNI/BRI/CIMB Giro IDR

// contra accounts for RECEIPT (revenue/other income)
const REVENUE_CONTRA = [259n, 260n, 261n, 264n, 268n, 269n];
// contra accounts for DISBURSEMENT (expenses)
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

const DATE_FROM = new Date('2025-01-01T00:00:00Z');
const DATE_TO = new Date('2026-06-07T00:00:00Z');

type Dir = 'RECEIPT' | 'DISBURSEMENT';
type Kind = 'CASH' | 'BANK';

const SOURCE = 'CASH_BANK';
const SOURCE_DOC_TYPE = 'fin_cash_bank_transactions';

const rand = <T>(arr: T[]): T => arr[Math.floor(Math.random() * arr.length)];
const randInt = (min: number, max: number) =>
  Math.floor(Math.random() * (max - min + 1)) + min;

function randDate(): Date {
  const t = DATE_FROM.getTime() + Math.random() * (DATE_TO.getTime() - DATE_FROM.getTime());
  const d = new Date(t);
  return new Date(Date.UTC(d.getUTCFullYear(), d.getUTCMonth(), d.getUTCDate()));
}

// amount: 500.000 .. 50.000.000 rounded to nearest 1.000
function randAmount(): Prisma.Decimal {
  const v = randInt(500, 50_000) * 1000;
  return new Prisma.Decimal(v);
}

interface Plan {
  kind: Kind;
  dir: Dir;
  docCode: string;
}
const PLANS: Plan[] = [
  { kind: 'CASH', dir: 'RECEIPT', docCode: 'CASH_RECEIPT' },
  { kind: 'CASH', dir: 'DISBURSEMENT', docCode: 'CASH_DISBURSEMENT' },
  { kind: 'BANK', dir: 'RECEIPT', docCode: 'BANK_RECEIPT' },
  { kind: 'BANK', dir: 'DISBURSEMENT', docCode: 'BANK_DISBURSEMENT' },
];
const PER_PLAN = 250;

async function main() {
  console.log('→ Loading fiscal periods…');
  const periods = await prisma.erpFiscalPeriod.findMany({
    where: { deletedAt: null },
    select: { id: true, startDate: true, endDate: true },
  });
  const periodFor = (d: Date): bigint => {
    const p = periods.find((x) => x.startDate <= d && x.endDate >= d);
    if (!p) throw new Error(`No fiscal period covers ${d.toISOString()}`);
    return p.id;
  };

  console.log('→ Loading document numberings…');
  const numberings = new Map<string, { id: bigint; prefix: string; next: number; digits: number }>();
  for (const plan of PLANS) {
    const n = await prisma.erpDocumentNumbering.findFirst({
      where: { documentCode: plan.docCode, deletedAt: null },
    });
    if (!n) throw new Error(`No numbering for ${plan.docCode}`);
    numberings.set(plan.docCode, {
      id: n.id, prefix: n.prefix, next: n.nextNumber, digits: n.digitCount,
    });
  }

  const ledgerRows: Prisma.ErpFinLedgerEntryCreateManyInput[] = [];
  let made = 0;
  const now = new Date();

  for (const plan of PLANS) {
    const seq = numberings.get(plan.docCode)!;
    const isReceipt = plan.dir === 'RECEIPT';
    const headerAccts = plan.kind === 'CASH' ? CASH_ACCOUNTS : BANK_ACCOUNTS;
    const contraAccts = isReceipt ? REVENUE_CONTRA : EXPENSE_CONTRA;
    const descPool = isReceipt ? RECEIPT_DESC : DISBURSE_DESC;

    for (let i = 0; i < PER_PLAN; i++) {
      const date = randDate();
      const fiscalPeriodId = periodFor(date);
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
          docNumber,
          autoNumber: docNumber,
          direction: plan.dir as never,
          kind: plan.kind as never,
          paymentMethod: (plan.kind === 'BANK' ? 'TRANSFER' : 'CASH') as never,
          branchId,
          source: 'DUMMY_SEED',
          transactionDate: date,
          fiscalPeriodId,
          bankAccountId,
          description,
          currencyId: CURRENCY_IDR,
          exchangeRate: new Prisma.Decimal(1),
          amount,
          status: 'POSTED' as never,
          previousStatus: 'APPROVED' as never,
          postingStatus: 'POSTED' as never,
          postedAt: now,
          createdById: actor,
          updatedById: actor,
          lines: {
            create: [{
              accountId: contraId,
              currencyId: CURRENCY_IDR,
              exchangeRate: new Prisma.Decimal(1),
              amount,
              notes: description,
              lineNo: 1,
            }],
          },
        },
        select: { id: true },
      });

      const zero = new Prisma.Decimal(0);
      const base = {
        branchId, source: SOURCE, sourceDocType: SOURCE_DOC_TYPE, sourceId: txn.id,
        docNumber, entryDate: date, fiscalPeriodId, currencyId: CURRENCY_IDR,
        exchangeRate: new Prisma.Decimal(1),
        reconciliationStatus: 'UNRECONCILED' as const,
        status: 'POSTED' as const, postingStatus: 'POSTED' as const,
        postedAt: now, createdById: actor, updatedById: actor,
      };
      // header cash/bank row
      ledgerRows.push({
        ...base, accountId: bankAccountId, description,
        debit: isReceipt ? amount : zero, credit: isReceipt ? zero : amount, lineNo: 1,
      });
      // contra row
      ledgerRows.push({
        ...base, accountId: contraId, description, notes: description,
        debit: isReceipt ? zero : amount, credit: isReceipt ? amount : zero, lineNo: 2,
      });

      made++;
      if (made % 100 === 0) console.log(`  …${made}/1000 transactions`);
    }
  }

  console.log(`→ Inserting ${ledgerRows.length} ledger entries…`);
  // chunk to stay under param limits
  for (let i = 0; i < ledgerRows.length; i += 500) {
    await prisma.erpFinLedgerEntry.createMany({ data: ledgerRows.slice(i, i + 500) });
  }

  console.log('→ Advancing numbering sequences…');
  for (const [, n] of numberings) {
    await prisma.erpDocumentNumbering.update({
      where: { id: n.id }, data: { nextNumber: n.next },
    });
  }

  console.log(`✓ Done: ${made} transactions + ${ledgerRows.length} ledger entries.`);
}

main()
  .catch((e) => { console.error(e); process.exit(1); })
  .finally(() => prisma.$disconnect());
