// Kustomisasi Grid API — transaction-type catalog + per-transaction grids (tabs)
// + columns. Endpoints: /transaction-grids/types, /:code/columns (GET, compat),
// /:code/grids (GET + PUT).
// NOTE: BASE_URL already ends in /api/erp — paths here must NOT repeat the `erp` segment.

import { apiGet, apiPut } from './client';

export type GridColumnKind = 'STANDARD' | 'CUSTOM';
export type GridDataType = 'TEXT' | 'NUMBER' | 'DATE' | 'LOOKUP';

// Presentation/edit slots — fixed allowlists mirrored by the backend DTO.
export const LABEL_FORMATTERS = ['NONE', 'NUMBER', 'DECIMAL', 'CURRENCY', 'PERCENT', 'DATE', 'DATETIME', 'BOOLEAN'] as const;
export const HEADER_RENDERERS = ['DEFAULT', 'REQUIRED', 'CENTER', 'WRAP', 'HELP'] as const;
export const CELL_RENDERERS = ['TEXT', 'NUMERIC', 'CURRENCY', 'BADGE', 'CHECK', 'LINK', 'LOOKUP'] as const;
export const CELL_EDITORS = [
  'TEXT', 'NUMBER', 'DATE', 'LOOKUP', 'TEXTAREA', 'CHECKBOX', 'NONE',
  'DISCOUNT', 'STEPPER', 'COMBOBOX', 'ACCOUNT_PICKER', 'PARTNER_PICKER',
  'ROWNUM',
] as const;

export type LabelFormatter = (typeof LABEL_FORMATTERS)[number];
export type HeaderRenderer = (typeof HEADER_RENDERERS)[number];
export type CellRenderer = (typeof CELL_RENDERERS)[number];
export type CellEditor = (typeof CELL_EDITORS)[number];

// Semantic column type — single selector that derives all 4 presentation slots.
export const COLUMN_TYPES = [
  'text', 'number', 'rownum', 'currency', 'decimal', 'percent',
  'date', 'datetime', 'checkbox', 'lookup', 'textarea',
  'badge', 'link', 'discount', 'stepper', 'combobox',
  'account_picker', 'partner_picker',
] as const;
export type ColumnType = (typeof COLUMN_TYPES)[number];

export const COLUMN_TYPE_LABELS: Record<ColumnType, string> = {
  text: 'Text',
  number: 'Number',
  rownum: 'Nomor Urut',
  currency: 'Currency',
  decimal: 'Decimal',
  percent: 'Percent',
  date: 'Date',
  datetime: 'Datetime',
  checkbox: 'Checkbox',
  lookup: 'Lookup Kustom',
  textarea: 'Textarea',
  badge: 'Badge (read-only)',
  link: 'Link (read-only)',
  discount: 'Input Discount',
  stepper: 'Numeric Stepper',
  combobox: 'Combo Box',
  account_picker: 'Account Picker',
  partner_picker: 'Partner Picker',
};

// Maps a ColumnType to all 4 derived slot values.
export interface ColumnTypePreset {
  labelFormatter: LabelFormatter;
  headerRenderer: HeaderRenderer;
  cellRenderer: CellRenderer;
  cellEditor: CellEditor;
}

