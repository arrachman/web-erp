import type { SlsQuotationTransition } from '@/lib/api/sls-quotations';

/** Canonical list path (seeded `sys_menus.path`); base for /new and /:id. */
export const SQ_BASE = '/sales/quotations';

export const TRANSITION_VERBS: Record<SlsQuotationTransition, string> = {
  SUBMIT: 'mengajukan', APPROVE: 'menyetujui', REJECT: 'menolak',
  POST: 'memposting', REOPEN: 'membuka kembali',
};

export const LIST_COLS: [string | null, string][] = [
  ['docNumber', 'No Transaksi'], ['docDate', 'Tanggal'],
  [null, 'Pelanggan'], [null, 'Uraian'],
  ['grandTotal', 'Total'], [null, 'Uang'], ['status', 'Status'],
];