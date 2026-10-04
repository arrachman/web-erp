'use client';

/**
 * Freight Payable (PP) — list + form. URL: /purchasing/freight-payables · /new · /:id.
 * Standalone document on its own backend (erp-pur-freight-payables, stored in
 * fin_ap_payments with source='PP') — NOT a Purchase Invoice view.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import {
  ErpListLayout,
  type ListPaginationConfig,
  type SummaryConfig,
} from '@/components/organisms/erp-list-layout';
import {
  Table, TableHeader, TableBody, TableRow, TableHead,
  TableCell, TableEmpty, CodeLinkCell,
} from '@/components/organisms/table';
import { RowActionsMenu, RowContextMenu, type RowActionItem } from '@/components/molecules/row-actions-menu';
import { confirmAction, notify } from '@/lib/feedback';
import { cashBankWorkflowActions } from '@/lib/fin-cash-bank-workflow';
import { type TrxFormPageProps, trxNewRoute, trxEditRoute } from '@/lib/trx-route';
import { useErpList } from '@/lib/use-erp-list';
import { useListPagination } from '@/lib/use-list-pagination';
import { formatNumber } from '@/lib/format';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import {
  listFreightPayables, createFreightPayable, updateFreightPayable,
  deleteFreightPayable, getFreightPayable, transitionFreightPayable,
  type ErpFreightPayable, type FreightPayableTransition,
} from '@/lib/api/pur-freight-payables';
import {
  PurFreightPayableForm,
  defaultPurFreightPayableForm, fromFreightPayable, toFreightPayablePayload,
  type PurFreightPayableFormData,
} from './pur-freight-payable-form';

const BASE = '/purchasing/freight-payables';

export function ErpFreightPayablesPage({ formMode, recordId, onNavigate }: TrxFormPageProps = {}) {
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const [form, setForm] = React.useState<PurFreightPayableFormData>(defaultPurFreightPayableForm());
  const [saving, setSaving] = React.useState(false);

  const formReady =
    formMode === 'create' ||
    (formMode === 'edit' && String(form.id ?? '') === String(recordId ?? ''));

  const goList = React.useCallback(() => onNavigate?.(BASE), [onNavigate]);

  const [search, setSearch] = React.useState('');
  const { page, pageSize, setPage, setPageSize } = useListPagination('pur-freight-payables');
  const [debouncedSearch, setDebouncedSearch] = React.useState(search);
  React.useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(search), 300);
    return () => clearTimeout(t);
  }, [search]);

  const { rows, meta, loading, error, reload } = useErpList(
    () => listFreightPayables({ page, limit: pageSize, search: debouncedSearch || undefined, sortBy: 'docNumber', sortDir: 'desc' }),
    [page, pageSize, debouncedSearch],
  );
  React.useEffect(() => { setPage(1); }, [debouncedSearch, pageSize]);

  const [focused, setFocused] = React.useState(-1);
  const [selected, setSelected] = React.useState<Set<string>>(new Set());
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  const openCreate = () => onNavigate?.(trxNewRoute(BASE));
  const openEdit = (r: ErpFreightPayable) => onNavigate?.(trxEditRoute(BASE, r.id));

  const loadForm = React.useCallback(() => {
    if (formMode === 'create') { setForm(defaultPurFreightPayableForm()); return undefined; }
    if (formMode === 'edit' && recordId) {
      let alive = true;
      getFreightPayable(recordId)
        .then((full) => alive && setForm(fromFreightPayable(full)))
        .catch(() => { if (!alive) return; notify('Gagal memuat Biaya Pengiriman', 'danger'); goList(); });
      return () => { alive = false; };
    }
    return undefined;
  }, [formMode, recordId, goList]);
  React.useEffect(() => loadForm(), [loadForm]);

  const persist = async (closeAfter: boolean, newAfter = false) => {
    if (!form.branchId || !form.transactionDate || !form.currencyId) {
      notify('Cabang, Tanggal, dan Mata Uang wajib diisi.', 'warn'); return;
    }
    if (!form.partnerId) { notify('Supplier / Ekspedisi wajib diisi.', 'warn'); return; }
    if (!form.description.trim()) { notify('Keterangan wajib diisi.', 'warn'); return; }
    if (!(Number(form.amount) > 0)) { notify('Jumlah biaya harus lebih besar dari 0.', 'warn'); return; }
    setSaving(true);
    try {
      const payload = toFreightPayablePayload(form);
      if (form.id) { await updateFreightPayable(form.id, payload); notify('Biaya Pengiriman diperbarui', 'success'); }
      else { await createFreightPayable(payload); notify('Biaya Pengiriman dibuat', 'success'); }
      reload();
      if (newAfter) { setForm(defaultPurFreightPayableForm()); onNavigate?.(trxNewRoute(BASE)); }
      else if (closeAfter) { goList(); }
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally { setSaving(false); }
  };

  const runTransition = async (r: ErpFreightPayable, action: FreightPayableTransition) => {
    let reason: string | undefined;
    if (action === 'REJECT') { reason = window.prompt('Alasan menolak?') ?? undefined; if (!reason) return; }
    try { await transitionFreightPayable(r.id, action, reason); notify(`Berhasil: ${r.docNumber}`, 'success'); reload(); }
    catch (e: unknown) { notify(e instanceof Error ? e.message : 'Gagal', 'danger'); }
  };

  const handleDelete = (r: ErpFreightPayable) => {
    confirmAction({
      title: 'Hapus Biaya Pengiriman?', message: `${r.docNumber} akan dihapus permanen.`,
      variant: 'danger', confirmLabel: 'Hapus', confirmIcon: 'trash',
      onConfirm: async () => {
        try { await deleteFreightPayable(r.id); notify('Dihapus', 'success'); reload(); }
        catch (e: unknown) { notify(e instanceof Error ? e.message : 'Gagal', 'danger'); }
      },
    });
  };

  const rowActions = (r: ErpFreightPayable): RowActionItem[] => [
    { label: 'Edit / Lihat', onSelect: () => openEdit(r) },
    ...cashBankWorkflowActions(r.status as never, (a) => runTransition(r, a as FreightPayableTransition)),
    { label: 'Hapus', onSelect: () => handleDelete(r), danger: true, separatorBefore: true },
  ];

  if (mode === 'form') {
    return (
      <div className="page">
        <div className="page-header">
          <h1 className="page-title flex items-center gap-2">
            <button className="iconbtn" onClick={goList} style={{ fontSize: 18, lineHeight: 1 }}>←</button>
            Biaya Pengiriman Terutang <span className="code-tag">PP</span>
          </h1>
        </div>
        <div className="page-body overflow-auto p-4">
          {formReady ? (
            <PurFreightPayableForm
              data={form} onChange={setForm} saving={saving}
              onSave={() => persist(true)} onSaveNew={() => persist(false, true)} onReset={loadForm}
            />
          ) : (
            <div className="p-8 text-center text-muted">Memuat…</div>
          )}
        </div>
      </div>
    );
  }

  const summary: SummaryConfig = { metricLabel: 'Σ Biaya Pengiriman', rowCount: rows.length, totalCount: totalRows };
  const pagination: ListPaginationConfig = { page, pageCount, pageSize, totalRows, onPage: setPage, onPageSize: setPageSize };
  const toggleSel = (id: string) => setSelected((s) => { const n = new Set(s); n.has(id) ? n.delete(id) : n.add(id); return n; });

  return (
    <ErpListLayout
      title="Biaya Pengiriman Terutang (PP)" code="PP"
      loading={loading} error={error} search={search} onSearch={setSearch}
      onAdd={openCreate} onRefresh={reload}
      toolbar={null} summary={summary} pagination={pagination}
      keyboardRows={{ rowCount: rows.length, focusedIndex: focused, onFocusChange: setFocused, onToggle: (i) => rows[i] && toggleSel(rows[i].id), onOpen: (i) => rows[i] && openEdit(rows[i]) }}
    >
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
                <TableRow
                  style={focused === i ? { boxShadow: 'inset 2px 0 0 var(--primary)' } : undefined}
                  className="cursor-pointer"
                >
                  <TableCell style={{ textAlign: 'center' }}>
                    <input type="checkbox" checked={selected.has(r.id)} onChange={() => toggleSel(r.id)} />
                  </TableCell>
                  <CodeLinkCell code={r.docNumber} onOpen={() => openEdit(r)} />
                  <TableCell>{r.transactionDate.slice(0, 10)}</TableCell>
                  <TableCell>{r.partner?.name ?? '—'}</TableCell>
                  <TableCell>{r.description ?? '—'}</TableCell>
                  <TableCell className="tabular-nums" style={{ textAlign: 'right' }}>
                    {formatNumber(Number(r.amount), 2)}
                  </TableCell>
                  <TableCell>
                    <Badge variant={statusBadgeVariant(r.status)} dot>{statusLabel(r.status)}</Badge>
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
