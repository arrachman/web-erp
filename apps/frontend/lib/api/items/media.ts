// ─── Item media (gambar produk + video pendek) ───────────────────────────────
// Gallery per item: max 8 gambar (satu "utama") + 1 video pendek.
// File binary di-stream dari GET /items/:id/media/:mediaId/file (cookie auth).

import { apiGet, apiPatch, apiDelete, apiUpload, buildApiUrl } from '../client';
import type { ApiResponse } from '../types';

export type ItemMediaKind = 'IMAGE' | 'VIDEO';

export interface ItemMedia {
  id: string;
  itemId: string;
  kind: ItemMediaKind;
  fileName: string;
  mimeType: string;
  sizeBytes: number;
  sortOrder: number;
  isPrimary: boolean;
  createdAt: string;
}

export async function listItemMedia(itemId: string): Promise<ItemMedia[]> {
  const res = await apiGet<ApiResponse<ItemMedia[]>>(`/items/${itemId}/media`);
  return res.data;
}

export async function uploadItemMedia(
  itemId: string,
  kind: ItemMediaKind,
  file: File,
): Promise<ItemMedia> {
  const form = new FormData();
  form.append('kind', kind);
  form.append('file', file);
  const res = await apiUpload<ApiResponse<ItemMedia>>(`/items/${itemId}/media`, form);
  return res.data;
}

export async function setPrimaryItemMedia(itemId: string, mediaId: string): Promise<ItemMedia> {
  const res = await apiPatch<ApiResponse<ItemMedia>>(`/items/${itemId}/media/${mediaId}/primary`);
  return res.data;
}

export async function deleteItemMedia(itemId: string, mediaId: string): Promise<void> {
  await apiDelete<void>(`/items/${itemId}/media/${mediaId}`);
}

/** Absolute URL untuk <img src> / <video src> (cookie erp_token ikut same-origin). */
export function itemMediaFileUrl(itemId: string, mediaId: string): string {
  return buildApiUrl(`/items/${itemId}/media/${mediaId}/file`);
}