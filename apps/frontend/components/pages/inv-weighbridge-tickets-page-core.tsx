'use client';

/**
 * Shared list (§2.7) + master-detail form for inventory weighbridge tickets
 * (Receipt Weigher). URL-driven list↔form via trx sub-routes (§2.3.1):
 * <base> · /new · /:id. Full workflow (SUBMIT/APPROVE/REJECT/POST/REOPEN).
 * Atomic tier: Page.
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
  Table, TableHeader, TableBody, TableRow,
  TableHead, TableCell, TableEmpty, CodeLinkCell,
} from '@/components/organisms/table';
import {
  InvWeighbridgeFiltersBar,
  emptyInvWeighbridgeFilters,
  type InvWeighbridgeFilters,
} from './inv-weighbridge-tickets-filters';
import {
  RowActionsMenu, RowContextMenu, type RowActionItem,
} from '@/components/molecules/row-actions-menu';
import { confirmAction, notify } from '@/lib/feedback';
import { invWeighbridgeTicketWorkflowActions } from '@/lib/inv-weighbridge-ticket-workflow';
import { trxNewRoute, trxEditRoute, type TrxFormPageProps } from '@/lib/trx-route';
import { useErpList } from '@/lib/use-erp-list';
import { useListPagination } from '@/lib/use-list-pagination';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import {
  listInvWeighbridgeTickets, createInvWeighbridgeTicket, updateInvWeighbridgeTicket,
  deleteInvWeighbridgeTicket, getInvWeighbridgeTicket, transitionInvWeighbridgeTicket,
  type InvWeighbridgeTicketTransition, type ErpInvWeighbridgeTicket,
  type ErpDocumentStatus,
} from '@/lib/api/inv-weighbridge-tickets';
import { InvWeighbridgeTicketForm } from './inv-weighbridge-ticket-form';
import {
  defaultInvWeighbridgeTicketForm, fromInvWeighbridgeTicket,
  toInvWeighbridgeTicketPayload, type InvWeighbridgeTicketFormData,
} from './inv-weighbridge-ticket-form-model';

export interface InvWeighbridgeTicketPageConfig {
  transactionCode: string;
  base: string;
  title: string;
  code: string;
}

const VERB: Record<InvWeighbridgeTicketTransition, string> = {
  SUBMIT: 'mengajukan', APPROVE: 'menyetujui', REJECT: 'menolak',
  POST: 'memposting', REOPEN: 'membuka kembali',
};

export function InvWeighbridgeTicketsPage(
  cfg: InvWeighbridgeTicketPageConfig,
  { formMode, recordId, onNavigate }: TrxFormPageProps = {},
) {
  const { transactionCode, base, title, code } = cfg;
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const [form, setForm] = React.useState<InvWeighbridgeTicketFormData>(() => defaultInvWeighbridgeTicketForm());
  const [saving, setSaving] = React.useState(false);
  const formReady = formMode === 'create' || (formMode === 'edit' && String(form.id ?? '') === String(recordId ?? ''));
  const goList = React.useCallback(() => onNavigate?.(base), [onNavigate, base]);

  const [search, setSearch] = React.useState('');
  const [filters, setFilters] = React.useState<InvWeighbridgeFilters>(emptyInvWeighbridgeFilters);
  const { page, pageSize, setPage, setPageSize } = useListPagination(`inv-weighbridge-${transactionCode}`);
  const [debouncedSearch, setDebouncedSearch] = React.useState(search);
  const [debF, setDebF] = React.useState(filters);
  React.useEffect(() => { const t = setTimeout(() => setDebouncedSearch(search), 300); return () => clearTimeout(t); }, [search]);
  React.useEffect(() => { const t = setTimeout(() => setDebF(filters), 350); return () => clearTimeout(t); }, [filters]);

  const { rows, meta, loading, error, reload } = useErpList(
    () => listInvWeighbridgeTickets({
      page, limit: pageSize, search: debouncedSearch || undefined,
      status: (debF.status || undefined) as ErpDocumentStatus | undefined,
      dateFrom: debF.dateFrom || undefined, dateTo: debF.dateTo || undefined,
      sortBy: 'ticketDate', sortDir: 'desc',
    }),
    [page, pageSize, debouncedSearch, debF],
  );
  React.useEffect(() => { setPage(1); }, [debouncedSearch, debF, pageSize]);

  const [focused, setFocused] = React.useState(-1);
  const [selected, setSelected] = React.useState<Set<string>>(new Set());
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;
  const openCreate = () => onNavigate?.(trxNewRoute(base));
  const openEdit = (r: ErpInvWeighbridgeTicket) => onNavigate?.(trxEditRoute(base, r.id));
  const toggleSel = (id: string) =>
    setSelected((s) => { const n = new Set(s); n.has(id) ? n.delete(id) : n.add(id); return n; });

  const loadForm = React.useCallback(() => {
    if (formMode === 'create') { setForm(defaultInvWeighbridgeTicketForm()); return undefined; }
    if (formMode === 'edit' && recordId) {
      let alive = true;
      getInvWeighbridgeTicket(recordId)
        .then((full) => alive && setForm(fromInvWeighbridgeTicket(full)))
        .catch(() => { if (!alive) return; notify(`Gagal memuat ${title}`, 'danger'); goList(); });
      return () => { alive = false; };
    }
    return undefined;
  }, [formMode, recordId, goList, title]);
  React.useEffect(() => loadForm(), [loadForm]);

  const persist = async (closeAfter: boolean, newAfter = false) => {
    if (!form.branchId || !form.ticketDate) { notify('Cabang dan Tanggal wajib diisi.', 'warn'); return; }
    setSaving(true);
    try {
      const payload = toInvWeighbridgeTicketPayload(form);
      if (form.id) { await updateInvWeighbridgeTicket(form.id, payload); notify(`${title} diperbarui`, 'success'); }
      else { await createInvWeighbridgeTicket(payload); notify(`${title} dibuat`, 'success'); }
      reload();
      if (newAfter) { setForm(defaultInvWeighbridgeTicketForm()); onNavigate?.(trxNewRoute(base)); }
      else if (closeAfter) { goList(); }
    } catch (e: unknown) { notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger'); }
    finally { setSaving(false); }
  };

  const runTransition = async (r: ErpInvWeighbridgeTicket, action: InvWeighbridgeTicketTransition) => {
    let reason: string | undefined;
    if (action === 'REJECT') { reason = window.prompt('Alasan menolak dokumen ini?') ?? undefined; if (!reason) return; }
    try {
      await transitionInvWeighbridgeTicket(r.id, action, reason);
      notify(`Berhasil ${VERB[action]} ${r.docNumber}`, 'success'); reload();
    } catch (e: unknown) { notify(e instanceof Error ? e.message : 'Gagal', 'danger'); }
  };

  const handleDelete = (r: ErpInvWeighbridgeTicket) =>
    confirmAction({
      title: `Hapus ${title}?`, message: `${r.docNumber} akan dihapus permanen.`,
      variant: 'danger', confirmLabel: 'Hapus', confirmIcon: 'trash',
      onConfirm: async () => {
        try { await deleteInvWeighbridgeTicket(r.id); notify(`${title} dihapus`, 'success'); reload(); }
        catch (e: unknown) { notify(e instanceof Error ? e.message : 'Gagal', 'danger'); }
      },
    });

  const rowActions = (r: ErpInvWeighbridgeTicket): RowActionItem[] => [
    { label: 'Edit / Lihat', onSelect: () => openEdit(r) },
    ...invWeighbridgeTicketWorkflowActions(r.status, (a) => runTransition(r, a)),
    { label: 'Hapus', onSelect: () => handleDelete(r), danger: true, separatorBefore: true },
  ];

  // ── form view ────────────────────────────────────────────────────────────────
  if (mode === 'form') {
    const handleFormTransition = async (action: InvWeighbridgeTicketTransition) => {
      if (!form.id) return;
      await runTransition({ id: form.id, docNumber: form.docNumber } as ErpInvWeighbridgeTicket, action);
      loadForm();
    };
    return (
      <div className="page">
        <div className="page-header">
          <h1 className="page-title flex items-center gap-2">
            <button className="iconbtn" onClick={goList} title="Kembali" style={{ fontSize: 18, lineHeight: 1 }}>←</button>
            {title}<span className="code-tag">{code}</span>
          </h1>
        </div>
        <div className="page-body overflow-auto p-4">
          {formReady ? (
            <InvWeighbridgeTicketForm data={form} onChange={setForm} transactionCode={transactionCode}
              saving={saving} onSave={() => persist(true)} onSaveNew={() => persist(false, true)}
              onReset={loadForm} onTransition={form.id ? handleFormTransition : undefined} />
          ) : <div className="p-8 text-center text-muted">Memuat…</div>}
        </div>
      </div>
    );
  }

  // ── list view ─────────────────────────────────────────────────────────────────
  const summary: SummaryConfig = { metricLabel: `Σ ${title}`, rowCount: rows.length, totalCount: totalRows };
  const pagination: ListPaginationConfig = { page, pageCount, pageSize, totalRows, onPage: setPage, onPageSize: setPageSize };

  return (
    <ErpListLayout
      title={title} code={code} loading={loading} error={error}
      search={search} onSearch={setSearch} onAdd={openCreate} onRefresh={reload}
      toolbar={<InvWeighbridgeFiltersBar value={filters} onChange={setFilters} />}
      summary={summary} pagination={pagination}
      keyboardRows={{ rowCount: rows.length, focusedIndex: focused, onFocusChange: setFocused,
        onToggle: (i) => rows[i] && toggleSel(rows[i].id), onOpen: (i) => rows[i] && openEdit(rows[i]) }}
    >
      {selected.size > 0 && (
        <div className="bulk-bar flex items-center gap-3 px-3 py-2 mb-2 rounded-md bg-secondary text-sm">
          <strong>{selected.size}</strong> baris dipilih
          <button className="btn sm danger" onClick={() => confirmAction({
            title: 'Hapus terpilih?', message: `${selected.size} ${title} akan dihapus permanen.`,
            variant: 'danger', confirmLabel: 'Hapus',
            onConfirm: async () => {
              await Promise.all([...selected].map((id) => deleteInvWeighbridgeTicket(id).catch(() => null)));
              notify(`${selected.size} dokumen dihapus`, 'success'); setSelected(new Set()); reload();
            },
          })}><Icon name="trash" size={12} /> Hapus</button>
          <button className="btn ghost sm" onClick={() => setSelected(new Set())}>Batal pilihan</button>
        </div>
      )}
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead style={{ width: 36 }} />
            <TableHead>No Transaksi</TableHead>
            <TableHead>Tanggal</TableHead>
            <TableHead>Cabang</TableHead>
            <TableHead>Partner</TableHead>
            <TableHead>Kendaraan</TableHead>
            <TableHead style={{ textAlign: 'right' }}>Bruto</TableHead>
            <TableHead style={{ textAlign: 'right' }}>Netto</TableHead>
            <TableHead>Status</TableHead>
            <TableHead style={{ width: 44 }} />
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.length === 0 ? (
            <TableEmpty colSpan={10} />
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
                    <TableCell>{r.ticketDate.slice(0, 10)}</TableCell>
                    <TableCell>{r.branch?.name ?? '—'}</TableCell>
                    <TableCell>{r.partner?.name ?? '—'}</TableCell>
                    <TableCell>{r.vehiclePlate ?? '—'}</TableCell>
                    <TableCell style={{ textAlign: 'right' }} className="tabular-nums">{r.grossWeight}</TableCell>
                    <TableCell style={{ textAlign: 'right' }} className="tabular-nums">{r.netWeight}</TableCell>
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
