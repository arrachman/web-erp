/**
 * Barcode rendering via bwip-js (Wave G0). HTML export embeds SVG,
 * PDF/DOCX exporters embed PNG buffers. Symbology names from .mrt
 * (Stimulsoft) are mapped to bwip-js bcids; unknown → code128.
 */

// eslint-disable-next-line @typescript-eslint/no-var-requires
const bwipjs = require('bwip-js');

const SYMBOLOGY_MAP: Record<string, string> = {
  code128: 'code128',
  code128a: 'code128',
  code128b: 'code128',
  code128c: 'code128',
  code39: 'code39',
  code39extended: 'code39ext',
  code93: 'code93',
  ean13: 'ean13',
  ean8: 'ean8',
  upca: 'upca',
  upce: 'upce',
  interleaved2of5: 'interleaved2of5',
  itf14: 'itf14',
  codabar: 'rationalizedCodabar',
  msi: 'msi',
  qrcode: 'qrcode',
  datamatrix: 'datamatrix',
  pdf417: 'pdf417',
  pharmacode: 'pharmacode',
};

export function mapSymbology(name: string | undefined): string {
  if (!name) return 'code128';
  const key = name.toLowerCase().replace(/[\s_-]/g, '');
  return SYMBOLOGY_MAP[key] ?? 'code128';
}

export function barcodeSvg(value: string, symbology: string): string {
  try {
    return bwipjs.toSVG({
      bcid: mapSymbology(symbology),
      text: value || '0',
      scale: 2,
      includetext: false,
    });
  } catch {
    return '';
  }
}

export async function barcodePng(value: string, symbology: string): Promise<Buffer | null> {
  try {
    const buf = await bwipjs.toBuffer({
      bcid: mapSymbology(symbology),
      text: value || '0',
      scale: 3,
      includetext: false,
    });
    return Buffer.isBuffer(buf) ? buf : Buffer.from(buf as Uint8Array);
  } catch {
    return null;
  }
}
