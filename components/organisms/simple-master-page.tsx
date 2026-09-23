'use client';

/**
 * Generic CRUD organism for "simple" master entities (code + name + isActive
 * + optional extra columns/fields). Used by ORGANIZATION group masters
 * (Division, SubDivision, Project, CostCenter, Department, SubDepartment)
 * to avoid 280-line page boilerplate per entity.
 *
 * Each entity page = ~80 lines of config + form fields, not a fork of
 * branches-page.tsx. Pattern matches branches-page.tsx feature-by-feature:
 * server-driven paginate/sort/filter, keyboard nav, bulk actions, audit
 * panel, kebab+context menu (RowActionsMenu).
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import {
  RowActionsMenu,
  RowContextMenu,
  type RowActionItem,
} from '@/components/molecules/row-actions-menu';
import { FormErrorSummary } from '@/components/molecules/form-error-summary';
import { BulkActionBar } from '@/components/molecules/bulk-action-bar';
import { AuditModal } from '@/components/molecules/audit-modal';
import {
  Modal,
  ModalContent,
  ModalHeader,
  ModalTitle,
  ModalFooter,
} from '@/components/organisms/modal';
import {
  ErpListLayout,
  type FilterConfig,
  type SummaryConfig,
  type ListPaginationConfig,
  type KeyboardRowConfig,
} from '@/components/organisms/erp-list-layout';
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableCell,
  TableEmpty,
  CheckboxHead,
  CheckboxCell,
  CodeLinkCell,
  SortableHead,
} from '@/components/organisms/table';
import { confirmAction, notify } from '@/lib/feedback';
import { validateForm, hasErrors, type FormErrors, type FieldRule } from '@/lib/form-validation';
import { tGlobal } from '@/lib/mock';
import { useErpList } from '@/lib/use-erp-list';
import { useListPagination } from '@/lib/use-list-pagination';
import { useModalShortcuts } from '@/lib/use-modal-shortcuts';
import type { PaginatedResponse, PaginationParams } from '@/lib/api/types';

export interface BaseEntity {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
}

export interface ExtraColumn<T> {
  key: string;
  label: string;
  sortable?: boolean;
  render: (row: T) => React.ReactNode;
}

export interface ExtraFilterDef {
  key: string;
  label: string;
  defaultValue?: string;
  options: { label: string; value: string }[];
}

export interface SimpleMasterPageProps<T extends BaseEntity, F> {
  // Display
  title: string;
  code?: string;
  entityLabel: string; // singular, lowercase, mis. "divisi"
  storageKey: string;
  auditEntityName: string;
  // Data
  list: (p: PaginationParams) => Promise<PaginatedResponse<T>>;
  create: (payload: any) => Promise<T>;
  update: (id: string, payload: any) => Promise<T>;
  remove: (id: string) => Promise<void>;
  bulkStatus: (ids: string[], isActive: boolean) => Promise<{ affected: number }>;
  bulkDelete: (ids: string[]) => Promise<{ affected: number }>;
  // Form
  defaultForm: () => F;
  fromRecord: (row: T) => F;
  toPayload: (form: F) => any;
  FormFields: React.ComponentType<{ data: F; onChange: (d: F) => void; errors?: FormErrors<F> }>;
  /** Optional client-side validation. Return empty object (or omit) to skip. */
  validate?: (form: F) => FormErrors<F>;
  // Optional extra columns shown between Name and Status
  extraColumns?: ExtraColumn<T>[];
  // Default sort (overrides organism default of createdAt desc)
  defaultSortBy?: string;
  defaultSortDir?: 'asc' | 'desc';
  extraFilters?: ExtraFilterDef[];
  /** Called whenever extra filter values change so parent can sync & wrap list fn. */
  onExtraFilterChange?: (values: Record<string, string>) => void;
  /** Modal width for the create/edit form. Default 'md' (560px). 'lg' (900px) for rich; 'xl' (1100px) for side-nav layouts. */
  modalSize?: 'md' | 'lg' | 'xl';
}

