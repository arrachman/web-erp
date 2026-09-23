// Workflow row-actions for giro transactions (Register/Clear × Incoming/Outgoing)
// — same §2.7 state machine as cash/bank and journals, on the shared
// erp-fin-giro-entries backend.

import type { RowActionItem } from '@/components/molecules/row-actions-menu';
import type { ErpDocumentStatus } from '@/lib/api/fin-journal-entries';
import type { GiroTransition } from '@/lib/api/fin-giro-entries';

/** Workflow actions offered per status (§2.7 state machine). */
export function giroWorkflowActions(
  status: ErpDocumentStatus,
  run: (a: GiroTransition) => void,
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
