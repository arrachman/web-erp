'use client';
// Customer Advance (AS) — list + header-only form. §2.3.1 URL-driven list↔form.
// AS has NO item lines — uses SlsCustomerAdvanceForm (not SalesTransactionForm).

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
  SlsCustomerAdvanceFilters,
  emptySlsAsFilters,
  type SlsAsFilters,
} from './sls-customer-advance-filters';
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
  listSlsCustomerAdvances,
  createSlsCustomerAdvance,
  updateSlsCustomerAdvance,
  deleteSlsCustomerAdvance,
  getSlsCustomerAdvance,
  transitionSlsCustomerAdvance,
  type SlsCustomerAdvanceTransition,
  type ErpSlsCustomerAdvance,
  type ErpDocumentStatus,
} from '@/lib/api/sls-customer-advances';
import {
  SlsCustomerAdvanceForm,
  defaultSlsCustomerAdvanceForm,
  fromSlsCustomerAdvance,
  type SlsCustomerAdvanceFormData,
} from './sls-customer-advance-form';

const AS_BASE = '/sales/customer-advances';

const toAdvancePayload = (d: SlsCustomerAdvanceFormData) => ({
  auto: d.auto,
  docNumber: d.auto ? undefined : d.docNumber || undefined,
  docDate: d.docDate,
  dueDate: d.dueDate || undefined,
  branchId: d.branchId,
  customerId: d.customerId || undefined,
  currencyId: d.currencyId,
  exchangeRate: d.exchangeRate || '1',
  paymentTermId: d.paymentTermId || undefined,
  amount: d.amount,
  description: d.description || undefined,
  notes: d.notes || undefined,
});

