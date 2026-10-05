/**
 * PDF exporter (Wave G0) — the reference renderer of the RenderModel,
 * drawn absolutely with @react-pdf/renderer (same engine as v1).
 * Fonts: DejaVu Sans (metric substitute for Lao UI) registered from the
 * container's system fonts when present; Helvetica fallback otherwise.
 */

import * as fs from 'fs';
import { Document, Image, Page, Text, View, renderToBuffer } from '@react-pdf/renderer';

// Font exists at runtime but is missing from this version's bundled types.
// eslint-disable-next-line @typescript-eslint/no-var-requires
const { Font } = require('@react-pdf/renderer') as {
  Font: {
    register: (cfg: unknown) => void;
    registerHyphenationCallback: (cb: (word: string) => string[]) => void;
  };
};
import * as React from 'react';
import type { ModelComponent, RenderModel } from '../engine-types-v2';
import { mm } from '../geometry';
import { barcodePng } from '../barcode';

let fontsRegistered = false;
let fontFamily = 'Helvetica';

function ensureFonts(): string {
  if (fontsRegistered) return fontFamily;
  fontsRegistered = true;
  const dir = '/usr/share/fonts/truetype/dejavu';
  const files = {
    normal: `${dir}/DejaVuSans.ttf`,
    bold: `${dir}/DejaVuSans-Bold.ttf`,
    oblique: `${dir}/DejaVuSans-Oblique.ttf`,
    boldOblique: `${dir}/DejaVuSans-BoldOblique.ttf`,
  };
  try {
    if (fs.existsSync(files.normal)) {
      Font.register({
        family: 'DejaVu Sans',
        fonts: [
          { src: files.normal },
          ...(fs.existsSync(files.bold) ? [{ src: files.bold, fontWeight: 700 as const }] : []),
          ...(fs.existsSync(files.oblique) ? [{ src: files.oblique, fontStyle: 'italic' as const }] : []),
          ...(fs.existsSync(files.boldOblique)
            ? [{ src: files.boldOblique, fontWeight: 700 as const, fontStyle: 'italic' as const }]
            : []),
        ],
      });
      fontFamily = 'DejaVu Sans';
    }
  } catch {
    fontFamily = 'Helvetica';
  }
  Font.registerHyphenationCallback((word) => [word]);
  return fontFamily;
}

function borderProps(c: ModelComponent): Record<string, unknown> {
  const b = c.style?.border;
  if (!b) return {};
  const w = b.width ?? 0.4;
  const color = b.color ?? '#000';
  const style = b.style === 'dashed' ? 'dashed' : b.style === 'dotted' ? 'dotted' : 'solid';
  const out: Record<string, unknown> = {};
  if (b.sides.includes('all')) {
    out.borderWidth = w;
    out.borderColor = color;
    out.borderStyle = style;
  } else {
    if (b.sides.includes('top')) { out.borderTopWidth = w; out.borderTopColor = color; out.borderTopStyle = style; }
    if (b.sides.includes('bottom')) { out.borderBottomWidth = w; out.borderBottomColor = color; out.borderBottomStyle = style; }
    if (b.sides.includes('left')) { out.borderLeftWidth = w; out.borderLeftColor = color; out.borderLeftStyle = style; }
    if (b.sides.includes('right')) { out.borderRightWidth = w; out.borderRightColor = color; out.borderRightStyle = style; }
  }
  return out;
}

function textStyle(c: ModelComponent, family: string): Record<string, unknown> {
  const st = c.style;
  const out: Record<string, unknown> = {
    position: 'absolute',
    left: mm(c.x),
    top: mm(c.y),
    width: mm(c.width),
    fontFamily: family,
    fontSize: st?.fontSize ?? 9,
    lineHeight: 1.22,
  };
  if (st?.bold) out.fontWeight = 700;
  if (st?.italic) out.fontStyle = 'italic';
  if (st?.color) out.color = st.color;
  if (st?.align) out.textAlign = st.align;
  if (st?.background && st.background.toLowerCase() !== 'transparent') out.backgroundColor = st.background;
  return { ...out, ...borderProps(c) };
}

