// Workflow row-actions for Manufacturing Work Orders (WO).
// WO state machine: DRAFT → SUBMIT → NEED_APPROVE → APPROVE/REJECT → REOPEN.
// No POST step (WO does not trigger a GL posting event directly).

import type { RowActionItem } from '@/components/molecules/row-actions-menu';
import type { MfgWorkOrderTransition, ErpDocumentStatus } from '@/lib/api/mfg-work-orders';

/** Workflow actions offered per WO status. */
export function woWorkflowActions(
  status: ErpDocumentStatus,
  run: (a: MfgWorkOrderTransition) => void,
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
    case 'POSTED':
      return [{ label: 'Reopen', onSelect: () => run('REOPEN') }];
    default:
      return [];
  }
}
