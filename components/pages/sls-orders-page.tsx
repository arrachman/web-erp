'use client';

/**
 * Sales Order (SO) — list (§2.7) + master-detail form. Atomic tier: Page.
 * URL-driven list↔form via trx sub-routes (§2.3.1): /sales/orders · /new · /:id.
 * Route = SSOT, no internal mode state.
 */

import * as React from 'react';
import {
  type ListPaginationConfig,
  type SummaryConfig,
} from '@/components/organisms/erp-list-layout';
import { emptySlsFilters, type SlsFilters } from './sls-orders-filters';
import type { RowActionItem } from '@/components/molecules/row-actions-menu';
import { confirmAction, notify } from '@/lib/feedback';
import { cashBankWorkflowActions } from '@/lib/fin-cash-bank-workflow';
import {
  trxNewRoute,
  trxEditRoute,
  type TrxFormPageProps,
} from '@/lib/trx-route';
import { useErpList } from '@/lib/use-erp-list';
import { useListPagination } from '@/lib/use-list-pagination';
import { formatNumber } from '@/lib/format';
import {
  listSlsOrders,
  createSlsOrder,
  updateSlsOrder,
  deleteSlsOrder,
  getSlsOrder,
  transitionSlsOrder,
  type SlsOrderTransition,
  type ErpSlsOrder,
  type ErpDocumentStatus,
} from '@/lib/api/sls-orders';
import { useAllowedCreationStatuses } from '@/lib/use-allowed-creation-statuses';
import {
  SlsOrderForm,
  defaultSlsOrderForm,
  fromSlsOrder,
  toSlsOrderPayload,
  type SlsOrderFormData,
} from './sls-order-form';
import { SO_BASE, TRANSITION_VERBS } from './sls-orders-config';
import { SlsOrdersFormView } from './sls-orders-form-view';
import { SlsOrdersList } from './sls-orders-list';

