// Resolves a Kustomisasi-Grid `lookupSource` slug (+ optional default filter/sort)
// → a SearchSelect loader. Everything routes through the shared lookup-source
// registry (`buildLookupLoader`), so the 10 sources, their default filter and
// default sort behave identically to Form Builder. Legacy short slugs
// (`account`, `costCenter`, …) saved before unification are aliased.

import { buildLookupLoader } from '@/lib/lookup-source-registry';

export type GridLookupLoader = (
  search: string,
  page: number,
  limit: number,
) => Promise<{ data: { value: string; label: string; code?: unknown }[]; total: number }>;

// Old grid/seed slug → canonical registry slug.
const SOURCE_ALIAS: Record<string, string> = {
  account: 'accounts',
  partner: 'partners',
  costCenter: 'cost-centers',
  division: 'divisions',
  subdivision: 'sub-divisions',
  project: 'projects',
};

/** Normalize any historical slug to its canonical registry slug. */
export function canonicalSource(source: string | null | undefined): string {
  if (!source) return '';
  return SOURCE_ALIAS[source] ?? source;
}

/** Default loader when a column has no (or an unknown) source. */
export const DEFAULT_LOOKUP_LOADER = buildLookupLoader('accounts') as unknown as GridLookupLoader;

/**
 * Loader for a grid lookup column. Merges the column's default filter/sort into
 * every fetch. Returns null if the slug is unknown.
 */
export function gridLookupLoader(
  source: string | null | undefined,
  defaultFilter?: Record<string, unknown> | null,
  defaultSort?: string | null,
): GridLookupLoader | null {
  const s = canonicalSource(source);
  if (!s) return null;
  return buildLookupLoader(s, defaultFilter ?? null, defaultSort ?? null) as GridLookupLoader | null;
}
