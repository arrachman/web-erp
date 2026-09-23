// Snap-to-edge untuk drag komponen tunggal: ratakan tepi/tengah komponen ke
// tepi komponen lain & batas band, dengan garis bantu. Semua satuan mm.

export interface SnapBox { x: number; y: number; width: number; height: number }

export interface SnapResult {
  x: number;
  y: number;
  guideVX?: number; // garis bantu vertikal (posisi x)
  guideHY?: number; // garis bantu horizontal (posisi y)
}

/** offset = jarak tepi dari origin (x/y); cari line terdekat < threshold. */
function bestSnap(
  edges: Array<{ pos: number; offset: number }>,
  lines: number[],
  threshold: number,
): { origin: number; line: number } | null {
  let best: { origin: number; line: number; dist: number } | null = null;
  for (const e of edges) {
    for (const line of lines) {
      const dist = Math.abs(e.pos - line);
      if (dist <= threshold && (!best || dist < best.dist)) {
        best = { origin: line - e.offset, line, dist };
      }
    }
  }
  return best ? { origin: best.origin, line: best.line } : null;
}

export function computeSnap(
  box: SnapBox,
  others: SnapBox[],
  bounds: { w: number; h: number },
  threshold = 1.2,
): SnapResult {
  const linesX = [0, bounds.w / 2, bounds.w];
  const linesY = [0, bounds.h / 2, bounds.h];
  for (const o of others) {
    linesX.push(o.x, o.x + o.width / 2, o.x + o.width);
    linesY.push(o.y, o.y + o.height / 2, o.y + o.height);
  }

  const snapX = bestSnap(
    [{ pos: box.x, offset: 0 }, { pos: box.x + box.width / 2, offset: box.width / 2 }, { pos: box.x + box.width, offset: box.width }],
    linesX, threshold,
  );
  const snapY = bestSnap(
    [{ pos: box.y, offset: 0 }, { pos: box.y + box.height / 2, offset: box.height / 2 }, { pos: box.y + box.height, offset: box.height }],
    linesY, threshold,
  );

  return {
    x: snapX ? Math.max(0, snapX.origin) : box.x,
    y: snapY ? Math.max(0, snapY.origin) : box.y,
    guideVX: snapX?.line,
    guideHY: snapY?.line,
  };
}
