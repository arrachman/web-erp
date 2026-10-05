/**
 * Dataset-builder configs for Finance DOCUMENT reports (Wave G2).
 *
 * Same contract as erp-md-reports (design D4/D5): one generic SQL
 * builder (erp-fin-doc-mrt-reports.service) executes these configs;
 * `select` maps legacy .mrt dictionary field names → SQL expressions
 * over fin_* / sls_* / pur_* / md_* tables. ONLY config constants
 * appear in SQL text; runtime param values are always bound.
 * Fields a template declares but a config does not map are selected
 * as NULL by the builder, so every template binding resolves.
 * `empty: true` = no ERP equivalent exists — the report renders
 * header-only and stays CONVERTED (never VERIFIED with fake data).
 */

export interface FinParamFilter {
  /** SQL fragment with one or more `?` placeholders, all bound to the same value. */
  sql: string;
  kind: 'text' | 'number' | 'date';
}

export interface FinTerbilangSpec {
  /** Dataset column receiving the spelled-out amount (legacy f_nominal). */
  column: string;
  /** sum: total of amountColumn per document · first: header amount repeated per row. */
  mode: 'sum' | 'first';
  amountColumn: string;
  /** Dataset column separating documents (the document number). */
  groupByColumn: string;
  /** Dataset column holding the currency code for the suffix. */
  currencyColumn?: string;
}

export interface FinDatasetConfig {
  from?: string;
  select: Record<string, string>;
  where?: string;
  groupBy?: string;
  orderBy?: string;
  /** Main alias whose deleted_at must be NULL. */
  deletedAlias?: string;
  paramFilters?: Record<string, FinParamFilter>;
  empty?: boolean;
  note?: string;
  terbilang?: FinTerbilangSpec;
}

export interface FinReportConfig {
  datasets: Record<string, FinDatasetConfig>;
}

export function emptyConfig(note: string): FinDatasetConfig {
  return { select: {}, empty: true, note };
}

/** Document-number equality filter (the normal print path). */
export const docNoFilter = (expr: string): Record<string, FinParamFilter> => ({
  document_no: { sql: `${expr} = ?`, kind: 'text' },
});

export const periodFilters = (dateExpr: string): Record<string, FinParamFilter> => ({
  period_start: { sql: `${dateExpr} >= ?::date`, kind: 'date' },
  period_end: { sql: `${dateExpr} <= ?::date`, kind: 'date' },
});

/** Partner match by numeric id, code substring, or name substring (mirrors md). */
export const partnerFilter = (
  idExpr: string,
  codeExpr: string,
  nameExpr: string,
): Record<string, FinParamFilter> => ({
  partner: {
    sql: `(${idExpr} = CASE WHEN ? ~ '^[0-9]+$' THEN ?::bigint ELSE -1 END OR ${codeExpr} ILIKE '%' || ? || '%' OR ${nameExpr} ILIKE '%' || ? || '%')`,
    kind: 'text',
  },
});

export const branchFilter = (expr: string): Record<string, FinParamFilter> => ({
  branch: { sql: `${expr} = ?`, kind: 'number' },
});

/** The standard document-report filter set for alias `t` + partner alias `p`. */
export function docFilters(
  dateExpr: string,
  opts: { partner?: boolean; branch?: boolean } = {},
): Record<string, FinParamFilter> {
  return {
    ...docNoFilter('t.doc_number'),
    ...periodFilters(dateExpr),
    ...(opts.partner === false
      ? {}
      : partnerFilter('t.partner_id', 'p.code', 'p.name')),
    ...(opts.branch === false ? {} : branchFilter('t.branch_id')),
  };
}

/** Indonesian workflow-status label (mirrors frontend lib/status.ts). */
export const statusLabel = (expr: string): string =>
  `CASE ${expr} WHEN 'DRAFT' THEN 'Draft' WHEN 'NEED_APPROVE' THEN 'Need Approve' ` +
  `WHEN 'APPROVED' THEN 'Approved' WHEN 'REJECTED' THEN 'Rejected' WHEN 'POSTED' THEN 'Posted' ` +
  `WHEN 'VOID' THEN 'Void' WHEN 'CANCELLED' THEN 'Cancelled' ELSE ${expr} END`;
