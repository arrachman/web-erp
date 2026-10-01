/**
 * Shared helpers for Sales (M1) report resolvers. Keeps resolver files under
 * the 400-line cap and column shapes DRY. All helpers are read-only and
 * Decimal-safe.
 */

import { PrismaService } from '../prisma/prisma.service';
import { ReportFilters } from './report-types';

/** A Prisma.Decimal-like value (has .toNumber) or a plain number/null. */
type DecimalLike = { toNumber(): number } | number | null | undefined;

/** Safely coerce a Prisma.Decimal (or number) to a JS number. */
export function num(v: DecimalLike): number {
  if (v == null) return 0;
  if (typeof v === 'number') return v;
  return v.toNumber();
}

/** Coerce a Date (or null) to an ISO date string ('' when null). */
export function isoDate(d: Date | null | undefined): string {
  return d ? d.toISOString() : '';
}

/** Code + name display string, e.g. "C001 — PT Maju". '' when missing. */
type Ref = { id: string; code: string; name: string } | null;
export function display(ref: Ref): string {
  if (!ref) return '';
  if (ref.code && ref.name) return `${ref.code} — ${ref.name}`;
  return ref.name || ref.code || '';
}

/** Clamp pagination to sane bounds (default limit 50, max 500). */
export function paginate(
  filters: ReportFilters,
): { skip: number; take: number; page: number; limit: number } {
  const limit = Math.min(Math.max(Number(filters.limit) || 50, 1), 500);
  const page = Math.max(Number(filters.page) || 1, 1);
  return { skip: (page - 1) * limit, take: limit, page, limit };
}

/** Build the common document date-range filter for a given date field. */
export function dateRange(
  field: string,
  filters: ReportFilters,
): Record<string, unknown> {
  const range: Record<string, Date> = {};
  if (filters.dateFrom) range.gte = new Date(filters.dateFrom);
  if (filters.dateTo) range.lte = new Date(filters.dateTo);
  return Object.keys(range).length ? { [field]: range } : {};
}

/** Doc-number search (case-insensitive contains). */
export function searchWhere(filters: ReportFilters): Record<string, unknown> {
  if (!filters.search) return {};
  return { docNumber: { contains: filters.search, mode: 'insensitive' } };
}

/** Status equality filter when provided. */
export function statusWhere(filters: ReportFilters): Record<string, unknown> {
  return filters.status ? { status: filters.status } : {};
}

/** Soft-delete guard shared by every read query. */
export const SOFT_DELETE = { deletedAt: null } as const;

/**
 * Merge the standard filter predicates for a header doc on a given date field
 * (soft-delete + date range + status + doc-number search).
 */
export function baseWhere(
  dateField: string,
  filters: ReportFilters,
): Record<string, unknown> {
  return {
    ...SOFT_DELETE,
    ...dateRange(dateField, filters),
    ...statusWhere(filters),
    ...searchWhere(filters),
  };
}

const SELECT = { id: true, code: true, name: true } as const;

async function toMap(
  rows: Promise<{ id: bigint; code: string; name: string }[]>,
): Promise<Map<string, Ref>> {
  const map = new Map<string, Ref>();
  for (const r of await rows) {
    map.set(r.id.toString(), {
      id: r.id.toString(),
      code: r.code,
      name: r.name,
    });
  }
  return map;
}

export interface PartnerInfo {
  id: string;
  code: string;
  name: string;
  salesmanId: string | null;
  categoryId: string | null;
}

/**
 * Batch-resolve md_partners with salesmanId + categoryId for sales analytics.
 */
export async function resolvePartners(
  prisma: PrismaService,
  ids: (bigint | null)[],
): Promise<Map<string, PartnerInfo>> {
  const uniq = [...new Set(ids.filter((v): v is bigint => v != null))];
  if (!uniq.length) return new Map();
  const rows = await prisma.erpPartner.findMany({
    where: { id: { in: uniq } },
    select: { id: true, code: true, name: true, salesmanId: true, categoryId: true },
  });
  const map = new Map<string, PartnerInfo>();
  for (const r of rows) {
    map.set(r.id.toString(), {
      id: r.id.toString(),
      code: r.code,
      name: r.name,
      salesmanId: r.salesmanId?.toString() ?? null,
      categoryId: r.categoryId?.toString() ?? null,
    });
  }
  return map;
}

/**
 * Batch-resolve md_items code+name.
 */
export async function resolveItems(
  prisma: PrismaService,
  ids: (bigint | null)[],
): Promise<Map<string, Ref>> {
  const uniq = [...new Set(ids.filter((v): v is bigint => v != null))];
  if (!uniq.length) return new Map();
  return toMap(
    prisma.erpItem.findMany({ where: { id: { in: uniq } }, select: SELECT }),
  );
}

/**
 * Batch-resolve any simple id→{code,name} table.
 */
export async function resolveRefs(
  rows: Promise<{ id: bigint; code: string; name: string }[]>,
): Promise<Map<string, Ref>> {
  return toMap(rows);
}

/** Look up a Ref from a map by bigint id. */
export function ref(map: Map<string, Ref>, id: bigint | null | undefined): Ref {
  return id != null ? (map.get(id.toString()) ?? null) : null;
}
