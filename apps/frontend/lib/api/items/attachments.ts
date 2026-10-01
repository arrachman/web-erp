// ─── Item attachments (lampiran dokumen pendukung) ───────────────────────────
// Dokumen generik per item (datasheet, sertifikat, kontrak, dll): max 20 file,
// 10MB/file. File binary di-stream dari GET /items/:id/attachments/:id/file.

import { apiGet, apiPatch, apiDelete, apiUpload, buildApiUrl } from '../client';
import type { ApiResponse } from '../types';

export interface ItemAttachment {
  id: string;
  itemId: string;
  fileName: string;
  mimeType: string;
  sizeBytes: number;
  note: string | null;
  sortOrder: number;
  createdAt: string;
}

export async function listItemAttachments(itemId: string): Promise<ItemAttachment[]> {
  const res = await apiGet<ApiResponse<ItemAttachment[]>>(`/items/${itemId}/attachments`);
  return res.data;
}

export async function uploadItemAttachment(
  itemId: string,
  file: File,
  note?: string,
): Promise<ItemAttachment> {
  const form = new FormData();
  form.append('file', file);
  if (note) form.append('note', note);
  const res = await apiUpload<ApiResponse<ItemAttachment>>(`/items/${itemId}/attachments`, form);
  return res.data;
}

export async function updateItemAttachmentNote(
  itemId: string,
  attachmentId: string,
  note: string,
): Promise<ItemAttachment> {
  const res = await apiPatch<ApiResponse<ItemAttachment>>(
    `/items/${itemId}/attachments/${attachmentId}`,
    { note },
  );
  return res.data;
}

export async function deleteItemAttachment(itemId: string, attachmentId: string): Promise<void> {
  await apiDelete<void>(`/items/${itemId}/attachments/${attachmentId}`);
}

/** Absolute URL untuk membuka/unduh file lampiran (cookie erp_token same-origin). */
export function itemAttachmentFileUrl(itemId: string, attachmentId: string): string {
  return buildApiUrl(`/items/${itemId}/attachments/${attachmentId}/file`);
}