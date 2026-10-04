// D1 — school-catalog profile API for items (erp/item-catalog).
import { apiGet, apiPut } from './client';
import type { ApiResponse } from './types';

export type ItemCurriculum = 'MERDEKA' | 'KURIKULUM_2013' | 'KTSP_2006' | 'LAINNYA';
export type ItemStockClass = 'DAGANGAN' | 'BAHAN_BAKU' | 'BARANG_JADI' | 'KONSINYASI';
export type ItemJenjang = 'PAUD' | 'TK' | 'SD' | 'SMP' | 'SMA' | 'SMK' | 'SLB' | 'OTHER';

export interface ItemCatalogProfile {
  publisherName: string | null;
  jenjang: ItemJenjang | null;
  gradeLevel: string | null;
  curriculum: ItemCurriculum | null;
  subject: string | null;
  hetPrice: number | null;
  isCustomPrint: boolean;
  stockClass: ItemStockClass;
  channels: Record<string, boolean> | null;
  isActive: boolean;
}

export interface ItemCatalogRow {
  id: number;
  code: string;
  name: string;
  barcode: string | null;
  salePrice: number | null;
  categoryId: number;
  category: { id: number; code: string; name: string } | null;
  vendorId: number | null;
  profile: ItemCatalogProfile;
}

export async function getItemCatalog(itemId: string): Promise<ItemCatalogRow> {
  const res = await apiGet<ApiResponse<ItemCatalogRow>>(`/item-catalog/${itemId}`);
  return res.data;
}

export async function upsertItemCatalog(
  itemId: string,
  payload: Partial<ItemCatalogProfile>,
): Promise<ItemCatalogRow> {
  const res = await apiPut<ApiResponse<ItemCatalogRow>>(`/item-catalog/${itemId}`, payload);
  return res.data;
}
