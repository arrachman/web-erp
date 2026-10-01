/**
 * Config contract for read-only "Data register" pages (legacy DATA group).
 *
 * A register lists every document of one type across ALL statuses (read-only):
 * search + status filter + server-driven pagination + export, with the document
 * code linking out to the existing transaction (TX) edit form. Entry/create stays
 * in the TX page — registers never mutate.
 *
 * One reusable renderer (`DocumentRegisterPage`) consumes these configs; each
 * module supplies its configs keyed by canonical `sys_menus.path`. Do NOT fork
 * the renderer per document type.
 */

import type * as React from 'react';
import type { IconName } from '@/components/ui/icons';
import type { PaginatedResponse } from '@/lib/api/types';

/** Params the renderer feeds every list call (merged with `extraParams`). */
export interface RegisterListParams {
  page: number;
  limit: number;
  search?: string;
  status?: string;
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
}

/** A non-canonical column rendered between Tanggal and Status. */
export interface RegisterColumn<Row> {
  header: string;
  align?: 'left' | 'right' | 'center';
  /** Cell node. */
  render: (row: Row) => React.ReactNode;
  /** Plain value for CSV export. Falls back to `render` only when it is a string. */
  csv?: (row: Row) => string | number | null | undefined;
}

export interface DocumentRegisterConfig<Row> {
  /** Breadcrumb group label (e.g. "Purchasing"). */
  group: string;
  /** Page + tab title. */
  title: string;
  /** Short code tag shown next to the title. */
  code: string;
  /** Icon for route meta / breadcrumb. */
  icon: IconName;
  /** Summary metric label; defaults to `Σ <title>`. */
  summaryLabel?: string;
  /** List fetcher (an existing `lib/api/*` list function). */
  list: (params: RegisterListParams & Record<string, unknown>) => Promise<PaginatedResponse<Row>>;
  /** Static params merged into every list call (movementType / returnType / isOpeningBalance / code…). */
  extraParams?: Record<string, unknown>;
  /** Default server-side sort column. */
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
  /** Show the status filter + status badge column (default: true). */
  hasStatus?: boolean;
  /**
   * Override status-filter dropdown options (first entry = "all", empty value).
   * Defaults to `DOC_STATUS_FILTER_OPTIONS`. Use for documents whose status
   * enum differs from the 7-value doc-status (e.g. price adjustments).
   */
  statusOptions?: ReadonlyArray<{ value: string; label: string }>;
  getId: (row: Row) => string;
  /** Document number — rendered as the (linking) code cell. */
  getDocNumber: (row: Row) => string;
  /** Document date (ISO); rendered formatted via `formatDate`. Omit to hide the column. */
  getDocDate?: (row: Row) => string | null | undefined;
  /** Status value for the badge (required when `hasStatus !== false`). */
  getStatus?: (row: Row) => string;
  /** Extra columns between Tanggal and Status. */
  columns: RegisterColumn<Row>[];
  /** TX base path; document code → opens `<editBase>/<id>`. Omit = code not linked. */
  editBase?: string;
}

/** Registry value type — Row is erased so a Record can hold heterogeneous configs. */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type AnyDocumentRegisterConfig = DocumentRegisterConfig<any>;
