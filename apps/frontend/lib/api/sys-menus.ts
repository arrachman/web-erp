// ERP Sys Menu resource API — CRUD for sys_menus (MODULE→GROUP→ITEM tree)
// Endpoints: /sys-menus  (flat / tree / create / update / delete)
// Kept separate from menus.ts (which owns role-filtered nav mapping).

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export type ErpMenuType = 'MODULE' | 'GROUP' | 'ITEM';

export const ERP_MENU_TYPES: ErpMenuType[] = ['MODULE', 'GROUP', 'ITEM'];

export interface ErpSysMenu {
  id: string;
  code: string;
  title: string;
  path: string | null;
  icon: string | null;
  type: ErpMenuType;
  parentId: string | null;
  sortOrder: number;
  isActive: boolean;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface ErpSysMenuNode extends ErpSysMenu {
  children: ErpSysMenuNode[];
}

export interface CreateSysMenuPayload {
  code: string;
  title: string;
  path?: string | null;
  icon?: string | null;
  type: ErpMenuType;
  parentId?: string | null;
  sortOrder: number;
  isActive: boolean;
}

export type UpdateSysMenuPayload = Partial<CreateSysMenuPayload>;

// ─── API functions ────────────────────────────────────────────────────────────

/**
 * Flat list of all menus. Backend returns { success, data: [] } (no meta) and
 * doesn't support page/limit/sort/search query params (per CLAUDE.md §2.12
 * `menus` is an explicit pagination exception — list is small). We fetch all,
 * then apply filter/sort/slice client-side so SimpleMasterPage's server-driven
 * pagination contract still holds.
 */
export async function listSysMenus(
  params: PaginationParams = {},
): Promise<PaginatedResponse<ErpSysMenu>> {
  const res = await apiGet<ApiResponse<ErpSysMenu[]>>('/sys-menus');
  let rows = res.data;

  const q = params.search?.trim().toLowerCase();
  if (q) {
    rows = rows.filter(
      (r) =>
        r.code.toLowerCase().includes(q) ||
        r.title.toLowerCase().includes(q) ||
        (r.path?.toLowerCase().includes(q) ?? false),
    );
  }
  if (params.isActive !== undefined) {
    rows = rows.filter((r) => r.isActive === params.isActive);
  }

  const sortBy = params.sortBy ?? 'sortOrder';
  const sortDir: 'asc' | 'desc' = params.sortDir ?? 'asc';
  const dir = sortDir === 'asc' ? 1 : -1;
  rows = [...rows].sort((a, b) => {
    const av = (a as unknown as Record<string, unknown>)[sortBy];
    const bv = (b as unknown as Record<string, unknown>)[sortBy];
    if (av == null && bv == null) return 0;
    if (av == null) return -1 * dir;
    if (bv == null) return 1 * dir;
    if (typeof av === 'number' && typeof bv === 'number') return (av - bv) * dir;
    return String(av).localeCompare(String(bv)) * dir;
  });

  const total = rows.length;
  const limit = params.limit ?? (total || 1);
  const page = params.page ?? 1;
  const start = (page - 1) * limit;
  const pageRows = rows.slice(start, start + limit);

  return {
    success: res.success,
    data: pageRows,
    meta: {
      page,
      limit,
      total,
      totalPages: Math.max(1, Math.ceil(total / limit)),
    },
  };
}

export async function getSysMenuTree(): Promise<ErpSysMenuNode[]> {
  const res = await apiGet<ApiResponse<ErpSysMenuNode[]>>('/sys-menus/tree');
  return res.data;
}

export async function createSysMenu(
  payload: CreateSysMenuPayload,
): Promise<ErpSysMenu> {
  const res = await apiPost<ApiResponse<ErpSysMenu>>('/sys-menus', payload);
  return res.data;
}

export async function updateSysMenu(
  id: string,
  payload: UpdateSysMenuPayload,
): Promise<ErpSysMenu> {
  const res = await apiPatch<ApiResponse<ErpSysMenu>>(
    `/sys-menus/${id}`,
    payload,
  );
  return res.data;
}

export async function deleteSysMenu(id: string): Promise<void> {
  await apiDelete<void>(`/sys-menus/${id}`);
}

export async function bulkUpdateSysMenuStatus(
  ids: string[],
  isActive: boolean,
): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(
    '/sys-menus/bulk/status',
    { ids, isActive },
  );
  return { affected: res.affected };
}

export interface ReorderSysMenuItem {
  id: string;
  parentId: string | null;
  sortOrder: number;
}

/**
 * Atomic cross-parent reorder. Send the desired final state for all
 * affected items; backend validates hierarchy rules + no-cycle and
 * applies in a single transaction.
 */
export async function reorderSysMenus(
  items: ReorderSysMenuItem[],
): Promise<{ affected: number }> {
  const res = await apiPost<{ success: boolean; affected: number }>(
    '/sys-menus/reorder',
    { items },
  );
  return { affected: res.affected };
}

export async function bulkDeleteSysMenus(
  ids: string[],
): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(
    '/sys-menus/bulk',
    { ids },
  );
  return { affected: res.affected };
}
