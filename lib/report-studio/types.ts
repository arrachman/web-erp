/** ReportStudio domain types (band-based report designer). */

export type RsElKind = 'label' | 'field' | 'expr' | 'line' | 'box';

export type RsAlign = 'left' | 'center' | 'right' | 'justify';
export type RsVAlign = 'top' | 'middle' | 'bottom';

export interface RsElement {
  id: string;
  kind: RsElKind;
  x: number; y: number; w: number; h: number;
  text: string;
  bind: string;
  size: number;
  bold: boolean; italic: boolean; underline: boolean; strike: boolean;
  align: RsAlign;
  valign: RsVAlign;
  color: string;
  bg: string;
  mono: boolean;
  font: string;
  format: string;
  bTop: boolean; bBottom: boolean; bLeft: boolean; bRight: boolean;
  bColor: string;
  bWidth: number;
  canGrow: boolean; canShrink: boolean; wordWrap: boolean; enabled: boolean;
}

export type RsBandType =
  | 'ReportHeader' | 'PageHeader' | 'ColumnHeader' | 'Detail'
  | 'GroupHeader' | 'GroupFooter' | 'ColumnFooter' | 'PageFooter' | 'ReportFooter';

export interface RsBand {
  id: string;
  type: RsBandType;
  h: number;
  els: RsElement[];
  bg: string;
  canGrow: boolean; canShrink: boolean; printAll: boolean;
}

export interface RsReport {
  bands: RsBand[];
  key?: string;
  /** Optional SQL; when set, preview rows come from executeSqlQuery instead of sample data. */
  sql?: string;
}

export type RsTplKey = 'invoice' | 'sales' | 'purchasing' | 'finance' | 'customers';
export type RsView = 'design' | 'preview';
export type RsLang = 'id' | 'en';
export type RsTheme = 'light' | 'dark';
export type RsRibbon = 'home' | 'page' | 'layout' | 'view';
export type RsLeftTab = 'data' | 'relations' | 'params' | 'funcs';
export type RsRightTab = 'props' | 'dictionary' | 'tree';

export interface RsReportData {
  headerCtx: Record<string, string>;
  rows: Array<Record<string, string | number>>;
}

/** Mutable, non-render scratch state held in refs by the controller. */
export interface RsDrag {
  mode: 'move' | 'resize' | 'bandH';
  id: string;
  sx?: number; sy: number;
  ox?: number; oy?: number;
  ow?: number; oh?: number;
  w?: number; h?: number;
  bandH?: number;
}
