import type { SlsOrderTransition } from '@/lib/api/sls-orders';

/** Canonical list path (seeded `sys_menus.path`); base for /new and /:id. */
export const SO_BASE = '/sales/orders';

export const TRANSITION_VERBS: Record<SlsOrderTransition, string> = {
  SUBMIT: 'mengajukan', APPROVE: 'menyetujui', REJECT: 'menolak',
  POST: 'memposting', REOPEN: 'membuka kembali',
};

export const LIST_COLS: [string | null, string][] = [
  ['docNumber', 'No Transaksi'], ['docDate', 'Tanggal'],
  [null, 'Pelanggan'], [null, 'Uraian'],
  ['grandTotal', 'Total'], [null, 'Uang'], ['status', 'Status'],
];