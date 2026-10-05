/**
 * Dataset-builder configs for Purchasing (M4) .mrt reports (Wave G5).
 *
 * Same contract as erp-md-reports / erp-fin-doc-reports (design D4/D5):
 * one generic SQL builder (erp-pur-mrt-reports.service) executes these
 * configs; `select` maps legacy .mrt dictionary field names → SQL
 * expressions over pur_* / fin_* / md_* tables. ONLY config constants
 * appear in SQL text; runtime param values are always bound. Fields a
 * template declares but a config does not map are selected as NULL by
 * the builder, so every template binding resolves. `empty: true` = no
 * ERP equivalent exists — the report renders header-only and stays
 * CONVERTED (never VERIFIED with fake data).
 *
 * Legacy → ERP entity map (db-design/entities-m4-purchasing.md):
 *   m4_pr  → pur_requisitions        m4_rq  → pur_quotations
 *   m4_rfq → pur_rfqs                m4_bs  → pur_bid_selections
 *   m4_po  → pur_orders              m4_grn → pur_goods_receipts
 *   m4_ri  → pur_invoices            m4_prt → pur_returns (RETURN_TO_VENDOR)
 *   m4_dnr → pur_returns (DEBIT_NOTE)
 *   m4_ap  → fin_ap_payments (source='AP')   m4_vp  → fin_ap_payments (source='VP')
 *   m4_vpp → fin_ap_payments (source='VPP')  m4_pp  → fin_ap_payments (source='PP')
 *   m4_vp_detail  → fin_settlement_allocations (invoice_ref = settled doc number)
 *   m4_vp_pay     → fin_payment_instruments
 *   m4_pf  → — (no ERP entity; honest-empty)
 *   m4_pie → — (no ERP entity; honest-empty)
 */

export interface PurParamFilter {
  /** SQL fragment with one or more `?` placeholders, all bound to the same value. */
  sql: string;
  kind: 'text' | 'number' | 'date';
}