export const COLUMN_TYPE_PRESETS: Record<ColumnType, ColumnTypePreset> = {
  text:           { labelFormatter: 'NONE',     headerRenderer: 'DEFAULT', cellRenderer: 'TEXT',    cellEditor: 'TEXT'           },
  number:         { labelFormatter: 'NUMBER',   headerRenderer: 'DEFAULT', cellRenderer: 'NUMERIC', cellEditor: 'NUMBER'         },
  rownum:         { labelFormatter: 'NUMBER',   headerRenderer: 'CENTER',  cellRenderer: 'NUMERIC', cellEditor: 'ROWNUM'         },
  currency:       { labelFormatter: 'CURRENCY', headerRenderer: 'DEFAULT', cellRenderer: 'CURRENCY',cellEditor: 'NUMBER'         },
  decimal:        { labelFormatter: 'DECIMAL',  headerRenderer: 'DEFAULT', cellRenderer: 'NUMERIC', cellEditor: 'NUMBER'         },
  percent:        { labelFormatter: 'PERCENT',  headerRenderer: 'DEFAULT', cellRenderer: 'NUMERIC', cellEditor: 'NUMBER'         },
  date:           { labelFormatter: 'DATE',     headerRenderer: 'DEFAULT', cellRenderer: 'TEXT',    cellEditor: 'DATE'           },
  datetime:       { labelFormatter: 'DATETIME', headerRenderer: 'DEFAULT', cellRenderer: 'TEXT',    cellEditor: 'DATE'           },
  checkbox:       { labelFormatter: 'BOOLEAN',  headerRenderer: 'CENTER',  cellRenderer: 'CHECK',   cellEditor: 'CHECKBOX'       },
  lookup:         { labelFormatter: 'NONE',     headerRenderer: 'DEFAULT', cellRenderer: 'LOOKUP',  cellEditor: 'LOOKUP'         },
  textarea:       { labelFormatter: 'NONE',     headerRenderer: 'DEFAULT', cellRenderer: 'TEXT',    cellEditor: 'TEXTAREA'       },
  badge:          { labelFormatter: 'NONE',     headerRenderer: 'DEFAULT', cellRenderer: 'BADGE',   cellEditor: 'NONE'           },
  link:           { labelFormatter: 'NONE',     headerRenderer: 'DEFAULT', cellRenderer: 'LINK',    cellEditor: 'NONE'           },
  discount:       { labelFormatter: 'PERCENT',  headerRenderer: 'DEFAULT', cellRenderer: 'NUMERIC', cellEditor: 'DISCOUNT'       },
  stepper:        { labelFormatter: 'NUMBER',   headerRenderer: 'DEFAULT', cellRenderer: 'NUMERIC', cellEditor: 'STEPPER'        },
  combobox:       { labelFormatter: 'NONE',     headerRenderer: 'DEFAULT', cellRenderer: 'TEXT',    cellEditor: 'COMBOBOX'       },
  account_picker: { labelFormatter: 'NONE',     headerRenderer: 'DEFAULT', cellRenderer: 'LINK',    cellEditor: 'ACCOUNT_PICKER' },
  partner_picker: { labelFormatter: 'NONE',     headerRenderer: 'DEFAULT', cellRenderer: 'LINK',    cellEditor: 'PARTNER_PICKER' },
};

// Derives the best matching ColumnType from existing slot values (for loading saved columns).
// `dataType` is the legacy storage type and the last-resort fallback: columns seeded
// before the semantic slots existed carry only `dataType` (cellEditor/columnType null),
// so it must mirror `effectiveEditor()` in the live grid — otherwise a LOOKUP column
// would falsely read as "Text" here while the grid renders a search picker.
export function inferColumnType(
  labelFormatter?: LabelFormatter | null,
  cellRenderer?: CellRenderer | null,
  cellEditor?: CellEditor | null,
  dataType?: GridDataType | null,
): ColumnType {
  // Try to match by cellEditor first (most specific)
  const editorMap: Partial<Record<CellEditor, ColumnType>> = {
    TEXT: 'text', NUMBER: 'number', DATE: 'date', LOOKUP: 'lookup',
    TEXTAREA: 'textarea', CHECKBOX: 'checkbox', NONE: 'badge',
    DISCOUNT: 'discount', STEPPER: 'stepper', COMBOBOX: 'combobox',
    ACCOUNT_PICKER: 'account_picker', PARTNER_PICKER: 'partner_picker',
    ROWNUM: 'rownum',
  };
  if (cellEditor && editorMap[cellEditor]) {
    const candidate = editorMap[cellEditor]!;
    // Disambiguate NUMBER editor: currency vs decimal vs percent vs number
    if (cellEditor === 'NUMBER') {
      if (labelFormatter === 'CURRENCY') return 'currency';
      if (labelFormatter === 'DECIMAL') return 'decimal';
      if (labelFormatter === 'PERCENT') return 'percent';
      return 'number';
    }
    // Disambiguate DATE editor: date vs datetime
    if (cellEditor === 'DATE') {
      return labelFormatter === 'DATETIME' ? 'datetime' : 'date';
    }
    // Disambiguate NONE editor: badge vs link
    if (cellEditor === 'NONE') {
      return cellRenderer === 'LINK' ? 'link' : 'badge';
    }
    return candidate;
  }
  // No semantic editor configured (legacy rows) → fall back to storage dataType,
  // matching effectiveEditor() in the live grid so the label never contradicts
  // how the cell actually renders.
  if (dataType === 'LOOKUP') return 'lookup';
  if (dataType === 'NUMBER') return 'number';
  if (dataType === 'DATE') return 'date';
  return 'text';
}

