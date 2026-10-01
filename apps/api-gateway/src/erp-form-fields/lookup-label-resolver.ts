import { PrismaService } from '../prisma/prisma.service';

// Resolves the human label for a lookup field's stored `defaultValue` (an id).
// Form Builder persists only the id; the front-end SearchSelect can't always
// resolve it back to a label on reopen (the row may sit outside the first page
// of options), so the saved default looked "empty". We resolve it server-side
// where the data lives and return `defaultValueLabel` alongside each field.

type LabelRow = { id: bigint; code: string | null; name: string };
type LabelDelegate = { findMany: (args: unknown) => Promise<LabelRow[]> };

const FIELD_TYPES_WITH_SLUG: Record<string, string> = {
  PARTNER: 'partners',
  ACCOUNT: 'accounts',
  BRANCH: 'branches',
  LOCATION: 'locations',
  CURRENCY: 'currencies',
};

// Legacy/short slugs saved before the registry was unified → canonical slug.
const SLUG_ALIAS: Record<string, string> = {
  account: 'accounts',
  partner: 'partners',
  costCenter: 'cost-centers',
  division: 'divisions',
  subdivision: 'sub-divisions',
  project: 'projects',
};

function canonicalSlug(slug?: string | null): string {
  if (!slug) return '';
  return SLUG_ALIAS[slug] ?? slug;
}

function delegateForSlug(prisma: PrismaService, slug: string): LabelDelegate | null {
  const map: Record<string, LabelDelegate> = {
    partners: prisma.erpPartner as unknown as LabelDelegate,
    accounts: prisma.erpAccount as unknown as LabelDelegate,
    branches: prisma.erpBranch as unknown as LabelDelegate,
    locations: prisma.erpLocation as unknown as LabelDelegate,
    currencies: prisma.erpCurrency as unknown as LabelDelegate,
    'cost-centers': prisma.erpCostCenter as unknown as LabelDelegate,
    divisions: prisma.erpDivision as unknown as LabelDelegate,
    'sub-divisions': prisma.erpSubdivision as unknown as LabelDelegate,
    warehouses: prisma.erpWarehouse as unknown as LabelDelegate,
    projects: prisma.erpProject as unknown as LabelDelegate,
  };
  return map[slug] ?? null;
}

/** Mirrors SearchSelect's optLabel: "{code} - {name}" when code is present. */
function formatLabel(row: LabelRow): string {
  return row.code ? `${row.code} - ${row.name}` : row.name;
}

const isLookup = (fieldType: string) =>
  fieldType in FIELD_TYPES_WITH_SLUG || fieldType === 'LOOKUP';

const slugForField = (f: { fieldType: string; lookupSource: string | null }) =>
  f.fieldType === 'LOOKUP' ? canonicalSlug(f.lookupSource) : FIELD_TYPES_WITH_SLUG[f.fieldType];

/**
 * Attach `defaultValueLabel` to every lookup field that has a `defaultValue`.
 * Groups ids per source so each source is a single query.
 */
export async function withDefaultValueLabels<
  T extends { fieldType: string; lookupSource: string | null; defaultValue: string | null },
>(prisma: PrismaService, fields: T[]): Promise<(T & { defaultValueLabel: string | null })[]> {
  const idsBySlug = new Map<string, Set<bigint>>();
  for (const f of fields) {
    if (!f.defaultValue || !isLookup(f.fieldType)) continue;
    const slug = slugForField(f);
    if (!slug || !delegateForSlug(prisma, slug)) continue;
    let id: bigint;
    try { id = BigInt(f.defaultValue); } catch { continue; }
    if (!idsBySlug.has(slug)) idsBySlug.set(slug, new Set());
    idsBySlug.get(slug)!.add(id);
  }

  const labelBySlugId = new Map<string, string>();
  await Promise.all(
    [...idsBySlug.entries()].map(async ([slug, ids]) => {
      const rows = await delegateForSlug(prisma, slug)!.findMany({
        where: { id: { in: [...ids] } },
        select: { id: true, code: true, name: true },
      });
      for (const r of rows) labelBySlugId.set(`${slug}:${r.id}`, formatLabel(r));
    }),
  );

  return fields.map((f) => {
    let label: string | null = null;
    if (f.defaultValue && isLookup(f.fieldType)) {
      const slug = slugForField(f);
      if (slug) label = labelBySlugId.get(`${slug}:${f.defaultValue}`) ?? null;
    }
    return { ...f, defaultValueLabel: label };
  });
}
