import type { CashBankTransition } from '@/lib/api/fin-cash-receipts';

/** Canonical list path (seeded `sys_menus.path`); base for /new and /:id. */
export const CR_BASE = '/finance/cash-receipts';

export const TRANSITION_VERBS: Record<CashBankTransition, string> = {
  SUBMIT: 'mengajukan',
  APPROVE: 'menyetujui',
  REJECT: 'menolak',
  POST: 'memposting',
  REOPEN: 'membuka kembali',
};