export interface ErpTransactionType {
  id: string;
  code: string;
  name: string;
  moduleKey: string;
  moduleLabel: string;
  groupLabel?: string | null;
  lineTable?: string | null;
  sortOrder: number;
}

export interface ErpGridColumn {
  id?: string;
  sortOrder: number;
  headerText: string;
  dataField: string;
  width: number;
  isVisible: boolean;
  isRequired: boolean;
  isEditable: boolean;
  isSkippable: boolean;
  kind: GridColumnKind;
  dataType: GridDataType;
  lookupSource?: string | null;
  lookupDefaultFilter?: Record<string, unknown> | null;
  lookupDefaultSort?: string | null;
  /** Cell placeholder shown when empty; null/empty = built-in default. */
  placeholder?: string | null;
  /** Value prefilled on a new line row (lookup = id, others = raw string). */
  defaultValue?: string | null;
  /** Stored display label for a lookup `defaultValue` (set at pick time). */
  defaultValueLabel?: string | null;
  columnType?: ColumnType | null;
  labelFormatter?: LabelFormatter | null;
  headerRenderer?: HeaderRenderer | null;
  cellRenderer?: CellRenderer | null;
  cellEditor?: CellEditor | null;
}

export interface ErpTransactionGrid {
  id?: string;
  key: string;
  label: string;
  sortOrder: number;
  lineTable?: string | null;
  isPrimary: boolean;
  columns: ErpGridColumn[];
}

export interface GridColumnsResponse {
  type: ErpTransactionType;
  columns: ErpGridColumn[];
}

export interface GridsResponse {
  type: ErpTransactionType;
  grids: ErpTransactionGrid[];
}

export const listTransactionTypes = () =>
  apiGet<ErpTransactionType[]>('/transaction-grids/types');

/** Compat: primary-grid columns — read by the live cash/bank entry grid. */
export const getGridColumns = (code: string) =>
  apiGet<GridColumnsResponse>(`/transaction-grids/${encodeURIComponent(code)}/columns`);

export const getTransactionGrids = (code: string) =>
  apiGet<GridsResponse>(`/transaction-grids/${encodeURIComponent(code)}/grids`);

export const saveTransactionGrids = (code: string, grids: ErpTransactionGrid[]) =>
  apiPut<GridsResponse>(`/transaction-grids/${encodeURIComponent(code)}/grids`, {
    grids: grids.map((g, gi) => ({
      key: g.key,
      label: g.label,
      sortOrder: gi,
      lineTable: g.lineTable ?? undefined,
      isPrimary: g.isPrimary,
      columns: g.columns.map((c, ci) => ({
        sortOrder: ci,
        headerText: c.headerText,
        dataField: c.dataField,
        width: c.width,
        isVisible: c.isVisible,
        isRequired: c.isRequired,
        isEditable: c.isEditable,
        isSkippable: c.isSkippable,
        kind: c.kind,
        dataType: c.dataType,
        lookupSource: c.lookupSource ?? undefined,
        lookupDefaultFilter: c.lookupDefaultFilter && Object.keys(c.lookupDefaultFilter).length > 0
          ? c.lookupDefaultFilter : undefined,
        lookupDefaultSort: c.lookupDefaultSort ?? undefined,
        placeholder: c.placeholder ?? undefined,
        defaultValue: c.defaultValue ?? undefined,
        defaultValueLabel: c.defaultValueLabel ?? undefined,
        columnType: c.columnType ?? undefined,
        labelFormatter: c.labelFormatter ?? undefined,
        headerRenderer: c.headerRenderer ?? undefined,
        cellRenderer: c.cellRenderer ?? undefined,
        cellEditor: c.cellEditor ?? undefined,
      })),
    })),
  });
