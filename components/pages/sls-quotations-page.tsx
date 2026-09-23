'use client';

/**
 * Sales Quotation (SQ) — list (§2.7) + master-detail form. Atomic tier: Page.
 * URL-driven list↔form via trx sub-routes (§2.3.1): /sales/quotations · /new · /:id.
 * Route = SSOT, no internal mode state.
 */

import * as React from 'react';
import {
  type ListPaginationConfig,
  type SummaryConfig,
} from '@/components/organisms/erp-list-layout';
import {
  emptySlsQuotFilters,
  type SlsQuotFilters,
} from './sls-quotations-filters';
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
  listSlsQuotations,
  createSlsQuotation,
  updateSlsQuotation,
  deleteSlsQuotation,
  getSlsQuotation,
  transitionSlsQuotation,
  type SlsQuotationTransition,
  type ErpSlsQuotation,
  type ErpDocumentStatus,
} from '@/lib/api/sls-quotations';
import { useAllowedCreationStatuses } from '@/lib/use-allowed-creation-statuses';
import {
  SlsQuotationForm,
  defaultSlsQuotationForm,
  fromSlsQuotation,
  toSlsQuotationPayload,
  type SlsQuotationFormData,
} from './sls-quotation-form';
import { SQ_BASE, TRANSITION_VERBS } from './sls-quotations-config';
import { QuotationFormView } from './sls-quotations-form-view';
import { SlsQuotationsList } from './sls-quotations-list';

export function ErpSlsQuotationsPage({ formMode, recordId, onNavigate }: TrxFormPageProps = {}) {
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const [form, setForm] = React.useState<SlsQuotationFormData>(defaultSlsQuotationForm);
  const [saving, setSaving] = React.useState(false);
  const { statuses: allowedCreationStatuses } = useAllowedCreationStatuses('SLS.SQ');

  const formReady =
    formMode === 'create' ||
    (formMode === 'edit' && String(form.id ?? '') === String(recordId ?? ''));

  const goList = React.useCallback(() => onNavigate?.(SQ_BASE), [onNavigate]);

  const [search, setSearch] = React.useState('');
  const [filters, setFilters] = React.useState<SlsQuotFilters>(emptySlsQuotFilters);
  const [sortBy, setSortBy] = React.useState('docDate');
  const [sortDir, setSortDir] = React.useState<'asc' | 'desc'>('desc');
  const { page, pageSize, setPage, setPageSize } = useListPagination('sls-quotations');

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
      listSlsQuotations({
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

  const openCreate = () => onNavigate?.(trxNewRoute(SQ_BASE));
  const openEdit = (r: ErpSlsQuotation) => onNavigate?.(trxEditRoute(SQ_BASE, r.id));

  const loadForm = React.useCallback(() => {
    if (formMode === 'create') {
      setForm(defaultSlsQuotationForm());
      return undefined;
    }
    if (formMode === 'edit' && recordId) {
      let alive = true;
      getSlsQuotation(recordId)
        .then((full) => alive && setForm(fromSlsQuotation(full)))
        .catch(() => {
          if (!alive) return;
          notify('Gagal memuat Sales Quotation', 'danger');
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
      const payload = toSlsQuotationPayload(form);
      if (form.id) {
        await updateSlsQuotation(form.id, payload);
        notify('Sales Quotation diperbarui', 'success');
      } else {
        await createSlsQuotation(payload);
        notify('Sales Quotation dibuat', 'success');
      }
      reload();
      if (newAfter) {
        setForm(defaultSlsQuotationForm());
        onNavigate?.(trxNewRoute(SQ_BASE));
      } else if (closeAfter) {
        goList();
      }
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const runTransition = async (r: ErpSlsQuotation, action: SlsQuotationTransition) => {
    let reason: string | undefined;
    if (action === 'REJECT') {
      reason = window.prompt('Alasan menolak dokumen ini?') ?? undefined;
      if (!reason) return;
    }
    try {
      await transitionSlsQuotation(r.id, action, reason);
      notify(`Berhasil ${TRANSITION_VERBS[action]} ${r.docNumber}`, 'success');
      reload();
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal', 'danger');
    }
  };

  const handleDelete = (r: ErpSlsQuotation) => {
    confirmAction({
      title: 'Hapus Sales Quotation?',
      message: `${r.docNumber} akan dihapus permanen.`,
      variant: 'danger',
      confirmLabel: 'Hapus',
      confirmIcon: 'trash',
      onConfirm: async () => {
        try {
          await deleteSlsQuotation(r.id);
          notify('Sales Quotation dihapus', 'success');
          reload();
        } catch (e: unknown) {
          notify(e instanceof Error ? e.message : 'Gagal', 'danger');
        }
      },
    });
  };

  const rowActions = (r: ErpSlsQuotation): RowActionItem[] => [
    { label: 'Edit / Lihat', onSelect: () => openEdit(r) },
    ...cashBankWorkflowActions(r.status, (a) => runTransition(r, a)),
    { label: 'Hapus', onSelect: () => handleDelete(r), danger: true, separatorBefore: true },
  ];

  // ── form view ───────────────────────────────────────────────────────────────
  if (mode === 'form') {
    return (
      <QuotationFormView
        title="Sales Quotation"
        code="SQ"
        formReady={formReady}
        onBack={goList}
      >
        <SlsQuotationForm
          data={form}
          onChange={setForm}
          saving={saving}
          allowedCreationStatuses={formMode === 'create' ? allowedCreationStatuses : undefined}
          onSave={() => persist(true)}
          onSaveNew={() => persist(false, true)}
          onReset={loadForm}
        />
      </QuotationFormView>
    );
  }

  // ── list view ─────────────────────────────────────────────────────────────────
  const sumGT = (meta as { sumGrandTotal?: string } | null)?.sumGrandTotal;
  const summary: SummaryConfig = {
    metricLabel: 'Σ Quotation',
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
      message: `${selected.size} Sales Quotation akan dihapus permanen.`,
      variant: 'danger',
      confirmLabel: 'Hapus',
      onConfirm: async () => {
        await Promise.all([...selected].map((id) => deleteSlsQuotation(id).catch(() => null)));
        notify(`${selected.size} dokumen dihapus`, 'success');
        setSelected(new Set());
        reload();
      },
    });

  return (
    <SlsQuotationsList
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