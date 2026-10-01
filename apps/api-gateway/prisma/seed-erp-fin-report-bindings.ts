/**
 * Seed reportKey-bound Report Designer templates for the Finance reports
 * (today's "builder owns data, template owns layout" architecture — see
 * DECISIONS.md 2026-06-13). Distinct from the legacy `seed-erp-report-templates.ts`
 * (Jun 9, template-owns-SQL `{field}` format) which is left untouched.
 *
 * These are "auto" templates — presentation only (page size / orientation /
 * margins); the band layout is materialized from each report's live columns at
 * render time (ReportEngineService.materialize). Binding via `reportKey =
 * fin.<report>` is what routes a report's PDF through the engine.
 *
 * Run: npx ts-node prisma/seed-erp-fin-report-bindings.ts
 * Idempotent: upsert by unique `code` (RPT-FIN-<KEY>).
 */
import { PrismaClient, Prisma } from '@prisma/client';

const prisma = new PrismaClient();

type Orientation = 'portrait' | 'landscape';

interface FinReportSeed {
  key: string; // bare report key (matches ReportDocument.key)
  name: string;
  orientation: Orientation;
}

// Wide, column-heavy reports default to landscape; statements to portrait.
const FIN_REPORTS: FinReportSeed[] = [
  { key: 'general-ledger', name: 'General Ledger', orientation: 'landscape' },
  { key: 'trial-balance', name: 'Trial Balance', orientation: 'landscape' },
  { key: 'movement-balance', name: 'Neraca Mutasi', orientation: 'landscape' },
  { key: 'balance-sheet', name: 'Balance Sheet', orientation: 'portrait' },
  { key: 'income-statement', name: 'Income Statement', orientation: 'portrait' },
  { key: 'equity-changes', name: 'Equity Changes', orientation: 'landscape' },
  { key: 'cash-flow', name: 'Cash Flow', orientation: 'portrait' },
  { key: 'daily-cash-bank', name: 'Daily Cash & Bank', orientation: 'landscape' },
  { key: 'ar-card', name: 'AR Card', orientation: 'landscape' },
  { key: 'ar-aging', name: 'AR Aging', orientation: 'landscape' },
  { key: 'ap-card', name: 'AP Card', orientation: 'landscape' },
  { key: 'ap-aging', name: 'AP Aging', orientation: 'landscape' },
  { key: 'giro-maturity', name: 'Giro Maturity', orientation: 'landscape' },
  { key: 'budget-realization', name: 'Budget vs Realization', orientation: 'landscape' },
];

const MARGINS = { top: 12, right: 10, bottom: 12, left: 10 };

function autoTemplateJson(r: FinReportSeed): Prisma.InputJsonValue {
  return {
    auto: true,
    name: r.name,
    pageSize: 'A4',
    orientation: r.orientation,
    margins: MARGINS,
  };
}

async function main() {
  let created = 0;
  let updated = 0;
  for (const r of FIN_REPORTS) {
    const code = `RPT-FIN-${r.key.toUpperCase()}`;
    const reportKey = `fin.${r.key}`;
    const existing = await prisma.erpRptTemplate.findUnique({ where: { code } });
    const data = {
      name: `${r.name} (Default)`,
      module: 'fin',
      description: `Template laporan default untuk ${r.name}`,
      reportKey,
      templateJson: autoTemplateJson(r),
      isActive: true,
    };
    if (existing) {
      await prisma.erpRptTemplate.update({ where: { code }, data });
      updated += 1;
    } else {
      await prisma.erpRptTemplate.create({ data: { code, ...data } });
      created += 1;
    }
    console.log(`✓ ${code} → ${reportKey}`);
  }
  console.log(`Finance report bindings seeded — created: ${created}, updated: ${updated}`);
}

main()
  .catch((err) => {
    console.error(err);
    process.exit(1);
  })
  .finally(() => prisma.$disconnect());
