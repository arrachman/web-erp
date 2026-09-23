/**
 * Row model for the sales item-line grid (Sales Order/Invoice/DO…). Item-based
 * master-detail lines: Item · Qty · Satuan · Harga · Disc · Pajak · Total.
 * Binds the generic config-driven grid engine (`useGridNav` / `LineCell`,
 * `grid-line-core`) to the `sls_*_lines` shape. Columns are config-driven
 * (Kustomisasi Grid, transaction code e.g. "SLS.SO"); `defaultSlsItemCols()` is
 * the fallback until config loads.
 */

import {
  GridCol, GridModel, GridRowBase,
} from './grid-line-core';

export interface SlsItemLineRow extends GridRowBase {
  itemId: string;
  itemLabel?: string;
  quantity: string;
  unitId: string;
  unitLabel?: string;
  unitPrice: string;
  discountPercent?: string;
  discountAmount?: string;
  tax1Id?: string;
  tax1Label?: string;
  tax2Id?: string;
  tax2Label?: string;
  warehouseId?: string;
  warehouseLabel?: string;
  notes?: string;
  costCenterId?: string;
  divisionId?: string;
  subdivisionId?: string;
  projectId?: string;
}

let seq = 0;
export const newSlsItemLine = (): SlsItemLineRow => ({
  key: `sl-${(seq += 1)}`,
  itemId: '',
  quantity: '',
  unitId: '',
  unitPrice: '',
});

/** Net line value: qty × price − discount (explicit amount wins over percent). */
export function computeLineTotal(row: SlsItemLineRow): number {
  const qty = Number(row.quantity || 0);
  const price = Number(row.unitPrice || 0);
  const gross = qty * price;
  const disc = row.discountAmount
    ? Number(row.discountAmount)
    : row.discountPercent
      ? (gross * Number(row.discountPercent)) / 100
      : 0;
  return gross - disc;
}

const STANDARD_FIELDS = new Set([
  'itemId', 'quantity', 'unitId', 'unitPrice', 'discountPercent', 'discountAmount',
  'tax1Id', 'tax2Id', 'warehouseId', 'notes',
  'costCenterId', 'divisionId', 'subdivisionId', 'projectId',
]);

const isStandard = (col: GridCol) => col.kind === 'STANDARD' && STANDARD_FIELDS.has(col.dataField);

/** Read a cell's raw string value. `lineTotal` is derived (qty×price−disc), read-only. */
export function getCellRaw(row: SlsItemLineRow, col: GridCol): string {
  if (col.dataField === 'lineTotal') return String(computeLineTotal(row));
  if (isStandard(col)) return String((row as unknown as Record<string, unknown>)[col.dataField] ?? '');
  return String(row.customFields?.[col.dataField] ?? '');
}

/** Lookup-column dataField → the row property holding its resolved display label. */
const LABEL_KEYS: Record<string, keyof SlsItemLineRow> = {
  itemId: 'itemLabel',
  unitId: 'unitLabel',
  tax1Id: 'tax1Label',
  tax2Id: 'tax2Label',
  warehouseId: 'warehouseLabel',
};

/** Build the partial-row patch to write `value` (+ optional lookup label) into a column. */
export function buildCellPatch(
  row: SlsItemLineRow,
  col: GridCol,
  value: string,
  label?: string,
): Partial<SlsItemLineRow> {
  // lineTotal is derived — never written.
  if (col.dataField === 'lineTotal') return {};
  if (isStandard(col)) {
    const patch: Partial<SlsItemLineRow> = { [col.dataField]: value } as unknown as Partial<SlsItemLineRow>;
    if (label !== undefined) {
      patch.labels = { ...row.labels, [col.dataField]: label };
      const labelKey = LABEL_KEYS[col.dataField];
      if (labelKey) (patch as Record<string, unknown>)[labelKey] = label;
    }
    return patch;
  }
  return { customFields: { ...row.customFields, [col.dataField]: value } };
}

/** GridModel adapter binding the generic engine to the sales item-line shape. */
export const slsItemGridModel: GridModel<SlsItemLineRow> = {
  newRow: newSlsItemLine,
  getCellRaw,
  buildCellPatch,
};

/** Default item-line columns — mirrors the SLS_ITEM_COLUMNS seed (fallback before config loads). */
export function defaultSlsItemCols(): GridCol[] {
  return [
    { dataField: 'rowNo', headerText: 'No.', width: 56, dataType: 'NUMBER', kind: 'STANDARD', isEditable: false, isRequired: false, cellEditor: 'ROWNUM' },
    { dataField: 'itemId', headerText: 'Item', width: 300, dataType: 'LOOKUP', lookupSource: 'items', kind: 'STANDARD', isEditable: true, isRequired: true },
    { dataField: 'quantity', headerText: 'Qty', width: 110, dataType: 'NUMBER', kind: 'STANDARD', isEditable: true, isRequired: true, cellEditor: 'STEPPER' },
    { dataField: 'unitId', headerText: 'Satuan', width: 140, dataType: 'LOOKUP', lookupSource: 'units', kind: 'STANDARD', isEditable: true, isRequired: true },
    { dataField: 'unitPrice', headerText: 'Harga', width: 150, dataType: 'NUMBER', kind: 'STANDARD', isEditable: true, isRequired: false, cellEditor: 'NUMBER' },
    { dataField: 'discountPercent', headerText: 'Disc %', width: 100, dataType: 'NUMBER', kind: 'STANDARD', isEditable: true, isRequired: false, cellEditor: 'DISCOUNT' },
    { dataField: 'tax1Id', headerText: 'Pajak', width: 150, dataType: 'LOOKUP', lookupSource: 'taxes', kind: 'STANDARD', isEditable: true, isRequired: false },
    { dataField: 'lineTotal', headerText: 'Total', width: 160, dataType: 'NUMBER', kind: 'STANDARD', isEditable: false, isRequired: false, isSkippable: true, cellEditor: 'NUMBER' },
    { dataField: 'notes', headerText: 'Catatan', width: 220, dataType: 'TEXT', kind: 'STANDARD', isEditable: true, isRequired: false },
  ];
}
