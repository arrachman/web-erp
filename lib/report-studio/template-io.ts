import type { RsReport, RsTplKey, RsBand } from './types';
import { buildReport } from './templates';

/** Built-in template keys (offline fallback when the API has no templates). */
export const BUILTIN_KEYS: RsTplKey[] = ['invoice', 'sales', 'purchasing', 'finance', 'customers'];
export function isBuiltinKey(v: string): v is RsTplKey {
  return (BUILTIN_KEYS as string[]).includes(v);
}

/**
 * A persisted `templateJson` is ReportStudio-native only if every band has an
 * `els` array + string `type`. Legacy designer templates also use `bands` but
 * with a different element schema, so they are rejected here (→ default layout).
 */
export function isRsReport(json: unknown): json is RsReport {
  const j = json as { bands?: unknown[] } | null;
  if (!j || !Array.isArray(j.bands) || j.bands.length === 0) return false;
  return j.bands.every((b) => {
    const bb = b as Partial<RsBand>;
    return !!bb && Array.isArray(bb.els) && typeof bb.type === 'string';
  });
}

/** Map a backend template `module` to the closest built-in starter layout. */
const MODULE_DEFAULT: Record<string, RsTplKey> = {
  SALES: 'sales', SLS: 'sales',
  PURCHASING: 'purchasing', PUR: 'purchasing',
  FINANCE: 'finance', FIN: 'finance',
  INVENTORY: 'customers', INV: 'customers',
};

/**
 * Resolve an editable RsReport from a backend record's `templateJson`.
 * Native ReportStudio JSON is used as-is; anything else (legacy/empty) opens a
 * module-appropriate starter layout the user can redesign and save.
 */
export function reportFromTemplateJson(json: unknown, module: string, nextId: () => number): RsReport {
  if (isRsReport(json)) return json;
  const key = MODULE_DEFAULT[(module || '').toUpperCase()] || 'invoice';
  const r = buildReport(key, nextId);
  r.key = key;
  return r;
}
