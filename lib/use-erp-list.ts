'use client';

// Generic hook for ERP list pages — handles load, reload, params-reactive refetch, and error state.
// Server-driven: page/limit/search/sort live in caller state and are passed via `fetcher`.
// Caller is responsible for putting those state values into `deps` so the hook refetches.

import * as React from 'react';
import { notify } from '@/lib/feedback';
import { toToastMessage } from '@/lib/error-message';
import type { PaginatedMeta, PaginatedResponse } from '@/lib/api/types';

export interface ErpListState<T> {
  rows: T[];
  meta: PaginatedMeta | null;
  /** True only during the very first fetch (no data yet). Use this to block the table. */
  loading: boolean;
  /** True during any fetch — initial or refetch. Use for non-blocking indicators. */
  fetching: boolean;
  error: string | null;
  reload: () => void;
}

interface State<T> {
  rows: T[];
  meta: PaginatedMeta | null;
  fetching: boolean;
  hasData: boolean;
  error: string | null;
  tick: number;
}

type Action<T> =
  | { type: 'start' }
  | { type: 'success'; rows: T[]; meta: PaginatedMeta }
  | { type: 'error'; message: string }
  | { type: 'reload' };

function reducer<T>(state: State<T>, action: Action<T>): State<T> {
  switch (action.type) {
    case 'start':
      return { ...state, fetching: true, error: null };
    case 'success':
      return { ...state, fetching: false, hasData: true, rows: action.rows, meta: action.meta };
    case 'error':
      return { ...state, fetching: false, error: action.message };
    case 'reload':
      return { ...state, tick: state.tick + 1 };
    default:
      return state;
  }
}

/**
 * Fetches a paginated list resource. Refetches whenever `deps` change or
 * `reload()` is called. Exposes `meta` so caller can drive footer pagination
 * from server-reported `total` / `totalPages`.
 *
 * Pattern (canonical):
 *   const { rows, meta, loading, error, reload } = useErpList(
 *     () => listBranches({ page, limit: pageSize, search, sortBy, sortDir, isActive }),
 *     [page, pageSize, search, sortBy, sortDir, isActive],
 *   );
 */
export function useErpList<T>(
  fetcher: () => Promise<PaginatedResponse<T>>,
  deps: React.DependencyList = [],
): ErpListState<T> {
  const [state, dispatch] = React.useReducer(reducer<T>, {
    rows: [],
    meta: null,
    fetching: true,
    hasData: false,
    error: null,
    tick: 0,
  });

  // Keep latest fetcher in ref so we don't refetch when caller passes a new
  // closure on every render — only `deps` and `tick` drive refetches.
  const fetcherRef = React.useRef(fetcher);
  fetcherRef.current = fetcher;

  React.useEffect(() => {
    let cancelled = false;

    Promise.resolve().then(() => {
      if (cancelled) return;
      dispatch({ type: 'start' });
    });

    fetcherRef.current()
      .then((res) => {
        if (!cancelled) dispatch({ type: 'success', rows: res.data, meta: res.meta });
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        const msg = err instanceof Error ? err.message : 'Gagal memuat data';
        dispatch({ type: 'error', message: msg });
        notify(toToastMessage(msg), 'danger');
      });

    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [state.tick, ...deps]);

  const reload = React.useCallback(() => dispatch({ type: 'reload' }), []);

  return {
    rows: state.rows,
    meta: state.meta,
    loading: state.fetching && !state.hasData,
    fetching: state.fetching,
    error: state.error,
    reload,
  };
}
