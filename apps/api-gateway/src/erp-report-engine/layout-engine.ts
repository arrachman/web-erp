/**
 * Band-plan emission for the layout engine (Wave G0): report/column
 * headers, recursive group levels (≤5), data + child (master-detail)
 * bands, minRows padding, empty band, footers. Pagination, scope
 * creation and finalization live in layout-base.ts; component
 * evaluation in layout-components.ts.
 */

import type { StiBand } from './engine-types-v2';
import type { DataRow, RenderModel } from './engine-types-v2';
import type { AggScopeEntry } from './expr-scope';
import { LayoutBase, type FlowItem, type GroupFrame, type LayoutInput } from './layout-base';
import { evalStiCondition, evalStiExpression } from './sti-expr';
import { compareLoose } from './layout-base';
export { TOTAL_PAGES_SENTINEL } from './layout-base';

export function buildRenderModel(input: LayoutInput): RenderModel {
  return new LayoutBuilder(input).build();
}

export class LayoutBuilder extends LayoutBase {
  /* ---------------- band plan emission ---------------- */

  build(): RenderModel {
    const t = this.template;
    const flow = t.bands.filter(
      (b) => b.type !== 'pageHeader' && b.type !== 'pageFooter' && b.type !== 'columnHeader',
    );
    const reportHeader = flow.find((b) => b.type === 'reportHeader');
    const reportFooter = flow.find((b) => b.type === 'reportFooter');
    const columnFooter = flow.find((b) => b.type === 'columnFooter');
    const emptyBand = flow.find((b) => b.type === 'empty');
    const groupHeaders = flow.filter((b) => b.type === 'groupHeader');
    const groupFooters = flow.filter((b) => b.type === 'groupFooter');
    const dataBands = flow.filter((b) => b.type === 'data' || b.type === 'child');

    this.startDocument();
    // Column header repeats per page; emit the first occurrence now.
    if (this.columnHeaderBand) {
      const scope = this.makeScope(new Map([[this.primaryDataset, this.datasets[this.primaryDataset]?.[0]]]), {}, 1);
      this.emit({ type: 'columnHeader', name: this.columnHeaderBand.name, groupPath: '', band: this.columnHeaderBand, scope });
    }
    const rootRows = this.primaryRows();
    if (reportHeader) {
      const scope = this.makeScope(new Map([[this.primaryDataset, rootRows[0]]]), this.rootAgg(rootRows), 1);
      this.emit({ type: 'reportHeader', name: reportHeader.name, groupPath: '', band: reportHeader, scope });
    }
    if (rootRows.length === 0 && emptyBand) {
      const scope = this.makeScope(new Map(), {}, 1);
      this.emit({ type: 'empty', name: emptyBand.name, groupPath: '', band: emptyBand, scope });
    }
    this.emitLevel(0, rootRows, '', groupHeaders, groupFooters, dataBands);
    if (columnFooter) {
      const scope = this.makeScope(new Map([[this.primaryDataset, rootRows[0]]]), this.rootAgg(rootRows), 1);
      this.emit({ type: 'columnFooter', name: columnFooter.name, groupPath: '', band: columnFooter, scope });
    }
    if (reportFooter) {
      const scope = this.makeScope(new Map([[this.primaryDataset, rootRows[rootRows.length - 1]]]), this.rootAgg(rootRows), 1);
      this.emit({ type: 'reportFooter', name: reportFooter.name, groupPath: '', band: reportFooter, scope });
    }
    return this.finalize();
  }

  private primaryRows(): DataRow[] {
    const band = this.template.bands.find((b) => b.type === 'data');
    let rows = [...(this.datasets[this.primaryDataset] ?? [])];
    if (band) rows = this.applySortAndFilters(rows, band, this.primaryDataset);
    return rows;
  }

  private applySortAndFilters(rows: DataRow[], band: StiBand, dataset: string): DataRow[] {
    let out = rows;
    if (band.filters?.length) {
      out = out.filter((row) =>
        band.filters!.every((f) => evalStiCondition(f, this.rowScope(row, dataset), this.onWarn)),
      );
    }
    if (band.sort?.length) {
      const specs = band.sort;
      out = [...out].sort((a, b) => {
        for (const spec of specs) {
          const va = evalStiExpression(spec.expr, this.rowScope(a, dataset), this.onWarn);
          const vb = evalStiExpression(spec.expr, this.rowScope(b, dataset), this.onWarn);
          const cmp = compareLoose(va, vb);
          if (cmp !== 0) return spec.dir === 'desc' ? -cmp : cmp;
        }
        return 0;
      });
    }
    return out;
  }

  private rootAgg(rows: DataRow[]): Record<string, AggScopeEntry> {
    const agg: Record<string, AggScopeEntry> = {};
    const entry: AggScopeEntry = {
      dataset: this.primaryDataset,
      rows: rows.map((row, i) => ({ row, line: i + 1 })),
    };
    for (const band of this.template.bands) {
      if ((band.type === 'data' || band.type === 'child') && band.name) agg[band.name] = entry;
    }
    return agg;
  }

  private groupKey(row: DataRow, dataset: string, expr: string): string {
    const v = evalStiExpression(expr, this.rowScope(row, dataset), this.onWarn);
    return v === null || v === undefined ? '' : String(v);
  }

