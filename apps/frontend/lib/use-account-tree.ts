'use client';

/**
 * Lazy CoA tree loader — roots first, children fetched on expand.
 * Replaces limit=5000 full-chart load.
 */

import * as React from 'react';
import {
  listAccountTree,
  type AccountTreeNode,
  type ErpAccount,
} from '@/lib/api/accounts';
import { notify } from '@/lib/feedback';
import { toToastMessage } from '@/lib/error-message';

export interface UseAccountTreeParams {
  isActive?: boolean;
  accountType?: string;
  accountKind?: string;
}

export interface UseAccountTreeResult {
  rows: ErpAccount[];
  hasChildrenMap: Map<string, boolean>;
  loading: boolean;
  fetching: boolean;
  error: string | null;
  reload: () => void;
  ensureChildren: (parentId: string) => Promise<void>;
  loadedParents: Set<string>;
}

export function useAccountTree(params: UseAccountTreeParams): UseAccountTreeResult {
  const [rows, setRows] = React.useState<ErpAccount[]>([]);
  const [hasChildrenMap, setHasChildrenMap] = React.useState<Map<string, boolean>>(
    () => new Map(),
  );
  const [loadedParents, setLoadedParents] = React.useState<Set<string>>(
    () => new Set(),
  );
  const [loading, setLoading] = React.useState(true);
  const [fetching, setFetching] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);
  const [tick, setTick] = React.useState(0);

  const paramsKey = JSON.stringify(params);

  React.useEffect(() => {
    let cancelled = false;
    setFetching(true);
    setError(null);

    listAccountTree({
      parentId: null,
      isActive: params.isActive,
      accountType: params.accountType as ErpAccount['type'] | undefined,
      accountKind: params.accountKind as ErpAccount['kind'] | undefined,
    })
      .then((res) => {
        if (cancelled) return;
        const data = res.data ?? [];
        const map = new Map<string, boolean>();
        for (const n of data) map.set(n.id, n.hasChildren);
        setRows(data.map(stripHasChildren));
        setHasChildrenMap(map);
        setLoadedParents(new Set(['root']));
        setLoading(false);
        setFetching(false);
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        const msg = err instanceof Error ? err.message : 'Gagal memuat bagan akun';
        setError(msg);
        setLoading(false);
        setFetching(false);
        notify(toToastMessage(msg), 'danger');
      });

    return () => {
      cancelled = true;
    };
    // paramsKey captures isActive/type/kind; tick forces reload
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [paramsKey, tick]);

  const ensureChildren = React.useCallback(
    async (parentId: string) => {
      if (loadedParents.has(parentId)) return;
      setFetching(true);
      try {
        const res = await listAccountTree({
          parentId,
          isActive: params.isActive,
          accountType: params.accountType as ErpAccount['type'] | undefined,
          accountKind: params.accountKind as ErpAccount['kind'] | undefined,
        });
        const children = res.data ?? [];
        setHasChildrenMap((prev) => {
          const next = new Map(prev);
          for (const n of children) next.set(n.id, n.hasChildren);
          return next;
        });
        setRows((prev) => {
          const ids = new Set(prev.map((r) => r.id));
          const merged = [...prev];
          for (const n of children) {
            if (!ids.has(n.id)) merged.push(stripHasChildren(n));
          }
          return merged;
        });
        setLoadedParents((prev) => new Set(prev).add(parentId));
      } catch (err: unknown) {
        const msg = err instanceof Error ? err.message : 'Gagal memuat anak akun';
        notify(toToastMessage(msg), 'danger');
      } finally {
        setFetching(false);
      }
    },
    [loadedParents, params.isActive, params.accountType, params.accountKind],
  );

  return {
    rows,
    hasChildrenMap,
    loading,
    fetching,
    error,
    reload: () => setTick((t) => t + 1),
    ensureChildren,
    loadedParents,
  };
}

function stripHasChildren(n: AccountTreeNode): ErpAccount {
  const { hasChildren: _h, ...rest } = n;
  return rest;
}
