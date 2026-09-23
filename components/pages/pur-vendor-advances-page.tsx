'use client';

/**
 * Vendor Advance / Uang Muka Pembelian (AP) — list + form.
 * URL: /purchasing/vendor-advances · /new · /:id
 * Backend: /pur/vendor-advances (source='AP'). §2.3.1
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { ErpListLayout, type ListPaginationConfig, type SummaryConfig } from '@/components/organisms/erp-list-layout';
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell, TableEmpty, CodeLinkCell } from '@/components/organisms/table';
import { RowActionsMenu, RowContextMenu, type RowActionItem } from '@/components/molecules/row-actions-menu';
import { confirmAction, notify } from '@/lib/feedback';
import { cashBankWorkflowActions } from '@/lib/fin-cash-bank-workflow';
import { type TrxFormPageProps, trxNewRoute, trxEditRoute } from '@/lib/trx-route';
import { useErpList } from '@/lib/use-erp-list';
import { useListPagination } from '@/lib/use-list-pagination';
import { formatNumber } from '@/lib/format';
import { statusBadgeVariant, statusLabel, type ApprovalStatus } from '@/lib/status';
import {
  listVendorAdvances, getVendorAdvance, createVendorAdvance,
  updateVendorAdvance, deleteVendorAdvance, transitionVendorAdvance,
  type ErpVendorAdvance, type VendorAdvanceTransition,
} from '@/lib/api/pur-vendor-advances';
import {
  VendorAdvanceForm, emptyVendorAdvanceForm, type VendorAdvanceFormData,
} from './pur-vendor-advance-form';

const BASE = '/purchasing/vendor-advances';

function fromRecord(r: ErpVendorAdvance): VendorAdvanceFormData {
  return {
    id: r.id, docNumber: r.docNumber, autoNumber: false,
    transactionDate: r.transactionDate.slice(0, 10),
    fiscalPeriodId: r.fiscalPeriodId, branchId: r.branchId,
    partnerId: r.partner?.id ?? '', partnerLabel: r.partner?.name,
    description: r.description, currencyId: r.currencyId,
    exchangeRate: r.exchangeRate, amount: r.amount,
    notes: r.notes ?? '', status: r.status,
  };
}

export function ErpVendorAdvancesPage({ formMode, recordId, onNavigate }: TrxFormPageProps = {}) {
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const [form, setForm] = React.useState<VendorAdvanceFormData>(emptyVendorAdvanceForm);
  const [saving, setSaving] = React.useState(false);

  const formReady =
    formMode === 'create' ||
    (formMode === 'edit' && String(form.id ?? '') === String(recordId ?? ''));

  const goList = React.useCallback(() => onNavigate?.(BASE), [onNavigate]);
  const [search, setSearch] = React.useState('');
  const { page, pageSize, setPage, setPageSize } = useListPagination('pur-vendor-advances');
  const [debouncedSearch, setDebouncedSearch] = React.useState(search);
  React.useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(search), 300);
    return () => clearTimeout(t);
  }, [search]);
  React.useEffect(() => { setPage(1); }, [debouncedSearch, pageSize, setPage]);

  const { rows, meta, loading, error, reload } = useErpList(
    () => listVendorAdvances({ page, limit: pageSize, search: debouncedSearch || undefined, sortBy: 'transactionDate', sortDir: 'desc' }),
    [page, pageSize, debouncedSearch],
  );

  const [focused, setFocused] = React.useState(-1);
  const [selected, setSelected] = React.useState<Set<string>>(new Set());
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  const openCreate = () => onNavigate?.(trxNewRoute(BASE));
  const openEdit = (r: ErpVendorAdvance) => onNavigate?.(trxEditRoute(BASE, r.id));

  const loadForm = React.useCallback(() => {
    if (formMode === 'create') { setForm(emptyVendorAdvanceForm()); return undefined; }
    if (formMode === 'edit' && recordId) {
      let alive = true;
      getVendorAdvance(recordId)
        .then((full) => { if (alive) setForm(fromRecord(full)); })
        .catch(() => { if (!alive) return; notify('Gagal memuat', 'danger'); goList(); });
      return () => { alive = false; };
    }
    return undefined;
  }, [formMode, recordId, goList]);
  React.useEffect(() => loadForm(), [loadForm]);

  const persist = async (closeAfter: boolean, newAfter = false) => {
    if (!form.branchId || !form.transactionDate || !form.currencyId || !form.partnerId) {
      notify('Supplier, Cabang, Tanggal, dan Mata Uang wajib diisi.', 'warn'); return;
    }
    if (!form.amount || Number(form.amount) <= 0) {
      notify('Nominal harus lebih dari 0.', 'warn'); return;
    }
    setSaving(true);
    try {
      const payload = { docNumber: form.autoNumber ? 'AUTO' : form.docNumber, transactionDate: form.transactionDate, fiscalPeriodId: form.fiscalPeriodId, branchId: form.branchId, partnerId: form.partnerId, description: form.description, currencyId: form.currencyId, exchangeRate: form.exchangeRate, amount: form.amount, notes: form.notes || undefined };
      if (form.id) { await updateVendorAdvance(form.id, payload); notify('Diperbarui', 'success'); }
      else { await createVendorAdvance(payload); notify('Dibuat', 'success'); }
      reload();
      if (newAfter) { setForm(emptyVendorAdvanceForm()); onNavigate?.(trxNewRoute(BASE)); }
      else if (closeAfter) { goList(); }
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally { setSaving(false); }
  };

  const runTransition = async (r: ErpVendorAdvance, action: VendorAdvanceTransition) => {
    const reason = action === 'REJECT' ? (window.prompt('Alasan menolak?') ?? undefined) : undefined;
    if (action === 'REJECT' && !reason) return;
    try { await transitionVendorAdvance(r.id, action, reason); notify(`${r.docNumber}`, 'success'); reload(); }
    catch (e: unknown) { notify(e instanceof Error ? e.message : 'Gagal', 'danger'); }
  };

  const handleDelete = (r: ErpVendorAdvance) => {
    confirmAction({ title: 'Hapus?', message: `${r.docNumber} akan dihapus permanen.`, variant: 'danger', confirmLabel: 'Hapus', confirmIcon: 'trash',
      onConfirm: async () => { try { await deleteVendorAdvance(r.id); notify('Dihapus', 'success'); reload(); } catch (e: unknown) { notify(e instanceof Error ? e.message : 'Gagal', 'danger'); } } });
  };

  const rowActions = (r: ErpVendorAdvance): RowActionItem[] => [
    { label: 'Edit / Lihat', onSelect: () => openEdit(r) },
    ...cashBankWorkflowActions(r.status as never, (a) => runTransition(r, a as VendorAdvanceTransition)),
    { label: 'Hapus', onSelect: () => handleDelete(r), danger: true, separatorBefore: true },
  ];

  const toggleSel = (id: string) => setSelected((s) => {
    const n = new Set(s);
    if (n.has(id)) { n.delete(id); } else { n.add(id); }
    return n;
  });

  if (mode === 'form') {
    return (
      <div className="page">
        <div className="page-header">
          <h1 className="page-title flex items-center gap-2">
            <button className="iconbtn" onClick={goList} style={{ fontSize: 18, lineHeight: 1 }}>←</button>
            Uang Muka Pembelian <span className="code-tag">AP</span>
          </h1>
        </div>
        <div className="page-body overflow-auto p-4">
          {formReady ? (
            <VendorAdvanceForm data={form} onChange={setForm} saving={saving}
              onSave={() => persist(true)} onSaveNew={() => persist(false, true)} onReset={loadForm} />
          ) : <div className="p-8 text-center text-muted">Memuat…</div>}
        </div>
      </div>
    );
  }

  const summary: SummaryConfig = { metricLabel: 'Σ Uang Muka', rowCount: rows.length, totalCount: totalRows };
  const pagination: ListPaginationConfig = { page, pageCount, pageSize, totalRows, onPage: setPage, onPageSize: setPageSize };

  return (
    <ErpListLayout title="Uang Muka Pembelian" code="AP" loading={loading} error={error}
      search={search} onSearch={setSearch} onAdd={openCreate} onRefresh={reload}
      summary={summary} pagination={pagination}
      keyboardRows={{ rowCount: rows.length, focusedIndex: focused, onFocusChange: setFocused, onToggle: (i) => rows[i] && toggleSel(rows[i].id), onOpen: (i) => rows[i] && openEdit(rows[i]) }}>
      {selected.size > 0 && (
        <div className="bulk-bar flex items-center gap-3 px-3 py-2 mb-2 rounded-md bg-secondary text-sm">
          <strong>{selected.size}</strong> baris dipilih
          <button className="btn ghost sm" onClick={() => setSelected(new Set())}>Batal pilihan</button>
        </div>
      )}
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead style={{ width: 36 }} />
            <TableHead>No Transaksi</TableHead>
            <TableHead>Tanggal</TableHead>
            <TableHead>Supplier</TableHead>
            <TableHead>Uraian</TableHead>
            <TableHead style={{ textAlign: 'right' }}>Jumlah</TableHead>
            <TableHead>Status</TableHead>
            <TableHead style={{ width: 44 }} />
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.length === 0 ? <TableEmpty colSpan={8} /> : rows.map((r, i) => {
            const actions = rowActions(r);
            return (
              <RowContextMenu key={r.id} items={actions}>
                <TableRow style={focused === i ? { boxShadow: 'inset 2px 0 0 var(--primary)' } : undefined} className="cursor-pointer">
                  <TableCell style={{ textAlign: 'center' }}>
                    <input type="checkbox" checked={selected.has(r.id)} onChange={() => toggleSel(r.id)} />
                  </TableCell>
                  <CodeLinkCell code={r.docNumber} onOpen={() => openEdit(r)} />
                  <TableCell>{r.transactionDate.slice(0, 10)}</TableCell>
                  <TableCell>{r.partner?.name ?? '—'}</TableCell>
                  <TableCell>{r.description ?? '—'}</TableCell>
                  <TableCell className="tabular-nums" style={{ textAlign: 'right' }}>{formatNumber(Number(r.amount), 2)}</TableCell>
                  <TableCell>
                    <Badge variant={statusBadgeVariant(r.status as ApprovalStatus)} dot>
                      {statusLabel(r.status as ApprovalStatus)}
                    </Badge>
                  </TableCell>
                  <TableCell><RowActionsMenu items={actions} /></TableCell>
                </TableRow>
              </RowContextMenu>
            );
          })}
        </TableBody>
      </Table>
    </ErpListLayout>
  );
}
