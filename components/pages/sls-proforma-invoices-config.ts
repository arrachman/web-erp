import type { SlsProformaInvoiceTransition } from '@/lib/api/sls-proforma-invoices';

/** Canonical list path (seeded `sys_menus.path`); base for /new and /:id. */
export const PI_BASE = '/sales/proforma-invoices';

export const TRANSITION_VERBS: Record<SlsProformaInvoiceTransition, string> = {
  SUBMIT: 'mengajukan', APPROVE: 'menyetujui', REJECT: 'menolak',
  POST: 'memposting', REOPEN: 'membuka kembali',
};

export const LIST_COLS: [string | null, string][] = [
  ['docNumber', 'No Transaksi'], ['docDate', 'Tanggal'],
  [null, 'Pelanggan'], [null, 'Uraian'],
  ['grandTotal', 'Total'], [null, 'Uang'], ['status', 'Status'],
];