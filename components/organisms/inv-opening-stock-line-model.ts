/**
 * Row model for the inventory opening-stock line grid (Saldo Awal Persediaan).
 * Value-based master-detail lines: Item · Qty · Satuan · Biaya/Unit · Gudang ·
 * Catatan. Binds the generic config-driven grid engine (`useGridNav` / `LineCell`,
 * `grid-line-core`) to the `inv_opening_stock_lines` shape. Columns are
 * config-driven (Kustomisasi Grid, e.g. "INV.IB"); `defaultInvOpeningStockCols()`
 * is the fallback until config loads. Unlike movements: unitCost is REQUIRED
 * (opening balance carries value); the inventory GL account is resolved
 * server-side and is NOT a grid column.
 */

import {
  GridCol, GridModel, GridRowBase,
} from './grid-line-core';

export interface InvOpeningStockLineRow extends GridRowBase {
  itemId: string;
  itemLabel?: string;
  quantity: string;
  unitId: string;
  unitLabel?: string;
  unitCost: string;
  warehouseId?: string;
  warehouseLabel?: string;
  inventoryAccountId?: string;
  inventoryAccountLabel?: string;
  costCenterId?: string;
  notes?: string;
}

let seq = 0;
export const newInvOpeningStockLine = (): InvOpeningStockLineRow => ({
  key: `iob-${(seq += 1)}`,
  itemId: '',
  quantity: '',
  unitId: '',
  unitCost: '',
});

const STANDARD_FIELDS = new Set([
  'itemId', 'quantity', 'unitId', 'unitCost',
  'warehouseId', 'inventoryAccountId', 'costCenterId', 'notes',
]);

const isStandard = (col: GridCol) => col.kind === 'STANDARD' && STANDARD_FIELDS.has(col.dataField);

/** Read a cell's raw string value. */
export function getCellRaw(row: InvOpeningStockLineRow, col: GridCol): string {
  if (isStandard(col)) return String((row as unknown as Record<string, unknown>)[col.dataField] ?? '');
  return String(row.customFields?.[col.dataField] ?? '');
}

/** Lookup-column dataField → the row property holding its resolved display label. */
const LABEL_KEYS: Record<string, keyof InvOpeningStockLineRow> = {
  itemId: 'itemLabel',
  unitId: 'unitLabel',
  warehouseId: 'warehouseLabel',
  inventoryAccountId: 'inventoryAccountLabel',
};

/** Build the partial-row patch to write `value` (+ optional lookup label) into a column. */
export function buildCellPatch(
  row: InvOpeningStockLineRow,
  col: GridCol,
  value: string,
  label?: string,
): Partial<InvOpeningStockLineRow> {
  if (isStandard(col)) {
    const patch: Partial<InvOpeningStockLineRow> = { [col.dataField]: value } as unknown as Partial<InvOpeningStockLineRow>;
    if (label !== undefined) {
      patch.labels = { ...row.labels, [col.dataField]: label };
      const labelKey = LABEL_KEYS[col.dataField];
      if (labelKey) (patch as Record<string, unknown>)[labelKey] = label;
    }
    return patch;
  }
  return { customFields: { ...row.customFields, [col.dataField]: value } };
}

/** GridModel adapter binding the generic engine to the opening-stock line shape. */
export const invOpeningStockGridModel: GridModel<InvOpeningStockLineRow> = {
  newRow: newInvOpeningStockLine,
  getCellRaw,
  buildCellPatch,
};

/** Default opening-stock-line columns — fallback before Kustomisasi Grid config loads. */
export function defaultInvOpeningStockCols(): GridCol[] {
  return [
    { dataField: 'rowNo', headerText: 'No.', width: 56, dataType: 'NUMBER', kind: 'STANDARD', isEditable: false, isRequired: false, cellEditor: 'ROWNUM' },
    { dataField: 'itemId', headerText: 'Item', width: 300, dataType: 'LOOKUP', lookupSource: 'items', kind: 'STANDARD', isEditable: true, isRequired: true },
    { dataField: 'quantity', headerText: 'Qty', width: 110, dataType: 'NUMBER', kind: 'STANDARD', isEditable: true, isRequired: true, cellEditor: 'STEPPER' },
    { dataField: 'unitId', headerText: 'Satuan', width: 140, dataType: 'LOOKUP', lookupSource: 'units', kind: 'STANDARD', isEditable: true, isRequired: true },
    { dataField: 'unitCost', headerText: 'Biaya/Unit', width: 150, dataType: 'NUMBER', kind: 'STANDARD', isEditable: true, isRequired: true, cellEditor: 'NUMBER' },
    { dataField: 'warehouseId', headerText: 'Gudang', width: 180, dataType: 'LOOKUP', lookupSource: 'warehouses', kind: 'STANDARD', isEditable: true, isRequired: false },
    { dataField: 'inventoryAccountId', headerText: 'Akun Persediaan', width: 240, dataType: 'LOOKUP', lookupSource: 'accounts', kind: 'STANDARD', isEditable: true, isRequired: false },
    { dataField: 'notes', headerText: 'Catatan', width: 220, dataType: 'TEXT', kind: 'STANDARD', isEditable: true, isRequired: false },
  ];
}
