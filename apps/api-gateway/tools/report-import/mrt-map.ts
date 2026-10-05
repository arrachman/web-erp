/**
 * .mrt (Stimulsoft old serialization) → template_json v2 conversion
 * (Wave G0 importer, design §3). Bands/components are mapped with
 * geometry converted to mm; expressions stay VERBATIM except the
 * header placeholder normalization in mrt-expr.ts. Anything unmapped
 * is skipped with a recorded import warning — never silently dropped.
 */

import type {
  Component,
  CompStyle,
  TextComp,
} from '../../src/erp-report-engine/engine-types';
import type {
  ReportTemplateV2,
  StiBand,
  StiDatasetSpec,
  StiRelationSpec,
} from '../../src/erp-report-engine/engine-types-v2';
import { child, childText, propMap, type XmlEl } from './mrt-xml';
import { fontToStyle, stiAlign, stiBool, stiBorder, stiColor, stiVertAlign, parseConditionsList, parseFiltersList } from './mrt-style';
import { normalizeHeaderPlaceholders, placeholderForComponentName } from './mrt-expr';
import { parseDatasets, parseRelations, parseStyles } from './mrt-datasets';

const UNIT_FACTORS: Record<string, number> = {
  inches: 25.4,
  hundredths: 0.254,
  hundredthsofinch: 0.254,
  centimeters: 10,
  cm: 10,
  millimeters: 1,
  mm: 1,
  tenthsofmillimeter: 0.1,
};

const BAND_TYPE_MAP: Record<string, StiBand['type']> = {
  pageheaderband: 'pageHeader',
  pagefooterband: 'pageFooter',
  headerband: 'reportHeader',
  footerband: 'reportFooter',
  columnheaderband: 'columnHeader',
  columnfooterband: 'columnFooter',
  groupheaderband: 'groupHeader',
  groupfooterband: 'groupFooter',
  databand: 'data',
  childband: 'child',
  emptyband: 'empty',
  reportheaderband: 'reportHeader',
  reportfooterband: 'reportFooter',
};

export interface MrtConversion {
  template: ReportTemplateV2;
  warnings: string[];
  sessionLiteralCount: number;
}

interface Ctx {
  factor: number;
  warnings: string[];
  styles: Record<string, Partial<CompStyle>>;
  warnedTypes: Set<string>;
}

const round2 = (n: number) => Math.round(n * 100) / 100;

function rectOf(el: XmlEl, factor: number): { x: number; y: number; width: number; height: number } {
  const raw = childText(el, 'ClientRectangle') ?? '0,0,0,0';
  const [x = 0, y = 0, w = 0, h = 0] = raw.split(',').map((v) => Number(v) || 0);
  return { x: round2(x * factor), y: round2(y * factor), width: round2(w * factor), height: round2(h * factor) };
}

function baseStyleOf(el: XmlEl, ctx: Ctx): CompStyle {
  const style: CompStyle = { ...fontToStyle(childText(el, 'Font')) };
  const color = stiColor(childText(el, 'TextBrush'));
  if (color) style.color = color;
  const brush = childText(el, 'Brush');
  const bg = stiColor(brush && brush !== 'Transparent' ? brush : undefined);
  if (bg) style.background = bg;
  style.align = stiAlign(childText(el, 'HorAlignment'));
  style.vertAlign = stiVertAlign(childText(el, 'VertAlignment'));
  const border = stiBorder(childText(el, 'Border'));
  if (border) style.border = border;
  const styleName = childText(el, 'ComponentStyle');
  if (styleName && ctx.styles[styleName]) {
    return { ...ctx.styles[styleName], ...style };
  }
  return style;
}

function listValues(el: XmlEl, tag: string): string[] {
  const list = child(el, tag);
  if (!list) return [];
  return list.children.filter((c) => c.tag === 'value').map((c) => c.text);
}

