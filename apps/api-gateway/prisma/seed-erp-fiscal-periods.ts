/**
 * Seed real fiscal periods into sys_fiscal_periods.
 * Generates 144 monthly periods covering 2020–2031 (12 years × 12 months).
 * Idempotent — uses upsert on @@unique([year, periodNo]).
 *
 * Status logic (based on currentDate 2026-05-23):
 *   - 2020–2025 all months   → CLOSED
 *   - 2026 Jan–Apr           → CLOSED
 *   - 2026 May (current)     → OPEN
 *   - 2026 Jun–Dec           → OPEN
 *   - 2027–2031 all months   → OPEN
 *
 * Run: npm run db:seed:fiscal-periods
 *   or: npx ts-node --skip-project prisma/seed-erp-fiscal-periods.ts
 */
import { PrismaClient, ErpFiscalPeriodStatus } from '@prisma/client';

const prisma = new PrismaClient();

const MONTH_NAMES_ID = [
  'Jan', 'Feb', 'Mar', 'Apr', 'Mei', 'Jun',
  'Jul', 'Agu', 'Sep', 'Okt', 'Nov', 'Des',
];

// currentDate reference: 2026-05-23
const CURRENT_YEAR = 2026;
const CURRENT_MONTH = 5; // 1-indexed (May)

function resolveStatus(year: number, month: number): ErpFiscalPeriodStatus {
  if (year < CURRENT_YEAR) return ErpFiscalPeriodStatus.CLOSED;
  if (year === CURRENT_YEAR && month < CURRENT_MONTH) return ErpFiscalPeriodStatus.CLOSED;
  // current month (2026-05) and all future months → OPEN
  return ErpFiscalPeriodStatus.OPEN;
}

/**
 * Last day of a 1-indexed month.
 * JS: new Date(year, M, 0) — month M is 0-indexed in Date, so this is day 0 of month M
 * which equals the last day of month M-1 (1-indexed). Hence for 1-indexed month we pass M directly.
 * Example: new Date(2025, 2, 0) = Feb 28, 2025 (last day of month 2 = February).
 */
function lastDayOfMonth(year: number, month: number): Date {
  // month is 1-indexed; new Date(year, month, 0) returns last day of that month
  return new Date(year, month, 0);
}

function firstDayOfMonth(year: number, month: number): Date {
  // month is 1-indexed
  return new Date(year, month - 1, 1);
}

async function main() {
  const START_YEAR = 2020;
  const END_YEAR   = 2031;

  let upserted = 0;

  for (let year = START_YEAR; year <= END_YEAR; year++) {
    for (let month = 1; month <= 12; month++) {
      const periodNo  = month;
      const name      = `${MONTH_NAMES_ID[month - 1]} ${year}`;
      const startDate = firstDayOfMonth(year, month);
      const endDate   = lastDayOfMonth(year, month);
      const status    = resolveStatus(year, month);

      await prisma.erpFiscalPeriod.upsert({
        where: { year_periodNo: { year, periodNo } },
        update: {
          name,
          startDate,
          endDate,
          status,
        },
        create: {
          year,
          periodNo,
          name,
          startDate,
          endDate,
          status,
        },
      });

      upserted++;
    }
  }

  const total = await prisma.erpFiscalPeriod.count();
  console.log(`Done. Upserted ${upserted} fiscal periods. sys_fiscal_periods now has ${total} rows.`);
}

main()
  .catch(err => { console.error(err); process.exit(1); })
  .finally(() => prisma.$disconnect());
