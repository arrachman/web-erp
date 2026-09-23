'use client';

/**
 * Goods Receipt (GRN) — list + form. URL-driven via trx sub-routes (§2.3.1):
 * /purchasing/goods-receipts · /new · /:id. Header config-driven via PUR.GRN
 * Form Builder; grid columns (incl QC delta) via PUR.GRN Kustomisasi Grid.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Badge } from '@/components/ui/badge';
import {
  ErpListLayout, type ListPaginationConfig, type SummaryConfig,
} from '@/components/organisms/erp-list-layout';
import {
  Table, TableHeader, TableBody, TableRow, TableHead, TableCell, TableEmpty, CodeLinkCell,
} from '@/components/organisms/table';
import { RowActionsMenu, RowContextMenu, type RowActionItem } from '@/components/molecules/row-actions-menu';
import { confirmAction, notify } from '@/lib/feedback';
import { cashBankWorkflowActions } from '@/lib/fin-cash-bank-workflow';
import { trxNewRoute, trxEditRoute, type TrxFormPageProps } from '@/lib/trx-route';
import { useErpList } from '@/lib/use-erp-list';
import { useListPagination } from '@/lib/use-list-pagination';
import { formatNumber } from '@/lib/format';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import {
  listPurGoodsReceipts, getPurGoodsReceipt, createPurGoodsReceipt, updatePurGoodsReceipt,
  deletePurGoodsReceipt, transitionPurGoodsReceipt, type ErpPurGoodsReceipt,
} from '@/lib/api/pur-goods-receipts';
import { useAllowedCreationStatuses } from '@/lib/use-allowed-creation-statuses';
import { PurGoodsReceiptForm } from './pur-goods-receipt-form';
import { fromPurGoodsReceipt, toPurGoodsReceiptPayload } from './pur-goods-receipt-form-model';
import { defaultPurOrderForm, type PurOrderFormData } from './pur-order-form-model';

const BASE = '/purchasing/goods-receipts';

export function ErpGoodsReceiptsPage({ formMode, recordId, onNavigate }: TrxFormPageProps = {}) {
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const [form, setForm] = React.useState<PurOrderFormData>(defaultPurOrderForm);
  const [saving, setSaving] = React.useState(false);
  const { statuses: allowedCreationStatuses } = useAllowedCreationStatuses('PUR.GRN');

  const formReady =
    formMode === 'create' ||
    (formMode === 'edit' && String(form.id ?? '') === String(recordId ?? ''));

  const goList = React.useCallback(() => onNavigate?.(BASE), [onNavigate]);
  const [search, setSearch] = React.useState('');
  const [statusFilter, setStatusFilter] = React.useState('');
  const { page, pageSize, setPage, setPageSize } = useListPagination('pur-goods-receipts');

  const [debouncedSearch, setDebouncedSearch] = React.useState(search);
  React.useEffect(() => { const t = setTimeout(() => setDebouncedSearch(search), 300); return () => clearTimeout(t); }, [search]);

  const { rows, meta, loading, error, reload } = useErpList(
    () => listPurGoodsReceipts({ page, limit: pageSize, search: debouncedSearch || undefined, status: (statusFilter || undefined) as ErpPurGoodsReceipt['status'] | undefined, sortBy: 'docDate', sortDir: 'desc' }),
    [page, pageSize, debouncedSearch, statusFilter],
  );

  React.useEffect(() => { setPage(1); }, [debouncedSearch, statusFilter, pageSize]);

  const [focused, setFocused] = React.useState(-1);
  const [selected, setSelected] = React.useState<Set<string>>(new Set());
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  const openCreate = () => onNavigate?.(trxNewRoute(BASE));
  const openEdit = (r: ErpPurGoodsReceipt) => onNavigate?.(trxEditRoute(BASE, r.id));

  const loadForm = React.useCallback(() => {
    if (formMode === 'create') { setForm(defaultPurOrderForm()); return undefined; }
    if (formMode === 'edit' && recordId) {
      let alive = true;
      getPurGoodsReceipt(recordId)
        .then((r) => alive && setForm(fromPurGoodsReceipt(r)))
        .catch(() => { if (alive) { notify('Gagal memuat Goods Receipt', 'danger'); goList(); } });
      return () => { alive = false; };
    }
    return undefined;
  }, [formMode, recordId, goList]);
  React.useEffect(() => loadForm(), [loadForm]);

  const persist = async (closeAfter: boolean, newAfter = false) => {
    if (!form.branchId || !form.docDate || !form.currencyId) {
      notify('Cabang, Tanggal, dan Uang wajib diisi.', 'warn'); return;
    }
    setSaving(true);
    try {
      const payload = toPurGoodsReceiptPayload(form);
      if (form.id) { await updatePurGoodsReceipt(form.id, payload); notify('Goods Receipt diperbarui', 'success'); }
      else { await createPurGoodsReceipt(payload); notify('Goods Receipt dibuat', 'success'); }
      reload();
      if (newAfter) { setForm(defaultPurOrderForm()); onNavigate?.(trxNewRoute(BASE)); }
      else if (closeAfter) { goList(); }
    } catch (e: unknown) { notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger'); }
    finally { setSaving(false); }
  };

  const handleDelete = (r: ErpPurGoodsReceipt) => {
    confirmAction({
      title: 'Hapus Goods Receipt?', message: `${r.docNumber} akan dihapus permanen.`,
      variant: 'danger', confirmLabel: 'Hapus', confirmIcon: 'trash',
      onConfirm: async () => {
        try { await deletePurGoodsReceipt(r.id); notify('Goods Receipt dihapus', 'success'); reload(); }
        catch (e: unknown) { notify(e instanceof Error ? e.message : 'Gagal', 'danger'); }
      },
    });
  };

  const runTransition = async (r: ErpPurGoodsReceipt, action: string) => {
    let reason: string | undefined;
    if (action === 'REJECT') { reason = window.prompt('Alasan menolak?') ?? undefined; if (!reason) return; }
    try { await transitionPurGoodsReceipt(r.id, action as never, reason); notify(`Berhasil: ${r.docNumber}`, 'success'); reload(); }
    catch (e: unknown) { notify(e instanceof Error ? e.message : 'Gagal', 'danger'); }
  };

  const rowActions = (r: ErpPurGoodsReceipt): RowActionItem[] => [
    { label: 'Edit / Lihat', onSelect: () => openEdit(r) },
    ...cashBankWorkflowActions(r.status as never, (a) => runTransition(r, a)),
    { label: 'Hapus', onSelect: () => handleDelete(r), danger: true, separatorBefore: true },
  ];

  if (mode === 'form') {
    return (
      <div className="page">
        <div className="page-header">
          <h1 className="page-title flex items-center gap-2">
            <button className="iconbtn" onClick={goList} title="Kembali" style={{ fontSize: 18, lineHeight: 1 }}>←</button>
            Penerimaan Barang <span className="code-tag">GRN</span>
          </h1>
        </div>
        <div className="page-body overflow-auto p-4">
          {formReady ? (
            <PurGoodsReceiptForm data={form} onChange={setForm} saving={saving}
              allowedCreationStatuses={formMode === 'create' ? allowedCreationStatuses : undefined}
              onSave={() => persist(true)} onSaveNew={() => persist(false, true)} onReset={loadForm} />
          ) : <div className="p-8 text-center text-muted">Memuat…</div>}
        </div>
      </div>
    );
  }

  const summary: SummaryConfig = { metricLabel: 'Σ Penerimaan Barang', rowCount: rows.length, totalCount: totalRows };
  const pagination: ListPaginationConfig = { page, pageCount, pageSize, totalRows, onPage: setPage, onPageSize: setPageSize };
  const toggleSel = (id: string) => setSelected((s) => { const n = new Set(s); n.has(id) ? n.delete(id) : n.add(id); return n; });

  return (
    <ErpListLayout title="Penerimaan Barang" code="GRN" loading={loading} error={error}
      search={search} onSearch={setSearch} onAdd={openCreate} onRefresh={reload}
      toolbar={null} summary={summary} pagination={pagination}
      keyboardRows={{ rowCount: rows.length, focusedIndex: focused, onFocusChange: setFocused, onToggle: (i) => rows[i] && toggleSel(rows[i].id), onOpen: (i) => rows[i] && openEdit(rows[i]) }}>
      {selected.size > 0 && (
        <div className="bulk-bar flex items-center gap-3 px-3 py-2 mb-2 rounded-md bg-secondary text-sm">
          <strong>{selected.size}</strong> baris dipilih
          <button className="btn sm danger" onClick={() => confirmAction({ title: 'Hapus terpilih?', message: `${selected.size} dokumen akan dihapus permanen.`, variant: 'danger', confirmLabel: 'Hapus', onConfirm: async () => { await Promise.all([...selected].map((id) => deletePurGoodsReceipt(id).catch(() => null))); notify(`${selected.size} dokumen dihapus`, 'success'); setSelected(new Set()); reload(); } })}>
            <Icon name="trash" size={12} /> Hapus
          </button>
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
            <TableHead style={{ textAlign: 'right' }}>Total</TableHead>
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
