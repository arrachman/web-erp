import type { CashBankTransition } from '@/lib/api/fin-cash-disbursements';

/** Canonical list path (seeded `sys_menus.path`); base for /new and /:id. */
export const CD_BASE = '/finance/cash-disbursements';

export const TRANSITION_VERBS: Record<CashBankTransition, string> = {
  SUBMIT: 'mengajukan', APPROVE: 'menyetujui', REJECT: 'menolak',
  POST: 'memposting', REOPEN: 'membuka kembali',
};