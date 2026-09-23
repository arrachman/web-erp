'use client';

/**
 * Sales Invoice Swap (SIE) — list (§2.7) + simple form.
 * URL-driven list↔form via trx sub-routes (§2.3.1):
 *   /sales/invoice-swaps · /new · /:id
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
  RowActionsMenu,
  RowContextMenu,
  type RowActionItem,
} from '@/components/molecules/row-actions-menu';
import { confirmAction, notify } from '@/lib/feedback';
import { trxNewRoute, trxEditRoute, type TrxFormPageProps } from '@/lib/trx-route';
import { useErpList } from '@/lib/use-erp-list';
import { useListPagination } from '@/lib/use-list-pagination';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import { cashBankWorkflowActions } from '@/lib/fin-cash-bank-workflow';
import {
  SlsSieFilters,
  emptySlsSieFilters,
  type SlsSieFilters as SlsSieFiltersType,
} from './sls-invoice-swap-filters';
import {
  SlsInvoiceSwapForm,
  defaultSieForm,
  fromSlsInvoiceSwap,
  toSiePayload,
  type SieFormData,
} from './sls-invoice-swap-form';
import {
  listSlsInvoiceSwaps,
  createSlsInvoiceSwap,
  updateSlsInvoiceSwap,
  deleteSlsInvoiceSwap,
  getSlsInvoiceSwap,
  transitionSlsInvoiceSwap,
  type ErpSlsInvoiceSwap,
  type ErpDocumentStatus,
  type SlsInvoiceSwapTransition,
} from '@/lib/api/sls-invoice-swaps';

const SIE_BASE = '/sales/invoice-swaps';

export function ErpSlsInvoiceSwapsPage({ formMode, recordId, onNavigate }: TrxFormPageProps = {}) {
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const [form, setForm] = React.useState<SieFormData>(defaultSieForm);
  const [saving, setSaving] = React.useState(false);

  const formReady =
    formMode === 'create' ||
    (formMode === 'edit' && String(form.id ?? '') === String(recordId ?? ''));

  const goList = React.useCallback(() => onNavigate?.(SIE_BASE), [onNavigate]);

  // list state
  const [search, setSearch] = React.useState('');
  const [filters, setFilters] = React.useState<SlsSieFiltersType>(emptySlsSieFilters);
  const [sortBy, setSortBy] = React.useState('docDate');
  const [sortDir, setSortDir] = React.useState<'asc' | 'desc'>('desc');
  const { page, pageSize, setPage, setPageSize } = useListPagination('sls-invoice-swaps');

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
      listSlsInvoiceSwaps({
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

  const openCreate = () => onNavigate?.(trxNewRoute(SIE_BASE));
  const openEdit = (r: ErpSlsInvoiceSwap) => onNavigate?.(trxEditRoute(SIE_BASE, r.id));

  const loadForm = React.useCallback(() => {
    if (formMode === 'create') {
      setForm(defaultSieForm());
      return undefined;
    }
    if (formMode === 'edit' && recordId) {
      let alive = true;
      getSlsInvoiceSwap(recordId)
        .then((full) => alive && setForm(fromSlsInvoiceSwap(full)))
        .catch(() => {
          if (!alive) return;
          notify('Gagal memuat Invoice Swap', 'danger');
          goList();
        });
      return () => { alive = false; };
    }
    return undefined;
  }, [formMode, recordId, goList]);
  React.useEffect(() => loadForm(), [loadForm]);

  const persist = async (closeAfter: boolean) => {
    if (!form.branchId || !form.docDate) {
      notify('Cabang dan Tanggal wajib diisi.', 'warn');
      return;
    }
    setSaving(true);
    try {
      const payload = toSiePayload(form);
      if (form.id) {
        await updateSlsInvoiceSwap(form.id, payload);
        notify('Invoice Swap diperbarui', 'success');
      } else {
        await createSlsInvoiceSwap(payload);
        notify('Invoice Swap dibuat', 'success');
      }
      reload();
      if (closeAfter) goList();
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const runTransition = async (r: ErpSlsInvoiceSwap, action: SlsInvoiceSwapTransition) => {
    let reason: string | undefined;
    if (action === 'REJECT') {
      reason = window.prompt('Alasan menolak dokumen ini?') ?? undefined;
      if (!reason) return;
    }
    const verb: Record<SlsInvoiceSwapTransition, string> = {
      SUBMIT: 'mengajukan', APPROVE: 'menyetujui', REJECT: 'menolak',
      POST: 'memposting', REOPEN: 'membuka kembali',
    };
    try {
      await transitionSlsInvoiceSwap(r.id, action, reason);
      notify(`Berhasil ${verb[action]} ${r.docNumber}`, 'success');
      reload();
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal', 'danger');
    }
  };

  const handleDelete = (r: ErpSlsInvoiceSwap) => {
    confirmAction({
      title: 'Hapus Invoice Swap?',
      message: `${r.docNumber} akan dihapus permanen.`,
      variant: 'danger',
      confirmLabel: 'Hapus',
      confirmIcon: 'trash',
      onConfirm: async () => {
        try {
          await deleteSlsInvoiceSwap(r.id);
          notify('Invoice Swap dihapus', 'success');
          reload();
        } catch (e: unknown) {
          notify(e instanceof Error ? e.message : 'Gagal', 'danger');
        }
      },
    });
  };

  const rowActions = (r: ErpSlsInvoiceSwap): RowActionItem[] => [
    { label: 'Edit / Lihat', onSelect: () => openEdit(r) },
    ...cashBankWorkflowActions(r.status, (a) => runTransition(r, a)),
    { label: 'Hapus', onSelect: () => handleDelete(r), danger: true, separatorBefore: true },
  ];

  const toggleSel = (id: string) =>
    setSelected((s) => { const n = new Set(s); n.has(id) ? n.delete(id) : n.add(id); return n; });

  // ── form view ──────────────────────────────────────────────────────────────
  if (mode === 'form') {
    return (
      <div className="page">
        <div className="page-header">
          <h1 className="page-title flex items-center gap-2">
            <button className="iconbtn" onClick={goList} title="Kembali" style={{ fontSize: 18, lineHeight: 1 }}>
              ←
            </button>
            Invoice Swap
            <span className="code-tag">SIE</span>
          </h1>
        </div>
        <div className="page-body overflow-auto p-4">
          {formReady ? (
            <SlsInvoiceSwapForm
              data={form}
              onChange={setForm}
              saving={saving}
              onSave={() => persist(true)}
              onReset={loadForm}
            />
          ) : (
            <div className="p-8 text-center text-muted">Memuat…</div>
          )}
        </div>
      </div>
    );
  }

  // ── list view ──────────────────────────────────────────────────────────────
  const summary: SummaryConfig = {
    metricLabel: 'Σ Invoice Swap',
    rowCount: rows.length,
    totalCount: totalRows,
  };
  const pagination: ListPaginationConfig = {
    page, pageCount, pageSize, totalRows, onPage: setPage, onPageSize: setPageSize,
  };

  return (
    <ErpListLayout
      title="Invoice Swap"
      code="SIE"
      loading={loading}
      error={error}
      search={search}
      onSearch={setSearch}
      onAdd={openCreate}
      onRefresh={reload}
      toolbar={
        <>
          <SlsSieFilters value={filters} onChange={setFilters} />
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
                message: `${selected.size} Invoice Swap akan dihapus permanen.`,
                variant: 'danger',
                confirmLabel: 'Hapus',
                onConfirm: async () => {
                  await Promise.all([...selected].map((id) => deleteSlsInvoiceSwap(id).catch(() => null)));
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
              ['status', 'Status'],
            ] as [string | null, string][]).map(([col, label]) => (
              <TableHead
                key={label}
                style={{ cursor: col ? 'pointer' : undefined }}
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
            <TableEmpty colSpan={6} />
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
