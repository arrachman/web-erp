// Align / distribute / equalize untuk komponen report terpilih.
// Mengembalikan daftar patch {id, patch} untuk dispatch PATCH_COMPONENTS.

import type { RptComponent } from './report-types';

export type AlignOp =
  | 'left' | 'hcenter' | 'right'
  | 'top' | 'vmiddle' | 'bottom'
  | 'dist-h' | 'dist-v'
  | 'eq-w' | 'eq-h';

type Patch = { id: string; patch: Partial<RptComponent> };
const snap = (v: number) => Math.round(v * 2) / 2;

export function alignPatches(comps: RptComponent[], op: AlignOp): Patch[] {
  if (comps.length < 2) return [];
  const xs = comps.map(c => c.x);
  const rights = comps.map(c => c.x + c.width);
  const ys = comps.map(c => c.y);
  const bottoms = comps.map(c => c.y + c.height);

  const minX = Math.min(...xs), maxR = Math.max(...rights);
  const minY = Math.min(...ys), maxB = Math.max(...bottoms);
  const cx = (minX + maxR) / 2, cy = (minY + maxB) / 2;

  const set = (c: RptComponent, patch: Partial<RptComponent>): Patch => ({ id: c.id, patch });

  switch (op) {
    case 'left':    return comps.map(c => set(c, { x: snap(minX) }));
    case 'right':   return comps.map(c => set(c, { x: snap(maxR - c.width) }));
    case 'hcenter': return comps.map(c => set(c, { x: snap(cx - c.width / 2) }));
    case 'top':     return comps.map(c => set(c, { y: snap(minY) }));
    case 'bottom':  return comps.map(c => set(c, { y: snap(maxB - c.height) }));
    case 'vmiddle': return comps.map(c => set(c, { y: snap(cy - c.height / 2) }));
    case 'eq-w': {
      const w = Math.max(...comps.map(c => c.width));
      return comps.map(c => set(c, { width: w }));
    }
    case 'eq-h': {
      const h = Math.max(...comps.map(c => c.height));
      return comps.map(c => set(c, { height: h }));
    }
    case 'dist-h': {
      const sorted = [...comps].sort((a, b) => a.x - b.x);
      const step = (sorted[sorted.length - 1].x - sorted[0].x) / (sorted.length - 1);
      return sorted.map((c, i) => set(c, { x: snap(sorted[0].x + i * step) }));
    }
    case 'dist-v': {
      const sorted = [...comps].sort((a, b) => a.y - b.y);
      const step = (sorted[sorted.length - 1].y - sorted[0].y) / (sorted.length - 1);
      return sorted.map((c, i) => set(c, { y: snap(sorted[0].y + i * step) }));
    }
    default: return [];
  }
}
