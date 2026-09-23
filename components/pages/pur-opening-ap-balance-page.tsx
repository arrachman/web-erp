'use client';

/**
 * Opening AP Balance (OB) — list + form. URL: /purchasing/opening-ap-balance · /new · /:id.
 * Backend: pur_invoices (isOpeningBalance=true). Reuses PurInvoiceForm pinned to PUR.OB.
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
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import {
  listPurInvoices, createPurInvoice, updatePurInvoice, deletePurInvoice,
  getPurInvoice, transitionPurInvoice,
  type ErpPurInvoice, type PurInvoiceTransition,
} from '@/lib/api/pur-invoices';
import { defaultPurOrderForm, type PurOrderFormData } from './pur-order-form-model';
import { PurInvoiceForm } from './pur-invoice-form';
import { fromPurInvoice, toPurInvoicePayload } from './pur-invoice-form-model';

const BASE = '/purchasing/opening-ap-balance';

export function ErpOpeningApBalancePage({ formMode, recordId, onNavigate }: TrxFormPageProps = {}) {
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const [form, setForm] = React.useState<PurOrderFormData>(defaultPurOrderForm());
  const [saving, setSaving] = React.useState(false);

  const formReady =
    formMode === 'create' ||
    (formMode === 'edit' && String(form.id ?? '') === String(recordId ?? ''));

  const goList = React.useCallback(() => onNavigate?.(BASE), [onNavigate]);
  const [search, setSearch] = React.useState('');
  const { page, pageSize, setPage, setPageSize } = useListPagination('pur-opening-ap-balance');
  const [debouncedSearch, setDebouncedSearch] = React.useState(search);
  React.useEffect(() => { const t = setTimeout(() => setDebouncedSearch(search), 300); return () => clearTimeout(t); }, [search]);

  const { rows, meta, loading, error, reload } = useErpList(
    () => listPurInvoices({ page, limit: pageSize, search: debouncedSearch || undefined, sortBy: 'docDate', sortDir: 'desc' }),
    [page, pageSize, debouncedSearch],
  );
  React.useEffect(() => { setPage(1); }, [debouncedSearch, pageSize]);

  const [focused, setFocused] = React.useState(-1);
  const [selected, setSelected] = React.useState<Set<string>>(new Set());
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  const openCreate = () => onNavigate?.(trxNewRoute(BASE));
  const openEdit = (r: ErpPurInvoice) => onNavigate?.(trxEditRoute(BASE, r.id));

  const loadForm = React.useCallback(() => {
    if (formMode === 'create') { setForm(defaultPurOrderForm()); return undefined; }
    if (formMode === 'edit' && recordId) {
      let alive = true;
      getPurInvoice(recordId)
        .then((full) => alive && setForm(fromPurInvoice(full)))
        .catch(() => { if (!alive) return; notify('Gagal memuat Saldo Awal AP', 'danger'); goList(); });
      return () => { alive = false; };
    }
    return undefined;
  }, [formMode, recordId, goList]);
  React.useEffect(() => loadForm(), [loadForm]);

  const persist = async (closeAfter: boolean, newAfter = false) => {
    if (!form.branchId || !form.docDate || !form.currencyId) { notify('Cabang, Tanggal, dan Mata Uang wajib diisi.', 'warn'); return; }
    if (!form.lines.some((l) => l.itemId && Number(l.quantity) > 0)) { notify('Minimal satu baris item dengan qty > 0.', 'warn'); return; }
    setSaving(true);
    try {
      const payload = toPurInvoicePayload(form);
      if (form.id) { await updatePurInvoice(form.id, payload); notify('Saldo Awal AP diperbarui', 'success'); }
      else { await createPurInvoice(payload); notify('Saldo Awal AP dibuat', 'success'); }
      reload();
      if (newAfter) { setForm(defaultPurOrderForm()); onNavigate?.(trxNewRoute(BASE)); }
      else if (closeAfter) { goList(); }
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally { setSaving(false); }
  };

  const runTransition = async (r: ErpPurInvoice, action: PurInvoiceTransition) => {
    let reason: string | undefined;
    if (action === 'REJECT') { reason = window.prompt('Alasan menolak?') ?? undefined; if (!reason) return; }
    try { await transitionPurInvoice(r.id, action, reason); notify(`Berhasil: ${r.docNumber}`, 'success'); reload(); }
    catch (e: unknown) { notify(e instanceof Error ? e.message : 'Gagal', 'danger'); }
  };

  const handleDelete = (r: ErpPurInvoice) => {
    confirmAction({
      title: 'Hapus Saldo Awal AP?', message: `${r.docNumber} akan dihapus permanen.`,
      variant: 'danger', confirmLabel: 'Hapus', confirmIcon: 'trash',
      onConfirm: async () => {
        try { await deletePurInvoice(r.id); notify('Dihapus', 'success'); reload(); }
        catch (e: unknown) { notify(e instanceof Error ? e.message : 'Gagal', 'danger'); }
      },
    });
  };

  const rowActions = (r: ErpPurInvoice): RowActionItem[] => [
    { label: 'Edit / Lihat', onSelect: () => openEdit(r) },
    ...cashBankWorkflowActions(r.status as never, (a) => runTransition(r, a as PurInvoiceTransition)),
    { label: 'Hapus', onSelect: () => handleDelete(r), danger: true, separatorBefore: true },
  ];

  if (mode === 'form') {
    return (
      <div className="page">
        <div className="page-header">
          <h1 className="page-title flex items-center gap-2">
            <button className="iconbtn" onClick={goList} style={{ fontSize: 18, lineHeight: 1 }}>←</button>
            Saldo Awal AP <span className="code-tag">OB</span>
          </h1>
        </div>
        <div className="page-body overflow-auto p-4">
          {formReady ? (
            <PurInvoiceForm data={form} onChange={setForm} saving={saving}
              onSave={() => persist(true)} onSaveNew={() => persist(false, true)} onReset={loadForm} />
          ) : (
            <div className="p-8 text-center text-muted">Memuat…</div>
          )}
        </div>
      </div>
    );
  }

  const summary: SummaryConfig = { metricLabel: 'Σ Saldo Awal AP', rowCount: rows.length, totalCount: totalRows };
  const pagination: ListPaginationConfig = { page, pageCount, pageSize, totalRows, onPage: setPage, onPageSize: setPageSize };
  const toggleSel = (id: string) => setSelected((s) => { const n = new Set(s); n.has(id) ? n.delete(id) : n.add(id); return n; });

  return (
    <ErpListLayout title="Saldo Awal AP" code="OB" loading={loading} error={error}
      search={search} onSearch={setSearch} onAdd={openCreate} onRefresh={reload}
      toolbar={null} summary={summary} pagination={pagination}
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
            <TableHead>No Transaksi</TableHead><TableHead>Tanggal</TableHead>
            <TableHead>Supplier</TableHead><TableHead>Uraian</TableHead>
            <TableHead style={{ textAlign: 'right' }}>Total</TableHead>
            <TableHead>Status</TableHead><TableHead style={{ width: 44 }} />
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.length === 0 ? <TableEmpty colSpan={8} /> : rows.map((r, i) => {
            const actions = rowActions(r);
            return (
              <RowContextMenu key={r.id} items={actions}>
                <TableRow style={focused === i ? { boxShadow: 'inset 2px 0 0 var(--primary)' } : undefined} className="cursor-pointer">
                  <TableCell style={{ textAlign: 'center' }}><input type="checkbox" checked={selected.has(r.id)} onChange={() => toggleSel(r.id)} /></TableCell>
                  <CodeLinkCell code={r.docNumber} onOpen={() => openEdit(r)} />
                  <TableCell>{r.docDate.slice(0, 10)}</TableCell>
                  <TableCell>{r.supplier?.name ?? '—'}</TableCell>
                  <TableCell>{r.description ?? '—'}</TableCell>
                  <TableCell className="tabular-nums" style={{ textAlign: 'right' }}>{formatNumber(Number(r.grandTotal), 2)}</TableCell>
                  <TableCell><Badge variant={statusBadgeVariant(r.status)} dot>{statusLabel(r.status)}</Badge></TableCell>
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
