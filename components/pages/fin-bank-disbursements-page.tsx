'use client';

/**
 * Bank Keluar (Bank Disbursement / SM) — list (§2.7) + master-detail form.
 * Atomic tier: Page. URL-driven list↔form via trx sub-routes (§2.3.1):
 * /finance/bank-disbursements · /new · /:id. Shared backend
 * (direction=DISBURSEMENT, kind=BANK). Mirrors Kas Keluar; bank-only bits =
 * Cara Bayar column + Giro tab (handled in the form wrapper).
 */

import * as React from 'react';
import {
  type ListPaginationConfig,
  type SummaryConfig,
} from '@/components/organisms/erp-list-layout';
import {
  emptyBdFilters,
  type BdFilters,
} from './fin-bank-disbursements-filters';
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
  listBankDisbursements,
  createBankDisbursement,
  updateBankDisbursement,
  deleteBankDisbursement,
  getBankDisbursement,
  transitionBankDisbursement,
  type CashBankTransition,
  type ErpBankDisbursement,
  type ErpDocumentStatus,
} from '@/lib/api/fin-bank-disbursements';
import { useAllowedCreationStatuses } from '@/lib/use-allowed-creation-statuses';
import {
  BankDisbursementForm,
  defaultBankDisbursementForm,
  fromBankDisbursement,
  toBankDisbursementPayload,
  type BankDisbursementFormData,
} from './fin-bank-disbursements-form';
import { BD_BASE, TRANSITION_VERBS } from './fin-bank-disbursements-config';
import { BankDisbursementFormView } from './fin-bank-disbursements-form-view';
import { BankDisbursementsList } from './fin-bank-disbursements-list';

export function ErpBankDisbursementsPage({
  formMode,
  recordId,
  onNavigate,
}: TrxFormPageProps = {}) {
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const [form, setForm] = React.useState<BankDisbursementFormData>(defaultBankDisbursementForm);
  const [saving, setSaving] = React.useState(false);
  const { statuses: allowedCreationStatuses } = useAllowedCreationStatuses('BANK_DISBURSEMENT');

  // In edit mode the form is only ready once the loaded record matches the
  // route id — mounting earlier lets the currency effect clobber it (race).
  const formReady =
    formMode === 'create' ||
    (formMode === 'edit' && String(form.id ?? '') === String(recordId ?? ''));

  const goList = React.useCallback(() => onNavigate?.(BD_BASE), [onNavigate]);

  const [search, setSearch] = React.useState('');
  const [filters, setFilters] = React.useState<BdFilters>(emptyBdFilters);
  const { page, pageSize, setPage, setPageSize } = useListPagination('fin-bank-disbursements');

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
      listBankDisbursements({
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

  const openCreate = () => onNavigate?.(trxNewRoute(BD_BASE));
  const openEdit = (r: ErpBankDisbursement) => onNavigate?.(trxEditRoute(BD_BASE, r.id));

  const loadForm = React.useCallback(() => {
    if (formMode === 'create') {
      setForm(defaultBankDisbursementForm());
      return undefined;
    }
    if (formMode === 'edit' && recordId) {
      let alive = true;
      getBankDisbursement(recordId)
        .then((full) => alive && setForm(fromBankDisbursement(full)))
        .catch(() => {
          if (!alive) return;
          notify('Gagal memuat Bank Keluar', 'danger');
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
      notify('Cabang, Akun Bank, dan Uraian wajib diisi.', 'warn');
      return;
    }
    setSaving(true);
    try {
      const payload = toBankDisbursementPayload(form);
      if (form.id) {
        await updateBankDisbursement(form.id, payload);
        notify('Bank Keluar diperbarui', 'success');
      } else {
        await createBankDisbursement(payload);
        notify('Bank Keluar dibuat', 'success');
      }
      reload();
      if (newAfter) {
        setForm(defaultBankDisbursementForm());
        onNavigate?.(trxNewRoute(BD_BASE));
      } else if (closeAfter) {
        goList();
      }
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const runTransition = async (r: ErpBankDisbursement, action: CashBankTransition) => {
    let reason: string | undefined;
    if (action === 'REJECT') {
      reason = window.prompt('Alasan menolak dokumen ini?') ?? undefined;
      if (!reason) return;
    }
    try {
      await transitionBankDisbursement(r.id, action, reason);
      notify(`Berhasil ${TRANSITION_VERBS[action]} ${r.docNumber}`, 'success');
      reload();
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal', 'danger');
    }
  };

  const handleDelete = (r: ErpBankDisbursement) => {
    confirmAction({
      title: 'Hapus Bank Keluar?',
      message: `${r.docNumber} akan dihapus permanen.`,
      variant: 'danger',
      confirmLabel: 'Hapus',
      confirmIcon: 'trash',
      onConfirm: async () => {
        try {
          await deleteBankDisbursement(r.id);
          notify('Bank Keluar dihapus', 'success');
          reload();
        } catch (e: unknown) {
          notify(e instanceof Error ? e.message : 'Gagal', 'danger');
        }
      },
    });
  };

  const rowActions = (r: ErpBankDisbursement): RowActionItem[] => [
    { label: 'Edit / Lihat', onSelect: () => openEdit(r) },
    ...cashBankWorkflowActions(r.status, (a) => runTransition(r, a)),
    { label: 'Hapus', onSelect: () => handleDelete(r), danger: true, separatorBefore: true },
  ];

  // ── form view ───────────────────────────────────────────────────────────────
  if (mode === 'form') {
    return (
      <BankDisbursementFormView
        title="Bank Keluar"
        code="SM"
        formReady={formReady}
        onBack={goList}
      >
        <BankDisbursementForm
          data={form}
          onChange={setForm}
          saving={saving}
          allowedCreationStatuses={formMode === 'create' ? allowedCreationStatuses : undefined}
          onSave={() => persist(true)}
          onSaveNew={() => persist(false, true)}
          onReset={loadForm}
        />
      </BankDisbursementFormView>
    );
  }

  // ── list view ─────────────────────────────────────────────────────────────────
  const summary: SummaryConfig = { metricLabel: 'Σ Bank Keluar', rowCount: rows.length, totalCount: totalRows };
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
      message: `${selected.size} Bank Keluar akan dihapus permanen.`,
      variant: 'danger',
      confirmLabel: 'Hapus',
      onConfirm: async () => {
        await Promise.all([...selected].map((id) => deleteBankDisbursement(id).catch(() => null)));
        notify(`${selected.size} dokumen dihapus`, 'success');
        setSelected(new Set());
        reload();
      },
    });

  return (
    <BankDisbursementsList
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
      onBulkDelete={handleBulkDelete}
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