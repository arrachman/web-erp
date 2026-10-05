/**
 * Pagination/layout engine (Wave G0, design D3). Computes ONE RenderModel
 * from a template v2 + data: recursive grouping ≤5 levels, band kinds,
 * PrintOnAllPages / NewPageBefore / CanGrow / CanShrink / keep-together,
 * master-detail child bands, minRows padding, running sums, and a
 * two-pass TotalPageCount (sentinel substituted after pagination).
 * All exporters (PDF/HTML/DOCX/XLSX) consume this model only.
 */

import type { Component } from './engine-types';
import type {
  DataRow,
  ModelBand,
  ModelComponent,
  RenderContextV2,
  RenderModel,
  ReportData,
  ReportTemplateV2,
  StiBand,
} from './engine-types-v2';
import type { AggScopeEntry, StiScope } from './expr-scope';
import { evalStiCondition, evalStiExpression, evalStiFormatted, evalStiText } from './sti-expr';
import { helperDatasets, type ReportFormatSettings } from './report-format';
import { pageSizeMm } from './geometry';
import { evaluateBandComponents } from './layout-components';

export const TOTAL_PAGES_SENTINEL = '\u0001TPC\u0001';
const EPS = 0.6;

export interface FlowItem {
  type: StiBand['type'];
  name?: string;
  groupPath: string;
  band: StiBand;
  scope: StiScope;
  level?: number;
  isGroupHeader?: boolean;
  resolved?: { height: number; components: ModelComponent[] };
}

export interface GroupFrame {
  level: number;
  dataset: string;
  rows: DataRow[];
  headerItem?: FlowItem;
}

export interface LayoutInput {
  template: ReportTemplateV2;
  data: ReportData;
  ctx: RenderContextV2;
  format: ReportFormatSettings;
  onWarn?: (message: string) => void;
}


export abstract class LayoutBase {
  protected readonly template: ReportTemplateV2;
  protected readonly datasets: Record<string, DataRow[]>;
  protected readonly ctx: RenderContextV2;
  protected readonly format: ReportFormatSettings;
  protected readonly onWarn?: (m: string) => void;
  protected readonly pageW: number;
  protected readonly pageH: number;
  protected readonly primaryDataset: string;
  protected pageHeaderBand?: StiBand;
  protected pageFooterBand?: StiBand;
  protected columnHeaderBand?: StiBand;
  protected pageHeaderH = 0;
  protected pageFooterH = 0;
  protected pages: ModelBand[][] = [];
  protected cursorY = 0;
  protected pageHasFlow = false;
  protected openFrames: GroupFrame[] = [];
  protected running = new Map<string, number>();
  protected runningScopeId = '';

  constructor(input: LayoutInput) {
    this.template = input.template;
    this.ctx = input.ctx;
    this.format = input.format;
    this.onWarn = input.onWarn;
    const helpers = helperDatasets(input.format);
    this.datasets = { ...input.data.datasets };
    for (const [k, v] of Object.entries(helpers)) {
      if (!this.datasets[k] || this.datasets[k].length === 0) this.datasets[k] = v;
    }
    const geom = pageSizeMm(
      input.template.page.size,
      input.template.page.orientation,
      input.template.page.custom,
    );
    this.pageW = geom.widthMm;
    this.pageH = geom.heightMm;
    const dataBand = input.template.bands.find((x) => x.type === 'data');
    this.primaryDataset =
      dataBand?.dataSource ?? input.template.datasets[0]?.name ?? Object.keys(this.datasets)[0] ?? '';
    for (const band of input.template.bands) {
      if (band.type === 'pageHeader') this.pageHeaderBand = band;
      if (band.type === 'pageFooter') this.pageFooterBand = band;
      if (band.type === 'columnHeader') this.columnHeaderBand = band;
    }
    this.pageHeaderH = this.pageHeaderBand?.height ?? 0;
    this.pageFooterH = this.pageFooterBand?.height ?? 0;
  }

