/**
 * Row model for the inventory stock-count line grid (opname). Master-detail
 * lines: Item · Satuan · Gudang · Qty Sistem · Qty Fisik · Qty Baik · Qty Rusak ·
 * Selisih · Catatan. Binds the generic config-driven grid engine (`useGridNav` /
 * `LineCell`, `grid-line-core`) to the `inv_stock_count_lines` shape. Columns are
 * config-driven (Kustomisasi Grid, e.g. "INV.SP"); `defaultInvStockCountCols()`
 * is the fallback until config loads. Variance (Selisih) is read-only — derived
 * server-side from physical − system.
 */

import {
  GridCol, GridModel, GridRowBase,
} from './grid-line-core';

export interface InvStockCountLineRow extends GridRowBase {
  itemId: string;
  itemLabel?: string;
  unitId: string;
  unitLabel?: string;
  warehouseId?: string;
  warehouseLabel?: string;
  systemQty?: string;
  physicalQty: string;
  goodQty?: string;
  damagedQty?: string;
  varianceQty?: string;
  costCenterId?: string;
  notes?: string;
}

let seq = 0;
export const newInvStockCountLine = (): InvStockCountLineRow => ({
  key: `icl-${(seq += 1)}`,
  itemId: '',
  unitId: '',
  physicalQty: '',
});

const STANDARD_FIELDS = new Set([
  'itemId', 'unitId', 'warehouseId', 'systemQty', 'physicalQty',
  'goodQty', 'damagedQty', 'varianceQty', 'costCenterId', 'notes',
]);

const isStandard = (col: GridCol) => col.kind === 'STANDARD' && STANDARD_FIELDS.has(col.dataField);

/** Read a cell's raw string value. */
export function getCellRaw(row: InvStockCountLineRow, col: GridCol): string {
  if (isStandard(col)) return String((row as unknown as Record<string, unknown>)[col.dataField] ?? '');
  return String(row.customFields?.[col.dataField] ?? '');
}

/** Lookup-column dataField → the row property holding its resolved display label. */
const LABEL_KEYS: Record<string, keyof InvStockCountLineRow> = {
  itemId: 'itemLabel',
  unitId: 'unitLabel',
  warehouseId: 'warehouseLabel',
};

/** Build the partial-row patch to write `value` (+ optional lookup label) into a column. */
export function buildCellPatch(
  row: InvStockCountLineRow,
  col: GridCol,
  value: string,
  label?: string,
): Partial<InvStockCountLineRow> {
  if (isStandard(col)) {
    const patch: Partial<InvStockCountLineRow> = { [col.dataField]: value } as unknown as Partial<InvStockCountLineRow>;
    if (label !== undefined) {
      patch.labels = { ...row.labels, [col.dataField]: label };
      const labelKey = LABEL_KEYS[col.dataField];
      if (labelKey) (patch as Record<string, unknown>)[labelKey] = label;
    }
    return patch;
  }
  return { customFields: { ...row.customFields, [col.dataField]: value } };
}

/** GridModel adapter binding the generic engine to the stock-count line shape. */
export const invStockCountGridModel: GridModel<InvStockCountLineRow> = {
  newRow: newInvStockCountLine,
  getCellRaw,
  buildCellPatch,
};

/** Default count-line columns — fallback before Kustomisasi Grid config loads. */
export function defaultInvStockCountCols(): GridCol[] {
  return [
    { dataField: 'rowNo', headerText: 'No.', width: 56, dataType: 'NUMBER', kind: 'STANDARD', isEditable: false, isRequired: false, cellEditor: 'ROWNUM' },
    { dataField: 'itemId', headerText: 'Item', width: 300, dataType: 'LOOKUP', lookupSource: 'items', kind: 'STANDARD', isEditable: true, isRequired: true },
    { dataField: 'unitId', headerText: 'Satuan', width: 140, dataType: 'LOOKUP', lookupSource: 'units', kind: 'STANDARD', isEditable: true, isRequired: true },
    { dataField: 'warehouseId', headerText: 'Gudang', width: 180, dataType: 'LOOKUP', lookupSource: 'warehouses', kind: 'STANDARD', isEditable: true, isRequired: false },
    { dataField: 'systemQty', headerText: 'Qty Sistem', width: 120, dataType: 'NUMBER', kind: 'STANDARD', isEditable: true, isRequired: false, cellEditor: 'NUMBER' },
    { dataField: 'physicalQty', headerText: 'Qty Fisik', width: 120, dataType: 'NUMBER', kind: 'STANDARD', isEditable: true, isRequired: true, cellEditor: 'STEPPER' },
    { dataField: 'goodQty', headerText: 'Qty Baik', width: 120, dataType: 'NUMBER', kind: 'STANDARD', isEditable: true, isRequired: false, cellEditor: 'NUMBER' },
    { dataField: 'damagedQty', headerText: 'Qty Rusak', width: 120, dataType: 'NUMBER', kind: 'STANDARD', isEditable: true, isRequired: false, cellEditor: 'NUMBER' },
    { dataField: 'varianceQty', headerText: 'Selisih', width: 120, dataType: 'NUMBER', kind: 'STANDARD', isEditable: false, isRequired: false, cellEditor: 'NUMBER' },
    { dataField: 'notes', headerText: 'Catatan', width: 220, dataType: 'TEXT', kind: 'STANDARD', isEditable: true, isRequired: false },
  ];
}