export function ErpSlsCustomerAdvancesPage({ formMode, recordId, onNavigate }: TrxFormPageProps = {}) {
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const [form, setForm] = React.useState<SlsCustomerAdvanceFormData>(defaultSlsCustomerAdvanceForm);
  const [saving, setSaving] = React.useState(false);

  const formReady = formMode === 'create' || (formMode === 'edit' && String(form.id ?? '') === String(recordId ?? ''));

  const goList = React.useCallback(() => onNavigate?.(AS_BASE), [onNavigate]);

  const [search, setSearch] = React.useState('');
  const [filters, setFilters] = React.useState<SlsAsFilters>(emptySlsAsFilters);
  const [sortBy, setSortBy] = React.useState('docDate');
  const [sortDir, setSortDir] = React.useState<'asc' | 'desc'>('desc');
  const { page, pageSize, setPage, setPageSize } = useListPagination('sls-customer-advances');

  const [debouncedSearch, setDebouncedSearch] = React.useState(search);
  const [debF, setDebF] = React.useState(filters);
  React.useEffect(() => { const t = setTimeout(() => setDebouncedSearch(search), 300); return () => clearTimeout(t); }, [search]);
  React.useEffect(() => { const t = setTimeout(() => setDebF(filters), 350); return () => clearTimeout(t); }, [filters]);

  const { rows, meta, loading, error, reload } = useErpList(
    () =>
      listSlsCustomerAdvances({
        page,
        limit: pageSize,
        search: debouncedSearch || undefined,
        status: (debF.status || undefined) as ErpDocumentStatus | undefined,
        dateFrom: debF.dateFrom || undefined,
        dateTo: debF.dateTo || undefined,
        docNumberFrom: debF.docNumber || undefined,
        description: debF.description || undefined,
        settlementStatus: debF.settlementStatus || undefined,
        sortBy,
        sortDir,
      }),
    [page, pageSize, debouncedSearch, debF, sortBy, sortDir],
  );

  React.useEffect(() => { setPage(1); }, [debouncedSearch, debF, pageSize, sortBy, sortDir]);

  const [focused, setFocused] = React.useState(-1);
  const [selected, setSelected] = React.useState<Set<string>>(new Set());
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  const toggleSort = (col: string) => {
    if (sortBy === col) setSortDir((d) => (d === 'asc' ? 'desc' : 'asc')); else { setSortBy(col); setSortDir('asc'); }
    setPage(1);
  };

  const openCreate = () => onNavigate?.(trxNewRoute(AS_BASE));
  const openEdit = (r: ErpSlsCustomerAdvance) => onNavigate?.(trxEditRoute(AS_BASE, r.id));

  const loadForm = React.useCallback(() => {
    if (formMode === 'create') {
      setForm(defaultSlsCustomerAdvanceForm());
      return undefined;
    }
    if (formMode === 'edit' && recordId) {
      let alive = true;
      getSlsCustomerAdvance(recordId)
        .then((full) => alive && setForm(fromSlsCustomerAdvance(full)))
        .catch(() => {
          if (!alive) return;
          notify('Gagal memuat Customer Advance', 'danger');
          goList();
        });
      return () => { alive = false; };
    }
    return undefined;
  }, [formMode, recordId, goList]);
  React.useEffect(() => loadForm(), [loadForm]);

  const persist = async (closeAfter: boolean, newAfter = false) => {
    if (!form.branchId || !form.docDate || !form.currencyId) {
      notify('Cabang, Tanggal, dan Mata Uang wajib diisi.', 'warn');
      return;
    }
    if (!form.amount || Number(form.amount) <= 0) {
      notify('Jumlah advance harus lebih dari 0.', 'warn');
      return;
    }
    setSaving(true);
    try {
      const payload = toAdvancePayload(form);
      if (form.id) {
        await updateSlsCustomerAdvance(form.id, payload);
        notify('Customer Advance diperbarui', 'success');
      } else {
        await createSlsCustomerAdvance(payload);
        notify('Customer Advance dibuat', 'success');
      }
      reload();
      if (newAfter) {
        setForm(defaultSlsCustomerAdvanceForm());
        onNavigate?.(trxNewRoute(AS_BASE));
      } else if (closeAfter) {
        goList();
      }
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const runTransition = async (r: ErpSlsCustomerAdvance, action: SlsCustomerAdvanceTransition) => {
    let reason: string | undefined;
    if (action === 'REJECT') {
      reason = window.prompt('Alasan menolak dokumen ini?') ?? undefined;
      if (!reason) return;
    }
    const verb: Record<SlsCustomerAdvanceTransition, string> = { SUBMIT: 'mengajukan', APPROVE: 'menyetujui', REJECT: 'menolak', POST: 'memposting', REOPEN: 'membuka kembali' };
    try {
      await transitionSlsCustomerAdvance(r.id, action, reason);
      notify(`Berhasil ${verb[action]} ${r.docNumber}`, 'success');
      reload();
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal', 'danger');
    }
  };

  const handleDelete = (r: ErpSlsCustomerAdvance) => {
    confirmAction({
      title: 'Hapus Customer Advance?',
      message: `${r.docNumber} akan dihapus permanen.`,
      variant: 'danger',
      confirmLabel: 'Hapus',
      confirmIcon: 'trash',
      onConfirm: async () => {
        try {
          await deleteSlsCustomerAdvance(r.id);
          notify('Customer Advance dihapus', 'success');
          reload();
        } catch (e: unknown) {
          notify(e instanceof Error ? e.message : 'Gagal', 'danger');
        }
      },
    });
  };

  const rowActions = (r: ErpSlsCustomerAdvance): RowActionItem[] => [
    { label: 'Edit / Lihat', onSelect: () => openEdit(r) },
    ...cashBankWorkflowActions(r.status, (a) => runTransition(r, a)),
    { label: 'Hapus', onSelect: () => handleDelete(r), danger: true, separatorBefore: true },
  ];

  if (mode === 'form') {
    return (
      <div className="page">
        <div className="page-header">
          <h1 className="page-title flex items-center gap-2">
            <button className="iconbtn" onClick={goList} title="Kembali" style={{ fontSize: 18, lineHeight: 1 }}>
              ←
            </button>
            Customer Advance
            <span className="code-tag">AS</span>
          </h1>
        </div>
        <div className="page-body overflow-auto p-4">
          {formReady ? (
            <SlsCustomerAdvanceForm
              data={form}
              onChange={setForm}
              saving={saving}
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

  const sumAmount = (meta as { sumAmount?: string } | null)?.sumAmount;
  const summary: SummaryConfig = {
    metricLabel: 'Σ Advance',
    rowCount: rows.length,
    totalCount: totalRows,
    metricValue: sumAmount ? formatNumber(Number(sumAmount), 2) : undefined,
  };
  const pagination: ListPaginationConfig = {
    page, pageCount, pageSize, totalRows, onPage: setPage, onPageSize: setPageSize,
  };
  const toggleSel = (id: string) =>
    setSelected((s) => { const n = new Set(s); n.has(id) ? n.delete(id) : n.add(id); return n; });

  return (
    <ErpListLayout
      title="Customer Advance"
      code="AS"
      loading={loading}
      error={error}
      search={search}
      onSearch={setSearch}
      onAdd={openCreate}
      onRefresh={reload}
      toolbar={
        <>
          <SlsCustomerAdvanceFilters value={filters} onChange={setFilters} />
          <button
            type="button"
            className="btn ghost sm"
            onClick={() => notify('Export akan tersedia segera.', 'info')}
            title="Export ke CSV/Excel"
          >
            <Icon name="download" size={12} /> Export
          </button>
        </>
      }
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
                message: `${selected.size} Customer Advance akan dihapus permanen.`,
                variant: 'danger',
                confirmLabel: 'Hapus',
                onConfirm: async () => {
                  await Promise.all([...selected].map((id) => deleteSlsCustomerAdvance(id).catch(() => null)));
                  notify(`${selected.size} dokumen dihapus`, 'success');
                  setSelected(new Set());
                  reload();
                },
              })
            }
          >
            <Icon name="trash" size={12} /> Hapus
          </button>
          <button className="btn ghost sm" onClick={() => setSelected(new Set())}>Batal pilihan</button>
        </div>
      )}
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead style={{ width: 36, textAlign: 'center' }}>
              <input
                type="checkbox"
                checked={rows.length > 0 && rows.every((r) => selected.has(r.id))}
                ref={(el) => { if (el) el.indeterminate = selected.size > 0 && !rows.every((r) => selected.has(r.id)); }}
                onChange={(e) => setSelected(e.target.checked ? new Set(rows.map((r) => r.id)) : new Set())}
                title="Pilih semua"
              />
            </TableHead>
            {([
              ['docNumber', 'No Transaksi'],
              ['docDate', 'Tanggal'],
              [null, 'Pelanggan'],
              ['amount', 'Jumlah'],
              ['status', 'Status'],
              [null, 'Lunas'],
            ] as [string | null, string][]).map(([col, label]) => (
              <TableHead
                key={label}
                style={col === 'amount' ? { textAlign: 'right', cursor: col ? 'pointer' : undefined } : { cursor: col ? 'pointer' : undefined }}
                onClick={col ? () => toggleSort(col) : undefined}
              >
                {label}
                {col && sortBy === col && (
                  <span className="ml-1 text-muted-foreground text-xs">{sortDir === 'asc' ? '↑' : '↓'}</span>
                )}
              </TableHead>
            ))}
            <TableHead style={{ width: 44 }} />
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.length === 0 ? (
            <TableEmpty colSpan={8} />
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
                      <input type="checkbox" checked={selected.has(r.id)} onChange={() => toggleSel(r.id)} />
                    </TableCell>
                    <CodeLinkCell code={r.docNumber} onOpen={() => openEdit(r)} />
                    <TableCell>{r.docDate.slice(0, 10)}</TableCell>
                    <TableCell>{r.customer?.name ?? '—'}</TableCell>
                    <TableCell className="tabular-nums" style={{ textAlign: 'right' }}>
                      {formatNumber(Number(r.amount), 2)}
                    </TableCell>
                    <TableCell>
                      <Badge variant={statusBadgeVariant(r.status)} dot>
                        {statusLabel(r.status)}
                      </Badge>
                    </TableCell>
                    <TableCell>{r.settlementStatus ?? '—'}</TableCell>
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
