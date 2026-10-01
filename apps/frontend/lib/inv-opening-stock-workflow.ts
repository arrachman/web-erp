// Workflow row-actions for inventory opening stock (Saldo Awal), following the
// same §2.7 state machine as stock movements on the erp-inv-opening-stocks backend.

import type { RowActionItem } from '@/components/molecules/row-actions-menu';
import type {
  ErpDocumentStatus,
  InvOpeningStockTransition,
} from '@/lib/api/inv-opening-stocks';

/** Workflow actions offered per status (§2.7 state machine). */
export function invOpeningStockWorkflowActions(
  status: ErpDocumentStatus,
  run: (a: InvOpeningStockTransition) => void,
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