function textFormatOf(el: XmlEl): TextComp['format'] {
  const tf = child(el, 'TextFormat');
  if (!tf) return undefined;
  const kind = (tf.attrs.type ?? '').split('.').pop() ?? '';
  const props = propMap(tf);
  let pattern = props.StringFormat || undefined;
  if (!pattern && props.DecimalDigits !== undefined && kind !== 'DateFormat' && kind !== 'TimeFormat') {
    const dec = Number(props.DecimalDigits) || 0;
    pattern = dec > 0 ? `#,##0.${'0'.repeat(dec)}` : '#,##0';
  }
  return { kind, pattern };
}

function mapText(el: XmlEl, ctx: Ctx): TextComp | null {
  const rect = rectOf(el, ctx.factor);
  const name = childText(el, 'Name');
  const style = baseStyleOf(el, ctx);
  const textOptions = childText(el, 'TextOptions');
  if (textOptions && /WordWrap=(True|T)/i.test(textOptions)) style.wordWrap = true;
  if (textOptions && /WordWrap=(False|F)/i.test(textOptions)) style.wordWrap = false;
  let expression = childText(el, 'Text') ?? '';
  const typeProp = childText(el, 'Type');
  if (typeProp === 'DataColumn' && expression && !expression.includes('{')) {
    expression = `{${expression}}`;
  }
  const byName = placeholderForComponentName(name);
  if (byName) expression = byName;
  else expression = normalizeHeaderPlaceholders(expression);
  return {
    type: 'text',
    name,
    ...rect,
    expression,
    format: textFormatOf(el),
    style,
    canGrow: stiBool(childText(el, 'CanGrow')),
    canShrink: stiBool(childText(el, 'CanShrink')),
    conditions: parseConditionsList(listValues(el, 'Conditions')),
  };
}

function mapComponent(el: XmlEl, ctx: Ctx, offset: { x: number; y: number }): Component[] {
  const type = (el.attrs.type ?? el.tag ?? '').split('.').pop() ?? '';
  const rect = rectOf(el, ctx.factor);
  const abs = { x: round2(rect.x + offset.x), y: round2(rect.y + offset.y), width: rect.width, height: rect.height };
  const name = childText(el, 'Name');
  switch (type) {
    case 'Text': {
      const t = mapText(el, ctx);
      if (t) {
        t.x = abs.x;
        t.y = abs.y;
        return [t];
      }
      return [];
    }
    case 'Image': {
      const isLogo = /logo/i.test(name ?? '') || /logo/i.test(childText(el, 'ImageName') ?? '');
      if (child(el, 'Image')?.text) ctx.warnings.push(`Gambar tertanam di ${name ?? 'Image'} tidak diimpor (pakai logo perusahaan)`);
      return [{ type: 'image', name, ...abs, src: isLogo ? '' : '' }];
    }
    case 'CheckBox':
      return [
        {
          type: 'checkbox',
          name,
          ...abs,
          expression: normalizeHeaderPlaceholders(childText(el, 'CheckedValue') ?? childText(el, 'Text') ?? ''),
          style: baseStyleOf(el, ctx),
        },
      ];
    case 'BarCode':
      return [
        {
          type: 'barcode',
          name,
          ...abs,
          expression: normalizeHeaderPlaceholders(childText(el, 'CodeValue') ?? childText(el, 'Code') ?? ''),
          symbology: childText(el, 'BarCodeType') ?? 'Code128',
          style: baseStyleOf(el, ctx),
        },
      ];
    case 'SubReport':
      ctx.warnings.push(`SubReport ${name ?? ''} dilewati di v1 (composite builder menyusul)`);
      return [{ type: 'subreport', name, ...abs }];
    case 'Rectangle':
      return [{ type: 'box', name, ...abs, style: baseStyleOf(el, ctx), fill: stiColor(childText(el, 'Brush')) }];
    case 'HorizontalLine':
    case 'VerticalLine':
    case 'Line':
    case 'CrossLinePrimitive':
      return [
        {
          type: 'line',
          ...abs,
          style: { color: stiColor(childText(el, 'Color')) ?? '#000000', width: Number(childText(el, 'Size') ?? '1') || 1 },
        },
      ];
    case 'Panel':
    case 'Clone': {
      const inner = child(el, 'Components');
      if (!inner) return [];
      return inner.children.flatMap((c) => mapComponent(c, ctx, abs));
    }
    default:
      if (!ctx.warnedTypes.has(type)) {
        ctx.warnedTypes.add(type);
        ctx.warnings.push(`Komponen tipe ${type} tidak dipetakan di v1`);
      }
      return [];
  }
}

