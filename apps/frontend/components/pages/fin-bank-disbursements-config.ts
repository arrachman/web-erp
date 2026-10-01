import type { CashBankTransition } from '@/lib/api/fin-bank-disbursements';

/** Canonical list path (seeded `sys_menus.path`); base for /new and /:id. */
export const BD_BASE = '/finance/bank-disbursements';

export const TRANSITION_VERBS: Record<CashBankTransition, string> = {
  SUBMIT: 'mengajukan', APPROVE: 'menyetujui', REJECT: 'menolak',
  POST: 'memposting', REOPEN: 'membuka kembali',
};