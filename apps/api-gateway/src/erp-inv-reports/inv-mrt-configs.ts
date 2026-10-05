/**
 * Dataset-builder configs for Inventory (M3) .mrt reports (Wave G4).
 *
 * Same contract as erp-md-reports / erp-fin-doc-reports (design D4/D5):
 * one generic SQL builder (erp-inv-mrt-reports.service) executes these
 * configs; `select` maps legacy .mrt dictionary field names → SQL
 * expressions over inv_* / md_* tables. ONLY config constants appear in
 * SQL text; runtime param values are always bound. Fields a template
 * declares but a config does not map are selected as NULL by the
 * builder, so every template binding resolves. `empty: true` = no ERP
 * equivalent exists — the report renders header-only and stays
 * CONVERTED (never VERIFIED with fake data).
 *
 * Stock semantics follow the ERP's own derivation (erp-inv-reports
 * stock-report-queries): balances are DERIVED from POSTED movement
 * lines (signed by movement_type) UNION POSTED opening stock lines;
 * line warehouse = COALESCE(destination, source). Costing = the ERP's
 * MOVING AVERAGE (InvMovingAverageCostService) — legacy templates named
 * "FIFO" receive moving-average data (the ERP keeps no FIFO layers).
 */

export interface InvParamFilter {
  /** SQL fragment with one or more `?` placeholders, all bound to the same value. */
  sql: string;
  kind: 'text' | 'number' | 'date';
}

export interface InvBindParam {
  name: string;
  kind: 'text' | 'number' | 'date';
}

export interface InvDatasetConfig {
  from?: string;
  select: Record<string, string>;
  where?: string;
  groupBy?: string;
  orderBy?: string;
  /** Main alias whose deleted_at must be NULL. */
  deletedAlias?: string;
  paramFilters?: Record<string, InvParamFilter>;
  /** Ordered params bound to `?` inside select/from/where/groupBy/orderBy. */
  bindParams?: InvBindParam[];
  empty?: boolean;
  note?: string;
}

export interface InvReportConfig {
  datasets: Record<string, InvDatasetConfig>;
}

export function emptyConfig(note: string): InvDatasetConfig {
  return { select: {}, empty: true, note };
}

/** Signed quantity of a movement line (alias m/l), ERP convention. */
export const SIGNED_QTY = `CASE m.movement_type
  WHEN 'TRANSFER_RECEIPT' THEN l.base_quantity
  WHEN 'RETURN' THEN l.base_quantity
  WHEN 'ISSUE' THEN -l.base_quantity
  WHEN 'TRANSFER' THEN -l.base_quantity
  ELSE 0 END`;

/** Warehouse a movement line is attributed to (ERP convention). */
export const LINE_WH = `COALESCE(l.destination_warehouse_id, l.source_warehouse_id)`;

/** Raw UNION SELECT of the unified stock-event stream (no alias wrapper). */
export const EVENTS_SELECT = `
  SELECT l.item_id AS item_id, ${LINE_WH} AS wh_id,
    m.movement_date AS evt_date, ${SIGNED_QTY} AS qty, m.doc_number AS doc_number
  FROM inv_stock_movement_lines l
  JOIN inv_stock_movements m ON m.id = l.stock_movement_id
  WHERE m.status = 'POSTED' AND m.deleted_at IS NULL
  UNION ALL
  SELECT ol.item_id, ol.warehouse_id, o.opening_date, ol.quantity, o.doc_number
  FROM inv_opening_stock_lines ol
  JOIN inv_opening_stocks o ON o.id = ol.opening_stock_id
  WHERE o.status = 'POSTED' AND o.deleted_at IS NULL
`;

/** Unified stock-event stream as a FROM subquery (alias `ev`). */
export const EVENTS_FROM = `(${EVENTS_SELECT}) ev`;

