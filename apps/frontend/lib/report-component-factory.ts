// Factory komponen report — dipakai canvas toolbar (tambah manual) & field
// palette (drag-to-bind). Satu sumber agar default style konsisten.

import type {
  RptBand,
  RptComponent,
  RptImageComponent,
  RptLineComponent,
  RptTextComponent,
} from './report-types';

/** 1mm @ 96dpi. Sumber tunggal skala mm→px (canvas, overlay, preview). */
export const MM_TO_PX = 3.7795275591;

/** Lebar gutter label band di canvas (px) — dipakai band-row & ruler/artboard. */
export const GUTTER_W = 150;

export function genCompId(): string {
  return `cmp_${Date.now().toString(36)}_${Math.random().toString(36).slice(2, 5)}`;
}

export function makeText(band: RptBand, expression = '{data.field}', name?: string): RptTextComponent {
  return {
    id: genCompId(),
    type: 'text',
    name: name ?? `Text${band.components.length + 1}`,
    x: 0, y: 0, width: 40, height: band.height,
    expression,
    style: { fontSize: 9, fontFamily: 'Arial', color: '#000000', align: 'left' },
    canGrow: false, canShrink: false,
  };
}

/** Text terikat satu kolom hasil query — expression `{kolom}`, nama = kolom. */
export function makeBoundText(band: RptBand, column: string, x = 0, y = 0): RptTextComponent {
  return { ...makeText(band, `{${column}}`, column), x, y };
}

export function makeLine(band: RptBand): RptLineComponent {
  return {
    id: genCompId(),
    type: 'line',
    name: `Line${band.components.length + 1}`,
    x: 0, y: band.height - 0.5, width: 185, height: 0,
    style: { color: '#000000', width: 0.5, style: 'solid' },
  };
}

export function makeImage(band: RptBand): RptImageComponent {
  return {
    id: genCompId(),
    type: 'image',
    name: `Image${band.components.length + 1}`,
    x: 0, y: 0, width: 30, height: 15,
    src: '{company.logoUrl}', fit: 'contain',
  };
}

/** Salin komponen dengan id baru + offset (untuk paste/duplicate). */
export function cloneComponents(comps: RptComponent[], dx = 3, dy = 3): RptComponent[] {
  return comps.map(c => ({
    ...(structuredClone(c) as RptComponent),
    id: genCompId(),
    x: Math.max(0, c.x + dx),
    y: Math.max(0, c.y + dy),
  }));
}

/** Band tujuan default saat user menambah komponen tanpa seleksi eksplisit. */
export function resolveTargetBand(bands: RptBand[], selectedBandId?: string): RptBand | undefined {
  if (selectedBandId) {
    const sel = bands.find(b => b.id === selectedBandId);
    if (sel) return sel;
  }
  return bands.find(b => b.type === 'data') ?? bands[0];
}

/** MIME khusus payload drag field palette → canvas. */
export const FIELD_DND_MIME = 'application/x-rpt-field';
export interface FieldDragPayload { alias: string; column: string }
