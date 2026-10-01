'use client';

// Tree-aware DnD master page organism. Built for sys_menus (CLAUDE.md §2.22).
// Drag handle replaces checkbox column → no bulk action. Cross-parent drop
// rule in `tree-dnd-helpers.ts#inferNewParent`; backend re-validates.
//
// Slimmed via pure-restructure split into co-located siblings:
//   tree-dnd-master-toolbar.tsx — header/filter/search/actions
//   tree-dnd-master-table.tsx   — DndContext + table + row actions
//   tree-dnd-master-dialogs.tsx — form modal + audit modal
// Behavior preserved exactly. Public export TreeDndMasterPage unchanged.

import * as React from 'react';
import {
  type DragEndEvent,
} from '@dnd-kit/core';
import {
  arrayMove,
} from '@dnd-kit/sortable';
import { ListFooter } from '@/components/organisms/list-footer';
import {
  flattenTree,
  inferNewParent,
  computeReorderChanges,
  validateDrop,
} from './tree-dnd-helpers';
import { TreeDndMasterToolbar } from './tree-dnd-master-toolbar';
import { TreeDndMasterTable } from './tree-dnd-master-table';
import {
  TreeDndMasterFormDialog,
  TreeDndMasterAuditDialog,
} from './tree-dnd-master-dialogs';
import type {
  TreeRow,
  TreeDndMasterPageProps,
} from './tree-dnd-master-page.types';
import { confirmAction, notify } from '@/lib/feedback';
import { hasErrors, type FormErrors } from '@/lib/form-validation';
import { tGlobal } from '@/lib/mock';
import { useTreeKeyboardNav } from '@/lib/use-tree-keyboard-nav';
import { useModalShortcuts } from '@/lib/use-modal-shortcuts';

export type { TreeRow, TreeNodeType } from './tree-dnd-master-page.types';
export type { TreeDndExtraColumn } from './tree-dnd-row';

