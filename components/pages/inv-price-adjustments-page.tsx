'use client';

/**
 * Inventory Price Adjustment (PA) list + create form page. URL-driven list↔form
 * via trx sub-routes (§2.3.1): <base> · /new · /:id. PA has no workflow
 * transitions — status is backend-managed. Create-only from UI; existing records
 * are view-only. Atomic tier: Page.
 *
 * transactionCode: 'INV.PA'
 * base: '/warehouse/price-adjustments'
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { Icon } from '@/components/ui/icons';
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
  InvPriceAdjustmentFiltersBar,
  emptyInvPriceAdjustmentFilters,
  type InvPriceAdjustmentFilters,
} from './inv-price-adjustments-filters';
import {
  RowActionsMenu,
  RowContextMenu,
  type RowActionItem,
} from '@/components/molecules/row-actions-menu';
import { confirmAction, notify } from '@/lib/feedback';
import { trxNewRoute, trxEditRoute, type TrxFormPageProps } from '@/lib/trx-route';
import { useErpList } from '@/lib/use-erp-list';
import { useListPagination } from '@/lib/use-list-pagination';
import {
  listInvPriceAdjustments,
  createInvPriceAdjustment,
  deleteInvPriceAdjustment,
  processInvPriceAdjustment,
  getInvPriceAdjustment,
  type ErpInvPriceAdjustment,
  type ErpInvPriceAdjustmentStatus,
} from '@/lib/api/inv-price-adjustments';
import { InvPriceAdjustmentForm } from './inv-price-adjustment-form';
import {
  defaultInvPriceAdjustmentForm,
  fromInvPriceAdjustment,
  toInvPriceAdjustmentPayload,
  type InvPriceAdjustmentFormData,
} from './inv-price-adjustment-form-model';
import { paBadgeVariant } from './inv-price-adjustment-form';

const TRANSACTION_CODE = 'INV.PA';
const BASE = '/warehouse/price-adjustments';
const TITLE = 'Price Adjustment';
const CODE = 'PA';

/** Price Adjustment (PA) — list + form page. */
export function ErpInvPriceAdjustmentsPage({
  formMode,
  recordId,
  onNavigate,
}: TrxFormPageProps = {}) {
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const [form, setForm] = React.useState<InvPriceAdjustmentFormData>(() =>
    defaultInvPriceAdjustmentForm(),
  );
  const [saving, setSaving] = React.useState(false);

  const formReady =
    formMode === 'create' ||
    (formMode === 'edit' && String(form.id ?? '') === String(recordId ?? ''));

  const goList = React.useCallback(() => onNavigate?.(BASE), [onNavigate]);

  const [search, setSearch] = React.useState('');
  const [filters, setFilters] = React.useState<InvPriceAdjustmentFilters>(
    emptyInvPriceAdjustmentFilters,
  );
  const { page, pageSize, setPage, setPageSize } = useListPagination('inv-price-adjustments');

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
      listInvPriceAdjustments({
        page,
        limit: pageSize,
        status: (debF.status || undefined) as ErpInvPriceAdjustmentStatus | undefined,
        dateFrom: debF.dateFrom || undefined,
        dateTo: debF.dateTo || undefined,
        sortBy: 'fromDate',
        sortDir: 'desc',
      }),
    [page, pageSize, debouncedSearch, debF],
  );

  React.useEffect(() => { setPage(1); }, [debouncedSearch, debF, pageSize]);

  const [focused, setFocused] = React.useState(-1);
  const [selected, setSelected] = React.useState<Set<string>>(new Set());
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  const openCreate = () => onNavigate?.(trxNewRoute(BASE));
  const openEdit = (r: ErpInvPriceAdjustment) => onNavigate?.(trxEditRoute(BASE, r.id));

  const loadForm = React.useCallback(() => {
    if (formMode === 'create') { setForm(defaultInvPriceAdjustmentForm()); return undefined; }
    if (formMode === 'edit' && recordId) {
      let alive = true;
      getInvPriceAdjustment(recordId)
        .then((full) => alive && setForm(fromInvPriceAdjustment(full)))
        .catch(() => { if (!alive) return; notify(`Gagal memuat ${TITLE}`, 'danger'); goList(); });
      return () => { alive = false; };
    }
    return undefined;
  }, [formMode, recordId, goList]);
  React.useEffect(() => loadForm(), [loadForm]);

  const persist = async () => {
    if (!form.fromDate) { notify('Dari Tanggal wajib diisi.', 'warn'); return; }
    setSaving(true);
    try {
      const created = await createInvPriceAdjustment(toInvPriceAdjustmentPayload(form));
      notify(`${TITLE} dibuat`, 'success');
      reload();
      onNavigate?.(trxEditRoute(BASE, created.id));
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally { setSaving(false); }
  };

  const handleDelete = (r: ErpInvPriceAdjustment) => {
    confirmAction({
      title: `Hapus ${TITLE}?`,
      message: `${r.docNumber} akan dihapus permanen.`,
      variant: 'danger', confirmLabel: 'Hapus', confirmIcon: 'trash',
      onConfirm: async () => {
        try {
          await deleteInvPriceAdjustment(r.id);
          notify(`${TITLE} dihapus`, 'success'); reload();
        } catch (e: unknown) { notify(e instanceof Error ? e.message : 'Gagal', 'danger'); }
      },
    });
  };

  const handleProcess = (r: ErpInvPriceAdjustment) => {
    confirmAction({
      title: 'Proses recalculation?',
      message: `${r.docNumber} akan dihitung ulang dari moving-average cost.`,
      confirmLabel: 'Proses', confirmIcon: 'refresh',
      onConfirm: async () => {
        try {
          await processInvPriceAdjustment(r.id);
          notify('Perhitungan ulang harga selesai', 'success');
          reload();
        } catch (e: unknown) {
          notify(e instanceof Error ? e.message : 'Gagal memproses', 'danger');
        }
      },
    });
  };

  const rowActions = (r: ErpInvPriceAdjustment): RowActionItem[] => {
    const canProcess = r.status === 'PENDING' || r.status === 'FAILED';
    return [
      { label: 'Lihat Detail', onSelect: () => openEdit(r) },
      ...(canProcess
        ? [{ label: 'Proses', onSelect: () => handleProcess(r) } as RowActionItem]
        : []),
      { label: 'Hapus', onSelect: () => handleDelete(r), danger: true, separatorBefore: true },
    ];
  };

  const toggleSel = (id: string) =>
    setSelected((s) => { const n = new Set(s); n.has(id) ? n.delete(id) : n.add(id); return n; });

  // ── form view ────────────────────────────────────────────────────────────────
  if (mode === 'form') {
    return (
      <div className="page">
        <div className="page-header">
          <h1 className="page-title flex items-center gap-2">
            <button className="iconbtn" onClick={goList} title="Kembali" style={{ fontSize: 18, lineHeight: 1 }}>←</button>
            {TITLE}<span className="code-tag">{CODE}</span>
          </h1>
        </div>
        <div className="page-body overflow-auto p-4">
          {formReady ? (
            <InvPriceAdjustmentForm data={form} onChange={setForm}
              transactionCode={TRANSACTION_CODE} saving={saving}
              onSave={persist} onReset={loadForm} />
          ) : (
            <div className="p-8 text-center text-muted">Memuat…</div>
          )}
        </div>
      </div>
    );
  }

  // ── list view ─────────────────────────────────────────────────────────────────
  const summary: SummaryConfig = { metricLabel: `Σ ${TITLE}`, rowCount: rows.length, totalCount: totalRows };
  const pagination: ListPaginationConfig = { page, pageCount, pageSize, totalRows, onPage: setPage, onPageSize: setPageSize };

  return (
    <ErpListLayout
      title={TITLE} code={CODE} loading={loading} error={error}
      search={search} onSearch={setSearch} onAdd={openCreate} onRefresh={reload}
      toolbar={<InvPriceAdjustmentFiltersBar value={filters} onChange={setFilters} />}
      summary={summary} pagination={pagination}
      keyboardRows={{ rowCount: rows.length, focusedIndex: focused, onFocusChange: setFocused,
        onToggle: (i) => rows[i] && toggleSel(rows[i].id), onOpen: (i) => rows[i] && openEdit(rows[i]) }}
    >
      {selected.size > 0 && (
        <div className="bulk-bar flex items-center gap-3 px-3 py-2 mb-2 rounded-md bg-secondary text-sm">
          <strong>{selected.size}</strong> baris dipilih
          <button className="btn sm danger" onClick={() => confirmAction({
            title: 'Hapus terpilih?', message: `${selected.size} ${TITLE} akan dihapus permanen.`,
            variant: 'danger', confirmLabel: 'Hapus',
            onConfirm: async () => {
              await Promise.all([...selected].map((id) => deleteInvPriceAdjustment(id).catch(() => null)));
              notify(`${selected.size} dokumen dihapus`, 'success'); setSelected(new Set()); reload();
            },
          })}>
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
            <TableHead>Dari Tanggal</TableHead>
            <TableHead>Sampai Tanggal</TableHead>
            <TableHead>Gudang</TableHead>
            <TableHead>Item</TableHead>
            <TableHead style={{ textAlign: 'right' }}>Total Delta</TableHead>
            <TableHead>Status</TableHead>
            <TableHead style={{ width: 44 }} />
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.length === 0 ? (
            <TableEmpty colSpan={9} />
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
                    <TableCell>{r.fromDate.slice(0, 10)}</TableCell>
                    <TableCell>{r.toDate ? r.toDate.slice(0, 10) : '—'}</TableCell>
                    <TableCell>{r.warehouse?.name ?? '—'}</TableCell>
                    <TableCell>{r.item?.name ?? '—'}</TableCell>
                    <TableCell style={{ textAlign: 'right' }} className="tabular-nums">{r.totalDelta ?? '—'}</TableCell>
                    <TableCell>
                      <Badge variant={paBadgeVariant(r.status)} dot>{r.status}</Badge>
                    </TableCell>
                    <TableCell><RowActionsMenu items={actions} /></TableCell>
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
