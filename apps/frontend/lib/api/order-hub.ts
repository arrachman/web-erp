// A2 Order Hub API — antrean order lintas kanal di atas sls_orders.
// Endpoints: /order-hub (controller erp/order-hub)

import { apiGet, apiPost } from './client';
import type { ApiResponse, PaginatedResponse } from './types';

export type ErpSalesChannel =
  | 'STANDARD'
  | 'POS'
  | 'SIPLAH'
  | 'SALES'
  | 'ADMIN'
  | 'PORTAL_SEKOLAH'
  | 'PORTAL_ORANGTUA';

export type ErpFundingSource = 'BOS' | 'NON_BOS';

export type ErpOrderHubStage =
  | 'BARU'
  | 'DIKONFIRMASI'
  | 'DISIAPKAN'
  | 'DIKIRIM'
  | 'DITERIMA'
  | 'DITAGIH'
  | 'LUNAS';

export const HUB_STAGES: ErpOrderHubStage[] = [
  'BARU',
  'DIKONFIRMASI',
  'DISIAPKAN',
  'DIKIRIM',
  'DITERIMA',
  'DITAGIH',
  'LUNAS',
];

export const HUB_STAGE_LABELS: Record<ErpOrderHubStage, string> = {
  BARU: 'Baru',
  DIKONFIRMASI: 'Dikonfirmasi',
  DISIAPKAN: 'Disiapkan',
  DIKIRIM: 'Dikirim',
  DITERIMA: 'Diterima (BAST)',
  DITAGIH: 'Ditagih',
  LUNAS: 'Lunas',
};

export const CHANNEL_LABELS: Record<ErpSalesChannel, string> = {
  STANDARD: 'Standar',
  POS: 'POS',
  SIPLAH: 'SIPLah',
  SALES: 'Sales',
  ADMIN: 'Admin',
  PORTAL_SEKOLAH: 'Portal Sekolah',
  PORTAL_ORANGTUA: 'Portal Orang Tua',
};

export interface OrderHubLine {
  id: string;
  itemId: string;
  quantity: string;
  unitPrice: string;
  lineNo: number;
  [key: string]: unknown;
}

export interface OrderHubRow {
  id: string;
  docNumber: string;
  docDate: string;
  status: string;
  channel: ErpSalesChannel;
  externalOrderId?: string | null;
  fundingSource?: ErpFundingSource | null;
  budgetYear?: number | null;
  budgetStage?: number | null;
  needsProduction: boolean;
  marketplaceFee?: string | null;
  disbursementRef?: string | null;
  grandTotal: string;
  customerId?: string | null;
  customer?: { id: string; code: string; name: string } | null;
  hubStage: ErpOrderHubStage | null;
  hubCounts: { deliveryOrders: number; deliveryReports: number; invoices: number };
  lines?: OrderHubLine[];
}

export interface OrderHubChainDoc {
  id: string;
  docNumber: string;
  docDate: string;
  status: string;
  postingStatus?: string;
  settlementStatus?: string;
  grandTotal?: string;
  deliveryOrderId?: string;
}

export interface OrderHubStatusLog {
  id: string;
  hubStatus: ErpOrderHubStage;
  source: string;
  note?: string | null;
  createdAt: string;
}

export interface OrderHubDetail extends OrderHubRow {
  chain: {
    deliveryOrders: OrderHubChainDoc[];
    deliveryReports: OrderHubChainDoc[];
    invoices: OrderHubChainDoc[];
  };
  statusLogs: OrderHubStatusLog[];
}

export interface ListOrderHubParams {
  page?: number;
  limit?: number;
  search?: string;
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
  channel?: ErpSalesChannel;
  hubStatus?: ErpOrderHubStage;
  fundingSource?: ErpFundingSource;
  budgetYear?: number;
  customerId?: string;
}

export function listOrderHub(params?: ListOrderHubParams): Promise<PaginatedResponse<OrderHubRow>> {
  return apiGet<PaginatedResponse<OrderHubRow>>(
    '/order-hub',
    params as unknown as Record<string, string | number | boolean | undefined>,
  );
}

export async function getOrderHubDetail(orderId: string): Promise<OrderHubDetail> {
  const res = await apiGet<ApiResponse<OrderHubDetail>>(`/order-hub/${orderId}`);
  return res.data;
}

export interface ImportSiplahRow {
  externalOrderId: string;
  orderDate: string;
  schoolCode?: string;
  schoolNpsn?: string;
  schoolName?: string;
  itemCode: string;
  quantity: string;
  unitPrice?: string;
  fundingSource?: ErpFundingSource;
  budgetYear?: number;
  budgetStage?: number;
  marketplaceFee?: string;
  disbursementRef?: string;
}

export interface ImportSiplahResult {
  totalOrders: number;
  created: number;
  skipped: number;
  errors: number;
  results: Array<{
    externalOrderId: string;
    status: 'created' | 'skipped' | 'error';
    docNumber?: string;
    message?: string;
  }>;
}

export async function importSiplahOrders(rows: ImportSiplahRow[]): Promise<ImportSiplahResult> {
  const res = await apiPost<ApiResponse<ImportSiplahResult>>('/order-hub/import-siplah', { rows });
  return res.data;
}

export async function syncOrderHub(): Promise<{ checked: number; transitionsLogged: number }> {
  const res = await apiPost<ApiResponse<{ checked: number; transitionsLogged: number }>>('/order-hub/sync', {});
  return res.data;
}
