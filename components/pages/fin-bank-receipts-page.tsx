'use client';

/**
 * Bank Masuk (Bank Receipt / RM) — list (§2.7) + master-detail form.
 * Atomic tier: Page. Twin of Kas Masuk: URL-driven list↔form via trx
 * sub-routes (§2.3.1): /finance/bank-receipts · /new · /:id. Route = SSOT.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Badge } from '@/components/ui/badge';
import {
  ErpListLayout,
  type ListPaginationConfig,
  type SummaryConfig,
} from '@/components/organisms/erp-list-layout';
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableCell,
  TableEmpty,
  CodeLinkCell,
} from '@/components/organisms/table';
import {
  CashReceiptFilters,
  emptyCrFilters,
  type CrFilters,
} from './fin-cash-receipts-filters';
import {
  RowActionsMenu,
  RowContextMenu,
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
import { formatNumber } from '@/lib/format';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import {
  listBankReceipts,
  createBankReceipt,
  updateBankReceipt,
  deleteBankReceipt,
  getBankReceipt,
  transitionBankReceipt,
  type CashBankTransition,
  type ErpBankReceipt,
  type ErpDocumentStatus,
} from '@/lib/api/fin-bank-receipts';
import {
  BankReceiptForm,
  defaultBankReceiptForm,
  fromBankReceipt,
  toBankReceiptPayload,
  type BankReceiptFormData,
} from './fin-bank-receipts-form';
import { paymentMethodLabel } from './fin-bank-receipts-form-model';
import { useAllowedCreationStatuses } from '@/lib/use-allowed-creation-statuses';

/** Canonical list path (seeded `sys_menus.path`); base for /new and /:id. */
const BR_BASE = '/finance/bank-receipts';

