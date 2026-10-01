/**
 * Row model for the inventory daily-check line grid. Quantity-based master-
 * detail lines: Item · Qty · Satuan · Gudang · CostCenter · Catatan. Binds the
 * generic config-driven grid engine (`useGridNav` / `LineCell`, `grid-line-core`)
 * to the `inv_daily_check_lines` shape. Columns are config-driven (Kustomisasi
 * Grid, "INV.DC"); `defaultInvDailyCheckCols()` is the fallback until config
 * loads. Atomic tier: Organism (model layer).
 */

import { GridCol, GridModel, GridRowBase } from './grid-line-core';

export interface InvDailyCheckLineRow extends GridRowBase {
  itemId: string;
  itemLabel?: string;
  quantity: string;
  unitId: string;
  unitLabel?: string;
  warehouseId?: string;
  warehouseLabel?: string;
  costCenterId?: string;
  machineRef?: string;
  workHours?: string;
  notes?: string;
}

let seq = 0;
export const newInvDailyCheckLine = (): InvDailyCheckLineRow => ({
  key: `dcl-${(seq += 1)}`,
  itemId: '',
  quantity: '',
  unitId: '',
});

const STANDARD_FIELDS = new Set([
  'itemId', 'quantity', 'unitId', 'warehouseId', 'costCenterId', 'machineRef', 'workHours', 'notes',
]);

const isStandard = (col: GridCol) =>
  col.kind === 'STANDARD' && STANDARD_FIELDS.has(col.dataField);

/** Read a cell's raw string value. */
export function getCellRaw(row: InvDailyCheckLineRow, col: GridCol): string {
  if (isStandard(col)) return String((row as unknown as Record<string, unknown>)[col.dataField] ?? '');
  return String(row.customFields?.[col.dataField] ?? '');
}

/** Lookup-column dataField → the row property holding its resolved display label. */
const LABEL_KEYS: Record<string, keyof InvDailyCheckLineRow> = {
  itemId: 'itemLabel',
  unitId: 'unitLabel',
  warehouseId: 'warehouseLabel',
};

/** Build the partial-row patch to write `value` (+ optional lookup label) into a column. */
export function buildCellPatch(
  row: InvDailyCheckLineRow,
  col: GridCol,
  value: string,
  label?: string,
): Partial<InvDailyCheckLineRow> {
  if (isStandard(col)) {
    const patch: Partial<InvDailyCheckLineRow> = {
      [col.dataField]: value,
    } as unknown as Partial<InvDailyCheckLineRow>;
    if (label !== undefined) {
      patch.labels = { ...row.labels, [col.dataField]: label };
      const labelKey = LABEL_KEYS[col.dataField];
      if (labelKey) (patch as Record<string, unknown>)[labelKey] = label;
    }
    return patch;
  }
  return { customFields: { ...row.customFields, [col.dataField]: value } };
}

/** GridModel adapter binding the generic engine to the daily-check line shape. */
export const invDailyCheckGridModel: GridModel<InvDailyCheckLineRow> = {
  newRow: newInvDailyCheckLine,
  getCellRaw,
  buildCellPatch,
};

/** Default daily-check line columns — fallback before Kustomisasi Grid config loads. */
export function defaultInvDailyCheckCols(): GridCol[] {
  return [
    { dataField: 'rowNo', headerText: 'No.', width: 56, dataType: 'NUMBER', kind: 'STANDARD', isEditable: false, isRequired: false, cellEditor: 'ROWNUM' },
    { dataField: 'itemId', headerText: 'Item', width: 300, dataType: 'LOOKUP', lookupSource: 'items', kind: 'STANDARD', isEditable: true, isRequired: true },
    { dataField: 'quantity', headerText: 'Qty', width: 110, dataType: 'NUMBER', kind: 'STANDARD', isEditable: true, isRequired: true, cellEditor: 'STEPPER' },
    { dataField: 'unitId', headerText: 'Satuan', width: 140, dataType: 'LOOKUP', lookupSource: 'units', kind: 'STANDARD', isEditable: true, isRequired: true },
    { dataField: 'warehouseId', headerText: 'Gudang', width: 180, dataType: 'LOOKUP', lookupSource: 'warehouses', kind: 'STANDARD', isEditable: true, isRequired: false },
    { dataField: 'machineRef', headerText: 'Mesin', width: 180, dataType: 'TEXT', kind: 'STANDARD', isEditable: true, isRequired: false },
    { dataField: 'workHours', headerText: 'Jam Kerja', width: 120, dataType: 'NUMBER', kind: 'STANDARD', isEditable: true, isRequired: false },
    { dataField: 'notes', headerText: 'Catatan', width: 220, dataType: 'TEXT', kind: 'STANDARD', isEditable: true, isRequired: false },
  ];
}