function sortOf(el: XmlEl): StiBand['sort'] {
  const values = listValues(el, 'Sort');
  if (!values.length) return undefined;
  return values.map((v) => {
    const parts = v.split(';').map((p) => p.trim());
    const dirRaw = parts[parts.length - 1]?.toLowerCase();
    const dir = dirRaw === 'desc' || dirRaw === 'descending' ? 'desc' : 'asc';
    const exprParts = dirRaw === 'desc' || dirRaw === 'descending' || dirRaw === 'asc' || dirRaw === 'ascending' ? parts.slice(0, -1) : parts;
    const expr = exprParts.join(';');
    return { expr: expr.startsWith('{') ? expr : `{${expr}}`, dir } as { expr: string; dir: 'asc' | 'desc' };
  });
}

function mapBand(el: XmlEl, ctx: Ctx, groupLevel: { open: number }): StiBand | null {
  const rawType = (el.attrs.type ?? '').split('.').pop() ?? '';
  const type = BAND_TYPE_MAP[rawType.toLowerCase()];
  if (!type) {
    if (!ctx.warnedTypes.has(rawType)) {
      ctx.warnedTypes.add(rawType);
      ctx.warnings.push(`Band tipe ${rawType} tidak dipetakan di v1`);
    }
    return null;
  }
  const rect = rectOf(el, ctx.factor);
  const compsEl = child(el, 'Components');
  const components = compsEl
    ? compsEl.children.flatMap((c) => mapComponent(c, ctx, { x: 0, y: 0 }))
    : [];
  const band: StiBand = {
    type,
    name: childText(el, 'Name'),
    height: rect.height,
    components,
    canGrow: stiBool(childText(el, 'CanGrow')),
    canShrink: stiBool(childText(el, 'CanShrink')),
    canBreak: stiBool(childText(el, 'CanBreak')),
    printOnAllPages: stiBool(childText(el, 'PrintOnAllPages')),
    newPageBefore: stiBool(childText(el, 'NewPageBefore')),
    newPageAfter: stiBool(childText(el, 'NewPageAfter')),
  };
  if (type === 'groupHeader') {
    groupLevel.open += 1;
    band.level = groupLevel.open;
    band.groupBy = childText(el, 'Condition');
    band.keepWithNext = true;
    const sort = sortOf(el);
    if (sort) band.sort = sort;
  }
  if (type === 'groupFooter') {
    band.level = Math.max(1, groupLevel.open);
    groupLevel.open = Math.max(0, groupLevel.open - 1);
  }
  if (type === 'data' || type === 'child') {
    band.dataSource = childText(el, 'DataSourceName');
    const sort = sortOf(el);
    if (sort) band.sort = sort;
    const filters = parseFiltersList(listValues(el, 'Filters'));
    if (filters.length) band.filters = filters;
    const relation = childText(el, 'RelationName') ?? childText(el, 'Relation');
    if (relation) (band as { relationName?: string }).relationName = relation;
  }
  return band;
}

const KNOWN_PAPER: Array<{ name: 'A4' | 'A5' | 'Letter' | 'Legal'; w: number; h: number }> = [
  { name: 'Letter', w: 215.9, h: 279.4 },
  { name: 'A4', w: 210, h: 297 },
  { name: 'A5', w: 148, h: 210 },
  { name: 'Legal', w: 215.9, h: 355.6 },
];