export function ErpSlsOrdersPage({ formMode, recordId, onNavigate }: TrxFormPageProps = {}) {
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const [form, setForm] = React.useState<SlsOrderFormData>(defaultSlsOrderForm);
  const [saving, setSaving] = React.useState(false);
  const { statuses: allowedCreationStatuses } = useAllowedCreationStatuses('SLS.SO');

  const formReady =
    formMode === 'create' ||
    (formMode === 'edit' && String(form.id ?? '') === String(recordId ?? ''));

  const goList = React.useCallback(() => onNavigate?.(SO_BASE), [onNavigate]);

  const [search, setSearch] = React.useState('');
  const [filters, setFilters] = React.useState<SlsFilters>(emptySlsFilters);
  const [sortBy, setSortBy] = React.useState('docDate');
  const [sortDir, setSortDir] = React.useState<'asc' | 'desc'>('desc');
  const { page, pageSize, setPage, setPageSize } = useListPagination('sls-orders');

  const [debouncedSearch, setDebouncedSearch] = React.useState(search);
  const [debF, setDebF] = React.useState(filters);
  React.useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(search), 300);
    return () => clearTimeout(t);
  }, [search]);
  React.useEffect(() => {
    const t = setTimeout(() => setDebF(filters), 350);
    return () => clearTimeout(t);
  }, [filters]);

  const { rows, meta, loading, error, reload } = useErpList(
    () =>
      listSlsOrders({
        page,
        limit: pageSize,
        search: debouncedSearch || undefined,
        status: (debF.status || undefined) as ErpDocumentStatus | undefined,
        dateFrom: debF.dateFrom || undefined,
        dateTo: debF.dateTo || undefined,
        docNumberFrom: debF.docNumber || undefined,
        description: debF.uraian || undefined,
        sortBy,
        sortDir,
      }),
    [page, pageSize, debouncedSearch, debF, sortBy, sortDir],
  );

  React.useEffect(() => {
    setPage(1);
  }, [debouncedSearch, debF, pageSize, sortBy, sortDir]);

  const [focused, setFocused] = React.useState(-1);
  const [selected, setSelected] = React.useState<Set<string>>(new Set());
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  const toggleSort = (col: string) => {
    if (sortBy === col) setSortDir((d) => (d === 'asc' ? 'desc' : 'asc'));
    else { setSortBy(col); setSortDir('asc'); }
    setPage(1);
  };

  const openCreate = () => onNavigate?.(trxNewRoute(SO_BASE));
  const openEdit = (r: ErpSlsOrder) => onNavigate?.(trxEditRoute(SO_BASE, r.id));

  const loadForm = React.useCallback(() => {
    if (formMode === 'create') {
      setForm(defaultSlsOrderForm());
      return undefined;
    }
    if (formMode === 'edit' && recordId) {
      let alive = true;
      getSlsOrder(recordId)
        .then((full) => alive && setForm(fromSlsOrder(full)))
        .catch(() => {
          if (!alive) return;
          notify('Gagal memuat Sales Order', 'danger');
          goList();
        });
      return () => {
        alive = false;
      };
    }
    return undefined;
  }, [formMode, recordId, goList]);
  React.useEffect(() => loadForm(), [loadForm]);

  const persist = async (closeAfter: boolean, newAfter = false) => {
    if (!form.branchId || !form.docDate || !form.currencyId) {
      notify('Cabang, Tanggal, dan Mata Uang wajib diisi.', 'warn');
      return;
    }
    if (!form.lines.some((l) => l.itemId && Number(l.quantity) > 0)) {
      notify('Minimal satu baris item dengan qty > 0.', 'warn');
      return;
    }
    setSaving(true);
    try {
      const payload = toSlsOrderPayload(form);
      if (form.id) {
        await updateSlsOrder(form.id, payload);
        notify('Sales Order diperbarui', 'success');
      } else {
        await createSlsOrder(payload);
        notify('Sales Order dibuat', 'success');
      }
      reload();
      if (newAfter) {
        setForm(defaultSlsOrderForm());
        onNavigate?.(trxNewRoute(SO_BASE));
      } else if (closeAfter) {
        goList();
      }
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const runTransition = async (r: ErpSlsOrder, action: SlsOrderTransition) => {
    let reason: string | undefined;
    if (action === 'REJECT') {
      reason = window.prompt('Alasan menolak dokumen ini?') ?? undefined;
      if (!reason) return;
    }
    try {
      await transitionSlsOrder(r.id, action, reason);
      notify(`Berhasil ${TRANSITION_VERBS[action]} ${r.docNumber}`, 'success');
      reload();
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal', 'danger');
    }
  };

  const handleDelete = (r: ErpSlsOrder) => {
    confirmAction({
      title: 'Hapus Sales Order?',
      message: `${r.docNumber} akan dihapus permanen.`,
      variant: 'danger',
      confirmLabel: 'Hapus',
      confirmIcon: 'trash',
      onConfirm: async () => {
        try {
          await deleteSlsOrder(r.id);
          notify('Sales Order dihapus', 'success');
          reload();
        } catch (e: unknown) {
          notify(e instanceof Error ? e.message : 'Gagal', 'danger');
        }
      },
    });
  };

  const rowActions = (r: ErpSlsOrder): RowActionItem[] => [
    { label: 'Edit / Lihat', onSelect: () => openEdit(r) },
    ...cashBankWorkflowActions(r.status, (a) => runTransition(r, a)),
    { label: 'Hapus', onSelect: () => handleDelete(r), danger: true, separatorBefore: true },
  ];

  // ── form view ───────────────────────────────────────────────────────────────
  if (mode === 'form') {
    return (
      <SlsOrdersFormView
        title="Sales Order"
        code="SO"
        formReady={formReady}
        onBack={goList}
      >
        <SlsOrderForm
          data={form}
          onChange={setForm}
          saving={saving}
          allowedCreationStatuses={formMode === 'create' ? allowedCreationStatuses : undefined}
          onSave={() => persist(true)}
          onSaveNew={() => persist(false, true)}
          onReset={loadForm}
        />
      </SlsOrdersFormView>
    );
  }

  // ── list view ─────────────────────────────────────────────────────────────────
  const sumGT = (meta as { sumGrandTotal?: string } | null)?.sumGrandTotal;
  const summary: SummaryConfig = {
    metricLabel: 'Σ Sales Order',
    rowCount: rows.length,
    totalCount: totalRows,
    metricValue: sumGT ? formatNumber(Number(sumGT), 2) : undefined,
  };
  const pagination: ListPaginationConfig = {
    page,
    pageCount,
    pageSize,
    totalRows,
    onPage: setPage,
    onPageSize: setPageSize,
  };
  const toggleSel = (id: string) =>
    setSelected((s) => {
      const n = new Set(s);
      n.has(id) ? n.delete(id) : n.add(id);
      return n;
    });
  const handleBulkDelete = () =>
    confirmAction({
      title: 'Hapus terpilih?',
      message: `${selected.size} Sales Order akan dihapus permanen.`,
      variant: 'danger',
      confirmLabel: 'Hapus',
      onConfirm: async () => {
        await Promise.all([...selected].map((id) => deleteSlsOrder(id).catch(() => null)));
        notify(`${selected.size} dokumen dihapus`, 'success');
        setSelected(new Set());
        reload();
      },
    });

  return (
    <SlsOrdersList
      rows={rows}
      loading={loading}
      error={error}
      search={search}
      onSearch={setSearch}
      onAdd={openCreate}
      onRefresh={reload}
      filters={filters}
      onFiltersChange={setFilters}
      selected={selected}
      onToggleSelect={toggleSel}
      onSelectAll={(ids) => setSelected(new Set(ids))}
      onClearSelection={() => setSelected(new Set())}
      onBulkDelete={handleBulkDelete}
      sortBy={sortBy}
      sortDir={sortDir}
      onSort={toggleSort}
      focused={focused}
      onFocusChange={setFocused}
      rowActions={rowActions}
      onEdit={openEdit}
      summary={summary}
      pagination={pagination}
    />
  );
}