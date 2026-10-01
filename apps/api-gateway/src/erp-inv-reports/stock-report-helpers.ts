/**
 * Shared helpers for the stock report resolvers: cross-domain name batching
 * (item/warehouse code+name — scalar BigInt FKs with no Prisma @relation, same
 * pattern as inv-price-adjustment-enrich.ts), Decimal→number coercion, date
 * parsing, and pagination.
 */

import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { ReportFilters } from './report-types';

export type NameRef = { code: string; name: string };

const SELECT = { id: true, code: true, name: true } as const;

/** Batch-resolve md_items id → {code,name}. */
export async function itemNameMap(
  prisma: PrismaService,
  ids: bigint[],
): Promise<Map<string, NameRef>> {
  const map = new Map<string, NameRef>();
  if (!ids.length) return map;
  const rows = await prisma.erpItem.findMany({
    where: { id: { in: [...new Set(ids)] } },
    select: SELECT,
  });
  for (const r of rows) map.set(r.id.toString(), { code: r.code, name: r.name });
  return map;
}

/** Batch-resolve md_warehouses id → {code,name}. */
export async function warehouseNameMap(
  prisma: PrismaService,
  ids: bigint[],
): Promise<Map<string, NameRef>> {
  const map = new Map<string, NameRef>();
  if (!ids.length) return map;
  const rows = await prisma.erpWarehouse.findMany({
    where: { id: { in: [...new Set(ids)] } },
    select: SELECT,
  });
  for (const r of rows) map.set(r.id.toString(), { code: r.code, name: r.name });
  return map;
}

/** Decimal | number | string | null → number (0 for null/NaN). */
export function num(v: Prisma.Decimal | number | string | null | undefined): number {
  if (v == null) return 0;
  const n = typeof v === 'object' ? v.toNumber() : Number(v);
  return Number.isFinite(n) ? n : 0;
}

/** Date | string | null → ISO date (YYYY-MM-DD) string, or '' when absent. */
export function isoDate(v: Date | string | null | undefined): string {
  if (!v) return '';
  const d = v instanceof Date ? v : new Date(v);
  return Number.isNaN(d.getTime()) ? '' : d.toISOString().slice(0, 10);
}

/** Parse a filter date string → Date (UTC midnight), or null when absent/invalid. */
export function parseDate(v?: string): Date | null {
  if (!v) return null;
  const d = new Date(v.length <= 10 ? `${v}T00:00:00.000Z` : v);
  return Number.isNaN(d.getTime()) ? null : d;
}

/** Optional bigint id from a string filter. */
export function parseId(v?: string): bigint | null {
  if (!v) return null;
  try {
    return BigInt(v);
  } catch {
    return null;
  }
}

/** 1-based page + limit from filters (defaults: page 1, limit 50, cap 500). */
export function paging(filters: ReportFilters): { page: number; limit: number; skip: number } {
  const page = Math.max(1, Math.floor(filters.page ?? 1));
  const limit = Math.min(500, Math.max(1, Math.floor(filters.limit ?? 50)));
  return { page, limit, skip: (page - 1) * limit };
}

/** Slice an in-memory row array per the filters' page/limit. */
export function paginate<T>(rows: T[], filters: ReportFilters): T[] {
  const { skip, limit } = paging(filters);
  return rows.slice(skip, skip + limit);
}
