// Portal Sekolah — klien API ERP (Fase 3 W2). Token portal (JWT aud='portal')
// disimpan di localStorage; semua panggilan ke /erp/portal/* di api-gateway.

export const API_BASE =
  process.env.NEXT_PUBLIC_ERP_API_URL ?? 'https://erp.fr-labs.my.id/api/erp';

const TOKEN_KEY = 'portal_token';

export function getToken(): string | null {
  if (typeof window === 'undefined') return null;
  return window.localStorage.getItem(TOKEN_KEY);
}
export function setToken(t: string) {
  window.localStorage.setItem(TOKEN_KEY, t);
}
export function clearToken() {
  window.localStorage.removeItem(TOKEN_KEY);
}

export async function api<T = any>(
  path: string,
  opts: { method?: string; body?: unknown; auth?: boolean } = {},
): Promise<T> {
  const headers: Record<string, string> = { 'Content-Type': 'application/json' };
  if (opts.auth !== false) {
    const t = getToken();
    if (t) headers['Authorization'] = `Bearer ${t}`;
  }
  const res = await fetch(`${API_BASE}${path}`, {
    method: opts.method ?? 'GET',
    headers,
    body: opts.body !== undefined ? JSON.stringify(opts.body) : undefined,
  });
  let data: any = null;
  try {
    data = await res.json();
  } catch {
    /* respons kosong */
  }
  if (!res.ok) {
    const msg =
      (data && (data.message || data.error)) ||
      `Permintaan gagal (${res.status})`;
    throw new Error(Array.isArray(msg) ? msg.join(', ') : String(msg));
  }
  return data as T;
}

export function fmtIDR(v: string | number | null | undefined): string {
  const n = Number(v ?? 0);
  return 'Rp' + new Intl.NumberFormat('id-ID', { maximumFractionDigits: 0 }).format(n);
}

export function fmtDate(v: string | null | undefined): string {
  if (!v) return '—';
  const d = new Date(v);
  if (Number.isNaN(d.getTime())) return String(v);
  return d.toLocaleDateString('id-ID', { day: 'numeric', month: 'short', year: 'numeric' });
}

export interface CatalogItem {
  id: string;
  code: string;
  name: string;
  salePrice: string;
  /** Harga terselesaikan untuk sekolah akun (W7); sama dengan salePrice bila standar. */
  price: string;
  priceSource: 'KONTRAK_ITEM' | 'KONTRAK_KATEGORI' | 'KONTRAK_SEKOLAH' | 'STANDAR' | string;
  hetPrice: string | null;
  publisherName: string | null;
  jenjang: string | null;
  gradeLevel: string | null;
  subject: string | null;
  isCustomPrint: boolean;
  category: string | null;
  unit: string | null;
  /** Isi paket bila item ini paket/bundle (W7). */
  bundle: {
    components: { itemId: string; name: string | null; quantity: string; unit: string | null }[];
  } | null;
}

export interface PublicSchool {
  id: string;
  name: string;
  jenjang: string | null;
}

export interface PortalMe {
  account: {
    id: string;
    email: string;
    fullName: string;
    role: string;
    schoolName: string;
    studentName?: string | null;
    studentClass?: string | null;
  };
  school: { partnerId: string; name: string } | null;
}

export interface PortalOrder {
  id: string;
  docNumber: string;
  docDate: string;
  status: string;
  channel: string;
  fundingSource: string | null;
  budgetYear: number | null;
  grandTotal: string;
  notes: string | null;
  stage: string;
  stageLabel: string;
  createdAt: string;
}

export interface PortalInvoice {
  id: string;
  docNumber: string;
  docDate: string;
  dueDate: string | null;
  grandTotal: string;
  settlementStatus: 'UNPAID' | 'PARTIAL' | 'PAID';
  status: string;
  orderId: string | null;
  orderDocNumber: string | null;
}

export interface PortalPayment {
  id: string;
  invoiceId: string;
  invoiceNumber: string | null;
  provider: string;
  providerRef: string;
  vaNumber: string | null;
  amount: string;
  status: 'PENDING' | 'MENUNGGU_KONFIRMASI' | 'PAID' | 'DITOLAK' | 'EXPIRED';
  expiresAt: string | null;
  paidAt: string | null;
  claimedAt: string | null;
  rejectedReason: string | null;
  instructions: {
    bankName: string;
    accountNumber: string;
    accountHolder: string;
    amount: string;
    reference: string;
  } | null;
}