  /* ---------------- scope ---------------- */

  protected makeScope(
    currentRows: Map<string, DataRow | undefined>,
    aggBands: Record<string, AggScopeEntry>,
    line: number,
  ): StiScope {
    const datasets: StiScope['datasets'] = {};
    for (const [name, rows] of Object.entries(this.datasets)) {
      // Single-row datasets are constants (format helpers, company info):
      // their current row is always row 0 in every scope, as in Stimulsoft.
      datasets[name] = {
        row: currentRows.get(name) ?? (rows.length === 1 ? rows[0] : undefined),
        rows,
      };
    }
    const scope: StiScope = {
      datasets,
      bands: aggBands,
      primaryDataset: this.primaryDataset,
      c: {
        companyName: this.ctx.company.name,
        name: this.ctx.company.name,
        address: this.ctx.company.address ?? '',
        city: this.ctx.company.city ?? '',
        phone: this.ctx.company.phone ?? '',
        email: this.ctx.company.email ?? '',
        npwp: this.ctx.company.npwp ?? '',
        logoUrl: this.ctx.company.logoUrl ?? '',
      },
      r: {
        title: this.ctx.report.title,
        code: this.ctx.report.code,
        paramLines: this.ctx.report.paramLines,
      },
      params: this.ctx.params,
      vars: {
        PageNumber: this.pages.length + 1,
        TotalPageCount: TOTAL_PAGES_SENTINEL,
        Line: line,
        Time: this.ctx.now,
        Today: new Date(this.ctx.now.getFullYear(), this.ctx.now.getMonth(), this.ctx.now.getDate()),
      },
      format: this.format,
      sumRunning: (ds, field) => this.sumRunning(ds, field, currentRows),
    };
    return scope;
  }

  protected sumRunning(
    ds: string | undefined,
    field: string,
    currentRows: Map<string, DataRow | undefined>,
  ): number {
    const dataset = ds ?? this.primaryDataset;
    const key = `${this.runningScopeId}|${dataset}|${field}`;
    const row = currentRows.get(dataset);
    const next = (this.running.get(key) ?? 0) + (row ? Number(row[field] ?? 0) || 0 : 0);
    this.running.set(key, next);
    return next;
  }

  protected rowScope(row: DataRow | undefined, dataset: string): StiScope {
    return this.makeScope(new Map([[dataset, row]]), {}, 1);
  }

  /**
   * Scope rows for header/footer bands: every dataset contributes its
   * FIRST row (Stimulsoft resolves {DS.field} in header/footer bands
   * against the first data row), with one dataset's row overridden by
   * the caller (e.g. the primary dataset's current group row).
   */
  protected firstRowsScopeMap(
    overrideName?: string,
    overrideRow?: DataRow,
  ): Map<string, DataRow | undefined> {
    const map = new Map<string, DataRow | undefined>();
    for (const [name, rows] of Object.entries(this.datasets)) {
      map.set(name, rows[0]);
    }
    if (overrideName) map.set(overrideName, overrideRow);
    return map;
  }

  /* ---------------- pagination ---------------- */

  protected contentTop(): number {
    return this.template.page.margins.top + this.pageHeaderH;
  }
  protected contentBottom(): number {
    return this.pageH - this.template.page.margins.bottom - this.pageFooterH;
  }

  protected currentPage(): ModelBand[] {
    if (this.pages.length === 0) this.pages.push([]);
    return this.pages[this.pages.length - 1];
  }

  protected newPage(): void {
    this.pages.push([]);
    this.cursorY = this.contentTop();
    this.pageHasFlow = false;
    // Re-emit column header + printOnAllPages group headers of open groups.
    if (this.columnHeaderBand) {
      const scope = this.makeScope(new Map(), {}, 1);
      this.place({ type: 'columnHeader', name: this.columnHeaderBand.name, groupPath: '', band: this.columnHeaderBand, scope });
    }
    for (const frame of this.openFrames) {
      if (frame.headerItem?.band.printOnAllPages) {
        this.place({ ...frame.headerItem });
      }
    }
  }

