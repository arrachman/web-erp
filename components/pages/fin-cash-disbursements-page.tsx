'use client';

/**
 * Kas Keluar (Cash Disbursement / CD) — list (§2.7) + master-detail form.
 * Atomic tier: Page. URL-driven list↔form via trx sub-routes (§2.3.1):
 * /finance/cash-disbursements · /new · /:id. Shared backend (direction=DISBURSEMENT).
 */

import * as React from 'react';
import {
  type ListPaginationConfig,
  type SummaryConfig,
} from '@/components/organisms/erp-list-layout';
import {
  emptyCdFilters,
  type CdFilters,
} from './fin-cash-disbursements-filters';
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
import {
  listCashDisbursements,
  createCashDisbursement,
  updateCashDisbursement,
  deleteCashDisbursement,
  getCashDisbursement,
  transitionCashDisbursement,
  type CashBankTransition,
  type ErpCashDisbursement,
  type ErpDocumentStatus,
} from '@/lib/api/fin-cash-disbursements';
import {
  CashDisbursementForm,
  defaultCashDisbursementForm,
  fromCashDisbursement,
  toCashDisbursementPayload,
  type CashDisbursementFormData,
} from './fin-cash-disbursements-form';
import { useAllowedCreationStatuses } from '@/lib/use-allowed-creation-statuses';
import { CD_BASE, TRANSITION_VERBS } from './fin-cash-disbursements-config';
import { CashDisbursementFormView } from './fin-cash-disbursements-form-view';
import { CashDisbursementsList } from './fin-cash-disbursements-list';

