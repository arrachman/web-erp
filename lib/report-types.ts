// ── Report Designer Types ────────────────────────────────────────────────────
// Semua tipe untuk template JSON, state designer, dan API contract.

export type PageSize = 'A4' | 'A5' | 'Letter' | 'Legal';
export type Orientation = 'portrait' | 'landscape';
export type BandType = 'pageHeader' | 'pageFooter' | 'groupHeader' | 'groupFooter' | 'data';
export type ComponentType = 'text' | 'image' | 'line';
export type AlignH = 'left' | 'center' | 'right';
export type AlignV = 'top' | 'middle' | 'bottom';

export interface BorderStyle {
  sides: Array<'top' | 'right' | 'bottom' | 'left' | 'all'>;
  style: 'solid' | 'dashed' | 'dotted';
  width: number;
  color: string;
}

export interface ComponentStyle {
  fontSize?: number;
  fontFamily?: string;
  bold?: boolean;
  italic?: boolean;
  underline?: boolean;
  color?: string;
  background?: string;
  align?: AlignH;
  vertAlign?: AlignV;
  wordWrap?: boolean;
  border?: Partial<BorderStyle>;
}

export interface ConditionalFormat {
  when: string;
  style: Partial<ComponentStyle>;
}

export interface RptTextComponent {
  id: string;
  type: 'text';
  name: string;
  x: number;
  y: number;
  width: number;
  height: number;
  expression: string;
  style: ComponentStyle;
  canGrow?: boolean;
  canShrink?: boolean;
  conditions?: ConditionalFormat[];
}

export interface RptImageComponent {
  id: string;
  type: 'image';
  name: string;
  x: number;
  y: number;
  width: number;
  height: number;
  src: string;
  fit?: 'contain' | 'cover' | 'fill';
}

export interface RptLineComponent {
  id: string;
  type: 'line';
  name: string;
  x: number;
  y: number;
  width: number;
  height: number;
  style: { color: string; width: number; style: 'solid' | 'dashed' | 'dotted' };
}

export type RptComponent = RptTextComponent | RptImageComponent | RptLineComponent;

export interface RptBand {
  id: string;
  type: BandType;
  level?: 1 | 2;
  height: number;
  groupBy?: string;
  printOnAllPages?: boolean;
  newPageBefore?: boolean;
  canGrow?: boolean;
  minRows?: number;
  components: RptComponent[];
}

export interface RptDataSourceParam {
  name: string;
  type: 'string' | 'number' | 'bigint' | 'boolean' | 'date';
  label?: string;
  required?: boolean;
  bindFrom?: string;
}

export interface RptDataSource {
  id: string;
  alias: string;
  name: string;
  sql: string;
  params: RptDataSourceParam[];
}

export interface RptReportParam {
  name: string;
  type: 'string' | 'number' | 'date' | 'dateRange';
  label: string;
  required?: boolean;
  defaultValue?: string;
}

export interface RptTemplate {
  id?: string;
  name: string;
  module: string;
  version?: number;
  pageSize: PageSize;
  orientation: Orientation;
  margins: { top: number; right: number; bottom: number; left: number };
  dataSources: RptDataSource[];
  params: RptReportParam[];
  bands: RptBand[];
}

// ── Designer UI State ────────────────────────────────────────────────────────

export interface DesignerSelection {
  type: 'band' | 'component' | null;
  bandId?: string;
  /** Komponen aktif (sumber properti detail). */
  componentId?: string;
  /** Semua komponen terpilih (multi-select, dalam satu band). Termasuk componentId. */
  componentIds?: string[];
}

/** Tab di dock kiri: editor data source vs palette field hasil query. */
export type LeftTab = 'data' | 'fields';

/** Tab editor properti komponen. */
export type PropTab = 'layout' | 'style' | 'data';

export interface DesignerState {
  template: RptTemplate;
  selection: DesignerSelection;
  isDirty: boolean;
  zoom: number;
  /** Dock kiri (Data/Fields), kanan (Properties), dan preview split — semua simultan & collapsible. */
  leftTab: LeftTab;
  leftOpen: boolean;
  rightOpen: boolean;
  previewOpen: boolean;
  /** Undo/redo history — snapshot template sebelum tiap mutasi (dibatasi HISTORY_LIMIT). */
  past: RptTemplate[];
  future: RptTemplate[];
}

export type DesignerAction =
  | { type: 'SET_TEMPLATE'; template: RptTemplate }
  | { type: 'SELECT_BAND'; bandId: string }
  | { type: 'SELECT_COMPONENT'; bandId: string; componentId: string }
  | { type: 'DESELECT' }
  | { type: 'ADD_BAND'; band: RptBand }
  | { type: 'UPDATE_BAND'; bandId: string; patch: Partial<RptBand>; transient?: boolean }
  | { type: 'REMOVE_BAND'; bandId: string }
  | { type: 'MOVE_BAND'; bandId: string; direction: 'up' | 'down' }
  | { type: 'ADD_COMPONENT'; bandId: string; component: RptComponent }
  | { type: 'ADD_COMPONENTS'; bandId: string; components: RptComponent[] }
  | { type: 'UPDATE_COMPONENT'; bandId: string; componentId: string; patch: Partial<RptComponent>; transient?: boolean }
  /** Patch banyak komponen sekaligus (group move, align, equalize) = 1 undo step. */
  | { type: 'PATCH_COMPONENTS'; bandId: string; patches: Array<{ id: string; patch: Partial<RptComponent> }>; transient?: boolean }
  | { type: 'REMOVE_COMPONENT'; bandId: string; componentId: string }
  | { type: 'REMOVE_SELECTED' }
  /** Tambah/kurangi komponen dari seleksi (shift/ctrl-click), dalam satu band. */
  | { type: 'TOGGLE_COMPONENT'; bandId: string; componentId: string }
  /** Set seleksi multi eksplisit (mis. setelah paste). */
  | { type: 'SELECT_COMPONENTS'; bandId: string; componentIds: string[] }
  | { type: 'ADD_DATASOURCE'; ds: RptDataSource }
  | { type: 'UPDATE_DATASOURCE'; dsId: string; patch: Partial<RptDataSource> }
  | { type: 'REMOVE_DATASOURCE'; dsId: string }
  | { type: 'SET_ZOOM'; zoom: number }
  | { type: 'SET_LEFT_TAB'; tab: LeftTab }
  | { type: 'TOGGLE_LEFT'; open?: boolean }
  | { type: 'TOGGLE_RIGHT'; open?: boolean }
  | { type: 'TOGGLE_PREVIEW'; open?: boolean }
  /** Snapshot manual sebelum operasi transient (mis. drag) agar 1 drag = 1 undo step. */
  | { type: 'PUSH_HISTORY' }
  | { type: 'UNDO' }
  | { type: 'REDO' }
  | { type: 'MARK_CLEAN' };

// ── API response types ───────────────────────────────────────────────────────

export interface RptTemplateRecord {
  id: string;
  code: string;
  name: string;
  module: string;
  description?: string;
  templateJson: RptTemplate;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SqlQueryResult {
  rows: Record<string, unknown>[];
  count: number;
  columns: string[];
}