/** Derived balance of one item in one warehouse (correlated subquery). */
export const balanceExpr = (itemExpr: string, whExpr: string): string => `(
  SELECT COALESCE(SUM(x.qty), 0) FROM (
    SELECT ${SIGNED_QTY} AS qty
    FROM inv_stock_movement_lines l
    JOIN inv_stock_movements m ON m.id = l.stock_movement_id
    WHERE m.status = 'POSTED' AND m.deleted_at IS NULL
      AND l.item_id = ${itemExpr} AND ${LINE_WH} = ${whExpr}
    UNION ALL
    SELECT ol.quantity
    FROM inv_opening_stock_lines ol
    JOIN inv_opening_stocks o ON o.id = ol.opening_stock_id
    WHERE o.status = 'POSTED' AND o.deleted_at IS NULL
      AND ol.item_id = ${itemExpr} AND ol.warehouse_id = ${whExpr}
  ) x
)`;

/** Current moving-average cost of an item (ERP stored valuation basis). */
/**
 * Derived weighted moving-average unit cost per item — mirrors
 * ErpInvMovingAverageCostService: Σ value / Σ qty over POSTED opening
 * lines + POSTED inbound movement lines (TRANSFER_RECEIPT/RETURN) that
 * carry a unit cost; cost-less inbound lines drop out of BOTH sums.
 * The md_items.average_cost stamp is unmaintained, so the fallbacks are
 * last_hpp / purchase_price only.
 */
export const AVG_COST = `COALESCE((
  SELECT SUM(av.v) / NULLIF(SUM(av.q), 0) FROM (
    SELECT ol.quantity AS q, ol.quantity * ol.unit_cost AS v
      FROM inv_opening_stock_lines ol
      JOIN inv_opening_stocks o ON o.id = ol.opening_stock_id
     WHERE o.status = 'POSTED' AND o.deleted_at IS NULL AND ol.item_id = i.id
    UNION ALL
    SELECT l.base_quantity AS q, l.base_quantity * l.unit_cost AS v
      FROM inv_stock_movement_lines l
      JOIN inv_stock_movements m ON m.id = l.stock_movement_id
     WHERE m.status = 'POSTED' AND m.deleted_at IS NULL
       AND m.movement_type IN ('TRANSFER_RECEIPT', 'RETURN')
       AND l.unit_cost IS NOT NULL AND l.item_id = i.id
  ) av
), NULLIF(i.last_hpp, 0), i.purchase_price, 0)`;

export const periodFilters = (dateExpr: string): Record<string, InvParamFilter> => ({
  period_start: { sql: `${dateExpr} >= ?::date`, kind: 'date' },
  period_end: { sql: `${dateExpr} <= ?::date`, kind: 'date' },
});

export const docNoFilter = (expr: string): Record<string, InvParamFilter> => ({
  document_no: { sql: `${expr} = ?`, kind: 'text' },
});

/** Warehouse match by numeric id, exact code, or name substring. */
export const warehouseFilter = (
  idExpr: string,
  codeExpr: string,
  nameExpr: string,
): Record<string, InvParamFilter> => ({
  warehouse: {
    sql: `(${idExpr} = CASE WHEN ? ~ '^[0-9]+$' THEN ?::bigint ELSE -1 END OR ${codeExpr} = ? OR ${nameExpr} ILIKE '%' || ? || '%')`,
    kind: 'text',
  },
});

/** Partner match by numeric id, code substring, or name substring (mirrors fin/md). */
export const partnerFilter = (
  idExpr: string,
  codeExpr: string,
  nameExpr: string,
): Record<string, InvParamFilter> => ({
  partner: {
    sql: `(${idExpr} = CASE WHEN ? ~ '^[0-9]+$' THEN ?::bigint ELSE -1 END OR ${codeExpr} ILIKE '%' || ? || '%' OR ${nameExpr} ILIKE '%' || ? || '%')`,
    kind: 'text',
  },
});

/** Indonesian workflow-status label (mirrors frontend lib/status.ts). */
export const statusLabel = (expr: string): string =>
  `CASE ${expr}::text WHEN 'DRAFT' THEN 'Draft' WHEN 'NEED_APPROVE' THEN 'Need Approve' ` +
  `WHEN 'APPROVED' THEN 'Approved' WHEN 'REJECTED' THEN 'Rejected' WHEN 'POSTED' THEN 'Posted' ` +
  `WHEN 'VOID' THEN 'Void' WHEN 'CANCELLED' THEN 'Cancelled' ELSE ${expr}::text END`;
