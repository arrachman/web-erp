// ─── Purchase auto-fill ─────────────────────────────────────────────────────
// Defaults pushed into a PR/PO/RI/PRT item line when an item is picked
// (editable per line). Shape = subset of mapItem (GET /items/:id).

import { apiGet } from '../client';
import type { ApiResponse } from '../types';

export interface ItemPurchaseAutoFill {
  id: string;
  code: string;
  name: string;
  purchasePrice?: string | null;    // Harga Beli Terakhir → default unit price
  purchaseDiscount?: string | null; // Diskon Pembelian (%) → default line discount
  unitId: string;
  unit?: { id: string; code: string; name: string } | null;
  fieldUnitId?: string | null;
  fieldUnit?: { id: string; code: string; name: string } | null;
  purchaseTaxId?: string | null;
  purchaseTax2Id?: string | null;
  purchaseTax?: { id: string; code: string; name: string; rate?: string } | null;
  purchaseTax2?: { id: string; code: string; name: string; rate?: string } | null;
}

export async function getItemForPurchaseAutoFill(id: string): Promise<ItemPurchaseAutoFill | null> {
  try {
    const res = await apiGet<ApiResponse<ItemPurchaseAutoFill>>(`/items/${id}`);
    return res.data;
  } catch {
    return null;
  }
}