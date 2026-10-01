/**
 * Seed module-default Report Designer templates: `<module>.__default`.
 *
 * The engine falls back to a module default when a report has no exact
 * `<module>.<report>` template bound (ReportEngineService.renderReport). This makes
 * every Sales / Purchasing / Inventory report (and any future Finance report) render
 * its PDF through the engine with a sensible auto layout, materialized from the
 * report's live columns. Editing a module default restyles all that module's reports;
 * a per-report template still overrides it.
 *
 * Run: npx ts-node prisma/seed-erp-report-module-defaults.ts
 * Idempotent: upsert by unique `code` (RPT-<MODULE>-DEFAULT).
 */
import { PrismaClient, Prisma } from '@prisma/client';

const prisma = new PrismaClient();

// Most list/tabular reports are column-heavy → landscape default.
const MODULES: { module: string; name: string }[] = [
  { module: 'fin', name: 'Finance' },
  { module: 'sls', name: 'Sales' },
  { module: 'pur', name: 'Purchasing' },
  { module: 'inv', name: 'Inventory' },
];

const MARGINS = { top: 12, right: 10, bottom: 12, left: 10 };

function defaultTemplateJson(name: string): Prisma.InputJsonValue {
  return {
    auto: true,
    name: `${name} — Default`,
    pageSize: 'A4',
    orientation: 'landscape',
    margins: MARGINS,
  };
}

async function main() {
  for (const m of MODULES) {
    const code = `RPT-${m.module.toUpperCase()}-DEFAULT`;
    const reportKey = `${m.module}.__default`;
    const data = {
      name: `${m.name} Report (Default)`,
      module: m.module,
      description: `Template default modul ${m.name} (dipakai bila laporan belum punya template khusus)`,
      reportKey,
      templateJson: defaultTemplateJson(m.name),
      isActive: true,
    };
    const existing = await prisma.erpRptTemplate.findUnique({ where: { code } });
    if (existing) {
      await prisma.erpRptTemplate.update({ where: { code }, data });
    } else {
      await prisma.erpRptTemplate.create({ data: { code, ...data } });
    }
    console.log(`✓ ${code} → ${reportKey}`);
  }
  console.log('Module-default report templates seeded.');
}

main()
  .catch((err) => {
    console.error(err);
    process.exit(1);
  })
  .finally(() => prisma.$disconnect());