  private emitLevel(
    levelIdx: number,
    rows: DataRow[],
    groupPath: string,
    groupHeaders: StiBand[],
    groupFooters: StiBand[],
    dataBands: StiBand[],
  ): void {
    const dataset = this.primaryDataset;
    if (levelIdx >= groupHeaders.length) {
      this.emitDataBands(rows, groupPath, dataBands);
      return;
    }
    const header = groupHeaders[levelIdx];
    const level = header.level ?? levelIdx + 1;
    const footer = groupFooters.find((f) => (f.level ?? 0) === level) ??
      groupFooters[levelIdx];
    let sorted = rows;
    if (header.sort?.length) sorted = this.applySortAndFilters(rows, { ...header, filters: undefined }, dataset);
    // Consecutive grouping by the verbatim condition expression.
    const groups: Array<{ key: string; rows: DataRow[] }> = [];
    for (const row of sorted) {
      const key = header.groupBy ? this.groupKey(row, dataset, header.groupBy) : '';
      const last = groups[groups.length - 1];
      if (last && last.key === key) last.rows.push(row);
      else groups.push({ key, rows: [row] });
    }
    for (const group of groups) {
      const path = groupPath ? `${groupPath}/${group.key}` : group.key;
      const agg = this.scopeAgg(group.rows, dataBands);
      const headerScope = this.makeScope(new Map([[dataset, group.rows[0]]]), agg, 1);
      const headerItem: FlowItem = {
        type: 'groupHeader',
        name: header.name,
        groupPath: path,
        band: header,
        scope: headerScope,
        level,
        isGroupHeader: true,
      };
      const frame: GroupFrame = { level, dataset, rows: group.rows, headerItem };
      this.openFrames.push(frame);
      this.emit(headerItem);
      this.emitLevel(levelIdx + 1, group.rows, path, groupHeaders, groupFooters, dataBands);
      if (footer) {
        const footerScope = this.makeScope(new Map([[dataset, group.rows[0]]]), agg, 1);
        this.emit({ type: 'groupFooter', name: footer.name, groupPath: path, band: footer, scope: footerScope, level });
      }
      this.openFrames = this.openFrames.filter((f) => f !== frame);
    }
  }

  private scopeAgg(rows: DataRow[], dataBands: StiBand[]): Record<string, AggScopeEntry> {
    const agg: Record<string, AggScopeEntry> = {};
    const entry: AggScopeEntry = {
      dataset: this.primaryDataset,
      rows: rows.map((row, i) => ({ row, line: i + 1 })),
    };
    for (const band of dataBands) if (band.name) agg[band.name] = entry;
    for (const band of this.template.bands) {
      if (band.type === 'groupHeader' && band.name) agg[band.name] = entry;
    }
    return agg;
  }

  private emitDataBands(rows: DataRow[], groupPath: string, dataBands: StiBand[]): void {
    const roots = dataBands.filter((b) => b.type === 'data');
    const children = dataBands.filter((b) => b.type === 'child');
    this.runningScopeId = `${groupPath}|${rows.length}|${rows[0] ? JSON.stringify(rows[0]).slice(0, 40) : ''}`;
    for (const band of roots.length ? roots : dataBands) {
      const bandRows = band.dataSource === this.primaryDataset || !band.dataSource
        ? rows
        : this.applySortAndFilters([...(this.datasets[band.dataSource] ?? [])], band, band.dataSource);
      let emitted = 0;
      bandRows.forEach((row, idx) => {
        const agg = this.scopeAgg(rows.length ? rows : bandRows, dataBands);
        const scope = this.makeScope(new Map([[band.dataSource ?? this.primaryDataset, row]]), agg, idx + 1);
        scope.vars.Line = idx + 1;
        this.emit({ type: 'data', name: band.name, groupPath, band, scope });
        emitted++;
        for (const child of children.filter((c) => !c.masterBand || c.masterBand === band.name)) {
          this.emitChild(child, row, groupPath, dataBands);
        }
      });
      const minRows = band.minRows ?? 0;
      if (minRows > emitted) {
        const agg = this.scopeAgg(rows, dataBands);
        for (let i = emitted; i < minRows; i++) {
          const scope = this.makeScope(new Map([[band.dataSource ?? this.primaryDataset, undefined]]), agg, i + 1);
          this.emit({ type: 'data', name: band.name, groupPath, band, scope });
        }
      }
    }
  }

  private emitChild(child: StiBand, masterRow: DataRow, groupPath: string, dataBands: StiBand[]): void {
    const dsName = child.dataSource ?? '';
    let rows = [...(this.datasets[dsName] ?? [])];
    if (child.relation) {
      const { parentField, childField } = child.relation;
      rows = rows.filter((r) => String(r[childField] ?? '') === String(masterRow[parentField] ?? ''));
    }
    rows = this.applySortAndFilters(rows, child, dsName);
    rows.forEach((row, idx) => {
      const agg = this.scopeAgg(rows, dataBands);
      const scope = this.makeScope(
        new Map<string, DataRow | undefined>([
          [dsName, row],
          [this.primaryDataset, masterRow],
        ]),
        agg,
        idx + 1,
      );
      this.emit({ type: 'child', name: child.name, groupPath, band: child, scope });
    });
  }


}
