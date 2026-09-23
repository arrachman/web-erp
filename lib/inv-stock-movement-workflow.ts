// Workflow row-actions for inventory stock movements (MR/TS/RS/RF), shared
// because all four follow the same §2.7 state machine on the same backend module.

import type { RowActionItem } from '@/components/molecules/row-actions-menu';
import type {
  ErpDocumentStatus,
  InvStockMovementTransition,
} from '@/lib/api/inv-stock-movements';

/** Workflow actions offered per status (§2.7 state machine). */
export function invStockMovementWorkflowActions(
  status: ErpDocumentStatus,
  run: (a: InvStockMovementTransition) => void,
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