export function TreeDndMasterPage<T extends TreeRow, F>({
  title,
  entityLabel,
  auditEntityName,
  loadAll,
  create,
  update,
  remove,
  reorder,
  defaultForm,
  fromRecord,
  toPayload,
  FormFields,
  validate,
  extraColumns = [],
  treeFilters,
}: TreeDndMasterPageProps<T, F>) {
  const [rows, setRows] = React.useState<T[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);
  const [search, setSearch] = React.useState('');
  const [debouncedSearch, setDebouncedSearch] = React.useState('');
  // One selected node id per `treeFilters` entry (empty string = "Semua").
  const [filterSel, setFilterSel] = React.useState<string[]>([]);
  const searchRef = React.useRef<HTMLInputElement>(null);
  const filterActive = filterSel.some(Boolean);

  const reload = React.useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setRows(await loadAll());
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Gagal memuat');
    } finally {
      setLoading(false);
    }
  }, [loadAll]);

  React.useEffect(() => {
    reload();
  }, [reload]);
  React.useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(search), 300);
    return () => clearTimeout(t);
  }, [search]);

  const flat = React.useMemo(() => {
    const out: { row: T; depth: number }[] = [];
    flattenTree(rows, null, 0, out);
    return out;
  }, [rows]);

  // Map of parentId → child rows, used for subtree collection (filters + scope).
  const childrenByParent = React.useMemo(() => {
    const m = new Map<string, T[]>();
    for (const f of flat) {
      const pid = f.row.parentId ?? '';
      const list = m.get(pid) ?? [];
      list.push(f.row);
      m.set(pid, list);
    }
    return m;
  }, [flat]);

  const subtreeIds = React.useCallback(
    (rootId: string) => {
      const ids = new Set<string>();
      const walk = (id: string) => {
        ids.add(id);
        for (const c of childrenByParent.get(id) ?? []) walk(c.id);
      };
      walk(rootId);
      return ids;
    },
    [childrenByParent],
  );

  // Most specific selected node id (largest filter index with a selection).
  const scopeRootId = React.useMemo(() => {
    for (let i = filterSel.length - 1; i >= 0; i--) {
      if (filterSel[i]) return filterSel[i];
    }
    return '';
  }, [filterSel]);

  // Options for each filter dimension, constrained to the subtree of the
  // nearest broader selection (so picking a Module narrows the Group list).
  const filterOptions = React.useMemo(() => {
    if (!treeFilters) return [];
    return treeFilters.map((cfg, i) => {
      let scopeSet: Set<string> | null = null;
      for (let j = i - 1; j >= 0; j--) {
        if (filterSel[j]) {
          scopeSet = subtreeIds(filterSel[j]);
          break;
        }
      }
      return flat
        .filter(
          (f) =>
            f.row.type === cfg.type &&
            (!scopeSet || scopeSet.has(f.row.id)),
        )
        .map((f) => f.row);
    });
  }, [treeFilters, flat, filterSel, subtreeIds]);

  // Scope the flat list to the selected node + its descendants. Without a
  // selection (or when filters are disabled) the full tree is returned.
  const scopedFlat = React.useMemo(() => {
    if (!treeFilters || !scopeRootId) return flat;
    const keep = subtreeIds(scopeRootId);
    return flat.filter((f) => keep.has(f.row.id));
  }, [flat, treeFilters, scopeRootId, subtreeIds]);

  const visibleFlat = React.useMemo(() => {
    const q = debouncedSearch.trim().toLowerCase();
    if (!q) return scopedFlat;
    const idxById = new Map(scopedFlat.map((f, i) => [f.row.id, i]));
    const matches = new Set<string>();
    for (const f of scopedFlat) {
      const r = f.row;
      if (r.code.toLowerCase().includes(q) || r.name.toLowerCase().includes(q)) {
        matches.add(r.id);
        let pid = r.parentId;
        while (pid) {
          matches.add(pid);
          const parent = scopedFlat[idxById.get(pid) ?? -1]?.row;
          pid = parent?.parentId ?? null;
        }
      }
    }
    return scopedFlat.filter((f) => matches.has(f.row.id));
  }, [scopedFlat, debouncedSearch]);

  // Select a value for filter `i`; clear all narrower (higher-index) selections.
  const setFilterAt = (i: number, value: string) =>
    setFilterSel((prev) => {
      const next = treeFilters ? treeFilters.map((_, k) => prev[k] ?? '') : [];
      next[i] = value;
      for (let k = i + 1; k < next.length; k++) next[k] = '';
      return next;
    });

  const sortableIds = visibleFlat.map((f) => f.row.id);

  // Modal state
  const [open, setOpen] = React.useState(false);
  const [editing, setEditing] = React.useState<T | null>(null);
  const [form, setForm] = React.useState<F>(defaultForm);
  const [formErrors, setFormErrors] = React.useState<FormErrors<F>>({});
  const [saving, setSaving] = React.useState(false);
  const [auditTarget, setAuditTarget] = React.useState<T | null>(null);

  const openCreate = () => {
    setEditing(null);
    setForm(defaultForm());
    setFormErrors({});
    setOpen(true);
  };
  const openEdit = (row: T) => {
    setEditing(row);
    setForm(fromRecord(row));
    setFormErrors({});
    setOpen(true);
  };

  // Keyboard-first row nav (CLAUDE.md §2.7 F). No X/select: this tree has no
  // row selection (§2.22 — drag handle replaces the checkbox column).
  const { focusedIndex } = useTreeKeyboardNav({
    rowCount: visibleFlat.length,
    resetKey: `${debouncedSearch}|${filterSel.join(',')}`,
    searchRef,
    onAdd: openCreate,
    onOpenFocused: (i) => {
      const target = visibleFlat[i]?.row;
      if (target) openEdit(target);
    },
  });

  const handleSave = async () => {
    if (validate) {
      const errs = validate(form);
      if (hasErrors(errs)) {
        setFormErrors(errs);
        setTimeout(() => {
          document
            .querySelector<HTMLElement>('[role="dialog"] [aria-invalid="true"]')
            ?.focus();
        }, 0);
        return;
      }
    }
    setSaving(true);
    try {
      if (editing) {
        const updated = await update(editing.id, toPayload(form));
        // Surgical update: replace only the changed row so the list doesn't
        // flicker through a full reload. `flat` re-sorts via useMemo.
        setRows((prev) => prev.map((r) => (r.id === editing.id ? updated : r)));
        notify(`${tGlobal(title)} ${tGlobal('diperbarui')}`, 'success');
      } else {
        const created = await create(toPayload(form));
        // Append the new row; flattenTree places it by parentId + sortOrder.
        setRows((prev) => [...prev, created]);
        notify(`${tGlobal(title)} ${tGlobal('dibuat')}`, 'success');
      }
      setOpen(false);
    } catch (e) {
      notify(e instanceof Error ? e.message : tGlobal('Gagal menyimpan'), 'danger');
    } finally {
      setSaving(false);
    }
  };

  useModalShortcuts({
    open,
    editing: !!editing,
    onSave: handleSave,
    onSaveAndNew: handleSave,
  });

  const handleDelete = (row: T) =>
    confirmAction({
      title: `${tGlobal('Hapus')} ${tGlobal(entityLabel)}?`,
      message: `${row.code} — ${row.name} ${tGlobal('akan dihapus permanen.')}`,
      variant: 'danger',
      confirmLabel: tGlobal('Hapus'),
      confirmIcon: 'trash',
      onConfirm: async () => {
        try {
          await remove(row.id);
          // Surgical removal: drop this row plus any descendants (delete may
          // cascade server-side) from local state. No full reload → no flicker.
          const removeIds = subtreeIds(row.id);
          setRows((prev) => prev.filter((r) => !removeIds.has(r.id)));
          notify(`${tGlobal(title)} ${tGlobal('dihapus')}`, 'success');
        } catch (e) {
          notify(e instanceof Error ? e.message : tGlobal('Gagal'), 'danger');
        }
      },
    });

  const handleDragEnd = async (event: DragEndEvent) => {
    const { active, over } = event;
    if (!over || active.id === over.id) return;
    if (debouncedSearch || filterActive) {
      notify(tGlobal('Bersihkan filter sebelum menata ulang'), 'warn');
      return;
    }
    const flatRows = flat.map((f) => f.row);
    const fromIdx = flatRows.findIndex((r) => r.id === active.id);
    const toIdx = flatRows.findIndex((r) => r.id === over.id);
    if (fromIdx < 0 || toIdx < 0) return;

    const reordered = arrayMove(flatRows, fromIdx, toIdx);
    const movedIdx = reordered.findIndex((r) => r.id === active.id);
    const newParentId = inferNewParent(reordered, movedIdx);

    const err = validateDrop(reordered, String(active.id), newParentId);
    if (err) {
      notify(tGlobal(err), 'warn');
      return;
    }

    const changes = computeReorderChanges(
      rows,
      reordered,
      String(active.id),
      newParentId,
    );
    if (changes.length === 0) return;

    // Optimistic local update
    setRows((prev) =>
      prev.map((r) => {
        const ch = changes.find((c) => c.id === r.id);
        if (!ch) return r;
        return { ...r, parentId: ch.parentId, sortOrder: ch.sortOrder } as T;
      }),
    );

    try {
      await reorder(changes);
      notify(`${tGlobal(title)} ${tGlobal('ditata ulang')}`, 'success');
    } catch (e) {
      notify(e instanceof Error ? e.message : tGlobal('Gagal menata ulang'), 'danger');
      reload();
    }
  };

  const colCount = 5 + extraColumns.length;

  return (
    <div className="card" style={{ display: 'flex', flexDirection: 'column', gap: 12, height: '100%', minHeight: 0 }}>
      <TreeDndMasterToolbar
        title={title}
        treeFilters={treeFilters}
        filterSel={filterSel}
        filterOptions={filterOptions}
        search={search}
        searchRef={searchRef}
        onFilterAt={setFilterAt}
        onSearchChange={setSearch}
        onReload={reload}
        onAdd={openCreate}
      />

      <TreeDndMasterTable
        loading={loading}
        error={error}
        visibleFlat={visibleFlat}
        sortableIds={sortableIds}
        extraColumns={extraColumns}
        colCount={colCount}
        filterActive={filterActive}
        debouncedSearch={debouncedSearch}
        entityLabel={entityLabel}
        focusedIndex={focusedIndex}
        onDragEnd={handleDragEnd}
        onOpenEdit={openEdit}
        onDelete={handleDelete}
        onOpenAudit={setAuditTarget}
      />

      {!loading && !error && (
        <ListFooter
          summary={{ rowCount: visibleFlat.length, totalRows: flat.length }}
          selectable={false}
          onAdd={openCreate}
        />
      )}

      <TreeDndMasterFormDialog
        open={open}
        editing={!!editing}
        title={title}
        form={form}
        formErrors={formErrors}
        saving={saving}
        FormFields={FormFields}
        onOpenChange={setOpen}
        onChange={setForm}
        onSave={handleSave}
      />

      <TreeDndMasterAuditDialog
        auditTarget={auditTarget}
        auditEntityName={auditEntityName}
        onOpenChange={(v) => { if (!v) setAuditTarget(null); }}
      />
    </div>
  );
}