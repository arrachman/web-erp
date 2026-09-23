/**
 * Canonical approval-status helpers.
 * Single source of truth for status → badge variant and display label.
 * Import these in every list/form page — never define STATUS_VARIANT inline.
 */

export type ApprovalStatus =
  | 'DRAFT'
  | 'NEED_APPROVE'
  | 'APPROVE_1'
  | 'APPROVE_2'
  | 'APPROVE_3'
  | 'APPROVE_4'
  | 'APPROVED'
  | 'REJECTED'
  | 'POSTED'
  | 'VOID'
  | 'CANCELLED';

type BadgeVariant = 'default' | 'primary' | 'success' | 'warn' | 'danger' | 'info';

const VARIANT_MAP: Record<string, BadgeVariant> = {
  DRAFT: 'default',
  NEED_APPROVE: 'warn',
  APPROVE_1: 'warn',
  APPROVE_2: 'warn',
  APPROVE_3: 'warn',
  APPROVE_4: 'warn',
  APPROVED: 'success',
  REJECTED: 'danger',
  POSTED: 'info',
  VOID: 'default',
  CANCELLED: 'danger',
  // Legacy display-string aliases
  Draft: 'default',
  'Need Approve': 'warn',
  Approved: 'success',
  Rejected: 'danger',
  Posted: 'info',
};

const LABEL_MAP: Record<string, string> = {
  DRAFT: 'Draft',
  NEED_APPROVE: 'Need Approve',
  APPROVE_1: 'Approve 1',
  APPROVE_2: 'Approve 2',
  APPROVE_3: 'Approve 3',
  APPROVE_4: 'Approve 4',
  APPROVED: 'Approved',
  REJECTED: 'Rejected',
  POSTED: 'Posted',
  VOID: 'Void',
  CANCELLED: 'Cancelled',
};

/**
 * Canonical document-status filter options for read-only registers & list pages.
 * First entry (empty value) = "Semua" (all statuses). Import this instead of
 * hand-building status `<Select>` option arrays per page.
 */
export const DOC_STATUS_FILTER_OPTIONS: ReadonlyArray<{ value: string; label: string }> = [
  { value: '', label: 'Semua Status' },
  { value: 'DRAFT', label: 'Draft' },
  { value: 'NEED_APPROVE', label: 'Need Approve' },
  { value: 'APPROVE_1', label: 'Approve 1' },
  { value: 'APPROVE_2', label: 'Approve 2' },
  { value: 'APPROVE_3', label: 'Approve 3' },
  { value: 'APPROVE_4', label: 'Approve 4' },
  { value: 'APPROVED', label: 'Approved' },
  { value: 'REJECTED', label: 'Rejected' },
  { value: 'POSTED', label: 'Posted' },
  { value: 'VOID', label: 'Void' },
  { value: 'CANCELLED', label: 'Cancelled' },
];

/** Maps a status key/string to its badge variant. Falls back to 'default'. */
export function statusBadgeVariant(status: string): BadgeVariant {
  return VARIANT_MAP[status] ?? 'default';
}

/** Maps a status enum key to its display label. Falls back to the raw value. */
export function statusLabel(status: string): string {
  return LABEL_MAP[status] ?? status;
}
