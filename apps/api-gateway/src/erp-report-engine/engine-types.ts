/**
 * Report-engine template + render-context types.
 *
 * The template schema is a pragmatic subset of `apps/web-erp/report-engine/README.md`
 * §4 (band-based, geometry in mm, Carbone-style `{d.field:formatter(args)}` markers).
 * The engine consumes the SAME shape the Report Designer round-trips, so a template
 * edited in the designer renders 1:1 here.
 *
 * Architecture: templates own LAYOUT; the report builders own DATA. The engine binds
 * a normalized {@link RenderContext} (derived from `ReportDocument`/`ReportDataset`)
 * into the template's expressions — it never runs the template's own SQL.
 */

export type PageSize = 'A4' | 'A5' | 'Letter' | 'Legal';
export type Orientation = 'portrait' | 'landscape';
export type TextAlign = 'left' | 'right' | 'center';
export type VertAlign = 'top' | 'middle' | 'bottom';
export type BorderSide = 'top' | 'right' | 'bottom' | 'left' | 'all';
export type LineStyle = 'solid' | 'dashed' | 'dotted';

export interface Margins {
  top: number;
  right: number;
  bottom: number;
  left: number;
}

export interface BorderStyle {
  sides: BorderSide[];
  style?: LineStyle;
  width?: number;
  color?: string;
}

export interface CompStyle {
  fontSize?: number;
  fontFamily?: string;
  bold?: boolean;
  italic?: boolean;
  color?: string;
  background?: string;
  align?: TextAlign;
  vertAlign?: VertAlign;
  wordWrap?: boolean;
  border?: BorderStyle;
}

export interface StyleCondition {
  when: string;
  style: Partial<CompStyle>;
}

export interface TextComp {
  type: 'text';
  name?: string;
  x: number;
  y: number;
  width: number;
  height: number;
  expression: string;
  /** Format tampilan dari TextFormat .mrt (template v2): NumberFormat/DateFormat/dll. */
  format?: { kind: string; pattern?: string };
  style?: CompStyle;
  canGrow?: boolean;
  canShrink?: boolean;
  conditions?: StyleCondition[];
}

export interface ImageComp {
  type: 'image';
  name?: string;
  x: number;
  y: number;
  width: number;
  height: number;
  src: string;
  fit?: 'contain' | 'cover' | 'fill';
}

export interface LineComp {
  type: 'line';
  x: number;
  y: number;
  width: number;
  height: number;
  style?: { color?: string; width?: number; style?: LineStyle };
}

export interface BoxComp {
  type: 'box';
  name?: string;
  x: number;
  y: number;
  width: number;
  height: number;
  style?: CompStyle;
  fill?: string;
  border?: BorderStyle;
}

export interface BarcodeComp {
  type: 'barcode';
  name?: string;
  x: number;
  y: number;
  width: number;
  height: number;
  /** Ekspresi Stimulsoft verbatim yang menghasilkan nilai barcode. */
  expression: string;
  symbology: string;
  style?: CompStyle;
}

export interface CheckboxComp {
  type: 'checkbox';
  name?: string;
  x: number;
  y: number;
  width: number;
  height: number;
  /** Ekspresi Stimulsoft verbatim; truthy = tercentang. */
  expression: string;
  style?: CompStyle;
}

export interface SubreportComp {
  type: 'subreport';
  name?: string;
  x: number;
  y: number;
  width: number;
  height: number;
}

export type Component =
  | TextComp
  | ImageComp
  | LineComp
  | BoxComp
  | BarcodeComp
  | CheckboxComp
  | SubreportComp;

export type BandType =
  | 'pageHeader'
  | 'pageFooter'
  | 'reportTitle'
  | 'columnHeader'
  | 'groupHeader'
  | 'data'
  | 'groupFooter';

export interface Band {
  type: BandType;
  height: number;
  /** groupHeader/groupFooter only. */
  level?: number;
  groupBy?: string;
  printOnAllPages?: boolean;
  newPageBefore?: boolean;
  /** data band: pad with blank rows up to this count. */
  minRows?: number;
  canGrow?: boolean;
  components: Component[];
}

export interface ReportTemplate {
  id?: string;
  name?: string;
  module?: string;
  version?: number;
  pageSize: PageSize;
  /** Dimensi eksplisit saat pageSize='Custom' (label/form khusus). */
  customSize?: { widthMm: number; heightMm: number };
  orientation: Orientation;
  margins: Margins;
  fonts?: string[];
  bands: Band[];
}

/**
 * Normalized data the engine binds. Header/footer/title bands resolve `{d.*}`
 * against {@link report}; the data band resolves `{d.*}` against each row.
 * `{c.*}` resolves against {@link company} everywhere.
 */
export interface RenderContext {
  report: Record<string, unknown>;
  rows: Record<string, unknown>[];
  company: Record<string, unknown>;
  /** Render time, used by `{Time}`. */
  now?: Date;
}
