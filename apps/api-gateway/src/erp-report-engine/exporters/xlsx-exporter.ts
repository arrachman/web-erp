/**
 * XLSX exporter (Wave G0) via exceljs (already a dependency).
 * Two modes:
 *  - layout: one sheet mirroring the page layout — columns derived from
 *    component x positions across the whole model, cell merges for wide
 *    components, bold/fill/border/alignment carried over.
 *  - data: one sheet per dataset with raw typed values (numbers stay
 *    numbers) — the mode for further analysis.
 */

import ExcelJS from 'exceljs';
import type { ModelComponent, RenderModel } from '../engine-types-v2';

function cellStyle(cell: ExcelJS.Cell, c: ModelComponent): void {
  const st = c.style;
  if (st) {
    cell.font = {
      name: 'Calibri',
      size: st.fontSize ?? 9,
      bold: st.bold ?? false,
      italic: st.italic ?? false,
      color: st.color ? { argb: `FF${st.color.replace('#', '').toUpperCase()}` } : undefined,
    };
    cell.alignment = {
      horizontal: st.align ?? 'left',
      vertical: 'middle',
      wrapText: true,
    };
    if (st.border) {
      const b = {
        style: (st.border.style === 'dashed' ? 'dashed' : st.border.style === 'dotted' ? 'dotted' : 'thin') as ExcelJS.BorderStyle,
        color: { argb: `FF${(st.border.color ?? '#000000').replace('#', '').toUpperCase()}` },
      };
      const sides = st.border.sides;
      cell.border = {
        top: sides.includes('all') || sides.includes('top') ? b : undefined,
        bottom: sides.includes('all') || sides.includes('bottom') ? b : undefined,
        left: sides.includes('all') || sides.includes('left') ? b : undefined,
        right: sides.includes('all') || sides.includes('right') ? b : undefined,
      };
    }
  }
  const bg = c.fill ?? st?.background;
  if (bg && bg.toLowerCase() !== 'transparent' && bg.startsWith('#')) {
    cell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: `FF${bg.replace('#', '').toUpperCase()}` } };
  }
}

function layoutSheet(wb: ExcelJS.Workbook, model: RenderModel): void {
  const ws = wb.addWorksheet('Laporan', {
    pageSetup: {
      paperSize: undefined,
      orientation: model.pageWidthMm > model.pageHeightMm ? 'landscape' : 'portrait',
      fitToPage: true,
    },
  });
  // Column boundaries from all component x/width edges.
  const edges = new Set<number>();
  for (const page of model.pages) {
    for (const band of page.bands) {
      for (const c of band.components) {
        edges.add(Math.round(c.x * 10) / 10);
        edges.add(Math.round((c.x + c.width) * 10) / 10);
      }
    }
  }
  const sorted = [...edges].sort((a, b) => a - b);
  const colIndex = (x: number) => sorted.findIndex((e) => Math.abs(e - x) < 0.6) + 1;
  sorted.slice(0, -1).forEach((edge, i) => {
    const wMm = sorted[i + 1] - edge;
    ws.getColumn(i + 1).width = Math.max(2, wMm / 2.1);
  });

  let rowNum = 0;
  for (const page of model.pages) {
    if (page.index > 0) rowNum += 1; // blank separator row between pages
    for (const band of page.bands) {
      const comps = band.components.filter(
        (c) => c.type === 'text' || c.type === 'barcode' || c.type === 'checkbox' || c.type === 'box',
      );
      if (comps.length === 0) continue;
      // Group the band's components into visual rows by y.
      const rowsByY = new Map<number, ModelComponent[]>();
      for (const c of comps) {
        const key = Math.round(c.y * 2) / 2;
        const arr = rowsByY.get(key) ?? [];
        arr.push(c);
        rowsByY.set(key, arr);
      }
      for (const key of [...rowsByY.keys()].sort((a, b) => a - b)) {
        rowNum += 1;
        const row = ws.getRow(rowNum);
        row.height = Math.max(12, band.height * 2.83);
        const mergedRanges: Array<[number, number]> = [];
        for (const c of rowsByY.get(key)!.sort((a, b) => a.x - b.x)) {
          const startCol = colIndex(Math.round(c.x * 10) / 10);
          const endCol = colIndex(Math.round((c.x + c.width) * 10) / 10);
          if (startCol < 1) continue;
          // Skip components whose start cell is already covered by a merge.
          if (mergedRanges.some(([a, b]) => startCol >= a && startCol <= b)) continue;
          const cell = row.getCell(startCol);
          cell.value =
            c.type === 'barcode'
              ? (c.barcode?.value ?? '')
              : c.type === 'checkbox'
                ? c.checked
                  ? '☑'
                  : '☐'
                : (c.text ?? '');
          cellStyle(cell, c);
          if (endCol > startCol + 1) {
            const mergeEnd = endCol - 1;
            const overlaps = mergedRanges.some(([a, b]) => mergeEnd >= a && startCol <= b);
            if (!overlaps) {
              ws.mergeCells(rowNum, startCol, rowNum, mergeEnd);
              mergedRanges.push([startCol, mergeEnd]);
            }
          }
        }
      }
    }
  }
}

function dataSheets(wb: ExcelJS.Workbook, model: RenderModel): void {
  for (const [name, rows] of Object.entries(model.datasets)) {
    const ws = wb.addWorksheet(name.slice(0, 31) || 'Data');
    if (rows.length === 0) {
      ws.addRow(['(tidak ada data)']);
      continue;
    }
    const keys: string[] = [];
    for (const row of rows.slice(0, 50)) {
      for (const k of Object.keys(row)) if (!keys.includes(k)) keys.push(k);
    }
    const header = ws.addRow(keys);
    header.font = { bold: true };
    for (const row of rows) {
      ws.addRow(
        keys.map((k) => {
          const v = row[k];
          if (v === null || v === undefined) return null;
          if (typeof v === 'number' || typeof v === 'boolean') return v;
          if (v instanceof Date) return v;
          const n = Number(v);
          return typeof v === 'string' && v.trim() !== '' && !Number.isNaN(n) && /^-?[\d.]+$/.test(v.trim()) ? n : String(v);
        }),
      );
    }
    keys.forEach((k, i) => {
      ws.getColumn(i + 1).width = Math.min(48, Math.max(10, k.length + 4));
    });
  }
}

export async function renderModelXlsx(model: RenderModel, mode: 'layout' | 'data'): Promise<Buffer> {
  const wb = new ExcelJS.Workbook();
  wb.creator = 'Senti ERP Report Engine';
  wb.created = new Date();
  if (mode === 'data') dataSheets(wb, model);
  else layoutSheet(wb, model);
  const buf = await wb.xlsx.writeBuffer();
  return Buffer.from(buf as ArrayBuffer);
}