export function convertMrt(root: XmlEl, opts: { sourceFile: string; registryCode: string }): MrtConversion {
  const warnings: string[] = [];
  const unitRaw = (childText(root, 'ReportUnit') ?? 'Inches').toLowerCase().replace(/[^a-z]/g, '');
  const factor = UNIT_FACTORS[unitRaw] ?? 25.4;
  const ctx: Ctx = { factor, warnings, styles: parseStyles(root), warnedTypes: new Set() };
  const { datasets, sqlByDs } = parseDatasets(root);
  const relations = parseRelations(root);

  let sessionLiteralCount = 0;
  for (const sql of Object.values(sqlByDs)) {
    const hits = sql.match(/idlogin|idmsmq/gi);
    if (hits) sessionLiteralCount += hits.length;
  }
  if (sessionLiteralCount > 0) {
    warnings.push(`SQL mengandung ${sessionLiteralCount} literal sesi legacy (idlogin/idmsmq) — dibuang dari spec, builder memakai konteks login`);
  }

  const pagesEl = child(root, 'Pages');
  const pageEls = pagesEl ? pagesEl.children : [];
  if (pageEls.length > 1) warnings.push(`Template multi-halaman (${pageEls.length}) — hanya halaman pertama yang dikonversi di v1`);
  const page = pageEls[0];
  if (!page) throw new Error('Tidak ada Page di .mrt');

  const pageWmm = round2((Number(childText(page, 'PageWidth') ?? '8.5') || 8.5) * factor);
  const pageHmm = round2((Number(childText(page, 'PageHeight') ?? '11') || 11) * factor);
  const orientation = pageWmm > pageHmm ? 'landscape' : 'portrait';
  const portraitW = orientation === 'landscape' ? pageHmm : pageWmm;
  const portraitH = orientation === 'landscape' ? pageWmm : pageHmm;
  const known = KNOWN_PAPER.find((p) => Math.abs(p.w - portraitW) < 1.5 && Math.abs(p.h - portraitH) < 1.5);
  const marginsRaw = (childText(page, 'Margins') ?? '0,0,0,0').split(',').map((v) => Number(v) || 0);
  const margins = {
    left: round2((marginsRaw[0] ?? 0) * factor),
    top: round2((marginsRaw[1] ?? 0) * factor),
    right: round2((marginsRaw[2] ?? 0) * factor),
    bottom: round2((marginsRaw[3] ?? 0) * factor),
  };
  const segW = Number(childText(page, 'SegmentWidth') ?? '0') || 0;
  const segH = Number(childText(page, 'SegmentHeight') ?? '0') || 0;
  const segments =
    segW > 0 && segH > 0
      ? { cols: Math.max(1, Math.round(pageWmm / (segW * factor))), rows: Math.max(1, Math.round(pageHmm / (segH * factor))) }
      : null;

  const compsEl = child(page, 'Components');
  const groupLevel = { open: 0 };
  const bands: StiBand[] = [];
  if (compsEl) {
    for (const bandEl of compsEl.children) {
      const band = mapBand(bandEl, ctx, groupLevel);
      if (band) bands.push(band);
    }
  }
  // Wire master-detail relations onto child/data bands.
  for (const band of bands) {
    const relName = (band as { relationName?: string }).relationName;
    if (relName) {
      const rel = relations.find((r) => r.child === band.dataSource);
      if (rel) {
        band.relation = { parentField: rel.parentField, childField: rel.childField };
        const master = bands.find((b) => b.type === 'data' && b.dataSource === rel.parent);
        if (master) band.masterBand = master.name;
        if (band.type === 'data') band.type = 'child';
      }
      delete (band as { relationName?: string }).relationName;
    }
  }

  const template: ReportTemplateV2 = {
    version: 2,
    name: childText(root, 'ReportName') ?? opts.sourceFile.replace(/\.mrt$/i, ''),
    page: {
      size: known ? known.name : 'Custom',
      custom: known ? undefined : { widthMm: portraitW, heightMm: portraitH },
      orientation,
      margins,
      segments,
    },
    styles: Object.keys(ctx.styles).length ? ctx.styles : undefined,
    datasets,
    relations: relations.length ? relations : undefined,
    bands,
    meta: { sourceFile: opts.sourceFile, registryCode: opts.registryCode, importWarnings: warnings },
  };
  return { template, warnings, sessionLiteralCount };
}
