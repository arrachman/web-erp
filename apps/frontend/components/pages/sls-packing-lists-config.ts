import type { SlsPackingListTransition } from '@/lib/api/sls-packing-lists';

/** Canonical list path (seeded `sys_menus.path`); base for /new and /:id. */
export const PL_BASE = '/sales/packing-lists';

export const TRANSITION_VERBS: Record<SlsPackingListTransition, string> = {
  SUBMIT: 'mengajukan', APPROVE: 'menyetujui', REJECT: 'menolak',
  POST: 'memposting', REOPEN: 'membuka kembali',
};

export const LIST_COLS: [string | null, string][] = [
  ['docNumber', 'No Transaksi'], ['docDate', 'Tanggal'],
  [null, 'Pelanggan'], [null, 'Uraian'],
  ['grandTotal', 'Total'], [null, 'Uang'], ['status', 'Status'],
];