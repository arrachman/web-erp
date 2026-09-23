/**
 * Row model for the cash/bank contra-account grid (Kas Masuk/Keluar/Bank).
 * Row shape + factory + grid-column descriptors + the `GridModel` adapter the
 * generic engine (`useGridNav` / `LineCell`) uses. Columns are config-driven
 * (Kustomisasi Grid): the organism renders whatever visible columns it is given,
 * falling back to a sensible default set when no config is available.
 *
 * The generic grid types/helpers live in `grid-line-core`; this file binds them
 * to the cash/bank row shape (re-exporting the same public API the cash/bank
 * organism + cell already import, so nothing downstream changes).
 */

import {
  GridCol, GridDataType, GridModel, GridRowBase,
  isLookupCol as coreIsLookupCol,
  isCellFilled as coreIsCellFilled,
  rowRequiredMissing as coreRowRequiredMissing,
  linesRequiredMissing as coreLinesRequiredMissing,
} from './grid-line-core';

export type { GridCol, GridDataType } from './grid-line-core';

export interface CashLineRow extends GridRowBase {
  accountId: string;
  accountLabel?: string;
  amount: string;
  amountFx?: string;
  notes?: string;
  costCenterId?: string;
  costCenterLabel?: string;
  divisionId?: string;
  subdivisionId?: string;
  projectId?: string;
}

let seq = 0;
export const newCashLine = (): CashLineRow => ({
  key: `nl-${(seq += 1)}`,
  accountId: '',
  amount: '',
});

const STANDARD_FIELDS = new Set([
  'accountId', 'amount', 'amountFx', 'notes',
  'costCenterId', 'divisionId', 'subdivisionId', 'projectId',
]);

const isStandard = (col: GridCol) => col.kind === 'STANDARD' && STANDARD_FIELDS.has(col.dataField);

/** Read a cell's raw string value from a row for the given column. */
export function getCellRaw(row: CashLineRow, col: GridCol): string {
  if (isStandard(col)) return String((row as unknown as Record<string, unknown>)[col.dataField] ?? '');
  return String(row.customFields?.[col.dataField] ?? '');
}

/** Build the partial-row patch to write `value` (+ optional lookup label) into a column. */
export function buildCellPatch(
  row: CashLineRow,
  col: GridCol,
  value: string,
  label?: string,
): Partial<CashLineRow> {
  if (isStandard(col)) {
    const patch: Partial<CashLineRow> = { [col.dataField]: value } as unknown as Partial<CashLineRow>;
    if (label !== undefined) patch.labels = { ...row.labels, [col.dataField]: label };
    return patch;
  }
  return { customFields: { ...row.customFields, [col.dataField]: value } };
}

/** GridModel adapter binding the generic engine to the cash/bank row shape. */
export const cashBankGridModel: GridModel<CashLineRow> = {
  newRow: newCashLine,
  getCellRaw,
  buildCellPatch,
};

// Convenience wrappers preserving the original (model-free) signatures used by
// the cash/bank organism — logic lives once in grid-line-core.
export const isLookupCol = coreIsLookupCol;
export const isCellFilled = (row: CashLineRow, col: GridCol) => coreIsCellFilled(cashBankGridModel, row, col);
export const rowRequiredMissing = (row: CashLineRow, cols: GridCol[]) =>
  coreRowRequiredMissing(cashBankGridModel, row, cols);
export const linesRequiredMissing = (rows: CashLineRow[], cols: GridCol[]) =>
  coreLinesRequiredMissing(cashBankGridModel, rows, cols);

/** Default columns when no Kustomisasi Grid config is available (mirrors legacy). */
export function defaultGridCols(showFx: boolean): GridCol[] {
  const cols: GridCol[] = [
    { dataField: 'accountId', headerText: 'Akun (No · Nama)', width: 320, dataType: 'LOOKUP', lookupSource: 'account', kind: 'STANDARD', isEditable: true, isRequired: true },
    { dataField: 'amount', headerText: 'Total', width: 160, dataType: 'NUMBER', kind: 'STANDARD', isEditable: true, isRequired: false },
  ];
  if (showFx) {
    cols.push({ dataField: 'amountFx', headerText: 'Total Valas', width: 140, dataType: 'NUMBER', kind: 'STANDARD', isEditable: true, isRequired: false });
  }
  cols.push(
    { dataField: 'notes', headerText: 'Catatan', width: 240, dataType: 'TEXT', kind: 'STANDARD', isEditable: true, isRequired: false },
    { dataField: 'costCenterId', headerText: 'Cost Center', width: 220, dataType: 'LOOKUP', lookupSource: 'costCenter', kind: 'STANDARD', isEditable: true, isRequired: false },
  );
  return cols;
}
