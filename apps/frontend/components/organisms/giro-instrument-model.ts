/**
 * Row model for the giro-register instrument grid (RG/SG — kind=REGISTER).
 * Each row is one physical giro instrument: No Giro · Bank · Jatuh Tempo ·
 * Nominal · Catatan. Binds the generic config-driven grid engine (`useGridNav` /
 * `LineCell`, `grid-line-core`) to the `fin_giro_instruments` shape. Columns are
 * config-driven (Kustomisasi Grid, e.g. "FIN.RG"); `defaultGiroCols()` is the
 * fallback until config loads.
 */

import { GridCol, GridModel, GridRowBase } from './grid-line-core';

export interface GiroInstrumentRow extends GridRowBase {
  giroNumber: string;
  bankName?: string;
  dueDate: string;
  amount: string;
  notes?: string;
  giroAccountId?: string;
  giroAccountLabel?: string;
}

let seq = 0;
export const newGiroInstrument = (): GiroInstrumentRow => ({
  key: `gi-${(seq += 1)}`,
  giroNumber: '',
  dueDate: '',
  amount: '',
});

/** Σ amount across all instrument rows. */
export function computeGiroTotal(rows: GiroInstrumentRow[]): number {
  return rows.reduce((s, r) => s + Number(r.amount || 0), 0);
}

const STANDARD_FIELDS = new Set([
  'giroNumber', 'bankName', 'dueDate', 'amount', 'notes', 'giroAccountId',
]);

const isStandard = (col: GridCol) => col.kind === 'STANDARD' && STANDARD_FIELDS.has(col.dataField);

/** Read a cell's raw string value. */
export function getCellRaw(row: GiroInstrumentRow, col: GridCol): string {
  if (isStandard(col)) return String((row as unknown as Record<string, unknown>)[col.dataField] ?? '');
  return String(row.customFields?.[col.dataField] ?? '');
}

/** Lookup-column dataField → the row property holding its resolved display label. */
const LABEL_KEYS: Record<string, keyof GiroInstrumentRow> = {
  giroAccountId: 'giroAccountLabel',
};

/** Build the partial-row patch to write `value` (+ optional lookup label) into a column. */
export function buildCellPatch(
  row: GiroInstrumentRow,
  col: GridCol,
  value: string,
  label?: string,
): Partial<GiroInstrumentRow> {
  if (isStandard(col)) {
    const patch: Partial<GiroInstrumentRow> = { [col.dataField]: value } as unknown as Partial<GiroInstrumentRow>;
    if (label !== undefined) {
      patch.labels = { ...row.labels, [col.dataField]: label };
      const labelKey = LABEL_KEYS[col.dataField];
      if (labelKey) (patch as Record<string, unknown>)[labelKey] = label;
    }
    return patch;
  }
  return { customFields: { ...row.customFields, [col.dataField]: value } };
}

/** GridModel adapter binding the generic engine to the giro-instrument shape. */
export const giroGridModel: GridModel<GiroInstrumentRow> = {
  newRow: newGiroInstrument,
  getCellRaw,
  buildCellPatch,
};

/** Default visible instrument columns — mirrors the FIN.RG seed (fallback before config). */
export function defaultGiroCols(): GridCol[] {
  return [
    { dataField: 'rowNo', headerText: 'No.', width: 56, dataType: 'NUMBER', kind: 'STANDARD', isEditable: false, isRequired: false, cellEditor: 'ROWNUM' },
    { dataField: 'giroNumber', headerText: 'No Giro', width: 200, dataType: 'TEXT', kind: 'STANDARD', isEditable: true, isRequired: true },
    { dataField: 'bankName', headerText: 'Bank', width: 220, dataType: 'TEXT', kind: 'STANDARD', isEditable: true, isRequired: false },
    { dataField: 'dueDate', headerText: 'Jatuh Tempo', width: 160, dataType: 'DATE', kind: 'STANDARD', isEditable: true, isRequired: false, cellEditor: 'DATE' },
    { dataField: 'amount', headerText: 'Nominal', width: 180, dataType: 'NUMBER', kind: 'STANDARD', isEditable: true, isRequired: false, cellEditor: 'NUMBER' },
    { dataField: 'notes', headerText: 'Catatan', width: 240, dataType: 'TEXT', kind: 'STANDARD', isEditable: true, isRequired: false },
  ];
}
