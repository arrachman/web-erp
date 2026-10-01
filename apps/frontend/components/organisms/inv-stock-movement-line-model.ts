/**
 * Row model for the inventory stock-movement line grid (Material Request /
 * Stock Transfer / Transfer Receipt / Fuel Refill). Quantity-based master-detail
 * lines: Item · Qty · Satuan · Gudang Asal · Gudang Tujuan · (Biaya) · Catatan.
 * Binds the generic config-driven grid engine (`useGridNav` / `LineCell`,
 * `grid-line-core`) to the `inv_stock_movement_lines` shape. Columns are
 * config-driven (Kustomisasi Grid, e.g. "INV.MR"); `defaultInvStockMovementCols()`
 * is the fallback until config loads. Unlike sales: NO price/discount/tax/total.
 */

import {
  GridCol, GridModel, GridRowBase,
} from './grid-line-core';

export interface InvStockMovementLineRow extends GridRowBase {
  itemId: string;
  itemLabel?: string;
  quantity: string;
  unitId: string;
  unitLabel?: string;
  unitCost?: string;
  sourceWarehouseId?: string;
  sourceWarehouseLabel?: string;
  destinationWarehouseId?: string;
  destinationWarehouseLabel?: string;
  costCenterId?: string;
  notes?: string;
}

let seq = 0;
export const newInvStockMovementLine = (): InvStockMovementLineRow => ({
  key: `iml-${(seq += 1)}`,
  itemId: '',
  quantity: '',
  unitId: '',
});

const STANDARD_FIELDS = new Set([
  'itemId', 'quantity', 'unitId', 'unitCost',
  'sourceWarehouseId', 'destinationWarehouseId', 'costCenterId', 'notes',
]);

const isStandard = (col: GridCol) => col.kind === 'STANDARD' && STANDARD_FIELDS.has(col.dataField);

/** Read a cell's raw string value. */
export function getCellRaw(row: InvStockMovementLineRow, col: GridCol): string {
  if (isStandard(col)) return String((row as unknown as Record<string, unknown>)[col.dataField] ?? '');
  return String(row.customFields?.[col.dataField] ?? '');
}

/** Lookup-column dataField → the row property holding its resolved display label. */
const LABEL_KEYS: Record<string, keyof InvStockMovementLineRow> = {
  itemId: 'itemLabel',
  unitId: 'unitLabel',
  sourceWarehouseId: 'sourceWarehouseLabel',
  destinationWarehouseId: 'destinationWarehouseLabel',
};

/** Build the partial-row patch to write `value` (+ optional lookup label) into a column. */
export function buildCellPatch(
  row: InvStockMovementLineRow,
  col: GridCol,
  value: string,
  label?: string,
): Partial<InvStockMovementLineRow> {
  if (isStandard(col)) {
    const patch: Partial<InvStockMovementLineRow> = { [col.dataField]: value } as unknown as Partial<InvStockMovementLineRow>;
    if (label !== undefined) {
      patch.labels = { ...row.labels, [col.dataField]: label };
      const labelKey = LABEL_KEYS[col.dataField];
      if (labelKey) (patch as Record<string, unknown>)[labelKey] = label;
    }
    return patch;
  }
  return { customFields: { ...row.customFields, [col.dataField]: value } };
}

/** GridModel adapter binding the generic engine to the inventory line shape. */
export const invStockMovementGridModel: GridModel<InvStockMovementLineRow> = {
  newRow: newInvStockMovementLine,
  getCellRaw,
  buildCellPatch,
};

/** Default movement-line columns — fallback before Kustomisasi Grid config loads. */
export function defaultInvStockMovementCols(): GridCol[] {
  return [
    { dataField: 'rowNo', headerText: 'No.', width: 56, dataType: 'NUMBER', kind: 'STANDARD', isEditable: false, isRequired: false, cellEditor: 'ROWNUM' },
    { dataField: 'itemId', headerText: 'Item', width: 300, dataType: 'LOOKUP', lookupSource: 'items', kind: 'STANDARD', isEditable: true, isRequired: true },
    { dataField: 'quantity', headerText: 'Qty', width: 110, dataType: 'NUMBER', kind: 'STANDARD', isEditable: true, isRequired: true, cellEditor: 'STEPPER' },
    { dataField: 'unitId', headerText: 'Satuan', width: 140, dataType: 'LOOKUP', lookupSource: 'units', kind: 'STANDARD', isEditable: true, isRequired: true },
    { dataField: 'sourceWarehouseId', headerText: 'Gudang Asal', width: 180, dataType: 'LOOKUP', lookupSource: 'warehouses', kind: 'STANDARD', isEditable: true, isRequired: false },
    { dataField: 'destinationWarehouseId', headerText: 'Gudang Tujuan', width: 180, dataType: 'LOOKUP', lookupSource: 'warehouses', kind: 'STANDARD', isEditable: true, isRequired: false },
    { dataField: 'unitCost', headerText: 'Biaya', width: 150, dataType: 'NUMBER', kind: 'STANDARD', isEditable: true, isRequired: false, isSkippable: true, cellEditor: 'NUMBER' },
    { dataField: 'notes', headerText: 'Catatan', width: 220, dataType: 'TEXT', kind: 'STANDARD', isEditable: true, isRequired: false },
  ];
}
