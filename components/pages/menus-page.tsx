'use client';

/**
 * Sys config — Menu Manager page (sys_menus).
 * Hierarchical tree (MODULE → GROUP → ITEM) with drag-and-drop reorder
 * (sibling + cross-parent). Drag handle replaces the checkbox column —
 * no bulk action on this page (CLAUDE.md §2.22, revised 2026-05-24).
 * Route path: /admin/menus
 * Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import {
  TreeDndMasterPage,
  type TreeDndExtraColumn,
} from '@/components/organisms/tree-dnd-master-page';
import {
  listSysMenus,
  createSysMenu,
  updateSysMenu,
  deleteSysMenu,
  reorderSysMenus,
  type ErpSysMenu,
} from '@/lib/api/sys-menus';
import {
  MenuFormFields,
  defaultMenuForm,
  fromMenu,
  toMenuPayload,
  validateMenu,
  type MenuForm,
} from './menus-form';

const TYPE_VARIANT: Record<string, 'success' | 'info' | 'default'> = {
  MODULE: 'success',
  GROUP: 'info',
  ITEM: 'default',
};

type SysMenuTreeRow = ErpSysMenu & { name: string };

const adaptRow = (m: ErpSysMenu): SysMenuTreeRow => ({ ...m, name: m.title });

async function loadAll(): Promise<SysMenuTreeRow[]> {
  // listSysMenus already returns the flat list (paginated wrapper). We want
  // the whole tree so request a very large page — the underlying endpoint
  // returns all rows regardless (see lib/api/sys-menus.ts).
  const res = await listSysMenus({ limit: 10000, page: 1 });
  return res.data.map(adaptRow);
}

const createAdapted = async (payload: ReturnType<typeof toMenuPayload>) =>
  adaptRow(await createSysMenu(payload));

const updateAdapted = async (
  id: string,
  payload: Partial<ReturnType<typeof toMenuPayload>>,
) => adaptRow(await updateSysMenu(id, payload));

const extraColumns: TreeDndExtraColumn<SysMenuTreeRow>[] = [
  {
    key: 'type',
    label: 'Tipe',
    render: (r) => (
      <Badge variant={TYPE_VARIANT[r.type] ?? 'default'}>{r.type}</Badge>
    ),
  },
  {
    key: 'path',
    label: 'Path',
    render: (r) => (r.path ? <span className="mono muted">{r.path}</span> : '—'),
  },
];

export function ErpMenusPage() {
  return (
    <TreeDndMasterPage<SysMenuTreeRow, MenuForm>
      title="Menu Manager"
      entityLabel="menu"
      auditEntityName="ErpMenu"
      loadAll={loadAll}
      create={createAdapted}
      update={updateAdapted}
      remove={deleteSysMenu}
      reorder={(items) => reorderSysMenus(items)}
      defaultForm={defaultMenuForm}
      fromRecord={fromMenu}
      toPayload={toMenuPayload}
      FormFields={MenuFormFields}
      validate={validateMenu}
      extraColumns={extraColumns}
      treeFilters={[
        { type: 'MODULE', label: 'Modul' },
        { type: 'GROUP', label: 'Grup' },
      ]}
    />
  );
}
