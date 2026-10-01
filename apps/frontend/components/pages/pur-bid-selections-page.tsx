'use client';

/**
 * Bid Selection / Comparison (BS) — list + form. URL: /purchasing/bid-comparisons · /new · /:id.
 * Full CRUD wired to PurBsForm.
 */

import * as React from 'react';
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
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import {
  listPurBidSelections, deletePurBidSelection, type ErpPurBidSelection,
} from '@/lib/api/pur-bid-selections';
import {
  PurBsForm, defaultPurBsForm, fromPurBidSelection, toPurBsPayload,
  type PurBsFormData, type PurBidSelectionTransition,
  createPurBidSelection, updatePurBidSelection, getPurBidSelection, transitionPurBidSelection,
} from './pur-bs-form';

const BASE = '/purchasing/bid-comparisons';

export function ErpBidSelectionsPage({ formMode, recordId, onNavigate }: TrxFormPageProps = {}) {
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const [form, setForm] = React.useState<PurBsFormData>(defaultPurBsForm());
  const [saving, setSaving] = React.useState(false);

  const formReady =
    formMode === 'create' ||
    (formMode === 'edit' && String(form.id ?? '') === String(recordId ?? ''));

  const goList = React.useCallback(() => onNavigate?.(BASE), [onNavigate]);

  const [search, setSearch] = React.useState('');
  const [statusFilter, setStatusFilter] = React.useState('');
  const { page, pageSize, setPage, setPageSize } = useListPagination('pur-bid-selections');

  const [debouncedSearch, setDebouncedSearch] = React.useState(search);
  React.useEffect(() => { const t = setTimeout(() => setDebouncedSearch(search), 300); return () => clearTimeout(t); }, [search]);

  const { rows, meta, loading, error, reload } = useErpList(
    () => listPurBidSelections({ page, limit: pageSize, search: debouncedSearch || undefined, status: (statusFilter || undefined) as ErpPurBidSelection['status'] | undefined, sortBy: 'docDate', sortDir: 'desc' }),
    [page, pageSize, debouncedSearch, statusFilter],
  );

  React.useEffect(() => { setPage(1); }, [debouncedSearch, statusFilter, pageSize]);

  const [focused, setFocused] = React.useState(-1);
  const [selected, setSelected] = React.useState<Set<string>>(new Set());
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  const openCreate = () => onNavigate?.(trxNewRoute(BASE));
  const openEdit = (r: ErpPurBidSelection) => onNavigate?.(trxEditRoute(BASE, r.id));

  const loadForm = React.useCallback(() => {
    if (formMode === 'create') { setForm(defaultPurBsForm()); return undefined; }
    if (formMode === 'edit' && recordId) {
      let alive = true;
      getPurBidSelection(recordId)
        .then((full) => alive && setForm(fromPurBidSelection(full)))
        .catch(() => { if (!alive) return; notify('Gagal memuat Bid Comparison', 'danger'); goList(); });
      return () => { alive = false; };
    }
    return undefined;
  }, [formMode, recordId, goList]);
  React.useEffect(() => loadForm(), [loadForm]);

  const persist = async (closeAfter: boolean, newAfter = false) => {
    if (!form.branchId || !form.docDate) { notify('Cabang dan Tanggal wajib diisi.', 'warn'); return; }
    if (!form.bidLines.some((l) => l.quotationLineId)) { notify('Minimal satu baris evaluasi.', 'warn'); return; }
    setSaving(true);
    try {
      const payload = toPurBsPayload(form);
      if (form.id) { await updatePurBidSelection(form.id, payload); notify('Bid Comparison diperbarui', 'success'); }
      else { await createPurBidSelection(payload); notify('Bid Comparison dibuat', 'success'); }
      reload();
      if (newAfter) { setForm(defaultPurBsForm()); onNavigate?.(trxNewRoute(BASE)); }
      else if (closeAfter) { goList(); }
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally { setSaving(false); }
  };

  const runTransition = async (r: ErpPurBidSelection, action: PurBidSelectionTransition) => {
    let reason: string | undefined;
    if (action === 'REJECT') { reason = window.prompt('Alasan menolak?') ?? undefined; if (!reason) return; }
    try { await transitionPurBidSelection(r.id, action, reason); notify(`Berhasil: ${r.docNumber}`, 'success'); reload(); }
    catch (e: unknown) { notify(e instanceof Error ? e.message : 'Gagal', 'danger'); }
  };

  const handleDelete = (r: ErpPurBidSelection) => {
    confirmAction({
      title: 'Hapus Bid Comparison?', message: `${r.docNumber} akan dihapus permanen.`,
      variant: 'danger', confirmLabel: 'Hapus', confirmIcon: 'trash',
      onConfirm: async () => {
        try { await deletePurBidSelection(r.id); notify('Bid Comparison dihapus', 'success'); reload(); }
        catch (e: unknown) { notify(e instanceof Error ? e.message : 'Gagal', 'danger'); }
      },
    });
  };

  const rowActions = (r: ErpPurBidSelection): RowActionItem[] => [
    { label: 'Edit / Lihat', onSelect: () => openEdit(r) },
    ...cashBankWorkflowActions(r.status as never, (a) => runTransition(r, a as PurBidSelectionTransition)),
    { label: 'Hapus', onSelect: () => handleDelete(r), danger: true, separatorBefore: true },
  ];

  // ── form view ─────────────────────────────────────────────────────────────────
  if (mode === 'form') {
    return (
      <div className="page">
        <div className="page-header">
          <h1 className="page-title flex items-center gap-2">
            <button className="iconbtn" onClick={goList} title="Kembali" style={{ fontSize: 18, lineHeight: 1 }}>←</button>
            Perbandingan Harga <span className="code-tag">BS</span>
          </h1>
        </div>
        <div className="page-body overflow-auto p-4">
          {formReady ? (
            <PurBsForm data={form} onChange={setForm} saving={saving}
              onSave={() => persist(true)} onSaveNew={() => persist(false, true)} onReset={loadForm} />
          ) : (
            <div className="p-8 text-center text-muted">Memuat…</div>
          )}
        </div>
      </div>
    );
  }

  // ── list view ─────────────────────────────────────────────────────────────────
  const summary: SummaryConfig = { metricLabel: 'Σ Bid Comparison', rowCount: rows.length, totalCount: totalRows };
  const pagination: ListPaginationConfig = { page, pageCount, pageSize, totalRows, onPage: setPage, onPageSize: setPageSize };
  const toggleSel = (id: string) => setSelected((s) => { const n = new Set(s); n.has(id) ? n.delete(id) : n.add(id); return n; });

  return (
    <ErpListLayout title="Perbandingan Harga (BS)" code="BS" loading={loading} error={error}
      search={search} onSearch={setSearch} onAdd={openCreate} onRefresh={reload}
      toolbar={null} summary={summary} pagination={pagination}
      keyboardRows={{ rowCount: rows.length, focusedIndex: focused, onFocusChange: setFocused, onToggle: (i) => rows[i] && toggleSel(rows[i].id), onOpen: (i) => rows[i] && openEdit(rows[i]) }}>
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead style={{ width: 36 }} />
            <TableHead>No Transaksi</TableHead>
            <TableHead>Tanggal</TableHead>
            <TableHead>Uraian</TableHead>
            <TableHead>Baris Evaluasi</TableHead>
            <TableHead>Status</TableHead>
            <TableHead style={{ width: 44 }} />
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.length === 0 ? <TableEmpty colSpan={7} /> : rows.map((r, i) => {
            const actions = rowActions(r);
            return (
              <RowContextMenu key={r.id} items={actions}>
                <TableRow style={focused === i ? { boxShadow: 'inset 2px 0 0 var(--primary)' } : undefined} className="cursor-pointer">
                  <TableCell style={{ textAlign: 'center' }}><input type="checkbox" checked={selected.has(r.id)} onChange={() => toggleSel(r.id)} /></TableCell>
                  <CodeLinkCell code={r.docNumber} onOpen={() => openEdit(r)} />
                  <TableCell>{r.docDate.slice(0, 10)}</TableCell>
                  <TableCell>{r.description ?? '—'}</TableCell>
                  <TableCell>{r.lines.length} baris</TableCell>
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
