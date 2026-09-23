// ERP Transaction attachments — lampiran dokumen pendukung per transaksi.
// Generik per-domain: domain = fin|inv|pur|sls memilih tabel
// <domain>_transaction_attachments; (docType, docId) mengunci ke satu record.
// Endpoints: /:domain/attachments/:docType/:docId[...]

import { apiGet, apiPatch, apiDelete, apiUpload, buildApiUrl } from './client';
import type { ApiResponse } from './types';

export type AttachmentDomain = 'fin' | 'inv' | 'pur' | 'sls';

export interface TransactionAttachment {
  id: string;
  docType: string;
  docId: string;
  fileName: string;
  mimeType: string;
  sizeBytes: number;
  note: string | null;
  sortOrder: number;
  createdAt: string;
}

const base = (domain: AttachmentDomain, docType: string, docId: string) =>
  `/${domain}/attachments/${encodeURIComponent(docType)}/${encodeURIComponent(docId)}`;

export async function listTransactionAttachments(
  domain: AttachmentDomain,
  docType: string,
  docId: string,
): Promise<TransactionAttachment[]> {
  const res = await apiGet<ApiResponse<TransactionAttachment[]>>(base(domain, docType, docId));
  return res.data;
}

export async function uploadTransactionAttachment(
  domain: AttachmentDomain,
  docType: string,
  docId: string,
  file: File,
  note?: string,
): Promise<TransactionAttachment> {
  const form = new FormData();
  form.append('file', file);
  if (note) form.append('note', note);
  const res = await apiUpload<ApiResponse<TransactionAttachment>>(base(domain, docType, docId), form);
  return res.data;
}

export async function updateTransactionAttachmentNote(
  domain: AttachmentDomain,
  docType: string,
  docId: string,
  attachmentId: string,
  note: string,
): Promise<TransactionAttachment> {
  const res = await apiPatch<ApiResponse<TransactionAttachment>>(
    `${base(domain, docType, docId)}/${attachmentId}`,
    { note },
  );
  return res.data;
}

export async function deleteTransactionAttachment(
  domain: AttachmentDomain,
  docType: string,
  docId: string,
  attachmentId: string,
): Promise<void> {
  await apiDelete<void>(`${base(domain, docType, docId)}/${attachmentId}`);
}

/** Absolute URL untuk membuka/unduh file lampiran (cookie erp_token same-origin). */
export function transactionAttachmentFileUrl(
  domain: AttachmentDomain,
  docType: string,
  docId: string,
  attachmentId: string,
): string {
  return buildApiUrl(`${base(domain, docType, docId)}/${attachmentId}/file`);
}
