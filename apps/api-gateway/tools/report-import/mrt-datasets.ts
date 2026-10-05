/**
 * Dictionary extraction for the .mrt importer: data sources (name +
 * declared columns + legacy SQL spec scan) and master-detail relations.
 * The legacy SQL is a data SPEC only — it is never executed; builders
 * own the real queries (design D5).
 */

import type { CompStyle } from '../../src/erp-report-engine/engine-types';
import type { StiDatasetSpec, StiRelationSpec } from '../../src/erp-report-engine/engine-types-v2';
import { child, childText, propMap, type XmlEl } from './mrt-xml';
import { fontToStyle, stiColor } from './mrt-style';

export export function parseDatasets(root: XmlEl): { datasets: StiDatasetSpec[]; sqlByDs: Record<string, string> } {
  const datasets: StiDatasetSpec[] = [];
  const sqlByDs: Record<string, string> = {};
  const dict = child(root, 'Dictionary');
  const sources = dict ? child(dict, 'DataSources') : undefined;
  if (sources) {
    for (const src of sources.children) {
      const name = childText(src, 'Name') ?? src.tag;
      const colsEl = child(src, 'Columns');
      const columns = colsEl
        ? colsEl.children
            .filter((c) => c.tag === 'value')
            .map((c) => (c.text.split(',')[0] ?? '').trim())
            .filter(Boolean)
        : [];
      datasets.push({ name, columns });
      const sql = childText(src, 'SqlCommand');
      if (sql) sqlByDs[name] = sql;
    }
  }
  return { datasets, sqlByDs };
}

export function parseRelations(root: XmlEl): StiRelationSpec[] {
  const dict = child(root, 'Dictionary');
  const relsEl = dict ? child(dict, 'Relations') : undefined;
  if (!relsEl) return [];
  const out: StiRelationSpec[] = [];
  for (const rel of relsEl.children) {
    const props = propMap(rel);
    const parent = props.ParentSourceName ?? props.ParentDataSource;
    const childName = props.ChildSourceName ?? props.ChildDataSource;
    const parentCols = child(rel, 'ParentColumns');
    const childCols = child(rel, 'ChildColumns');
    const pf = parentCols?.children.find((c) => c.tag === 'value')?.text.split(',')[0];
    const cf = childCols?.children.find((c) => c.tag === 'value')?.text.split(',')[0];
    if (parent && childName && pf && cf) {
      out.push({ parent, child: childName, parentField: pf, childField: cf });
    }
  }
  return out;
}

export function parseStyles(root: XmlEl): Record<string, Partial<CompStyle>> {
  const stylesEl = child(root, 'Styles');
  const out: Record<string, Partial<CompStyle>> = {};
  if (!stylesEl) return out;
  for (const st of stylesEl.children) {
    const name = childText(st, 'Name') ?? st.tag;
    const style: Partial<CompStyle> = { ...fontToStyle(childText(st, 'Font')) };
    const color = stiColor(childText(st, 'TextBrush'));
    if (color) style.color = color;
    const bg = stiColor(childText(st, 'Brush'));
    if (bg) style.background = bg;
    out[name] = style;
  }
  return out;
}

