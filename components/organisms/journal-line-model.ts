/**
 * Row model for the journal-entry line grid (General/Adjustment/Memorial/
 * Opening-Balance/Revaluation). Double-entry lines: Akun · Debit · Kredit ·
 * dimensions. Binds the generic config-driven grid engine (`useGridNav` /
 * `LineCell`, `grid-line-core`) to the `fin_journal_lines` shape. Columns are
 * config-driven (Kustomisasi Grid, e.g. "FIN.GJ"); `defaultJournalCols()` is the
 * fallback until config loads.
 */

import { GridCol, GridModel, GridRowBase } from './grid-line-core';

export interface JournalLineRow extends GridRowBase {
  accountId: string;
  accountLabel?: string;
  debit: string;
  credit: string;
  notes?: string;
  costCenterId?: string;
  costCenterLabel?: string;
  divisionId?: string;
  divisionLabel?: string;
  subdivisionId?: string;
  subdivisionLabel?: string;
  projectId?: string;
  projectLabel?: string;
}

let seq = 0;
export const newJournalLine = (): JournalLineRow => ({
  key: `jl-${(seq += 1)}`,
  accountId: '',
  debit: '',
  credit: '',
});

/** Σ debit, Σ credit, and whether the entry is balanced (and non-zero). */
export function computeJournalTotals(rows: JournalLineRow[]): {
  debit: number;
  credit: number;
  balanced: boolean;
} {
  const debit = rows.reduce((s, l) => s + Number(l.debit || 0), 0);
  const credit = rows.reduce((s, l) => s + Number(l.credit || 0), 0);
  return { debit, credit, balanced: debit === credit && debit > 0 };
}

const STANDARD_FIELDS = new Set([
  'accountId', 'debit', 'credit', 'notes',
  'costCenterId', 'divisionId', 'subdivisionId', 'projectId',
]);

const isStandard = (col: GridCol) => col.kind === 'STANDARD' && STANDARD_FIELDS.has(col.dataField);

/** Read a cell's raw string value. */
export function getCellRaw(row: JournalLineRow, col: GridCol): string {
  if (isStandard(col)) return String((row as unknown as Record<string, unknown>)[col.dataField] ?? '');
  return String(row.customFields?.[col.dataField] ?? '');
}

/** Lookup-column dataField → the row property holding its resolved display label. */
const LABEL_KEYS: Record<string, keyof JournalLineRow> = {
  accountId: 'accountLabel',
  costCenterId: 'costCenterLabel',
  divisionId: 'divisionLabel',
  subdivisionId: 'subdivisionLabel',
  projectId: 'projectLabel',
};

/** Build the partial-row patch to write `value` (+ optional lookup label) into a column. */
export function buildCellPatch(
  row: JournalLineRow,
  col: GridCol,
  value: string,
  label?: string,
): Partial<JournalLineRow> {
  if (isStandard(col)) {
    const patch: Partial<JournalLineRow> = { [col.dataField]: value } as unknown as Partial<JournalLineRow>;
    if (label !== undefined) {
      patch.labels = { ...row.labels, [col.dataField]: label };
      const labelKey = LABEL_KEYS[col.dataField];
      if (labelKey) (patch as Record<string, unknown>)[labelKey] = label;
    }
    return patch;
  }
  return { customFields: { ...row.customFields, [col.dataField]: value } };
}

/** GridModel adapter binding the generic engine to the journal-line shape. */
export const journalGridModel: GridModel<JournalLineRow> = {
  newRow: newJournalLine,
  getCellRaw,
  buildCellPatch,
};

/** Default visible journal-line columns — mirrors the FIN.GJ seed (fallback before config). */
export function defaultJournalCols(): GridCol[] {
  return [
    { dataField: 'rowNo', headerText: 'No.', width: 56, dataType: 'NUMBER', kind: 'STANDARD', isEditable: false, isRequired: false, cellEditor: 'ROWNUM' },
    { dataField: 'accountId', headerText: 'Akun', width: 320, dataType: 'LOOKUP', lookupSource: 'accounts', kind: 'STANDARD', isEditable: true, isRequired: true },
    { dataField: 'debit', headerText: 'Debit', width: 160, dataType: 'NUMBER', kind: 'STANDARD', isEditable: true, isRequired: false, cellEditor: 'NUMBER' },
    { dataField: 'credit', headerText: 'Kredit', width: 160, dataType: 'NUMBER', kind: 'STANDARD', isEditable: true, isRequired: false, cellEditor: 'NUMBER' },
    { dataField: 'notes', headerText: 'Catatan', width: 240, dataType: 'TEXT', kind: 'STANDARD', isEditable: true, isRequired: false },
    { dataField: 'costCenterId', headerText: 'Cost Center', width: 220, dataType: 'LOOKUP', lookupSource: 'cost-centers', kind: 'STANDARD', isEditable: true, isRequired: false },
  ];
}