export interface PurTerbilangSpec {
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

export interface PurBindParam {
  name: string;
  kind: 'text' | 'number' | 'date';
}

export interface PurDatasetConfig {
  from?: string;
  select: Record<string, string>;
  where?: string;
  groupBy?: string;
  orderBy?: string;
  /** Main alias whose deleted_at must be NULL. */
  deletedAlias?: string;
  paramFilters?: Record<string, PurParamFilter>;
  /** Ordered params bound to `?` inside select/from/where/groupBy/orderBy. */
  bindParams?: PurBindParam[];
  empty?: boolean;
  note?: string;
  terbilang?: PurTerbilangSpec[];
}

export interface PurReportConfig {
  datasets: Record<string, PurDatasetConfig>;
}

export function emptyConfig(note: string): PurDatasetConfig {
  return { select: {}, empty: true, note };
}

/* ------------------------------ filters ------------------------------ */

/** Document-number equality filter (the normal print path). */
export const docNoFilter = (expr: string): Record<string, PurParamFilter> => ({
  document_no: { sql: `${expr} = ?`, kind: 'text' },
});

export const periodFilters = (dateExpr: string): Record<string, PurParamFilter> => ({
  period_start: { sql: `${dateExpr} >= ?::date`, kind: 'date' },
  period_end: { sql: `${dateExpr} <= ?::date`, kind: 'date' },
});

/** Partner match by numeric id, code substring, or name substring (mirrors md/fin). */
export const partnerFilter = (
  idExpr: string,
  codeExpr: string,
  nameExpr: string,
): Record<string, PurParamFilter> => ({
  partner: {
    sql: `(${idExpr} = CASE WHEN ? ~ '^[0-9]+$' THEN ?::bigint ELSE -1 END OR ${codeExpr} ILIKE '%' || ? || '%' OR ${nameExpr} ILIKE '%' || ? || '%')`,
    kind: 'text',
  },
});

/** Warehouse match by numeric id, exact code, or name substring. */
export const warehouseFilter = (
  idExpr: string,
  codeExpr: string,
  nameExpr: string,
): Record<string, PurParamFilter> => ({
  warehouse: {
    sql: `(${idExpr} = CASE WHEN ? ~ '^[0-9]+$' THEN ?::bigint ELSE -1 END OR ${codeExpr} = ? OR ${nameExpr} ILIKE '%' || ? || '%')`,
    kind: 'text',
  },
});

export const branchFilter = (expr: string): Record<string, PurParamFilter> => ({
  branch: { sql: `${expr} = ?`, kind: 'number' },
});

export const projectFilter = (idExpr: string): Record<string, PurParamFilter> => ({
  project: { sql: `${idExpr} = ?`, kind: 'number' },
});

export const divisionFilter = (idExpr: string): Record<string, PurParamFilter> => ({
  division: { sql: `${idExpr} = ?`, kind: 'number' },
});

export const locationFilter = (idExpr: string): Record<string, PurParamFilter> => ({
  location: { sql: `${idExpr} = ?`, kind: 'number' },
});

/** The standard document-report filter set for header alias `t` + partner alias `p`. */
export function docFilters(
  dateExpr: string,
  opts: { warehouse?: { id: string; code: string; name: string }; partner?: boolean } = {},
): Record<string, PurParamFilter> {
  return {
    ...docNoFilter('t.doc_number'),
    ...periodFilters(dateExpr),
    ...(opts.partner === false ? {} : partnerFilter('p.id', 'p.code', 'p.name')),
    ...(opts.warehouse
      ? warehouseFilter(opts.warehouse.id, opts.warehouse.code, opts.warehouse.name)
      : {}),
  };
}

/* --------------------------- shared fragments --------------------------- */

/** Indonesian workflow-status label (mirrors frontend lib/status.ts). Enum-safe cast (G4 fix). */
export const statusLabel = (expr: string): string =>
  `CASE ${expr}::text WHEN 'DRAFT' THEN 'Draft' WHEN 'NEED_APPROVE' THEN 'Need Approve' ` +
  `WHEN 'APPROVED' THEN 'Approved' WHEN 'REJECTED' THEN 'Rejected' WHEN 'POSTED' THEN 'Posted' ` +
  `WHEN 'VOID' THEN 'Void' WHEN 'CANCELLED' THEN 'Cancelled' ELSE ${expr}::text END`;

/** Default partner address as LATERAL (alias `addr`), partner alias `p`. */
export const ADDR_LATERAL = `LEFT JOIN LATERAL (
  SELECT address_line1, address_line2, phone, fax FROM md_partner_addresses
  WHERE partner_id = p.id AND deleted_at IS NULL
  ORDER BY is_default DESC NULLS LAST, id LIMIT 1) addr ON TRUE`;

/** Line total in legacy semantics: (jml × harga) − jmldiskon (before tax). */
export const lineTotal = (l: string): string =>
  `(${l}.quantity * ${l}.unit_price - COALESCE(${l}.discount_amount, 0))`;

/**
 * Standard detail-line columns (aliases: l = line, i = item, u = unit).
 * Legacy `diskon` = line discount percent, `jmldiskon` = discount amount,
 * `total` = (jml × harga) − jmldiskon.
 */
export const lineCols = (l = 'l', i = 'i', u = 'u'): Record<string, string> => ({
  bkode: `${i}.code`,
  namabarang: `${i}.name`,
  bnama: `${i}.name`,
  jml: `${l}.quantity`,
  satuan: `${u}.name`,
  diskon: `${l}.discount_percent`,
  jmldiskon: `COALESCE(${l}.discount_amount, 0)`,
  harga: `${l}.unit_price`,
  total: lineTotal(l),
  catatan: `${l}.notes`,
  urutan: `${l}.line_no`,
});

/**
 * Standard document-header columns for prefix `x` (aliases: t = header,
 * p = partner, cur = currency). E.g. hdrCols('po') → ponotransaksi, …
 */
export const hdrCols = (x: string, t = 't', p = 'p', cur = 'cur'): Record<string, string> => ({
  [`${x}id`]: `${t}.id`,
  [`${x}notransaksi`]: `${t}.doc_number`,
  [`${x}tgl`]: `${t}.doc_date`,
  [`${x}supplier`]: `${p}.name`,
  [`${x}matauang`]: `${cur}.code`,
  [`${x}kurs`]: `${t}.exchange_rate`,
  [`${x}uraian`]: `${t}.description`,
  [`${x}catatan`]: `${t}.notes`,
  [`${x}status`]: statusLabel(`${t}.status`),
  [`${x}statusnama`]: statusLabel(`${t}.status`),
  [`${x}diskonpersen`]: `${t}.discount_percent`,
  [`${x}jmldiskon`]: `COALESCE(${t}.discount_amount, 0)`,
  [`${x}totalpajak1detail`]: `COALESCE(${t}.tax1_amount, 0)`,
  [`${x}totalpajak2detail`]: `COALESCE(${t}.tax2_amount, 0)`,
  [`${x}biayalain`]: `COALESCE(${t}.other_cost_amount, 0)`,
  [`${x}biayalainpersen`]: `${t}.other_cost_percent`,
  [`${x}totaltransaksi`]: `${t}.grand_total`,
  [`${x}tgljatuhtempo`]: `${t}.due_date`,
  [`${x}termin`]: 'pt.name',
  [`${x}hargatermasukpajak`]: `CASE ${t}.price_mode::text WHEN 'TAX_INCLUSIVE' THEN 1 ELSE 0 END`,
  caption: `CASE ${t}.price_mode::text WHEN 'TAX_INCLUSIVE' THEN 'Sudah Termasuk Pajak' ELSE 'Belum Termasuk Pajak' END`,
  kkode: `${p}.code`,
  knama: `${p}.name`,
});

/** Joins shared by every pur document family (aliases t/p/cur/pt/addr/pc/ctc). */
export const docJoins = (partnerFk: string): string => `
  LEFT JOIN md_partners p ON p.id = ${partnerFk}
  LEFT JOIN md_currencies cur ON cur.id = t.currency_id
  LEFT JOIN md_payment_terms pt ON pt.id = t.payment_term_id
  LEFT JOIN md_partner_categories pc ON pc.id = p.supplier_category_id
  LEFT JOIN md_partner_contacts ctc ON ctc.id = t.supplier_contact_id
  ${ADDR_LATERAL}`;

/** Supplier-category + address columns many list templates declare. */
export const partnerExtraCols = (): Record<string, string> => ({
  kkategorisupplier: 'pc.code',
  kkategorisuppliernama: 'pc.name',
  k1alamat1: 'addr.address_line1',
  k1notelp1: 'addr.phone',
  k1nofax: 'addr.fax',
  kkontakperson: 'ctc.name',
});

/**
 * Derived stock balance of item alias `i` across all warehouses —
 * same derivation as erp-inv-reports (POSTED movement lines signed by
 * movement_type + POSTED opening stock lines; balances never stored).
 */
export const stockBalance = (i = 'i'): string => `(
  SELECT COALESCE(SUM(x.qty), 0) FROM (
    SELECT CASE m.movement_type
        WHEN 'TRANSFER_RECEIPT' THEN ll.base_quantity
        WHEN 'RETURN' THEN ll.base_quantity
        WHEN 'ISSUE' THEN -ll.base_quantity
        WHEN 'TRANSFER' THEN -ll.base_quantity ELSE 0 END AS qty
    FROM inv_stock_movement_lines ll
    JOIN inv_stock_movements m ON m.id = ll.stock_movement_id
    WHERE m.status = 'POSTED' AND m.deleted_at IS NULL AND ll.item_id = ${i}.id
    UNION ALL
    SELECT ol.quantity
    FROM inv_opening_stock_lines ol
    JOIN inv_opening_stocks o ON o.id = ol.opening_stock_id
    WHERE o.status = 'POSTED' AND o.deleted_at IS NULL AND ol.item_id = ${i}.id
  ) x
)`;