export function ErpCashDisbursementsPage({
  formMode,
  recordId,
  onNavigate,
}: TrxFormPageProps = {}) {
  // The route drives list↔form: form view whenever a form sub-route is active.
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const [form, setForm] = React.useState<CashDisbursementFormData>(defaultCashDisbursementForm);
  const [saving, setSaving] = React.useState(false);
  const { statuses: allowedCreationStatuses } = useAllowedCreationStatuses('CASH_DISBURSEMENT');

  // In edit mode the form is only ready once the loaded record matches the
  // route id — mounting the form before then would let its currency effect
  // clobber the fetched record with stale defaults (race).
  const formReady =
    formMode === 'create' ||
    (formMode === 'edit' && String(form.id ?? '') === String(recordId ?? ''));

  const goList = React.useCallback(() => onNavigate?.(CD_BASE), [onNavigate]);

  const [search, setSearch] = React.useState('');
  const [filters, setFilters] = React.useState<CdFilters>(emptyCdFilters);
  const { page, pageSize, setPage, setPageSize } = useListPagination('fin-cash-disbursements');

  // Debounce search + the whole filter object (text fields type-as-you-go).
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
      listCashDisbursements({
        page,
        limit: pageSize,
        search: debouncedSearch || undefined,
        status: (debF.status || undefined) as ErpDocumentStatus | undefined,
        dateFrom: debF.dateFrom || undefined,
        dateTo: debF.dateTo || undefined,
        docNumberFrom: debF.noFrom || undefined,
        docNumberTo: debF.noTo || undefined,
        partnerId: debF.partnerId || undefined,
        locationId: debF.locationId || undefined,
        branchId: debF.branchId || undefined,
        description: debF.uraian || undefined,
        notes: debF.catatan || undefined,
        createdById: debF.userId || undefined,
        sortBy: 'transactionDate',
        sortDir: 'desc',
      }),
    [page, pageSize, debouncedSearch, debF],
  );

  React.useEffect(() => {
    setPage(1);
  }, [debouncedSearch, debF, pageSize]);

  const [focused, setFocused] = React.useState(-1);
  const [selected, setSelected] = React.useState<Set<string>>(new Set());
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  // Navigation drives the form: open/edit/back are all route changes.
  const openCreate = () => onNavigate?.(trxNewRoute(CD_BASE));
  const openEdit = (r: ErpCashDisbursement) => onNavigate?.(trxEditRoute(CD_BASE, r.id));

  // Populate the form from the active route. Create → blank; edit → fetch by
  // id (so a deep link / refresh on /:id loads correctly without a list row).
  const loadForm = React.useCallback(() => {
    if (formMode === 'create') {
      setForm(defaultCashDisbursementForm());
      return undefined;
    }
    if (formMode === 'edit' && recordId) {
      let alive = true;
      getCashDisbursement(recordId)
        .then((full) => alive && setForm(fromCashDisbursement(full)))
        .catch(() => {
          if (!alive) return;
          notify('Gagal memuat Kas Keluar', 'danger');
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
    if (!form.branchId || !form.bankAccountId || !form.description) {
      notify('Cabang, Akun Kas, dan Uraian wajib diisi.', 'warn');
      return;
    }
    setSaving(true);
    try {
      const payload = toCashDisbursementPayload(form);
      if (form.id) {
        await updateCashDisbursement(form.id, payload);
        notify('Kas Keluar diperbarui', 'success');
      } else {
        await createCashDisbursement(payload);
        notify('Kas Keluar dibuat', 'success');
      }
      reload();
      if (newAfter) {
        setForm(defaultCashDisbursementForm());
        onNavigate?.(trxNewRoute(CD_BASE));
      } else if (closeAfter) {
        goList();
      }
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const runTransition = async (r: ErpCashDisbursement, action: CashBankTransition) => {
    let reason: string | undefined;
    if (action === 'REJECT') {
      reason = window.prompt('Alasan menolak dokumen ini?') ?? undefined;
      if (!reason) return;
    }
    try {
      await transitionCashDisbursement(r.id, action, reason);
      notify(`Berhasil ${TRANSITION_VERBS[action]} ${r.docNumber}`, 'success');
      reload();
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal', 'danger');
    }
  };

  const handleDelete = (r: ErpCashDisbursement) => {
    confirmAction({
      title: 'Hapus Kas Keluar?',
      message: `${r.docNumber} akan dihapus permanen.`,
      variant: 'danger',
      confirmLabel: 'Hapus',
      confirmIcon: 'trash',
      onConfirm: async () => {
        try {
          await deleteCashDisbursement(r.id);
          notify('Kas Keluar dihapus', 'success');
          reload();
        } catch (e: unknown) {
          notify(e instanceof Error ? e.message : 'Gagal', 'danger');
        }
      },
    });
  };

  const rowActions = (r: ErpCashDisbursement): RowActionItem[] => [
    { label: 'Edit / Lihat', onSelect: () => openEdit(r) },
    ...cashBankWorkflowActions(r.status, (a) => runTransition(r, a)),
    { label: 'Hapus', onSelect: () => handleDelete(r), danger: true, separatorBefore: true },
  ];

  // ── form view ───────────────────────────────────────────────────────────────
  if (mode === 'form') {
    return (
      <CashDisbursementFormView
        title="Kas Keluar"
        code="CD"
        formReady={formReady}
        onBack={goList}
      >
        <CashDisbursementForm
          data={form}
          onChange={setForm}
          saving={saving}
          allowedCreationStatuses={formMode === 'create' ? allowedCreationStatuses : undefined}
          onSave={() => persist(true)}
          onSaveNew={() => persist(false, true)}
          onReset={loadForm}
        />
      </CashDisbursementFormView>
    );
  }

  // ── list view ─────────────────────────────────────────────────────────────────
  const summary: SummaryConfig = { metricLabel: 'Σ Kas Keluar', rowCount: rows.length, totalCount: totalRows };
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
      message: `${selected.size} Kas Keluar akan dihapus permanen.`,
      variant: 'danger',
      confirmLabel: 'Hapus',
      onConfirm: async () => {
        await Promise.all([...selected].map((id) => deleteCashDisbursement(id).catch(() => null)));
        notify(`${selected.size} dokumen dihapus`, 'success');
        setSelected(new Set());
        reload();
      },
    });

  return (
    <CashDisbursementsList
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
      onClearSelection={() => setSelected(new Set())}
      onBulkDelete={handleBulkDelete}
      focused={focused}
      onFocusChange={setFocused}
      rowActions={rowActions}
      onEdit={openEdit}
      summary={summary}
      pagination={pagination}
    />
  );
}