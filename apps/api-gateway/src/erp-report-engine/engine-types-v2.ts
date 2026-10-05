/**
 * Template v2 + render-model types for the .mrt-converted report engine
 * (Wave G0). Kept separate from the v1 types in engine-types.ts so the
 * Stimulsoft dialect (verbatim expressions, band plan, datasets) has its
 * own contract. Expressions are stored VERBATIM (Stimulsoft dialect) and
 * evaluated by sti-expr.ts — never rewritten at import time.
 */

import type { BandType, Component, Margins, Orientation, PageSize } from './engine-types';

export type StiBandType =
  | 'pageHeader'
  | 'pageFooter'
  | 'reportHeader'
  | 'reportFooter'
  | 'columnHeader'
  | 'columnFooter'
  | 'groupHeader'
  | 'groupFooter'
  | 'data'
  | 'child'
  | 'empty';

export interface StiDatasetSpec {
  name: string;
  columns: string[];
}

export interface StiRelationSpec {
  parent: string;
  child: string;
  parentField: string;
  childField: string;
}

export interface StiBand {
  type: StiBandType;
  name?: string;
  height: number;
  level?: number;
  groupBy?: string;
  dataSource?: string;
  sort?: { expr: string; dir: 'asc' | 'desc' }[];
  filters?: string[];
  masterBand?: string;
  relation?: { parentField: string; childField: string };
  printOnAllPages?: boolean;
  newPageBefore?: boolean;
  newPageAfter?: boolean;
  canGrow?: boolean;
  canShrink?: boolean;
  canBreak?: boolean;
  keepWithNext?: boolean;
  minRows?: number;
  components: Component[];
}

export interface TemplatePageV2 {
  size: PageSize | 'Custom';
  custom?: { widthMm: number; heightMm: number };
  orientation: Orientation;
  margins: Margins;
  copies?: number;
  segments?: { cols: number; rows: number } | null;
}

export interface ReportTemplateV2 {
  version: 2;
  name?: string;
  page: TemplatePageV2;
  styles?: Record<string, Partial<import('./engine-types').CompStyle>>;
  datasets: StiDatasetSpec[];
  relations?: StiRelationSpec[];
  bands: StiBand[];
  meta?: { sourceFile?: string; registryCode?: string; importWarnings?: string[] };
}

export function isTemplateV2(value: unknown): value is ReportTemplateV2 {
  return !!value && typeof value === 'object' && (value as { version?: unknown }).version === 2;
}

/* ------------------------------------------------------------------ */
/* Data + context contracts (dataset builders → engine)                */
/* ------------------------------------------------------------------ */

export type DataRow = Record<string, unknown>;

export interface ReportData {
  datasets: Record<string, DataRow[]>;
  relations?: StiRelationSpec[];
  paramLines?: string[];
}

export interface CompanyContext {
  name: string;
  address?: string;
  city?: string;
  phone?: string;
  email?: string;
  npwp?: string;
  logoUrl?: string;
}

export interface RenderContextV2 {
  company: CompanyContext;
  report: { title: string; code: string; paramLines: string[] };
  params: Record<string, unknown>;
  userName?: string;
  now: Date;
}

/* ------------------------------------------------------------------ */
/* Render model — the single pagination result every exporter consumes */
/* ------------------------------------------------------------------ */

export interface ModelComponent {
  type: 'text' | 'image' | 'line' | 'box' | 'barcode' | 'checkbox' | 'subreport';
  name?: string;
  x: number;
  y: number;
  width: number;
  height: number;
  text?: string;
  style?: import('./engine-types').CompStyle;
  src?: string;
  lineWidth?: number;
  barcode?: { value: string; symbology: string };
  checked?: boolean;
  fill?: string;
}

export interface ModelBand {
  type: StiBandType | BandType;
  name?: string;
  y: number;
  height: number;
  groupPath: string;
  components: ModelComponent[];
}

export interface ModelPage {
  index: number;
  bands: ModelBand[];
}

export interface RenderModel {
  name?: string;
  pageWidthMm: number;
  pageHeightMm: number;
  margins: Margins;
  pages: ModelPage[];
  pageCount: number;
  datasets: Record<string, DataRow[]>;
}
