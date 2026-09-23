// Workflow row-actions for inventory daily checks (DC), following the same
// §2.7 state machine as the other inventory documents.

import type { RowActionItem } from '@/components/molecules/row-actions-menu';
import type {
  ErpDocumentStatus,
  InvDailyCheckTransition,
} from '@/lib/api/inv-daily-checks';

/** Workflow actions offered per status (§2.7 state machine). */
export function invDailyCheckWorkflowActions(
  status: ErpDocumentStatus,
  run: (a: InvDailyCheckTransition) => void,
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