  protected place(item: FlowItem): void {
    if (!item.resolved) item.resolved = evaluateBandComponents(item.band, item.scope, this.ctx.company, this.onWarn);
    const { height, components } = item.resolved;
    this.currentPage().push({
      type: item.type,
      name: item.name,
      y: this.cursorY,
      height,
      groupPath: item.groupPath,
      components: components.map((c) => ({ ...c, y: c.y + this.cursorY })),
    });
    this.cursorY += height;
    this.pageHasFlow = true;
  }

  protected emit(item: FlowItem, nextHeight?: number): void {
    if (this.pages.length === 0) {
      this.pages.push([]);
      this.cursorY = this.contentTop();
    }
    if (!item.resolved) item.resolved = evaluateBandComponents(item.band, item.scope, this.ctx.company, this.onWarn);
    const h = item.resolved.height;
    if (item.band.newPageBefore && this.pageHasFlow) this.newPage();
    const keepNext = item.band.keepWithNext && nextHeight !== undefined ? nextHeight : 0;
    if (this.pageHasFlow && this.cursorY + h + keepNext > this.contentBottom() + EPS) {
      this.newPage();
    }
    this.place(item);
    if (item.band.newPageAfter) this.newPage();
  }

  /** Initialize the first page (called once by the builder). */
  protected startDocument(): void {
    this.pages.push([]);
    this.cursorY = this.contentTop();
  }

  /* ---------------- finalize ---------------- */

  protected finalize(): RenderModel {
    const total = this.pages.length;
    const modelPages = this.pages.map((flowBands, idx) => {
      const bands: ModelBand[] = [];
      if (this.pageHeaderBand) {
        const scope = this.makeScope(this.firstRowsScopeMap(), {}, 1);
        scope.vars.PageNumber = idx + 1;
        const { height, components } = evaluateBandComponents(this.pageHeaderBand, scope, this.ctx.company, this.onWarn);
        bands.push({
          type: 'pageHeader',
          name: this.pageHeaderBand.name,
          y: this.template.page.margins.top,
          height,
          groupPath: '',
          components: components.map((c) => ({ ...c, y: c.y + this.template.page.margins.top })),
        });
      }
      bands.push(...flowBands);
      if (this.pageFooterBand) {
        const scope = this.makeScope(this.firstRowsScopeMap(), {}, 1);
        scope.vars.PageNumber = idx + 1;
        const y = this.pageH - this.template.page.margins.bottom - this.pageFooterH;
        const { height, components } = evaluateBandComponents(this.pageFooterBand, scope, this.ctx.company, this.onWarn);
        bands.push({
          type: 'pageFooter',
          name: this.pageFooterBand.name,
          y,
          height,
          groupPath: '',
          components: components.map((c) => ({ ...c, y: c.y + y })),
        });
      }
      return { index: idx, bands };
    });
    // Pass 2: TotalPageCount substitution.
    for (const page of modelPages) {
      for (const band of page.bands) {
        for (const comp of band.components) {
          if (comp.text && comp.text.includes(TOTAL_PAGES_SENTINEL)) {
            comp.text = comp.text.split(TOTAL_PAGES_SENTINEL).join(String(total));
          }
        }
      }
    }
    return {
      name: this.template.name,
      pageWidthMm: this.pageW,
      pageHeightMm: this.pageH,
      margins: this.template.page.margins,
      pages: modelPages,
      pageCount: total,
      datasets: this.datasets,
    };
  }
}

export function compareLoose(a: unknown, b: unknown): number {
  const na = Number(a);
  const nb = Number(b);
  if (!Number.isNaN(na) && !Number.isNaN(nb) && a !== '' && b !== '') return na - nb;
  const sa = a === null || a === undefined ? '' : String(a);
  const sb = b === null || b === undefined ? '' : String(b);
  return sa < sb ? -1 : sa > sb ? 1 : 0;
}
