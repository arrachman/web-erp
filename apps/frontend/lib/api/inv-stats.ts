/**
 * Warehouse Statistics (M3) API client.
 *
 * Wraps `GET /inv/stats/:metric` for the 6 statistics widgets. Each call
 * goes through the shared `apiGet` wrapper (same BASE_URL + cookie auth);
 * the backend returns `{ success: true, data, ... }`. Types mirror the
 * backend contract.
 */

import { apiGet } from './client';

// ─── Shared query params ──────────────────────────────────────────────────────

export interface StatsParams {
  dateFrom?: string;
  dateTo?: string;
  branchId?: string;
  warehouseId?: string;
  limit?: number;
}

// ─── Row/data shapes (mirror backend) ─────────────────────────────────────────

export interface TopRevenueRow {
  itemId: string;
  itemCode: string;
  itemName: string;
  revenue: number;
}

export interface BestSellingRow {
  itemId: string;
  itemCode: string;
  itemName: string;
  qty: number;
  unitName?: string;
}

export interface MostProfitableRow {
  itemId: string;
  itemCode: string;
  itemName: string;
  revenue: number;
  cogs: number;
  profit: number;
  marginPct: number;
}

export interface BelowMinimumRow {
  itemId: string;
  itemCode: string;
  itemName: string;
  warehouseName?: string;
  onHand: number;
  minQty: number;
  shortage: number;
}

export interface ApprovalRow {
  docType: string;
  label: string;
  count: number;
}

export interface StatsKpi {
  totalItems: number;
  belowMinCount: number;
  pendingApprovals: number;
  periodRevenue: number;
  periodQtySold: number;
  stockValue: number;
}

// ─── Response envelopes ───────────────────────────────────────────────────────

interface StatsResponse<T> {
  success?: boolean;
  data: T;
}

interface MostProfitableResponse extends StatsResponse<MostProfitableRow[]> {
  note?: string;
}

interface ApprovalsResponse extends StatsResponse<ApprovalRow[]> {
  total?: number;
}

// ─── Helpers ──────────────────────────────────────────────────────────────────

function toQuery(
  params?: StatsParams,
): Record<string, string | number | undefined> {
  if (!params) return {};
  const out: Record<string, string | number | undefined> = {};
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === null || value === '') continue;
    out[key] = value as string | number;
  }
  return out;
}

// ─── API calls ────────────────────────────────────────────────────────────────

export async function getTopRevenue(
  params?: StatsParams,
): Promise<TopRevenueRow[]> {
  const res = await apiGet<StatsResponse<TopRevenueRow[]>>(
    '/inv/stats/top-revenue',
    toQuery(params),
  );
  return res.data ?? [];
}

export async function getBestSelling(
  params?: StatsParams,
): Promise<BestSellingRow[]> {
  const res = await apiGet<StatsResponse<BestSellingRow[]>>(
    '/inv/stats/best-selling',
    toQuery(params),
  );
  return res.data ?? [];
}

export async function getMostProfitable(
  params?: StatsParams,
): Promise<{ rows: MostProfitableRow[]; note?: string }> {
  const res = await apiGet<MostProfitableResponse>(
    '/inv/stats/most-profitable',
    toQuery(params),
  );
  return { rows: res.data ?? [], note: res.note };
}

export async function getBelowMinimum(
  params?: StatsParams,
): Promise<BelowMinimumRow[]> {
  const res = await apiGet<StatsResponse<BelowMinimumRow[]>>(
    '/inv/stats/below-minimum',
    toQuery(params),
  );
  return res.data ?? [];
}

export async function getApprovals(
  params?: StatsParams,
): Promise<{ rows: ApprovalRow[]; total: number }> {
  const res = await apiGet<ApprovalsResponse>(
    '/inv/stats/approvals',
    toQuery(params),
  );
  return { rows: res.data ?? [], total: res.total ?? 0 };
}

export async function getKpi(params?: StatsParams): Promise<StatsKpi> {
  const res = await apiGet<StatsResponse<StatsKpi>>(
    '/inv/stats/kpi',
    toQuery(params),
  );
  return res.data;
}
