'use client';

/**
 * Delivery Report (DR) — list (§2.7) + master-detail form. Atomic tier: Page.
 * URL-driven list↔form via trx sub-routes (§2.3.1): /sales/delivery-reports · /new · /:id.
 * Route = SSOT, no internal mode state. Table extracted → sls-delivery-reports-table.tsx.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import {
  ErpListLayout,
  type ListPaginationConfig,
  type SummaryConfig,
} from '@/components/organisms/erp-list-layout';
import {
  SlsDeliveryReportFilters,
  emptySlsDrFilters,
  type SlsDrFilters,
} from './sls-delivery-report-filters';
import { confirmAction, notify } from '@/lib/feedback';
import { cashBankWorkflowActions } from '@/lib/fin-cash-bank-workflow';
import { trxNewRoute, trxEditRoute, type TrxFormPageProps } from '@/lib/trx-route';
import { useErpList } from '@/lib/use-erp-list';
import { useListPagination } from '@/lib/use-list-pagination';
import { formatNumber } from '@/lib/format';
import {
  listSlsDeliveryReports,
  createSlsDeliveryReport,
  updateSlsDeliveryReport,
  deleteSlsDeliveryReport,
  getSlsDeliveryReport,
  transitionSlsDeliveryReport,
  type SlsDeliveryReportTransition,
  type ErpSlsDeliveryReport,
  type ErpDocumentStatus,
} from '@/lib/api/sls-delivery-reports';
import { useAllowedCreationStatuses } from '@/lib/use-allowed-creation-statuses';
import {
  SlsDeliveryReportForm,
  defaultSlsDeliveryReportForm,
  fromSlsDeliveryReport,
  toSlsDeliveryReportPayload,
  type SlsDeliveryReportFormData,
} from './sls-delivery-report-form';
import {
  SlsDeliveryReportsTable,
  SlsDeliveryReportsBulkBar,
} from './sls-delivery-reports-table';

const DR_BASE = '/sales/delivery-reports';

export function ErpSlsDeliveryReportsPage({ formMode, recordId, onNavigate }: TrxFormPageProps = {}) {
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const [form, setForm] = React.useState<SlsDeliveryReportFormData>(defaultSlsDeliveryReportForm);
  const [saving, setSaving] = React.useState(false);
  const { statuses: allowedCreationStatuses } = useAllowedCreationStatuses('SLS.DR');

  const formReady =
    formMode === 'create' ||
    (formMode === 'edit' && String(form.id ?? '') === String(recordId ?? ''));

  const goList = React.useCallback(() => onNavigate?.(DR_BASE), [onNavigate]);

  const [search, setSearch] = React.useState('');
  const [filters, setFilters] = React.useState<SlsDrFilters>(emptySlsDrFilters);
  const [sortBy, setSortBy] = React.useState('docDate');
  const [sortDir, setSortDir] = React.useState<'asc' | 'desc'>('desc');
  const { page, pageSize, setPage, setPageSize } = useListPagination('sls-delivery-reports');

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
      listSlsDeliveryReports({
        page, limit: pageSize,
        search: debouncedSearch || undefined,
        status: (debF.status || undefined) as ErpDocumentStatus | undefined,
        dateFrom: debF.dateFrom || undefined,
        dateTo: debF.dateTo || undefined,
        docNumberFrom: debF.docNumber || undefined,
        description: debF.uraian || undefined,
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

  const openCreate = () => onNavigate?.(trxNewRoute(DR_BASE));
  const openEdit = (r: ErpSlsDeliveryReport) => onNavigate?.(trxEditRoute(DR_BASE, r.id));

  const loadForm = React.useCallback(() => {
    if (formMode === 'create') { setForm(defaultSlsDeliveryReportForm()); return undefined; }
    if (formMode === 'edit' && recordId) {
      let alive = true;
      getSlsDeliveryReport(recordId)
        .then((full) => alive && setForm(fromSlsDeliveryReport(full)))
        .catch(() => { if (!alive) return; notify('Gagal memuat Delivery Report', 'danger'); goList(); });
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
      const payload = toSlsDeliveryReportPayload(form);
      if (form.id) { await updateSlsDeliveryReport(form.id, payload); notify('Delivery Report diperbarui', 'success'); }
      else { await createSlsDeliveryReport(payload); notify('Delivery Report dibuat', 'success'); }
      reload();
      if (newAfter) { setForm(defaultSlsDeliveryReportForm()); onNavigate?.(trxNewRoute(DR_BASE)); }
      else if (closeAfter) { goList(); }
    } catch (e: unknown) { notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger'); }
    finally { setSaving(false); }
  };

  const runTransition = async (r: ErpSlsDeliveryReport, action: SlsDeliveryReportTransition) => {
    let reason: string | undefined;
    if (action === 'REJECT') { reason = window.prompt('Alasan menolak dokumen ini?') ?? undefined; if (!reason) return; }
    const verb: Record<SlsDeliveryReportTransition, string> = { SUBMIT: 'mengajukan', APPROVE: 'menyetujui', REJECT: 'menolak', POST: 'memposting', REOPEN: 'membuka kembali' };
    try { await transitionSlsDeliveryReport(r.id, action, reason); notify(`Berhasil ${verb[action]} ${r.docNumber}`, 'success'); reload(); }
    catch (e: unknown) { notify(e instanceof Error ? e.message : 'Gagal', 'danger'); }
  };

  const handleDelete = (r: ErpSlsDeliveryReport) =>
    confirmAction({
      title: 'Hapus Delivery Report?', message: `${r.docNumber} akan dihapus permanen.`,
      variant: 'danger', confirmLabel: 'Hapus', confirmIcon: 'trash',
      onConfirm: async () => {
        try { await deleteSlsDeliveryReport(r.id); notify('Delivery Report dihapus', 'success'); reload(); }
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
            Delivery Report <span className="code-tag">DR</span>
          </h1>
        </div>
        <div className="page-body overflow-auto p-4">
          {formReady ? (
            <SlsDeliveryReportForm data={form} onChange={setForm} saving={saving}
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
    metricLabel: 'Σ Delivery Report', rowCount: rows.length, totalCount: totalRows,
    metricValue: sumGT ? formatNumber(Number(sumGT), 2) : undefined,
  };
  const pagination: ListPaginationConfig = { page, pageCount, pageSize, totalRows, onPage: setPage, onPageSize: setPageSize };

  return (
    <ErpListLayout
      title="Delivery Report" code="DR"
      loading={loading} error={error} search={search} onSearch={setSearch}
      onAdd={openCreate} onRefresh={reload}
      toolbar={
        <>
          <SlsDeliveryReportFilters value={filters} onChange={setFilters} />
          <button type="button" className="btn ghost sm" onClick={() => notify('Export akan tersedia segera.', 'info')} title="Export ke CSV/Excel">
            <Icon name="download" size={12} /> Export
          </button>
        </>
      }
      summary={summary} pagination={pagination}
      keyboardRows={{ rowCount: rows.length, focusedIndex: focused, onFocusChange: setFocused, onToggle: (i) => rows[i] && toggleSel(rows[i].id), onOpen: (i) => rows[i] && openEdit(rows[i]) }}
    >
      <SlsDeliveryReportsBulkBar selected={selected} onClear={() => setSelected(new Set())} onReload={reload} />
      <SlsDeliveryReportsTable
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
