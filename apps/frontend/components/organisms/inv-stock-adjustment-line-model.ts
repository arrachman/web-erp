/**
 * Row model for the inventory stock-adjustment line grid. Quantity-based
 * master-detail lines: Item · Arah · Qty · Satuan · Biaya · Gudang · Catatan.
 * Binds the generic config-driven grid engine (`useGridNav` / `LineCell`,
 * `grid-line-core`) to the `inv_stock_adjustment_lines` shape. Columns are
 * config-driven (Kustomisasi Grid, "INV.SA"); `defaultInvStockAdjustmentCols()`
 * is the fallback until config loads. `inventoryAccountId`/`contraAccountId` are
 * resolved SERVER-SIDE — never edited in the grid.
 */

import {
  GridCol, GridModel, GridRowBase,
} from './grid-line-core';
import type { ErpInvAdjustmentDirection } from '@/lib/api/inv-stock-adjustments';

export interface InvStockAdjustmentLineRow extends GridRowBase {
  itemId: string;
  itemLabel?: string;
  direction: ErpInvAdjustmentDirection | '';
  quantity: string;
  unitId: string;
  unitLabel?: string;
  unitCost?: string;
  warehouseId?: string;
  warehouseLabel?: string;
  inventoryAccountId?: string;
  inventoryAccountLabel?: string;
  contraAccountId?: string;
  contraAccountLabel?: string;
  costCenterId?: string;
  notes?: string;
}

let seq = 0;
export const newInvStockAdjustmentLine = (): InvStockAdjustmentLineRow => ({
  key: `ial-${(seq += 1)}`,
  itemId: '',
  direction: 'INCREASE',
  quantity: '',
  unitId: '',
});

const STANDARD_FIELDS = new Set([
  'itemId', 'direction', 'quantity', 'unitId', 'unitCost',
  'warehouseId', 'inventoryAccountId', 'contraAccountId', 'costCenterId', 'notes',
]);

const isStandard = (col: GridCol) => col.kind === 'STANDARD' && STANDARD_FIELDS.has(col.dataField);

/** Read a cell's raw string value. */
export function getCellRaw(row: InvStockAdjustmentLineRow, col: GridCol): string {
  if (isStandard(col)) return String((row as unknown as Record<string, unknown>)[col.dataField] ?? '');
  return String(row.customFields?.[col.dataField] ?? '');
}

/** Lookup-column dataField → the row property holding its resolved display label. */
const LABEL_KEYS: Record<string, keyof InvStockAdjustmentLineRow> = {
  itemId: 'itemLabel',
  unitId: 'unitLabel',
  warehouseId: 'warehouseLabel',
  inventoryAccountId: 'inventoryAccountLabel',
  contraAccountId: 'contraAccountLabel',
};

/** Build the partial-row patch to write `value` (+ optional lookup label) into a column. */
export function buildCellPatch(
  row: InvStockAdjustmentLineRow,
  col: GridCol,
  value: string,
  label?: string,
): Partial<InvStockAdjustmentLineRow> {
  if (isStandard(col)) {
    const patch: Partial<InvStockAdjustmentLineRow> = { [col.dataField]: value } as unknown as Partial<InvStockAdjustmentLineRow>;
    if (label !== undefined) {
      patch.labels = { ...row.labels, [col.dataField]: label };
      const labelKey = LABEL_KEYS[col.dataField];
      if (labelKey) (patch as Record<string, unknown>)[labelKey] = label;
    }
    return patch;
  }
  return { customFields: { ...row.customFields, [col.dataField]: value } };
}

/** GridModel adapter binding the generic engine to the adjustment line shape. */
export const invStockAdjustmentGridModel: GridModel<InvStockAdjustmentLineRow> = {
  newRow: newInvStockAdjustmentLine,
  getCellRaw,
  buildCellPatch,
};

/** Default adjustment-line columns — fallback before Kustomisasi Grid config loads. */
export function defaultInvStockAdjustmentCols(): GridCol[] {
  return [
    { dataField: 'rowNo', headerText: 'No.', width: 56, dataType: 'NUMBER', kind: 'STANDARD', isEditable: false, isRequired: false, cellEditor: 'ROWNUM' },
    { dataField: 'itemId', headerText: 'Item', width: 300, dataType: 'LOOKUP', lookupSource: 'items', kind: 'STANDARD', isEditable: true, isRequired: true },
    { dataField: 'direction', headerText: 'Arah', width: 130, dataType: 'TEXT', kind: 'STANDARD', isEditable: true, isRequired: true },
    { dataField: 'quantity', headerText: 'Qty', width: 110, dataType: 'NUMBER', kind: 'STANDARD', isEditable: true, isRequired: true, cellEditor: 'STEPPER' },
    { dataField: 'unitId', headerText: 'Satuan', width: 140, dataType: 'LOOKUP', lookupSource: 'units', kind: 'STANDARD', isEditable: true, isRequired: true },
    { dataField: 'unitCost', headerText: 'Biaya', width: 150, dataType: 'NUMBER', kind: 'STANDARD', isEditable: true, isRequired: false, isSkippable: true, cellEditor: 'NUMBER' },
    { dataField: 'warehouseId', headerText: 'Gudang', width: 180, dataType: 'LOOKUP', lookupSource: 'warehouses', kind: 'STANDARD', isEditable: true, isRequired: false },
    { dataField: 'inventoryAccountId', headerText: 'Akun Persediaan', width: 240, dataType: 'LOOKUP', lookupSource: 'accounts', kind: 'STANDARD', isEditable: true, isRequired: false },
    { dataField: 'contraAccountId', headerText: 'Akun Lawan', width: 240, dataType: 'LOOKUP', lookupSource: 'accounts', kind: 'STANDARD', isEditable: true, isRequired: false },
    { dataField: 'notes', headerText: 'Catatan', width: 220, dataType: 'TEXT', kind: 'STANDARD', isEditable: true, isRequired: false },
  ];
}
