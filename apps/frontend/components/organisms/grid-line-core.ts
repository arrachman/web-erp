/**
 * Generic, model-agnostic core for the config-driven transaction line grids
 * (Kustomisasi Grid). Shared by the cash/bank contra-account grid and the sales
 * item-line grid — anything master-detail that renders columns from
 * `sys_transaction_grid_columns` config. No row shape is baked in here: a
 * concrete grid supplies a `GridModel<Row>` adapter (how to read/patch a cell
 * and mint a blank row). Keeps the spreadsheet engine (`useGridNav`) and cell
 * renderer (`LineCell`) reusable across transaction families (§2.1 atomic reuse,
 * §3 no duplication).
 */

export type GridDataType = 'TEXT' | 'NUMBER' | 'DATE' | 'LOOKUP';

/** A single grid column (subset of the API column that the live grid needs). */
export interface GridCol {
  dataField: string;
  headerText: string;
  width: number;
  dataType: GridDataType;
  lookupSource?: string | null;
  lookupDefaultFilter?: Record<string, unknown> | null;
  lookupDefaultSort?: string | null;
  kind: 'STANDARD' | 'CUSTOM';
  isEditable: boolean;
  isRequired: boolean;
  /** Skip flag (Kustomisasi Grid) — cell is view-only (selectable but not editable);
   *  Enter-navigation jumps over it. Click / arrows / Tab can still land on it. */
  isSkippable?: boolean;
  /** Semantic editor type from Kustomisasi Grid (wins over dataType for widget selection). */
  cellEditor?: string | null;
  /** Static option list — used by COMBOBOX editor. */
  options?: string[];
  /** Cell placeholder shown when empty (Kustomisasi Grid field settings). */
  placeholder?: string | null;
  /** Value prefilled on a new line row (lookup = id, others = raw string). */
  defaultValue?: string | null;
  /** Stored display label for a lookup `defaultValue` (so the defaulted cell reads correctly). */
  defaultValueLabel?: string | null;
}

/** Common shape every grid row must satisfy (stable key + lookup labels + custom values). */
export interface GridRowBase {
  key: string;
  /** Resolved display labels for lookup columns, keyed by dataField. */
  labels?: Record<string, string>;
  /** User-defined column values (Kustomisasi Grid), keyed by dataField. */
  customFields?: Record<string, unknown>;
}

/**
 * Adapter that teaches the generic engine how to operate on a concrete row type:
 * mint a blank row, read a cell's raw string value, and build a partial-row patch
 * to write a value (+ optional resolved lookup label) into a column.
 */
export interface GridModel<Row extends GridRowBase> {
  newRow: () => Row;
  getCellRaw: (row: Row, col: GridCol) => string;
  buildCellPatch: (row: Row, col: GridCol, value: string, label?: string) => Partial<Row>;
}

const LOOKUP_EDITORS = new Set(['LOOKUP', 'ACCOUNT_PICKER', 'PARTNER_PICKER']);

/** True when a column edits through a search-picker (used to auto-open its search window). */
export function isLookupCol(col: GridCol): boolean {
  const editor = col.cellEditor || (col.dataType === 'LOOKUP' ? 'LOOKUP' : '');
  return LOOKUP_EDITORS.has(editor);
}

/** True when a cell holds a value (ROWNUM is auto → always considered filled). */
export function isCellFilled<Row extends GridRowBase>(
  model: GridModel<Row>,
  row: Row,
  col: GridCol,
): boolean {
  if (col.cellEditor === 'ROWNUM') return true;
  return model.getCellRaw(row, col).trim() !== '';
}

/** Header texts of the `isRequired` columns left empty in a single row. */
export function rowRequiredMissing<Row extends GridRowBase>(
  model: GridModel<Row>,
  row: Row,
  cols: GridCol[],
): string[] {
  return cols.filter((c) => c.isRequired && !isCellFilled(model, row, c)).map((c) => c.headerText);
}

/** Unique header texts of every required column left empty across all rows. */
export function linesRequiredMissing<Row extends GridRowBase>(
  model: GridModel<Row>,
  rows: Row[],
  cols: GridCol[],
): string[] {
  const missing = new Set<string>();
  rows.forEach((r) => rowRequiredMissing(model, r, cols).forEach((h) => missing.add(h)));
  return [...missing];
}

/** True when any column carries a configured default value (Kustomisasi Grid). */
export function colsHaveDefaults(cols: GridCol[]): boolean {
  return cols.some((c) => c.defaultValue != null && c.defaultValue !== '');
}

/**
 * Fold every column's configured `defaultValue` into a freshly minted row.
 * Lookup columns also carry their stored label so the cell reads correctly.
 * Patches are applied sequentially so per-row merges (labels/customFields) stack.
 * Columns without a default are left untouched; returns a new row.
 */
export function applyColumnDefaults<Row extends GridRowBase>(
  model: GridModel<Row>,
  row: Row,
  cols: GridCol[],
): Row {
  let next = row;
  for (const col of cols) {
    const dv = col.defaultValue;
    if (dv == null || dv === '') continue;
    next = { ...next, ...model.buildCellPatch(next, col, dv, col.defaultValueLabel ?? undefined) };
  }
  return next;
}

/** True when no editable, non-skip cell of the row holds a value (a blank/new line). */
export function isRowPristine<Row extends GridRowBase>(
  model: GridModel<Row>,
  row: Row,
  cols: GridCol[],
): boolean {
  return !cols.some((c) => c.isEditable && !c.isSkippable && isCellFilled(model, row, c));
}
