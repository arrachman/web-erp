'use client';

/**
 * AR Collection (IC) — list + form.
 * URL: /sales/ar-collections · /new · /:id
 * Backend: GET/POST /erp/sls/ar-collections (source='IC')
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Icon } from '@/components/ui/icons';
import { DateInput } from '@/components/ui/date-input';
import {
  ErpListLayout,
  type ListPaginationConfig,
  type SummaryConfig,
} from '@/components/organisms/erp-list-layout';
import {
  Table, TableHeader, TableBody, TableRow, TableHead, TableCell, TableEmpty, CodeLinkCell,
} from '@/components/organisms/table';
import {
  RowActionsMenu, RowContextMenu, type RowActionItem,
} from '@/components/molecules/row-actions-menu';
import { FormFieldRow } from '@/components/molecules/form-field-row';
import { SearchSelect } from '@/components/molecules/search-select';
import { confirmAction, notify } from '@/lib/feedback';
import { cashBankWorkflowActions } from '@/lib/fin-cash-bank-workflow';
import { type TrxFormPageProps, trxNewRoute, trxEditRoute } from '@/lib/trx-route';
import { useErpList } from '@/lib/use-erp-list';
import { useListPagination } from '@/lib/use-list-pagination';
import { formatNumber } from '@/lib/format';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import { loadBranchOptions, loadCurrencyOptions } from '@/components/pages/items-form-lookups';
import { loadCustomerOptions } from '@/components/pages/sls-form-lookups';
import {
  listArCollections,
  getArCollection,
  createArCollection,
  updateArCollection,
  deleteArCollection,
  transitionArCollection,
  type ErpArCollection,
  type ErpDocumentStatus,
  type ArCollectionTransition,
} from '@/lib/api/sls-ar-collections';

const BASE = '/sales/ar-collections';
const todayIso = () => new Date().toISOString().slice(0, 10);

interface FormState {
  docNumber: string;
  auto: boolean;
  transactionDate: string;
  branchId: string;
  branchLabel?: string;
  partnerId: string;
  partnerLabel?: string;
  description: string;
  currencyId: string;
  currencyLabel?: string;
  exchangeRate: string;
  amount: string;
  notes: string;
}

const defaultForm = (): FormState => ({
  docNumber: '',
  auto: true,
  transactionDate: todayIso(),
  branchId: '',
  partnerId: '',
  description: '',
  currencyId: '',
  exchangeRate: '1',
  amount: '0',
  notes: '',
});

function fromRecord(r: ErpArCollection): FormState {
  return {
    docNumber: r.docNumber,
    auto: false,
    transactionDate: r.transactionDate.slice(0, 10),
    branchId: r.branchId,
    branchLabel: undefined,
    partnerId: r.partner?.id ?? '',
    partnerLabel: r.partner?.name,
    description: r.description,
    currencyId: r.currencyId,
    exchangeRate: r.exchangeRate,
    amount: r.amount,
    notes: r.notes ?? '',
  };
}

export function ErpSlsArCollectionsPage({ formMode, recordId, onNavigate }: TrxFormPageProps = {}) {
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const goList = React.useCallback(() => onNavigate?.(BASE), [onNavigate]);

  // ── form state ───────────────────────────────────────────────────────────────
  const [record, setRecord] = React.useState<ErpArCollection | null>(null);
  const [form, setForm] = React.useState<FormState>(defaultForm());
  const [saving, setSaving] = React.useState(false);
  const set = (p: Partial<FormState>) => setForm((f) => ({ ...f, ...p }));

  React.useEffect(() => {
    if (formMode === 'edit' && recordId) {
      getArCollection(recordId)
        .then((r) => { setRecord(r); setForm(fromRecord(r)); })
        .catch(() => { notify('Gagal memuat data', 'danger'); goList(); });
    } else {
      setRecord(null);
      setForm(defaultForm());
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formMode, recordId]);

  const docStatus = (record?.status ?? 'DRAFT') as ErpDocumentStatus;
  const locked = docStatus !== 'DRAFT';

  const persist = async (closeAfter: boolean) => {
    if (!form.branchId || !form.transactionDate || !form.partnerId) {
      notify('Cabang, Tanggal, dan Customer wajib diisi.', 'warn');
      return;
    }
    setSaving(true);
    try {
      const payload = {
        docNumber: form.auto ? '' : form.docNumber,
        transactionDate: form.transactionDate,
        fiscalPeriodId: '1',
        branchId: form.branchId,
        partnerId: form.partnerId,
        description: form.description,
        currencyId: form.currencyId || '1',
        exchangeRate: form.exchangeRate || '1',
        amount: form.amount || '0',
        notes: form.notes || undefined,
      };
      if (record) {
        const updated = await updateArCollection(record.id, payload);
        setRecord(updated);
        notify('Penagihan Piutang diperbarui', 'success');
      } else {
        await createArCollection(payload);
        notify('Penagihan Piutang dibuat', 'success');
        if (closeAfter) goList();
      }
      if (record && closeAfter) goList();
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const runTransition = async (action: ArCollectionTransition) => {
    if (!record) return;
    try {
      const updated = await transitionArCollection(record.id, action);
      setRecord(updated);
      notify(`Dokumen berhasil: ${action.toLowerCase()}`, 'success');
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal', 'danger');
    }
  };

  // ── list state ───────────────────────────────────────────────────────────────
  const [search, setSearch] = React.useState('');
  const [statusFilter, setStatusFilter] = React.useState('');
  const { page, pageSize, setPage, setPageSize } = useListPagination('sls-ar-collections');
  const [debouncedSearch, setDebouncedSearch] = React.useState(search);
  React.useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(search), 300);
    return () => clearTimeout(t);
  }, [search]);

  const { rows, meta, loading, error, reload } = useErpList(
    () => listArCollections({
      page, limit: pageSize,
      search: debouncedSearch || undefined,
      status: (statusFilter || undefined) as ErpDocumentStatus | undefined,
      sortBy: 'transactionDate', sortDir: 'desc',
    }),
    [page, pageSize, debouncedSearch, statusFilter],
  );
  React.useEffect(() => { setPage(1); }, [debouncedSearch, statusFilter, pageSize]);

  const [focused, setFocused] = React.useState(-1);
  const [selected, setSelected] = React.useState<Set<string>>(new Set());
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  const openEdit = (r: ErpArCollection) => onNavigate?.(trxEditRoute(BASE, r.id));

  const rowActions = (r: ErpArCollection): RowActionItem[] => [
    { label: 'Edit / Lihat', onSelect: () => openEdit(r) },
    ...cashBankWorkflowActions(r.status, (a) => runTransition(a as ArCollectionTransition)),
    {
      label: 'Hapus', danger: true, separatorBefore: true,
      onSelect: () => confirmAction({
        title: 'Hapus Penagihan Piutang?',
        message: `${r.docNumber} akan dihapus permanen.`,
        variant: 'danger',
        confirmLabel: 'Hapus',
        confirmIcon: 'trash',
        onConfirm: async () => {
          try {
            await deleteArCollection(r.id);
            notify('Penagihan Piutang dihapus', 'success');
            reload();
          } catch (e: unknown) {
            notify(e instanceof Error ? e.message : 'Gagal', 'danger');
          }
        },
      }),
    },
  ];

  const toggleSel = (id: string) =>
    setSelected((s) => { const n = new Set(s); n.has(id) ? n.delete(id) : n.add(id); return n; });

  // ── form view ────────────────────────────────────────────────────────────────
  if (mode === 'form') {
    return (
      <div className="page">
        <div className="page-header">
          <h1 className="page-title flex items-center gap-2">
            <button className="iconbtn" onClick={goList} title="Kembali" style={{ fontSize: 18, lineHeight: 1 }}>←</button>
            Penagihan Piutang <span className="code-tag">IC</span>
          </h1>
        </div>
        <div className="page-body overflow-auto p-4">
          <div className="po-form flex flex-col gap-4">
            {/* Toolbar */}
            <div className="flex items-center gap-2 flex-wrap">
              <button type="button" className="btn primary" onClick={() => persist(true)} disabled={saving || locked}>
                <Icon name="save" size={13} /> Simpan
              </button>
              {!record && (
                <button type="button" className="btn" onClick={() => persist(false)} disabled={saving}>
                  Simpan &amp; Lanjut Edit
                </button>
              )}
              {record && docStatus === 'DRAFT' && (
                <button type="button" className="btn" onClick={() => runTransition('SUBMIT')} disabled={saving}>
                  <Icon name="check" size={13} /> Ajukan
                </button>
              )}
              <button type="button" className="btn ghost" onClick={goList} disabled={saving}>Batal</button>
              <div className="flex-1" />
              <Badge variant={statusBadgeVariant(docStatus)} dot>{statusLabel(docStatus)}</Badge>
            </div>

            {/* Header grid */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-x-6 gap-y-3 rounded-lg border border-border p-4">
              {/* LEFT — identitas */}
              <div className="flex flex-col gap-3">
                <FormFieldRow label="Customer" required>
                  <SearchSelect placeholder="Pilih customer…" value={form.partnerId}
                    initialLabel={form.partnerLabel} disabled={locked}
                    loadOptions={loadCustomerOptions}
                    onValueChange={(v) => set({ partnerId: v })}
                    onPick={(o) => set({ partnerId: o.value, partnerLabel: o.label })} />
                </FormFieldRow>
                <FormFieldRow label="Uraian">
                  <Input value={form.description} placeholder="Keterangan…" disabled={locked}
                    onChange={(e) => set({ description: e.target.value })} />
                </FormFieldRow>
                <FormFieldRow label="Catatan">
                  <Input value={form.notes} placeholder="Catatan tambahan…" disabled={locked}
                    onChange={(e) => set({ notes: e.target.value })} />
                </FormFieldRow>
              </div>

              {/* CENTER — dimensi */}
              <div className="flex flex-col gap-3">
                <FormFieldRow label="Cabang" required>
                  <SearchSelect placeholder="Pilih cabang…" value={form.branchId}
                    initialLabel={form.branchLabel} disabled={locked}
                    loadOptions={loadBranchOptions}
                    onValueChange={(v) => set({ branchId: v })}
                    onPick={(o) => set({ branchId: o.value, branchLabel: o.label })} />
                </FormFieldRow>
              </div>

              {/* RIGHT — tanggal & keuangan */}
              <div className="flex flex-col gap-3">
                <FormFieldRow label="Tanggal" required>
                  <DateInput value={form.transactionDate} disabled={locked}
                    onChange={(v) => set({ transactionDate: v ?? '' })} />
                </FormFieldRow>
                <FormFieldRow label="No Transaksi">
                  <div className="flex items-center gap-2">
                    <Input value={form.docNumber} placeholder="Auto" disabled={locked || form.auto}
                      onChange={(e) => set({ docNumber: e.target.value })} />
                    <label className="flex items-center gap-1 text-xs cursor-pointer">
                      <input type="checkbox" checked={form.auto} disabled={locked}
                        onChange={(e) => set({ auto: e.target.checked })} />
                      Auto
                    </label>
                  </div>
                </FormFieldRow>
                <FormFieldRow label="Mata Uang">
                  <SearchSelect placeholder="Pilih mata uang…" value={form.currencyId}
                    initialLabel={form.currencyLabel} disabled={locked}
                    loadOptions={loadCurrencyOptions}
                    onValueChange={(v) => set({ currencyId: v })}
                    onPick={(o) => set({ currencyId: o.value, currencyLabel: o.label })} />
                </FormFieldRow>
                <FormFieldRow label="Jumlah Tagih" required>
                  <Input type="text" value={form.amount} disabled={locked}
                    onChange={(e) => set({ amount: e.target.value })}
                    className="tabular-nums text-right" />
                </FormFieldRow>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // ── list view ────────────────────────────────────────────────────────────────
  const summary: SummaryConfig = { metricLabel: 'Σ Penagihan Piutang', rowCount: rows.length, totalCount: totalRows };
  const pagination: ListPaginationConfig = { page, pageCount, pageSize, totalRows, onPage: setPage, onPageSize: setPageSize };

  return (
    <ErpListLayout title="Penagihan Piutang (IC)" code="IC" loading={loading} error={error}
      search={search} onSearch={setSearch} onAdd={() => onNavigate?.(trxNewRoute(BASE))} onRefresh={reload}
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
            <TableHead>No Transaksi</TableHead>
            <TableHead>Tanggal</TableHead>
            <TableHead>Customer</TableHead>
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
