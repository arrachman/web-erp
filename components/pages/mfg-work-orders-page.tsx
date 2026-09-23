'use client';

/**
 * Manufacturing Work Order (WO) — list (§2.7) + master-detail form.
 * Atomic tier: Page.
 * URL-driven list↔form via trx sub-routes (§2.3.1): /manufacturing/work-orders
 * · /new · /:id. Route = SSOT, no internal mode state.
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
import { woWorkflowActions } from '@/lib/mfg-work-order-workflow';
import {
  listMfgWorkOrders,
  deleteMfgWorkOrder,
  getMfgWorkOrder,
  createMfgWorkOrder,
  updateMfgWorkOrder,
  transitionMfgWorkOrder,
  type MfgWorkOrderTransition,
  type ErpMfgWorkOrder,
  type ErpDocumentStatus,
} from '@/lib/api/mfg-work-orders';
import {
  MfgWorkOrderForm,
  defaultMfgWorkOrderForm,
  fromMfgWorkOrder,
  toMfgWorkOrderPayload,
  type MfgWorkOrderFormData,
} from './mfg-work-order-form';

const WO_BASE = '/manufacturing/work-orders';

const TRANSITION_VERB: Record<MfgWorkOrderTransition, string> = {
  SUBMIT: 'mengajukan',
  APPROVE: 'menyetujui',
  REJECT: 'menolak',
  REOPEN: 'membuka kembali',
};

export function MfgWorkOrdersPage({ formMode, recordId, onNavigate }: TrxFormPageProps = {}) {
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const [form, setForm] = React.useState<MfgWorkOrderFormData>(defaultMfgWorkOrderForm);
  const [saving, setSaving] = React.useState(false);

  const formReady =
    formMode === 'create' ||
    (formMode === 'edit' && String(form.id ?? '') === String(recordId ?? ''));

  const goList = React.useCallback(() => onNavigate?.(WO_BASE), [onNavigate]);

  const [search, setSearch] = React.useState('');
  const [statusFilter, setStatusFilter] = React.useState('');
  const { page, pageSize, setPage, setPageSize } = useListPagination('mfg-work-orders');

  const [debouncedSearch, setDebouncedSearch] = React.useState(search);
  React.useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(search), 300);
    return () => clearTimeout(t);
  }, [search]);

  const { rows, meta, loading, error, reload } = useErpList(
    () =>
      listMfgWorkOrders({
        page,
        limit: pageSize,
        search: debouncedSearch || undefined,
        status: (statusFilter || undefined) as ErpDocumentStatus | undefined,
        sortBy: 'docDate',
        sortDir: 'desc',
      }),
    [page, pageSize, debouncedSearch, statusFilter],
  );

  React.useEffect(() => { setPage(1); }, [debouncedSearch, statusFilter, pageSize]);

  const [focused, setFocused] = React.useState(-1);
  const [selected, setSelected] = React.useState<Set<string>>(new Set());
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  const openCreate = () => onNavigate?.(trxNewRoute(WO_BASE));
  const openEdit = (r: ErpMfgWorkOrder) => onNavigate?.(trxEditRoute(WO_BASE, r.id));

  const loadForm = React.useCallback(() => {
    if (formMode === 'create') { setForm(defaultMfgWorkOrderForm()); return undefined; }
    if (formMode === 'edit' && recordId) {
      let alive = true;
      getMfgWorkOrder(recordId)
        .then((full) => alive && setForm(fromMfgWorkOrder(full)))
        .catch(() => { if (!alive) return; notify('Gagal memuat Work Order', 'danger'); goList(); });
      return () => { alive = false; };
    }
    return undefined;
  }, [formMode, recordId, goList]);
  React.useEffect(() => loadForm(), [loadForm]);

  const persist = async (closeAfter: boolean, newAfter = false) => {
    if (!form.branchId || !form.docDate) {
      notify('Cabang dan Tanggal wajib diisi.', 'warn');
      return;
    }
    setSaving(true);
    try {
      const payload = toMfgWorkOrderPayload(form);
      if (form.id) {
        await updateMfgWorkOrder(form.id, payload);
        notify('Work Order diperbarui', 'success');
      } else {
        await createMfgWorkOrder(payload);
        notify('Work Order dibuat', 'success');
      }
      reload();
      if (newAfter) { setForm(defaultMfgWorkOrderForm()); onNavigate?.(trxNewRoute(WO_BASE)); }
      else if (closeAfter) { goList(); }
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const runTransition = async (r: ErpMfgWorkOrder, action: MfgWorkOrderTransition) => {
    let reason: string | undefined;
    if (action === 'REJECT') {
      reason = window.prompt('Alasan menolak dokumen ini?') ?? undefined;
      if (!reason) return;
    }
    try {
      await transitionMfgWorkOrder(r.id, action, reason);
      notify(`Berhasil ${TRANSITION_VERB[action]} ${r.docNumber}`, 'success');
      reload();
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal', 'danger');
    }
  };

  const handleDelete = (r: ErpMfgWorkOrder) =>
    confirmAction({
      title: 'Hapus Work Order?',
      message: `${r.docNumber} akan dihapus permanen.`,
      variant: 'danger',
      confirmLabel: 'Hapus',
      confirmIcon: 'trash',
      onConfirm: async () => {
        try {
          await deleteMfgWorkOrder(r.id);
          notify('Work Order dihapus', 'success');
          reload();
        } catch (e: unknown) {
          notify(e instanceof Error ? e.message : 'Gagal', 'danger');
        }
      },
    });

  const rowActions = (r: ErpMfgWorkOrder): RowActionItem[] => [
    { label: 'Edit / Lihat', onSelect: () => openEdit(r) },
    ...woWorkflowActions(r.status, (a) => runTransition(r, a)),
    { label: 'Hapus', onSelect: () => handleDelete(r), danger: true, separatorBefore: true },
  ];

  // ── form view ───────────────────────────────────────────────────────────────
  if (mode === 'form') {
    return (
      <div className="page">
        <div className="page-header">
          <h1 className="page-title flex items-center gap-2">
            <button className="iconbtn" onClick={goList} title="Kembali" style={{ fontSize: 18, lineHeight: 1 }}>←</button>
            Work Order
            <span className="code-tag">WO</span>
          </h1>
        </div>
        <div className="page-body overflow-auto p-4">
          {formReady ? (
            <MfgWorkOrderForm
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

  // ── list view ─────────────────────────────────────────────────────────────────
  const summary: SummaryConfig = { metricLabel: 'Σ Work Order', rowCount: rows.length, totalCount: totalRows };
  const pagination: ListPaginationConfig = { page, pageCount, pageSize, totalRows, onPage: setPage, onPageSize: setPageSize };
  const toggleSel = (id: string) => setSelected((s) => { const n = new Set(s); n.has(id) ? n.delete(id) : n.add(id); return n; });

  const toolbar = (
    <div className="flex items-center gap-2">
      <select className="input input-sm" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} style={{ minWidth: 140 }}>
        <option value="">Semua Status</option>
        <option value="DRAFT">Draft</option>
        <option value="NEED_APPROVE">Need Approve</option>
        <option value="APPROVED">Approved</option>
        <option value="REJECTED">Rejected</option>
        <option value="POSTED">Posted</option>
      </select>
      {statusFilter && <button className="btn ghost sm" onClick={() => setStatusFilter('')}>Reset</button>}
    </div>
  );

  return (
    <ErpListLayout
      title="Work Order" code="WO" loading={loading} error={error}
      search={search} onSearch={setSearch} onAdd={openCreate} onRefresh={reload}
      toolbar={toolbar} summary={summary} pagination={pagination}
      keyboardRows={{ rowCount: rows.length, focusedIndex: focused, onFocusChange: setFocused, onToggle: (i) => rows[i] && toggleSel(rows[i].id), onOpen: (i) => rows[i] && openEdit(rows[i]) }}
    >
      {selected.size > 0 && (
        <div className="bulk-bar flex items-center gap-3 px-3 py-2 mb-2 rounded-md bg-secondary text-sm">
          <strong>{selected.size}</strong> baris dipilih
          <button className="btn sm danger" onClick={() => confirmAction({ title: 'Hapus terpilih?', message: `${selected.size} Work Order akan dihapus permanen.`, variant: 'danger', confirmLabel: 'Hapus', onConfirm: async () => { await Promise.all([...selected].map((id) => deleteMfgWorkOrder(id).catch(() => null))); notify(`${selected.size} dokumen dihapus`, 'success'); setSelected(new Set()); reload(); } })}>
            <Icon name="trash" size={12} /> Hapus
          </button>
          <button className="btn ghost sm" onClick={() => setSelected(new Set())}>Batal pilihan</button>
        </div>
      )}
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead style={{ width: 36 }} />
            <TableHead>No WO</TableHead>
            <TableHead>Tanggal</TableHead>
            <TableHead>BOM Ref</TableHead>
            <TableHead>Gudang Produksi</TableHead>
            <TableHead>Keterangan</TableHead>
            <TableHead>Status</TableHead>
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
                    <TableCell>{r.bom?.docNumber ?? '—'}</TableCell>
                    <TableCell>{r.warehouse?.name ?? '—'}</TableCell>
                    <TableCell>{r.description ?? '—'}</TableCell>
                    <TableCell>
                      <Badge variant={statusBadgeVariant(r.status)} dot>{statusLabel(r.status)}</Badge>
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