export function SimpleMasterPage<T extends BaseEntity, F>({
  title,
  code,
  entityLabel,
  storageKey,
  auditEntityName,
  list,
  create,
  update,
  remove,
  bulkStatus,
  bulkDelete,
  defaultForm,
  fromRecord,
  toPayload,
  FormFields,
  validate,
  extraColumns = [],
  defaultSortBy = 'createdAt',
  defaultSortDir = 'desc',
  extraFilters = [],
  onExtraFilterChange,
  modalSize = 'md',
}: SimpleMasterPageProps<T, F>) {
  const [sortBy, setSortBy] = React.useState(defaultSortBy);
  const [sortDir, setSortDir] = React.useState<'asc' | 'desc'>(defaultSortDir);
  const [search, setSearch] = React.useState('');
  const [statusFilter, setStatusFilter] = React.useState('active');
  const [extraFilterValues, setExtraFilterValues] = React.useState<Record<string, string>>(
    () => Object.fromEntries(extraFilters.map(f => [f.key, f.defaultValue ?? '']))
  );
  const { page, pageSize, setPage, setPageSize } = useListPagination(storageKey);
  const setExtraFilter = (key: string, value: string) => {
    const next = { ...extraFilterValues, [key]: value };
    setExtraFilterValues(next);
    onExtraFilterChange?.(next);
    setPage(1);
  };

  const [debouncedSearch, setDebouncedSearch] = React.useState(search);
  React.useEffect(() => { const t = setTimeout(() => setDebouncedSearch(search), 300); return () => clearTimeout(t); }, [search]);

  const isActiveParam = statusFilter === 'active' ? true : statusFilter === 'inactive' ? false : undefined;

  const extraParams = Object.fromEntries(Object.entries(extraFilterValues).filter(([, v]) => v !== ''));
  const { rows, meta, loading, fetching, error, reload } = useErpList(
    () => list({
      page, limit: pageSize, search: debouncedSearch || undefined,
      sortBy, sortDir, isActive: isActiveParam, ...extraParams,
    } as Parameters<typeof list>[0]),
    [page, pageSize, debouncedSearch, sortBy, sortDir, isActiveParam, JSON.stringify(extraFilterValues)],
  );

  React.useEffect(() => { setPage(1); }, [debouncedSearch, statusFilter, sortBy, sortDir, pageSize]);

  const [selectedIds, setSelectedIds] = React.useState<Set<string>>(new Set());
  const [focusedIndex, setFocusedIndex] = React.useState(-1);
  const [open, setOpen] = React.useState(false);
  const [editing, setEditing] = React.useState<T | null>(null);
  const [form, setForm] = React.useState<F>(defaultForm);
  const [saving, setSaving] = React.useState(false);
  const [formErrors, setFormErrors] = React.useState<FormErrors<F>>({});
  const [auditTarget, setAuditTarget] = React.useState<T | null>(null);

  React.useEffect(() => { setFocusedIndex(-1); }, [page, debouncedSearch, statusFilter]);

  React.useEffect(() => {
    if (focusedIndex < 0) return;
    document.querySelector('[data-focused="true"]')?.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
  }, [focusedIndex]);

  const paged = rows;
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  const allSelected = paged.length > 0 && paged.every((r) => selectedIds.has(r.id));
  const someSelected = !allSelected && paged.some((r) => selectedIds.has(r.id));
  const toggleRow = (id: string) =>
    setSelectedIds((prev) => { const next = new Set(prev); if (next.has(id)) next.delete(id); else next.add(id); return next; });
  const toggleAll = (checked: boolean) =>
    setSelectedIds((prev) => { const next = new Set(prev); if (checked) paged.forEach((r) => next.add(r.id)); else paged.forEach((r) => next.delete(r.id)); return next; });

  const ALL = { label: 'Semua', value: '' };
  const filters: FilterConfig[] = [
    { key: 'status', label: 'Status', value: statusFilter, onChange: setStatusFilter,
      options: [ALL, { label: 'Aktif', value: 'active' }, { label: 'Nonaktif', value: 'inactive' }] },
    ...extraFilters.map(f => ({
      key: f.key, label: f.label,
      value: extraFilterValues[f.key] ?? '',
      onChange: (v: string) => setExtraFilter(f.key, v),
      options: [{ label: 'Semua', value: '' }, ...f.options],
    })),
  ];
  // (label/option strings here are kept as i18n keys — translated by ErpListLayout via tGlobal)
  const hasActiveFilter = search !== '' || statusFilter !== 'active' || Object.values(extraFilterValues).some(v => v !== '');
  const summary: SummaryConfig = { metricLabel: `Σ ${tGlobal(entityLabel)}`, rowCount: totalRows, totalCount: totalRows };
  const pagination: ListPaginationConfig = { page, pageCount, pageSize, totalRows, onPage: setPage, onPageSize: setPageSize };

  const openCreate = () => { setEditing(null); setForm(defaultForm()); setFormErrors({}); setOpen(true); };
  const openEdit = (row: T) => { setEditing(row); setForm(fromRecord(row)); setFormErrors({}); setOpen(true); };
  const openDuplicate = (row: T) => { setEditing(null); setForm({ ...fromRecord(row), code: '' } as F); setFormErrors({}); setOpen(true); };

  const keyboardCfg: KeyboardRowConfig = {
    rowCount: paged.length, focusedIndex, onFocusChange: setFocusedIndex,
    onToggle: (i) => toggleRow(paged[i].id), onOpen: (i) => openEdit(paged[i]),
  };

  const handleSave = async (keepOpen = false) => {
    if (validate) {
      const errors = validate(form);
      if (hasErrors(errors)) {
        setFormErrors(errors);
        setTimeout(() => {
          const el = document.querySelector<HTMLElement>('[role="dialog"] [aria-invalid="true"]');
          el?.focus();
        }, 0);
        return;
      }
    }
    setFormErrors({});
    setSaving(true);
    try {
      if (editing) { await update(editing.id, toPayload(form)); notify(`${tGlobal(title)} ${tGlobal('diperbarui')}`, 'success'); }
      else { await create(toPayload(form)); notify(`${tGlobal(title)} ${tGlobal('dibuat')}`, 'success'); }
      if (keepOpen && !editing) {
        setForm(defaultForm());
        setTimeout(() => {
          const el = document.querySelector<HTMLElement>('[role="dialog"] input:not([type="hidden"]):not([disabled])');
          el?.focus();
        }, 0);
      } else {
        setOpen(false);
      }
      reload();
    } catch (e: unknown) { notify(e instanceof Error ? e.message : tGlobal('Gagal menyimpan'), 'danger'); }
    finally { setSaving(false); }
  };

  useModalShortcuts({
    open, editing: !!editing,
    onSave: () => handleSave(false),
    onSaveAndNew: () => handleSave(true),
  });

  const entityT = tGlobal(entityLabel);
  const handleDelete = (row: T) =>
    confirmAction({
      title: `${tGlobal('Hapus')} ${entityT}?`,
      message: `${row.code} — ${row.name} ${tGlobal('akan dihapus permanen.')}`,
      variant: 'danger', confirmLabel: tGlobal('Hapus'), confirmIcon: 'trash',
      onConfirm: async () => { try { await remove(row.id); notify(`${tGlobal(title)} ${tGlobal('dihapus')}`, 'success'); reload(); } catch (e: unknown) { notify(e instanceof Error ? e.message : tGlobal('Gagal'), 'danger'); } },
    });

  const selectedArr = Array.from(selectedIds);
  const handleBulkStatus = (isActive: boolean) => {
    const actionLabel = tGlobal(isActive ? 'Aktifkan' : 'Nonaktifkan'); const doneLabel = tGlobal(isActive ? 'diaktifkan' : 'dinonaktifkan');
    confirmAction({
      title: `${actionLabel} ${selectedArr.length} ${entityT}?`,
      message: `${tGlobal('Semua')} ${entityT} ${tGlobal('yang dipilih akan')} ${doneLabel}.`,
      variant: isActive ? 'primary' : 'warn',
      confirmLabel: actionLabel,
      onConfirm: async () => {
        try { const { affected } = await bulkStatus(selectedArr, isActive); notify(`${affected} ${entityT} ${doneLabel}`, 'success'); setSelectedIds(new Set()); reload(); }
        catch (e: unknown) { notify(e instanceof Error ? e.message : tGlobal('Gagal'), 'danger'); }
      },
    });
  };

  const handleBulkDelete = () =>
    confirmAction({
      title: `${tGlobal('Hapus')} ${selectedArr.length} ${entityT}?`,
      message: `${tGlobal('Semua')} ${entityT} ${tGlobal('yang dipilih akan dihapus permanen.')}`,
      variant: 'danger', confirmLabel: tGlobal('Hapus'), confirmIcon: 'trash',
      onConfirm: async () => {
        try { const { affected } = await bulkDelete(selectedArr); notify(`${affected} ${entityT} ${tGlobal('dihapus')}`, 'success'); setSelectedIds(new Set()); reload(); }
        catch (e: unknown) { notify(e instanceof Error ? e.message : tGlobal('Gagal'), 'danger'); }
      },
    });

  const colCount = 5 + extraColumns.length; // checkbox + code + name + ...extra + status + actions

  return (
    <>
      <ErpListLayout
        title={title} code={code ?? ''} loading={loading} fetching={fetching} error={error}
        search={search} onSearch={setSearch} onAdd={openCreate} onRefresh={reload}
        onExport={() => notify(tGlobal('Export belum tersedia'), 'warn')}
        filters={filters} summary={summary} pagination={pagination}
        keyboardRows={keyboardCfg}
      >
        <div className="lines">
          <Table className="table-fixed">
            <colgroup>
              <col style={{ width: 36 }} />
              <col style={{ width: 200 }} />
              <col />
              {extraColumns.map((c) => <col key={c.key} style={{ width: 160 }} />)}
              <col style={{ width: 140 }} />
              <col style={{ width: 48 }} />
            </colgroup>
            <TableHeader>
              <TableRow>
                <CheckboxHead checked={someSelected ? 'indeterminate' : allSelected} onCheckedChange={toggleAll} />
                <SortableHead field="code" sortBy={sortBy} sortDir={sortDir} onSort={(f, d) => { setSortBy(f); setSortDir(d); }}>{tGlobal('Kode')}</SortableHead>
                <SortableHead field="name" sortBy={sortBy} sortDir={sortDir} onSort={(f, d) => { setSortBy(f); setSortDir(d); }}>{tGlobal('Nama')}</SortableHead>
                {extraColumns.map((c) => (
                  c.sortable
                    ? <SortableHead key={c.key} field={c.key} sortBy={sortBy} sortDir={sortDir} onSort={(f, d) => { setSortBy(f); setSortDir(d); }}>{tGlobal(c.label)}</SortableHead>
                    : <TableHead key={c.key}>{tGlobal(c.label)}</TableHead>
                ))}
                <SortableHead field="isActive" sortBy={sortBy} sortDir={sortDir} onSort={(f, d) => { setSortBy(f); setSortDir(d); }}>{tGlobal('Status')}</SortableHead>
                <TableHead />
              </TableRow>
            </TableHeader>
            <TableBody>
              {paged.length === 0 ? (
                <TableEmpty
                  colSpan={colCount}
                  variant={hasActiveFilter ? 'filtered' : 'empty'}
                  entityLabel={tGlobal(entityLabel)}
                  searchTerm={debouncedSearch || undefined}
                  onAction={hasActiveFilter ? () => { setSearch(''); setStatusFilter('active'); setExtraFilterValues(Object.fromEntries(extraFilters.map(f => [f.key, f.defaultValue ?? '']))); } : openCreate}
                  actionLabel={hasActiveFilter ? tGlobal('Reset filter') : `${tGlobal('Tambah')} ${tGlobal(entityLabel)}`}
                  actionShortcut={hasActiveFilter ? undefined : 'N'}
                />
              ) : paged.map((row, idx) => {
                const rowActions: RowActionItem[] = [
                  { label: tGlobal('Edit'), onSelect: () => openEdit(row) },
                  { label: tGlobal('Duplikat'), onSelect: () => openDuplicate(row) },
                  { label: tGlobal('Riwayat'), onSelect: () => setAuditTarget(row) },
                  { label: tGlobal('Hapus'), onSelect: () => handleDelete(row), danger: true, separatorBefore: true },
                ];
                return (
                  <RowContextMenu key={row.id} items={rowActions}>
                    <TableRow data-selected={selectedIds.has(row.id)} data-focused={focusedIndex === idx} className="cursor-pointer" onClick={() => setFocusedIndex(idx)} onDoubleClick={() => openEdit(row)}>
                      <CheckboxCell checked={selectedIds.has(row.id)} onCheckedChange={() => toggleRow(row.id)} />
                      <CodeLinkCell code={row.code} onOpen={() => openEdit(row)} />
                      <TableCell>{row.name}</TableCell>
                      {extraColumns.map((c) => <TableCell key={c.key} className="muted">{c.render(row)}</TableCell>)}
                      <TableCell>
                        <Badge variant={row.isActive ? 'success' : 'default'} dot className="-ml-[7px]">
                          {tGlobal(row.isActive ? 'Aktif' : 'Nonaktif')}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <RowActionsMenu items={rowActions} />
                      </TableCell>
                    </TableRow>
                  </RowContextMenu>
                );
              })}
            </TableBody>
          </Table>
        </div>
      </ErpListLayout>

      <BulkActionBar
        count={selectedIds.size}
        onActivate={() => handleBulkStatus(true)}
        onDeactivate={() => handleBulkStatus(false)}
        onDelete={handleBulkDelete}
        onCancel={() => setSelectedIds(new Set())}
      />

      <Modal open={open} onOpenChange={setOpen}>
        <ModalContent size={modalSize} className="max-h-[84vh]">
          <ModalHeader className="shrink-0"><ModalTitle>{editing ? `${tGlobal('Edit')} ${tGlobal(title)}` : `${tGlobal('Tambah')} ${tGlobal(title)}`}</ModalTitle></ModalHeader>
          <div className="min-h-0 flex-1 overflow-y-auto">
            <FormErrorSummary errors={formErrors} /><FormFields data={form} onChange={setForm} errors={formErrors} />
          </div><ModalFooter className="shrink-0">
            <button className="btn ghost" onClick={() => setOpen(false)}>{tGlobal('Batal')}</button>
            {!editing && (
              <button className="btn ghost" onClick={() => handleSave(true)} disabled={saving} title="Ctrl+Enter">
                {tGlobal('Simpan & Tambah Baru')}
              </button>
            )}
            <button className="btn primary" onClick={() => handleSave(false)} disabled={saving} title="Ctrl+S">
              {saving ? tGlobal('Menyimpan...') : tGlobal('Simpan')}
            </button>
          </ModalFooter>
        </ModalContent>
      </Modal>

      <AuditModal target={auditTarget} onClose={() => setAuditTarget(null)} entityName={auditEntityName} />
    </>
  );
}
