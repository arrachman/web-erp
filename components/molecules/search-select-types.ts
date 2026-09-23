// ─── Public types ────────────────────────────────────────────────────────────

export interface SearchSelectOption {
  value: string;
  label: string;
  [key: string]: unknown;
}

export interface SearchSelectColumn {
  key: string;
  header: string;
  /** Fixed width, e.g. "140px" or "20%". Omit for flex fill. */
  width?: string;
}

// ─── Props — discriminated by mode ───────────────────────────────────────────

export interface BaseProps {
  id?: string;
  placeholder?: string;
  loadOptions: (search: string, page: number, limit: number) => Promise<{ data: SearchSelectOption[]; total: number }>;
  debounceMs?: number;
  disabled?: boolean;
  /** Show red border when validation fails. */
  error?: boolean;
  /** Modal header title. Defaults to placeholder text (without trailing …). */
  title?: string;
  /** Column definitions. Auto-detected (always ≥ 3 cols) when omitted. */
  columns?: SearchSelectColumn[];
  /** Max rows shown in the modal. Default 10. */
  limit?: number;
  /** Pre-fill display label when value is known but may not appear in first-page results. */
  initialLabel?: string;
  /** Focus the inline input on mount (grid cell edit-mode). */
  autoFocus?: boolean;
  /** Seed the inline search text on mount (type-to-edit in a grid cell). Implies autoFocus. */
  initialQuery?: string;
  /** Grid-cell edit mode: input fills the cell (full row height, no rounded border/margin). */
  fill?: boolean;
  /** Open the modal window immediately on mount (e.g. triggered by a grid cell's search icon). */
  autoOpenModal?: boolean;
  /** Fires alongside onValueChange with the picked option (value + display label + optional meta/code fields). */
  onPick?: (opt: SearchSelectOption) => void;
}

export interface SingleProps extends BaseProps {
  mode?: 'single';
  value: string;
  onValueChange: (value: string) => void;
  values?: never;
  onValuesChange?: never;
}

export interface MultiProps extends BaseProps {
  mode: 'multi';
  values: string[];
  onValuesChange: (values: string[]) => void;
  value?: never;
  onValueChange?: never;
}

export type SearchSelectProps = SingleProps | MultiProps;

// ─── Default columns: always 3 — NO · KODE · NAMA ────────────────────────────
export const DEFAULT_COLS: SearchSelectColumn[] = [
  { key: '_no',   header: 'NO',   width: '48px'  },
  { key: 'code',  header: 'KODE', width: '160px' },
  { key: 'label', header: 'NAMA'                 },
];
