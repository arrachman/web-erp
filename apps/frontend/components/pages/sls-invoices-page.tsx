'use client';

/**
 * Sales Invoice (SI) — list (§2.7) + master-detail form. Atomic tier: Page.
 * URL-driven list↔form via trx sub-routes (§2.3.1): /sales/invoices · /new · /:id.
 * Route = SSOT, no internal mode state. Table extracted → sls-invoices-table.tsx.
 * Extra vs SO: settlementStatus column + Σ Invoice metric label.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import {
  ErpListLayout,
  type ListPaginationConfig,
  type SummaryConfig,
} from '@/components/organisms/erp-list-layout';
import { SlsInvoiceFilters, emptySlsSiFilters, type SlsSiFilters } from './sls-invoice-filters';
import { confirmAction, notify } from '@/lib/feedback';
import { cashBankWorkflowActions } from '@/lib/fin-cash-bank-workflow';
import { trxNewRoute, trxEditRoute, type TrxFormPageProps } from '@/lib/trx-route';
import { useErpList } from '@/lib/use-erp-list';
import { useListPagination } from '@/lib/use-list-pagination';
import { formatNumber } from '@/lib/format';
import {
  listSlsInvoices,
  createSlsInvoice,
  updateSlsInvoice,
  deleteSlsInvoice,
  getSlsInvoice,
  transitionSlsInvoice,
  type SlsInvoiceTransition,
  type ErpSlsInvoice,
  type ErpDocumentStatus,
} from '@/lib/api/sls-invoices';
import { useAllowedCreationStatuses } from '@/lib/use-allowed-creation-statuses';
import {
  SlsInvoiceForm,
  defaultSlsInvoiceForm,
  fromSlsInvoice,
  toSlsInvoicePayload,
  type SlsInvoiceFormData,
} from './sls-invoice-form';
import { SlsInvoicesTable, SlsInvoicesBulkBar } from './sls-invoices-table';

const SI_BASE = '/sales/invoices';

export function ErpSlsInvoicesPage({ formMode, recordId, onNavigate }: TrxFormPageProps = {}) {
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const [form, setForm] = React.useState<SlsInvoiceFormData>(defaultSlsInvoiceForm);
  const [saving, setSaving] = React.useState(false);
  const { statuses: allowedCreationStatuses } = useAllowedCreationStatuses('SLS.SI');

  const formReady =
    formMode === 'create' ||
    (formMode === 'edit' && String(form.id ?? '') === String(recordId ?? ''));

  const goList = React.useCallback(() => onNavigate?.(SI_BASE), [onNavigate]);

  const [search, setSearch] = React.useState('');
  const [filters, setFilters] = React.useState<SlsSiFilters>(emptySlsSiFilters);
  const [sortBy, setSortBy] = React.useState('docDate');
  const [sortDir, setSortDir] = React.useState<'asc' | 'desc'>('desc');
  const { page, pageSize, setPage, setPageSize } = useListPagination('sls-invoices');

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
      listSlsInvoices({
        page, limit: pageSize,
        search: debouncedSearch || undefined,
        status: (debF.status || undefined) as ErpDocumentStatus | undefined,
        dateFrom: debF.dateFrom || undefined,
        dateTo: debF.dateTo || undefined,
        docNumberFrom: debF.docNumber || undefined,
        description: debF.uraian || undefined,
        settlementStatus: debF.settlementStatus || undefined,
        sortBy, sortDir,
      }),
    [page, pageSize, debouncedSearch, debF, sortBy, sortDir],
  );

  React.useEffect(() => { setPage(1); }, [debouncedSearch, debF, pageSize, sortBy, sortDir]);

  const [focused, setFocused] = React.useState(-1);
  const [selected, setSelected] = React.useState<Set<string>>(new Set());
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  const toggleSort = (col: string) => {
    if (sortBy === col) setSortDir((d) => (d === 'asc' ? 'desc' : 'asc'));
    else { setSortBy(col); setSortDir('asc'); }
    setPage(1);
  };
  const toggleSel = (id: string) =>
    setSelected((s) => { const n = new Set(s); n.has(id) ? n.delete(id) : n.add(id); return n; });

  const openCreate = () => onNavigate?.(trxNewRoute(SI_BASE));
  const openEdit = (r: ErpSlsInvoice) => onNavigate?.(trxEditRoute(SI_BASE, r.id));

  const loadForm = React.useCallback(() => {
    if (formMode === 'create') { setForm(defaultSlsInvoiceForm()); return undefined; }
    if (formMode === 'edit' && recordId) {
      let alive = true;
      getSlsInvoice(recordId)
        .then((full) => alive && setForm(fromSlsInvoice(full)))
        .catch(() => { if (!alive) return; notify('Gagal memuat Sales Invoice', 'danger'); goList(); });
      return () => { alive = false; };
    }
    return undefined;
  }, [formMode, recordId, goList]);
  React.useEffect(() => loadForm(), [loadForm]);

  const persist = async (closeAfter: boolean, newAfter = false) => {
    if (!form.branchId || !form.docDate || !form.currencyId) {
      notify('Cabang, Tanggal, dan Mata Uang wajib diisi.', 'warn'); return;
    }
    if (!form.lines.some((l) => l.itemId && Number(l.quantity) > 0)) {
      notify('Minimal satu baris item dengan qty > 0.', 'warn'); return;
    }
    setSaving(true);
    try {
      const payload = toSlsInvoicePayload(form);
      if (form.id) { await updateSlsInvoice(form.id, payload); notify('Sales Invoice diperbarui', 'success'); }
      else { await createSlsInvoice(payload); notify('Sales Invoice dibuat', 'success'); }
      reload();
      if (newAfter) { setForm(defaultSlsInvoiceForm()); onNavigate?.(trxNewRoute(SI_BASE)); }
      else if (closeAfter) { goList(); }
    } catch (e: unknown) { notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger'); }
    finally { setSaving(false); }
  };

  const runTransition = async (r: ErpSlsInvoice, action: SlsInvoiceTransition) => {
    let reason: string | undefined;
    if (action === 'REJECT') { reason = window.prompt('Alasan menolak dokumen ini?') ?? undefined; if (!reason) return; }
    const verb: Record<SlsInvoiceTransition, string> = { SUBMIT: 'mengajukan', APPROVE: 'menyetujui', REJECT: 'menolak', POST: 'memposting', REOPEN: 'membuka kembali' };
    try { await transitionSlsInvoice(r.id, action, reason); notify(`Berhasil ${verb[action]} ${r.docNumber}`, 'success'); reload(); }
    catch (e: unknown) { notify(e instanceof Error ? e.message : 'Gagal', 'danger'); }
  };

  const handleDelete = (r: ErpSlsInvoice) =>
    confirmAction({
      title: 'Hapus Sales Invoice?', message: `${r.docNumber} akan dihapus permanen.`,
      variant: 'danger', confirmLabel: 'Hapus', confirmIcon: 'trash',
      onConfirm: async () => {
        try { await deleteSlsInvoice(r.id); notify('Sales Invoice dihapus', 'success'); reload(); }
        catch (e: unknown) { notify(e instanceof Error ? e.message : 'Gagal', 'danger'); }
      },
    });

  // ── form view ───────────────────────────────────────────────────────────────
  if (mode === 'form') {
    return (
      <div className="page">
        <div className="page-header">
          <h1 className="page-title flex items-center gap-2">
            <button className="iconbtn" onClick={goList} title="Kembali" style={{ fontSize: 18, lineHeight: 1 }}>←</button>
            Sales Invoice <span className="code-tag">SI</span>
          </h1>
        </div>
        <div className="page-body overflow-auto p-4">
          {formReady ? (
            <SlsInvoiceForm data={form} onChange={setForm} saving={saving}
              allowedCreationStatuses={formMode === 'create' ? allowedCreationStatuses : undefined}
              onSave={() => persist(true)} onSaveNew={() => persist(false, true)} onReset={loadForm} />
          ) : (
            <div className="p-8 text-center text-muted">Memuat…</div>
          )}
        </div>
      </div>
    );
  }

  // ── list view ─────────────────────────────────────────────────────────────────
  const sumGT = (meta as { sumGrandTotal?: string } | null)?.sumGrandTotal;
  const summary: SummaryConfig = {
    metricLabel: 'Σ Invoice', rowCount: rows.length, totalCount: totalRows,
    metricValue: sumGT ? formatNumber(Number(sumGT), 2) : undefined,
  };
  const pagination: ListPaginationConfig = { page, pageCount, pageSize, totalRows, onPage: setPage, onPageSize: setPageSize };

  return (
    <ErpListLayout
      title="Sales Invoice" code="SI"
      loading={loading} error={error} search={search} onSearch={setSearch}
      onAdd={openCreate} onRefresh={reload}
      toolbar={
        <>
          <SlsInvoiceFilters value={filters} onChange={setFilters} />
          <button type="button" className="btn ghost sm" onClick={() => notify('Export akan tersedia segera.', 'info')} title="Export ke CSV/Excel">
            <Icon name="download" size={12} /> Export
          </button>
        </>
      }
      summary={summary} pagination={pagination}
      keyboardRows={{ rowCount: rows.length, focusedIndex: focused, onFocusChange: setFocused, onToggle: (i) => rows[i] && toggleSel(rows[i].id), onOpen: (i) => rows[i] && openEdit(rows[i]) }}
    >
      <SlsInvoicesBulkBar selected={selected} onClear={() => setSelected(new Set())} onReload={reload} />
      <SlsInvoicesTable
        rows={rows} focused={focused} selected={selected}
        sortBy={sortBy} sortDir={sortDir}
        onToggleSort={toggleSort}
        onToggleSel={toggleSel}
        onSelectAll={(checked) => setSelected(checked ? new Set(rows.map((r) => r.id)) : new Set())}
        onOpen={openEdit}
        onDelete={handleDelete}
        extraActions={(r) => cashBankWorkflowActions(r.status, (a) => runTransition(r, a))}
      />
    </ErpListLayout>
  );
}
