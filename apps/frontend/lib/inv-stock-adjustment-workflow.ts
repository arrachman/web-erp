// Workflow row-actions for inventory stock adjustments, following the same §2.7
// state machine as the other inventory documents on the erp-inv-stock-adjustments
// backend module.

import type { RowActionItem } from '@/components/molecules/row-actions-menu';
import type {
  ErpDocumentStatus,
  InvStockAdjustmentTransition,
} from '@/lib/api/inv-stock-adjustments';

/** Workflow actions offered per status (§2.7 state machine). */
export function invStockAdjustmentWorkflowActions(
  status: ErpDocumentStatus,
  run: (a: InvStockAdjustmentTransition) => void,
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
