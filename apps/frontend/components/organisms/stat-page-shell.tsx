'use client';

/**
 * Reusable shell for the Warehouse Statistics pages. Renders the standard
 * `.page` chrome (header with title + optional code tag, scrollable body)
 * plus loading / error / empty states so each stats page only supplies its
 * content. Mirrors the page chrome in `styles/erp-shell.css`.
 *
 * Also exports `useStatData` — a tiny fetch-once hook (loading/error/data)
 * for the non-paginated statistics endpoints.
 *
 * Atomic tier: Organism.
 */

import * as React from 'react';

interface UseStatDataResult<T> {
  data: T | null;
  loading: boolean;
  error: string | null;
}

/** Fetch a stats payload once on mount; expose loading/error/data state. */
export function useStatData<T>(fetcher: () => Promise<T>): UseStatDataResult<T> {
  const [data, setData] = React.useState<T | null>(null);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);

  const fetcherRef = React.useRef(fetcher);
  React.useEffect(() => {
    fetcherRef.current = fetcher;
  });

  React.useEffect(() => {
    let active = true;
    fetcherRef
      .current()
      .then((res) => {
        if (active) setData(res);
      })
      .catch((err: unknown) => {
        if (active) {
          setError(err instanceof Error ? err.message : 'Gagal memuat data');
        }
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => {
      active = false;
    };
  }, []);

  return { data, loading, error };
}

export interface StatPageShellProps {
  title: string;
  code?: string;
  loading: boolean;
  error: string | null;
  /** True when the fetch succeeded but produced no rows. */
  empty?: boolean;
  emptyMessage?: string;
  children?: React.ReactNode;
}

export function StatPageShell({
  title,
  code,
  loading,
  error,
  empty,
  emptyMessage = 'Belum ada data',
  children,
}: StatPageShellProps) {
  return (
    <div className="page">
      <div className="page-header">
        <div className="page-title">
          {title}
          {code && <span className="code-tag">{code}</span>}
        </div>
      </div>
      <div className="page-body">
        <div className="p-4">
          {loading && (
            <div className="py-10 text-center text-xs text-muted-foreground">
              Memuat...
            </div>
          )}
          {!loading && error && (
            <div className="py-10 text-center text-xs text-danger">
              Gagal memuat data: {error}
            </div>
          )}
          {!loading && !error && empty && (
            <div className="py-10 text-center text-xs text-muted-foreground">
              {emptyMessage}
            </div>
          )}
          {!loading && !error && !empty && children}
        </div>
      </div>
    </div>
  );
}
