/**
 * Shared filter/result types for the read-only Warehouse Statistics module.
 * All BigInt ids are surfaced as strings and all Decimals as numbers so the
 * JSON responses are safe to serialize directly.
 */

export interface StatsFilters {
  dateFrom: string | null;
  dateTo: string | null;
  branchId: bigint | null;
  warehouseId: bigint | null;
  limit: number;
}

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

export interface KpiSummary {
  totalItems: number;
  belowMinCount: number;
  pendingApprovals: number;
  periodRevenue: number;
  periodQtySold: number;
  stockValue: number;
}
