// Workflow row-actions for cash/bank transactions (CR/CD/BD), shared because
// all three follow the same §2.7 state machine on the same backend module.

import type { RowActionItem } from '@/components/molecules/row-actions-menu';
import type {
  CashBankTransition,
  ErpDocumentStatus,
} from '@/lib/api/fin-cash-receipts';

/** Workflow actions offered per status (§2.7 state machine). */
export function cashBankWorkflowActions(
  status: ErpDocumentStatus,
  run: (a: CashBankTransition) => void,
): RowActionItem[] {
  switch (status) {
    case 'DRAFT':
    case 'REJECTED':
      return [{ label: 'Ajukan', onSelect: () => run('SUBMIT') }];
    case 'NEED_APPROVE':
      return [
        { label: 'Setujui', onSelect: () => run('APPROVE') },
        { label: 'Tolak', onSelect: () => run('REJECT') },
      ];
    case 'APPROVED':
      return [
        { label: 'Posting', onSelect: () => run('POST') },
        { label: 'Reopen', onSelect: () => run('REOPEN') },
      ];
    case 'POSTED':
      return [{ label: 'Reopen', onSelect: () => run('REOPEN') }];
    default:
      return [];
  }
}
