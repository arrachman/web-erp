// Workflow row-actions for inventory weighbridge tickets (Receipt Weigher),
// following the same §2.7 state machine as the other inventory documents.

import type { RowActionItem } from '@/components/molecules/row-actions-menu';
import type {
  ErpDocumentStatus,
  InvWeighbridgeTicketTransition,
} from '@/lib/api/inv-weighbridge-tickets';

/** Workflow actions offered per status (§2.7 state machine). */
export function invWeighbridgeTicketWorkflowActions(
  status: ErpDocumentStatus,
  run: (a: InvWeighbridgeTicketTransition) => void,
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
