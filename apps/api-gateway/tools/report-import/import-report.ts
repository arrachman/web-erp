/**
 * DB writes for the .mrt importer (Wave G0): upsert the converted
 * template into rpt_templates (keyed by report_key) and link the
 * m0_reports registry row (rpt_template_id + report_key +
 * translation_status=CONVERTED). A VERIFIED row is never overwritten
 * unless force is set (design D1).
 */

import { PrismaClient } from '@prisma/client';
import type { ReportTemplateV2 } from '../../src/erp-report-engine/engine-types-v2';

export interface RegistryRow {
  id: bigint;
  code: string;
  title: string;
  legacyModule: string;
  erpModule: string | null;
  filename: string;
  sourceRelPath: string;
  translationStatus: string;
}

const MODULE_KEY_PREFIX: Record<string, string> = {
  m0: 'adm',
  adm: 'adm',
  m1: 'md',
  md: 'md',
  m2: 'fin',
  fin: 'fin',
  m3: 'inv',
  inv: 'inv',
  m4: 'pur',
  pur: 'pur',
  m5: 'sls',
  sls: 'sls',
  m6: 'mfg',
  mfg: 'mfg',
  m7: 'fa',
  fa: 'fa',
  m8: 'bi',
  bi: 'bi',
  m12: 'pos',
  pos: 'pos',
  m13: 'm13',
};

/** RPT-M1-DAFTAR-HARGA → md.daftarharga (design D1 deterministic key). */
export function reportKeyForCode(code: string, legacyModule: string): string {
  const withoutPrefix = code.replace(/^RPT-/i, '');
  const parts = withoutPrefix.split('-');
  const moduleToken = (parts[0] ?? legacyModule).toLowerCase();
  const prefix = MODULE_KEY_PREFIX[moduleToken] ?? MODULE_KEY_PREFIX[legacyModule.toLowerCase()] ?? moduleToken;
  const slug = parts.slice(1).join('').toLowerCase().replace(/[^a-z0-9]/g, '');
  return `${prefix}.${slug}`;
}

export async function loadWorklist(
  prisma: PrismaClient,
  opts: { module?: string; code?: string; includeConverted: boolean },
): Promise<RegistryRow[]> {
  const rows = await prisma.erpM0Report.findMany({
    where: {
      ...(opts.module ? { legacyModule: opts.module } : {}),
      ...(opts.code ? { code: opts.code } : {}),
      ...(opts.includeConverted ? {} : { translationStatus: 'PENDING' }),
    },
    orderBy: [{ legacyModule: 'asc' }, { urutan: 'asc' }],
  });
  return rows.map((r) => ({
    id: r.id,
    code: r.code,
    title: r.title,
    legacyModule: r.legacyModule,
    erpModule: r.erpModule,
    filename: r.filename,
    sourceRelPath: r.sourceRelPath,
    translationStatus: r.translationStatus,
  }));
}

export async function saveConversion(
  prisma: PrismaClient,
  row: RegistryRow,
  template: ReportTemplateV2,
  force: boolean,
): Promise<'converted' | 'skipped-verified'> {
  if (row.translationStatus === 'VERIFIED' && !force) return 'skipped-verified';
  const reportKey = reportKeyForCode(row.code, row.legacyModule);
  const moduleCode = reportKey.split('.')[0];
  const existing = await prisma.erpRptTemplate.findFirst({
    where: { reportKey },
    select: { id: true },
  });
  const data = {
    code: row.code,
    name: row.title,
    module: moduleCode,
    description: `Konversi otomatis dari ${row.filename} (registry ${row.code})`,
    templateJson: template as unknown as object,
    reportKey,
    isActive: true,
  };
  const saved = existing
    ? await prisma.erpRptTemplate.update({ where: { id: existing.id }, data })
    : await prisma.erpRptTemplate.create({ data });
  await prisma.erpM0Report.update({
    where: { id: row.id },
    data: {
      rptTemplateId: saved.id,
      reportKey,
      translationStatus: 'CONVERTED',
    },
  });
  return 'converted';
}
