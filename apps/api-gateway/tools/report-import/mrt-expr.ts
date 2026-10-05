/**
 * Expression/text normalization for imported templates (design §3.3):
 * design-time header placeholders become context bindings. The Stimulsoft
 * dialect itself is kept VERBATIM — only these host-supplied placeholders
 * are rewritten:
 *   component/text PTNAMA  → {c.companyName}
 *   RTITLE                 → {r.title}
 *   PARAM1..PARAM5         → {r.paramLines[0..4]}
 * Also applied to bare {PTNAMA} / {rtitle} / {paramN} occurrences inside
 * larger texts and to component-name-driven replacement in mrt-map.
 */

export function normalizeHeaderPlaceholders(text: string): string {
  if (!text) return text;
  let out = text;
  out = out.replace(/\{\s*PTNAMA\s*\}/gi, '{c.companyName}');
  out = out.replace(/\{\s*RTITLE\s*\}/gi, '{r.title}');
  out = out.replace(/\{\s*PARAM([1-5])\s*\}/gi, (_, d: string) => `{r.paramLines[${Number(d) - 1}]}`);
  const trimmed = out.trim();
  if (/^PTNAMA$/i.test(trimmed)) return '{c.companyName}';
  if (/^RTITLE$/i.test(trimmed)) return '{r.title}';
  const pm = /^PARAM([1-5])$/i.exec(trimmed);
  if (pm) return `{r.paramLines[${Number(pm[1]) - 1}]}`;
  return out;
}

/** Replacement driven by the component NAME (PTNAMA/RTITLE/PARAMn placeholders). */
export function placeholderForComponentName(name: string | undefined): string | null {
  if (!name) return null;
  const n = name.trim().toUpperCase();
  if (n === 'PTNAMA') return '{c.companyName}';
  if (n === 'RTITLE') return '{r.title}';
  const pm = /^PARAM([1-5])$/.exec(n);
  if (pm) return `{r.paramLines[${Number(pm[1]) - 1}]}`;
  return null;
}
