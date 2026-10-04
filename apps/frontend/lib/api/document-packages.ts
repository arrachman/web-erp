// A3 Paket Dokumen API — dokumen pengadaan yang dihasilkan dari rantai
// transaksi order (tanpa input ulang), diarsipkan per sekolah + tahun
// anggaran dengan versi & audit tanda tangan.
// Endpoints: /document-packages (controller erp/document-packages)

import { apiGet, apiPost, downloadFile } from './client';
import type { ApiResponse, PaginatedResponse } from './types';

export type DocKind =
  | 'PENAWARAN'
  | 'SURAT_PESANAN'
  | 'INVOICE'
  | 'KUITANSI'
  | 'SURAT_JALAN'
  | 'BAST';

export type DocVariant = 'BOS' | 'NON_BOS';

export const DOC_KIND_LABELS: Record<DocKind, string> = {
  PENAWARAN: 'Surat Penawaran',
  SURAT_PESANAN: 'Surat Pesanan',
  INVOICE: 'Invoice',
  KUITANSI: 'Kuitansi',
  SURAT_JALAN: 'Surat Jalan',
  BAST: 'BAST',
};

export const ALL_DOC_KINDS = Object.keys(DOC_KIND_LABELS) as DocKind[];

export interface GeneratedDocument {
  id: string;
  orderId: string;
  docType: string;
  variant: string;
  sourceDocType: string;
  sourceId: string;
  version: number;
  fileName: string;
  mimeType: string;
  sizeBytes: number;
  status: 'GENERATED' | 'SIGNED' | 'SUPERSEDED';
  signedByName: string | null;
  signedAt: string | null;
  signatureNote: string | null;
  schoolId: string | null;
  budgetYear: number | null;
  createdAt: string;
  order?: { id: string; docNumber: string } | null;
  school?: { id: string; code: string; name: string } | null;
}

export interface DocAvailabilityItem {
  kind: DocKind;
  latest: { version: number; status: string } | null;
}

export interface DocAvailability {
  orderId: string;
  orderNumber: string;
  customer: { id: string; name: string } | null;
  defaultVariant: DocVariant;
  available: DocAvailabilityItem[];
}

export interface DocDeliveryReport {
  id: string;
  docNumber: string;
  docDate: string;
  status: string;
  acceptedAt: string | null;
  acceptedByName: string | null;
  acceptedByTitle: string | null;
  deliveryOrder: { id: string; docNumber: string; orderId: string | null } | null;
  customer: { id: string; code: string; name: string } | null;
}

export interface ListDocumentPackagesParams {
  page?: number;
  limit?: number;
  search?: string;
  schoolId?: string;
  budgetYear?: number;
  docType?: string;
  status?: string;
  orderId?: string;
}

export function listDocumentPackages(
  params?: ListDocumentPackagesParams,
): Promise<PaginatedResponse<GeneratedDocument>> {
  return apiGet<PaginatedResponse<GeneratedDocument>>(
    '/document-packages',
    params as unknown as Record<string, string | number | boolean | undefined>,
  );
}

export async function getDocAvailability(orderId: string): Promise<DocAvailability> {
  const res = await apiGet<ApiResponse<DocAvailability>>(`/document-packages/availability/${orderId}`);
  return res.data;
}

export async function generateDocuments(body: {
  orderId: string;
  docTypes: DocKind[];
  variant?: DocVariant;
  package?: boolean;
}): Promise<GeneratedDocument[]> {
  const res = await apiPost<ApiResponse<GeneratedDocument[]>>('/document-packages/generate', body);
  return res.data;
}

export async function signDocument(
  id: string,
  body: { signerName: string; note?: string },
): Promise<GeneratedDocument> {
  const res = await apiPost<ApiResponse<GeneratedDocument>>(`/document-packages/${id}/sign`, body);
  return res.data;
}

export async function downloadDocument(doc: GeneratedDocument): Promise<void> {
  await downloadFile(`/document-packages/${doc.id}/file`, undefined, doc.fileName);
}

export async function listDocDeliveryReports(acceptance?: string): Promise<DocDeliveryReport[]> {
  const res = await apiGet<ApiResponse<DocDeliveryReport[]>>(
    '/document-packages/delivery-reports',
    acceptance ? { acceptance } : undefined,
  );
  return res.data;
}

export async function recordAcceptance(
  drId: string,
  body: { acceptedAt?: string; acceptedByName: string; acceptedByTitle?: string; notes?: string },
): Promise<void> {
  await apiPost(`/document-packages/delivery-reports/${drId}/acceptance`, body);
}
