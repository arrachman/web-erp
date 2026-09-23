'use client';

/**
 * Kas Masuk (Cash Receipt / CR) — list (§2.7) + master-detail form.
 * Atomic tier: Page. URL-driven list↔form via trx sub-routes (§2.3.1):
 * /finance/cash-receipts · /new · /:id. Route = SSOT, no internal mode state.
 */

import * as React from 'react';
import {
  type ListPaginationConfig,
  type SummaryConfig,
} from '@/components/organisms/erp-list-layout';
import { emptyCrFilters, type CrFilters } from './fin-cash-receipts-filters';
import {
  type RowActionItem,
} from '@/components/molecules/row-actions-menu';
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
  listCashReceipts,
  createCashReceipt,
  updateCashReceipt,
  deleteCashReceipt,
  getCashReceipt,
  transitionCashReceipt,
  type CashBankTransition,
  type ErpCashReceipt,
  type ErpDocumentStatus,
} from '@/lib/api/fin-cash-receipts';
import {
  CashReceiptForm,
  defaultCashReceiptForm,
  fromCashReceipt,
  toCashReceiptPayload,
  type CashReceiptFormData,
  type CashReceiptFormHandle,
} from './fin-cash-receipts-form';
import { useAllowedCreationStatuses } from '@/lib/use-allowed-creation-statuses';
import { CR_BASE, TRANSITION_VERBS } from './fin-cash-receipts-config';
import { CashReceiptFormView } from './fin-cash-receipts-form-view';
import { CashReceiptsList } from './fin-cash-receipts-list';

export function ErpCashReceiptsPage({
  formMode,
  recordId,
  onNavigate,
}: TrxFormPageProps = {}) {
  // The route drives list↔form: form view whenever a form sub-route is active.
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const [form, setForm] = React.useState<CashReceiptFormData>(defaultCashReceiptForm);
  const [saving, setSaving] = React.useState(false);
  const formRef = React.useRef<CashReceiptFormHandle>(null);
  const { statuses: allowedCreationStatuses } = useAllowedCreationStatuses('CASH_RECEIPT');

  // In edit mode the form is only ready once the loaded record matches the
  // route id — mounting CashReceiptForm before then would let its currency
  // effect clobber the fetched record with stale defaults (race).
  const formReady =
    formMode === 'create' ||
    (formMode === 'edit' && String(form.id ?? '') === String(recordId ?? ''));

  const goList = React.useCallback(() => onNavigate?.(CR_BASE), [onNavigate]);

  const [search, setSearch] = React.useState('');
  const [filters, setFilters] = React.useState<CrFilters>(emptyCrFilters);
  const { page, pageSize, setPage, setPageSize } = useListPagination('fin-cash-receipts');

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

  // Fixed transaction-date descending sort is set in the API request (not
  // via clickable sort headers in the table) — preserve this exactly.
  const { rows, meta, loading, error, reload } = useErpList(
    () =>
      listCashReceipts({
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
  const openCreate = () => onNavigate?.(trxNewRoute(CR_BASE));
  const openEdit = (r: ErpCashReceipt) => onNavigate?.(trxEditRoute(CR_BASE, r.id));

  // Populate the form from the active route. Create → blank; edit → fetch by
  // id (so a deep link / refresh on /:id loads correctly without a list row).
  const loadForm = React.useCallback(() => {
    if (formMode === 'create') {
      setForm(defaultCashReceiptForm());
      return undefined;
    }
    if (formMode === 'edit' && recordId) {
      let alive = true;
      getCashReceipt(recordId)
        .then((full) => alive && setForm(fromCashReceipt(full)))
        .catch(() => {
          if (!alive) return;
          notify('Gagal memuat Kas Masuk', 'danger');
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
    // Validate required header fields; focus the first one that is empty.
    // Domain behavior — kept in the page (queries ref + focuses).
    const missing: { key: string; label: string }[] = [
      { key: 'branchId', label: 'Cabang' },
      { key: 'bankAccountId', label: 'Akun Kas' },
      { key: 'description', label: 'Uraian' },
    ].filter(({ key }) => !form[key as keyof typeof form]);

    if (missing.length) {
      notify(`${missing.map((f) => f.label).join(', ')} wajib diisi.`, 'warn');
      // Focus the first missing field so the user lands right on it.
      formRef.current?.focusField(missing[0].key);
      return;
    }
    setSaving(true);
    try {
      const payload = toCashReceiptPayload(form);
      if (form.id) {
        await updateCashReceipt(form.id, payload);
        notify('Kas Masuk diperbarui', 'success');
      } else {
        await createCashReceipt(payload);
        notify('Kas Masuk dibuat', 'success');
      }
      reload();
      if (newAfter) {
        setForm(defaultCashReceiptForm());
        onNavigate?.(trxNewRoute(CR_BASE));
      } else if (closeAfter) {
        goList();
      }
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const runTransition = async (r: ErpCashReceipt, action: CashBankTransition) => {
    let reason: string | undefined;
    if (action === 'REJECT') {
      reason = window.prompt('Alasan menolak dokumen ini?') ?? undefined;
      if (!reason) return;
    }
    try {
      await transitionCashReceipt(r.id, action, reason);
      notify(`Berhasil ${TRANSITION_VERBS[action]} ${r.docNumber}`, 'success');
      reload();
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal', 'danger');
    }
  };

  const handleDelete = (r: ErpCashReceipt) => {
    confirmAction({
      title: 'Hapus Kas Masuk?',
      message: `${r.docNumber} akan dihapus permanen.`,
      variant: 'danger',
      confirmLabel: 'Hapus',
      confirmIcon: 'trash',
      onConfirm: async () => {
        try {
          await deleteCashReceipt(r.id);
          notify('Kas Masuk dihapus', 'success');
          reload();
        } catch (e: unknown) {
          notify(e instanceof Error ? e.message : 'Gagal', 'danger');
        }
      },
    });
  };

  const rowActions = (r: ErpCashReceipt): RowActionItem[] => [
    { label: 'Edit / Lihat', onSelect: () => openEdit(r) },
    ...cashBankWorkflowActions(r.status, (a) => runTransition(r, a)),
    { label: 'Hapus', onSelect: () => handleDelete(r), danger: true, separatorBefore: true },
  ];

  // ── form view ───────────────────────────────────────────────────────────────
  if (mode === 'form') {
    return (
      <CashReceiptFormView
        title="Kas Masuk"
        code="CR"
        formReady={formReady}
        onBack={goList}
      >
        <CashReceiptForm
          ref={formRef}
          data={form}
          onChange={setForm}
          saving={saving}
          allowedCreationStatuses={formMode === 'create' ? allowedCreationStatuses : undefined}
          onSave={() => persist(true)}
          onSaveNew={() => persist(false, true)}
          onReset={loadForm}
        />
      </CashReceiptFormView>
    );
  }

  // ── list view ─────────────────────────────────────────────────────────────────
  const summary: SummaryConfig = {
    metricLabel: 'Σ Kas Masuk',
    rowCount: rows.length,
    totalCount: totalRows,
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

  return (
    <CashReceiptsList
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
      focused={focused}
      onFocusChange={setFocused}
      rowActions={rowActions}
      onEdit={openEdit}
      summary={summary}
      pagination={pagination}
    />
  );
}