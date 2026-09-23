// Workflow row-actions for journal transactions (General/Adjustment/Memorial/
// Opening-Balance/Revaluation) — same §2.7 state machine as cash/bank, on the
// shared erp-fin-journal-entries backend.

import type { RowActionItem } from '@/components/molecules/row-actions-menu';
import type { ErpDocumentStatus, JournalTransition } from '@/lib/api/fin-journal-entries';

/** Workflow actions offered per status (§2.7 state machine). */
export function journalWorkflowActions(
  status: ErpDocumentStatus,
  run: (a: JournalTransition) => void,
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