export async function renderModelPdf(model: RenderModel): Promise<Buffer> {
  const family = ensureFonts();
  // Pre-render barcodes to PNG data URIs (bwip-js is async).
  const barcodeUris = new Map<string, string>();
  for (const page of model.pages) {
    for (const band of page.bands) {
      for (const comp of band.components) {
        if (comp.type === 'barcode' && comp.barcode) {
          const key = `${comp.barcode.symbology}|${comp.barcode.value}`;
          if (!barcodeUris.has(key)) {
            const png = await barcodePng(comp.barcode.value, comp.barcode.symbology);
            if (png) barcodeUris.set(key, `data:image/png;base64,${png.toString('base64')}`);
          }
        }
      }
    }
  }

  const renderComp = (c: ModelComponent, idx: number): React.ReactNode => {
    const key = `c${idx}`;
    if (c.type === 'text') {
      return React.createElement(Text, { key, style: textStyle(c, family) }, c.text ?? '');
    }
    if (c.type === 'line') {
      const horizontal = c.height <= c.width;
      return React.createElement(View, {
        key,
        style: {
          position: 'absolute',
          left: mm(c.x),
          top: mm(c.y),
          width: horizontal ? mm(c.width) : 0,
          height: horizontal ? 0 : mm(c.height),
          borderTopWidth: horizontal ? (c.lineWidth ?? 0.5) : 0,
          borderLeftWidth: horizontal ? 0 : (c.lineWidth ?? 0.5),
          borderColor: c.style?.color ?? '#000',
        },
      });
    }
    if (c.type === 'box') {
      return React.createElement(View, {
        key,
        style: {
          position: 'absolute',
          left: mm(c.x),
          top: mm(c.y),
          width: mm(c.width),
          height: mm(c.height),
          backgroundColor: c.fill && c.fill.toLowerCase() !== 'transparent' ? c.fill : undefined,
          ...borderProps(c),
        },
      });
    }
    if (c.type === 'image' && c.src) {
      return React.createElement(Image, {
        key,
        src: c.src,
        style: { position: 'absolute', left: mm(c.x), top: mm(c.y), width: mm(c.width), height: mm(c.height), objectFit: 'contain' },
      });
    }
    if (c.type === 'barcode' && c.barcode) {
      const uri = barcodeUris.get(`${c.barcode.symbology}|${c.barcode.value}`);
      if (!uri) return null;
      return React.createElement(Image, {
        key,
        src: uri,
        style: { position: 'absolute', left: mm(c.x), top: mm(c.y), width: mm(c.width), height: mm(c.height) },
      });
    }
    if (c.type === 'checkbox') {
      const size = Math.min(c.width, c.height);
      return React.createElement(
        View,
        {
          key,
          style: {
            position: 'absolute',
            left: mm(c.x),
            top: mm(c.y),
            width: mm(size),
            height: mm(size),
            borderWidth: 0.5,
            borderColor: '#000',
            alignItems: 'center',
            justifyContent: 'center',
          },
        },
        c.checked
          ? React.createElement(Text, { style: { fontFamily: family, fontSize: 7 } }, 'X')
          : null,
      );
    }
    return null;
  };

  const doc = React.createElement(
    Document,
    { title: model.name ?? 'Laporan' },
    model.pages.map((page, pi) =>
      React.createElement(
        Page,
        {
          key: `p${pi}`,
          size: [mm(model.pageWidthMm), mm(model.pageHeightMm)],
          style: { fontFamily: family, position: 'relative' },
        },
        page.bands.flatMap((band, bi) => band.components.map((c, ci) => renderComp(c, bi * 1000 + ci))),
      ),
    ),
  );
  return renderToBuffer(doc);
}