export function ErpBankReceiptsPage({
  formMode,
  recordId,
  onNavigate,
}: TrxFormPageProps = {}) {
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const [form, setForm] = React.useState<BankReceiptFormData>(defaultBankReceiptForm);
  const [saving, setSaving] = React.useState(false);
  const { statuses: allowedCreationStatuses } = useAllowedCreationStatuses('BANK_RECEIPT');

  const formReady =
    formMode === 'create' ||
    (formMode === 'edit' && String(form.id ?? '') === String(recordId ?? ''));

  const goList = React.useCallback(() => onNavigate?.(BR_BASE), [onNavigate]);

  const [search, setSearch] = React.useState('');
  const [filters, setFilters] = React.useState<CrFilters>(emptyCrFilters);
  const { page, pageSize, setPage, setPageSize } = useListPagination('fin-bank-receipts');

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
      listBankReceipts({
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

  const openCreate = () => onNavigate?.(trxNewRoute(BR_BASE));
  const openEdit = (r: ErpBankReceipt) => onNavigate?.(trxEditRoute(BR_BASE, r.id));

  const loadForm = React.useCallback(() => {
    if (formMode === 'create') {
      setForm(defaultBankReceiptForm());
      return undefined;
    }
    if (formMode === 'edit' && recordId) {
      let alive = true;
      getBankReceipt(recordId)
        .then((full) => alive && setForm(fromBankReceipt(full)))
        .catch(() => {
          if (!alive) return;
          notify('Gagal memuat Bank Masuk', 'danger');
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
      const payload = toBankReceiptPayload(form);
      if (form.id) {
        await updateBankReceipt(form.id, payload);
        notify('Bank Masuk diperbarui', 'success');
      } else {
        await createBankReceipt(payload);
        notify('Bank Masuk dibuat', 'success');
      }
      reload();
      if (newAfter) {
        setForm(defaultBankReceiptForm());
        onNavigate?.(trxNewRoute(BR_BASE));
      } else if (closeAfter) {
        goList();
      }
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const runTransition = async (r: ErpBankReceipt, action: CashBankTransition) => {
    let reason: string | undefined;
    if (action === 'REJECT') {
      reason = window.prompt('Alasan menolak dokumen ini?') ?? undefined;
      if (!reason) return;
    }
    const verb: Record<CashBankTransition, string> = {
      SUBMIT: 'mengajukan',
      APPROVE: 'menyetujui',
      REJECT: 'menolak',
      POST: 'memposting',
      REOPEN: 'membuka kembali',
    };
    try {
      await transitionBankReceipt(r.id, action, reason);
      notify(`Berhasil ${verb[action]} ${r.docNumber}`, 'success');
      reload();
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal', 'danger');
    }
  };

  const handleDelete = (r: ErpBankReceipt) => {
    confirmAction({
      title: 'Hapus Bank Masuk?',
      message: `${r.docNumber} akan dihapus permanen.`,
      variant: 'danger',
      confirmLabel: 'Hapus',
      confirmIcon: 'trash',
      onConfirm: async () => {
        try {
          await deleteBankReceipt(r.id);
          notify('Bank Masuk dihapus', 'success');
          reload();
        } catch (e: unknown) {
          notify(e instanceof Error ? e.message : 'Gagal', 'danger');
        }
      },
    });
  };

  const rowActions = (r: ErpBankReceipt): RowActionItem[] => [
    { label: 'Edit / Lihat', onSelect: () => openEdit(r) },
    ...cashBankWorkflowActions(r.status, (a) => runTransition(r, a)),
    { label: 'Hapus', onSelect: () => handleDelete(r), danger: true, separatorBefore: true },
  ];

  // ── form view ───────────────────────────────────────────────────────────────
  if (mode === 'form') {
    return (
      <div className="page">
        <div className="page-header">
          <h1 className="page-title flex items-center gap-2">
            <button className="iconbtn" onClick={goList} title="Kembali" style={{ fontSize: 18, lineHeight: 1 }}>
              ←
            </button>
            Bank Masuk
            <span className="code-tag">RM</span>
          </h1>
        </div>
        <div className="page-body overflow-auto p-4">
          {formReady ? (
            <BankReceiptForm
              data={form}
              onChange={setForm}
              saving={saving}
              allowedCreationStatuses={formMode === 'create' ? allowedCreationStatuses : undefined}
              onSave={() => persist(true)}
              onSaveNew={() => persist(false, true)}
              onReset={loadForm}
            />
          ) : (
            <div className="p-8 text-center text-muted">Memuat…</div>
          )}
        </div>
      </div>
    );
  }

  // ── list view ─────────────────────────────────────────────────────────────────
  const summary: SummaryConfig = { metricLabel: 'Σ Bank Masuk', rowCount: rows.length, totalCount: totalRows };
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
    <ErpListLayout
      title="Bank Masuk"
      code="RM"
      loading={loading}
      error={error}
      search={search}
      onSearch={setSearch}
      onAdd={openCreate}
      onRefresh={reload}
      toolbar={<CashReceiptFilters value={filters} onChange={setFilters} />}
      summary={summary}
      pagination={pagination}
      keyboardRows={{
        rowCount: rows.length,
        focusedIndex: focused,
        onFocusChange: setFocused,
        onToggle: (i) => rows[i] && toggleSel(rows[i].id),
        onOpen: (i) => rows[i] && openEdit(rows[i]),
      }}
    >
      {selected.size > 0 && (
        <div className="bulk-bar flex items-center gap-3 px-3 py-2 mb-2 rounded-md bg-secondary text-sm">
          <strong>{selected.size}</strong> baris dipilih
          <button
            className="btn sm danger"
            onClick={() =>
              confirmAction({
                title: 'Hapus terpilih?',
                message: `${selected.size} Bank Masuk akan dihapus permanen.`,
                variant: 'danger',
                confirmLabel: 'Hapus',
                onConfirm: async () => {
                  await Promise.all([...selected].map((id) => deleteBankReceipt(id).catch(() => null)));
                  notify(`${selected.size} dokumen dihapus`, 'success');
                  setSelected(new Set());
                  reload();
                },
              })
            }
          >
            <Icon name="trash" size={12} /> Hapus
          </button>
          <button className="btn ghost sm" onClick={() => setSelected(new Set())}>
            Batal pilihan
          </button>
        </div>
      )}
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead style={{ width: 36 }} />
            <TableHead>No Transaksi</TableHead>
            <TableHead>Tanggal</TableHead>
            <TableHead>Terima Dari</TableHead>
            <TableHead>Uraian</TableHead>
            <TableHead>Cara Bayar</TableHead>
            <TableHead style={{ textAlign: 'right' }}>Total</TableHead>
            <TableHead>Uang</TableHead>
            <TableHead style={{ textAlign: 'right' }}>Kurs</TableHead>
            <TableHead>Status</TableHead>
            <TableHead style={{ width: 44 }} />
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.length === 0 ? (
            <TableEmpty colSpan={11} />
          ) : (
            rows.map((r, i) => {
              const actions = rowActions(r);
              return (
                <RowContextMenu key={r.id} items={actions}>
                  <TableRow
                    style={focused === i ? { boxShadow: 'inset 2px 0 0 var(--primary)' } : undefined}
                    className="cursor-pointer"
                  >
                    <TableCell style={{ textAlign: 'center' }}>
                      <input
                        type="checkbox"
                        checked={selected.has(r.id)}
                        onChange={() => toggleSel(r.id)}
                      />
                    </TableCell>
                    <CodeLinkCell code={r.docNumber} onOpen={() => openEdit(r)} />
                    <TableCell>{r.transactionDate.slice(0, 10)}</TableCell>
                    <TableCell>{r.partner?.name ?? r.contactPerson ?? '—'}</TableCell>
                    <TableCell>{r.description}</TableCell>
                    <TableCell className="muted">{paymentMethodLabel(r.paymentMethod)}</TableCell>
                    <TableCell className="tabular-nums" style={{ textAlign: 'right' }}>
                      {formatNumber(Number(r.amount), 2)}
                    </TableCell>
                    <TableCell>{r.currency?.code ?? '—'}</TableCell>
                    <TableCell className="tabular-nums" style={{ textAlign: 'right' }}>
                      {formatNumber(Number(r.exchangeRate), 2)}
                    </TableCell>
                    <TableCell>
                      <Badge variant={statusBadgeVariant(r.status)} dot>
                        {statusLabel(r.status)}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <RowActionsMenu items={actions} />
                    </TableCell>
                  </TableRow>
                </RowContextMenu>
              );
            })
          )}
        </TableBody>
      </Table>
    </ErpListLayout>
  );
}
