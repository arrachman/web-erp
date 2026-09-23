// TanStack Query hooks for frequently-fetched ERP endpoints.
// Using shared query keys ensures cross-component deduplication and cache sharing —
// multiple components calling the same hook within staleTime get one HTTP request.

import { useQuery } from '@tanstack/react-query';
import type { UseQueryOptions } from '@tanstack/react-query';
import { getMe } from './auth';
import type { ErpAuthUser } from './auth';
import { fetchMyMenus } from './menus';
import { listBranches } from './branches';
import type { ErpBranch } from './branches';
import type { NavItem } from '@/lib/nav';
import type { PaginationParams } from './types';

// ─── Query keys ───────────────────────────────────────────────────────────────

export const erpQueryKeys = {
  me: ['erp', 'me'] as const,
  myMenus: ['erp', 'my-menus'] as const,
  branches: (params?: PaginationParams) => ['erp', 'branches', params ?? {}] as const,
} as const;

// Reference / session data: cache 5 minutes to avoid repeated round-trips on
// tab switches and component remounts in the multi-tab shell.
const STABLE_STALE_TIME = 5 * 60 * 1000;

// ─── Hooks ────────────────────────────────────────────────────────────────────

export function useErpMe(
  options?: Partial<UseQueryOptions<ErpAuthUser>>,
) {
  return useQuery<ErpAuthUser>({
    queryKey: erpQueryKeys.me,
    queryFn: getMe,
    staleTime: STABLE_STALE_TIME,
    retry: false,
    ...options,
  });
}

export function useErpMyMenus(
  options?: Partial<UseQueryOptions<NavItem[]>>,
) {
  return useQuery<NavItem[]>({
    queryKey: erpQueryKeys.myMenus,
    queryFn: fetchMyMenus,
    staleTime: STABLE_STALE_TIME,
    ...options,
  });
}

export function useErpBranches(
  params?: PaginationParams,
  options?: Partial<UseQueryOptions<ErpBranch[]>>,
) {
  return useQuery<ErpBranch[]>({
    queryKey: erpQueryKeys.branches(params),
    queryFn: async () => {
      const res = await listBranches(params);
      return res.data;
    },
    staleTime: STABLE_STALE_TIME,
    ...options,
  });
}
