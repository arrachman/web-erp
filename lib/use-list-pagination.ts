'use client';

/**
 * Shared pagination state for ERP list pages.
 *
 * - `page` is ephemeral (always starts at 1).
 * - `pageSize` persists per-entity in localStorage under `erp.pageSize.<key>`.
 * - Changing `pageSize` resets `page` to 1 (avoids "where am I" surprises).
 * - Caller passes the validated set of allowed sizes; anything else falls back
 *   to `defaultSize` to prevent corrupted storage from breaking the page.
 */

import * as React from 'react';
import { DEFAULT_PAGE_SIZE_OPTIONS } from '@/components/molecules/table-pagination';

const STORAGE_PREFIX = 'erp.pageSize.';

export interface UseListPaginationResult {
  page: number;
  pageSize: number;
  setPage: (page: number) => void;
  setPageSize: (size: number) => void;
  /** Reset to page 1. Call after filter/search changes. */
  resetPage: () => void;
}

export function useListPagination(
  /** Stable identifier for this list (e.g. `'branches'`, `'partners'`). */
  key: string,
  options?: {
    defaultSize?: number;
    allowedSizes?: readonly number[];
  },
): UseListPaginationResult {
  const allowed = options?.allowedSizes ?? DEFAULT_PAGE_SIZE_OPTIONS;
  const defaultSize = options?.defaultSize ?? 25;

  const [page, setPage] = React.useState(1);
  const [pageSize, setPageSizeState] = React.useState(defaultSize);

  // Hydrate from localStorage on mount (client-only).
  React.useEffect(() => {
    if (typeof window === 'undefined') return;
    try {
      const raw = window.localStorage.getItem(STORAGE_PREFIX + key);
      if (!raw) return;
      const n = Number(raw);
      if (Number.isFinite(n) && allowed.includes(n)) setPageSizeState(n);
    } catch {
      // ignore — quota / SSR / private mode
    }
  }, [key, allowed]);

  const setPageSize = React.useCallback(
    (size: number) => {
      if (!allowed.includes(size)) return;
      setPageSizeState(size);
      setPage(1);
      try {
        window.localStorage.setItem(STORAGE_PREFIX + key, String(size));
      } catch {
        // ignore
      }
    },
    [key, allowed],
  );

  const resetPage = React.useCallback(() => setPage(1), []);

  return { page, pageSize, setPage, setPageSize, resetPage };
}
