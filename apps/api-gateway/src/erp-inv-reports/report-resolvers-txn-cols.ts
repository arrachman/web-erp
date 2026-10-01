/**
 * Shared column definitions + helpers for the transaction/item report
 * resolvers. Keeps `report-resolvers-txn.ts` under the 400-line cap and the
 * column shapes DRY. All helpers are read-only and Decimal-safe.
 */

import { PrismaService } from '../prisma/prisma.service';
import { ReportColumn, ReportFilters } from './report-types';

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

/** Code + name display string, e.g. "WH01 — Main Warehouse". '' when missing. */
type Ref = { id: string; code: string; name: string } | null;
export function display(ref: Ref): string {
  if (!ref) return '';
  if (ref.code && ref.name) return `${ref.code} — ${ref.name}`;
  return ref.name || ref.code || '';
}

/** Clamp pagination to sane bounds (default limit 50, max 500). */
export function paginate(filters: ReportFilters): { skip: number; take: number; page: number; limit: number } {
  const limit = Math.min(Math.max(Number(filters.limit) || 50, 1), 500);
  const page = Math.max(Number(filters.page) || 1, 1);
  return { skip: (page - 1) * limit, take: limit, page, limit };
}

/** Build the common document date-range filter for a given date field. */
export function dateRange(field: string, filters: ReportFilters): Record<string, unknown> {
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
 * (soft-delete + date range + status + doc-number search + optional warehouse).
 */
export function baseWhere(dateField: string, filters: ReportFilters): Record<string, unknown> {
  const where: Record<string, unknown> = {
    ...SOFT_DELETE,
    ...dateRange(dateField, filters),
    ...statusWhere(filters),
    ...searchWhere(filters),
  };
  if (filters.warehouseId) where.warehouseId = BigInt(filters.warehouseId);
  return where;
}

/**
 * Batch-resolve md_items / md_warehouses / md_partners code+name in one pass.
 * Mirrors the enrich-helper pattern (scalar BigInt FKs, no @relation).
 */
const SELECT = { id: true, code: true, name: true } as const;

async function toMap(rows: Promise<{ id: bigint; code: string; name: string }[]>): Promise<Map<string, Ref>> {
  const map = new Map<string, Ref>();
  for (const r of await rows) {
    map.set(r.id.toString(), { id: r.id.toString(), code: r.code, name: r.name });
  }
  return map;
}

export interface NameMaps {
  item: Map<string, Ref>;
  warehouse: Map<string, Ref>;
  partner: Map<string, Ref>;
  ref: (map: Map<string, Ref>, id: bigint | null | undefined) => Ref;
}

export async function resolveNames(
  prisma: PrismaService,
  ids: { itemIds?: (bigint | null)[]; warehouseIds?: (bigint | null)[]; partnerIds?: (bigint | null)[] },
): Promise<NameMaps> {
  const uniq = (arr?: (bigint | null)[]) => [...new Set((arr ?? []).filter((v): v is bigint => v != null))];

  const [item, warehouse, partner] = await Promise.all([
    toMap(prisma.erpItem.findMany({ where: { id: { in: uniq(ids.itemIds) } }, select: SELECT })),
    toMap(prisma.erpWarehouse.findMany({ where: { id: { in: uniq(ids.warehouseIds) } }, select: SELECT })),
    toMap(prisma.erpPartner.findMany({ where: { id: { in: uniq(ids.partnerIds) } }, select: SELECT })),
  ]);

  const ref = (map: Map<string, Ref>, id: bigint | null | undefined): Ref =>
    id != null ? map.get(id.toString()) ?? null : null;

  return { item, warehouse, partner, ref };
}

/* ---------------------------------------------------------------- columns */

/** Columns shared by the five stock-movement reports (MR/TS/RS/RF/Return). */
export const MOVEMENT_COLUMNS: ReportColumn[] = [
  { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
  { key: 'movementDate', header: 'Tanggal', type: 'date' },
  { key: 'sourceWarehouse', header: 'Gudang Asal', type: 'text' },
  { key: 'destinationWarehouse', header: 'Gudang Tujuan', type: 'text' },
  { key: 'referenceNo', header: 'No. Referensi', type: 'text' },
  { key: 'status', header: 'Status', type: 'status' },
  { key: 'totalQty', header: 'Total Qty', type: 'qty' },
  { key: 'lineCount', header: 'Jml Baris', type: 'number' },
];

export const STOCK_COUNT_COLUMNS: ReportColumn[] = [
  { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
  { key: 'countDate', header: 'Tanggal', type: 'date' },
  { key: 'warehouse', header: 'Gudang', type: 'text' },
  { key: 'status', header: 'Status', type: 'status' },
  { key: 'lineCount', header: 'Jml Baris', type: 'number' },
  { key: 'totalVarianceQty', header: 'Total Selisih', type: 'qty' },
];

export const STOCK_ADJUSTMENT_COLUMNS: ReportColumn[] = [
  { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
  { key: 'adjustmentDate', header: 'Tanggal', type: 'date' },
  { key: 'warehouse', header: 'Gudang', type: 'text' },
  { key: 'status', header: 'Status', type: 'status' },
  { key: 'lineCount', header: 'Jml Baris', type: 'number' },
  { key: 'totalValue', header: 'Total Nilai', type: 'money' },
];

export const PRICE_ADJUSTMENT_COLUMNS: ReportColumn[] = [
  { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
  { key: 'fromDate', header: 'Dari Tanggal', type: 'date' },
  { key: 'toDate', header: 'Sampai Tanggal', type: 'date' },
  { key: 'warehouse', header: 'Gudang', type: 'text' },
  { key: 'item', header: 'Item', type: 'text' },
  { key: 'status', header: 'Status', type: 'status' },
  { key: 'totalDelta', header: 'Total Delta', type: 'money' },
];

export const OPENING_STOCK_COLUMNS: ReportColumn[] = [
  { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
  { key: 'openingDate', header: 'Tanggal', type: 'date' },
  { key: 'warehouse', header: 'Gudang', type: 'text' },
  { key: 'status', header: 'Status', type: 'status' },
  { key: 'lineCount', header: 'Jml Baris', type: 'number' },
  { key: 'totalValue', header: 'Total Nilai', type: 'money' },
];

export const DAILY_CHECK_COLUMNS: ReportColumn[] = [
  { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
  { key: 'checkDate', header: 'Tanggal', type: 'date' },
  { key: 'branch', header: 'Cabang', type: 'text' },
  { key: 'machineRef', header: 'Mesin', type: 'text' },
  { key: 'operatorRef', header: 'Operator', type: 'text' },
  { key: 'status', header: 'Status', type: 'status' },
  { key: 'lineCount', header: 'Jml Baris', type: 'number' },
];

export const WEIGHBRIDGE_COLUMNS: ReportColumn[] = [
  { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
  { key: 'weighDate', header: 'Tanggal', type: 'date' },
  { key: 'partner', header: 'Partner', type: 'text' },
  { key: 'grossWeight', header: 'Bruto', type: 'number' },
  { key: 'tareWeight', header: 'Tara', type: 'number' },
  { key: 'netWeight', header: 'Netto', type: 'number' },
  { key: 'status', header: 'Status', type: 'status' },
];

export const BATCH_ITEM_COLUMNS: ReportColumn[] = [
  { key: 'itemCode', header: 'Kode Item', type: 'text' },
  { key: 'itemName', header: 'Nama Item', type: 'text' },
  { key: 'batchNo', header: 'No. Batch', type: 'text' },
  { key: 'warehouse', header: 'Gudang', type: 'text' },
  { key: 'quantity', header: 'Qty', type: 'qty' },
  { key: 'expiryDate', header: 'Kadaluarsa', type: 'date' },
];

export const SERIAL_ITEM_COLUMNS: ReportColumn[] = [
  { key: 'itemCode', header: 'Kode Item', type: 'text' },
  { key: 'itemName', header: 'Nama Item', type: 'text' },
  { key: 'serialNo', header: 'No. Serial', type: 'text' },
  { key: 'warehouse', header: 'Gudang', type: 'text' },
  { key: 'status', header: 'Status', type: 'text' },
];
