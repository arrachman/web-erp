'use client';

/**
 * Generic journal-entry transaction page (list §2.7 + master-detail form) shared by
 * the whole journal family — General (GJ), Adjustment (AJ), Memorial (JM), Opening
 * Balance (BB) and Revaluation (RV). One implementation, parameterized by
 * {@link JournalPageConfig}; each menu item is a thin wrapper that supplies its
 * base path, transaction code (Form Builder + Kustomisasi Grid key) and journalType.
 * URL-driven list↔form via trx sub-routes (§2.3.1): <base> · <base>/new · <base>/:id.
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
import { journalWorkflowActions } from '@/lib/fin-journal-workflow';
import { trxNewRoute, trxEditRoute, type TrxFormPageProps } from '@/lib/trx-route';
import { useAllowedCreationStatuses } from '@/lib/use-allowed-creation-statuses';
import { useErpList } from '@/lib/use-erp-list';
import { useListPagination } from '@/lib/use-list-pagination';
import { formatNumber } from '@/lib/format';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import {
  listJournalEntries,
  createJournalEntry,
  updateJournalEntry,
  deleteJournalEntry,
  getJournalEntry,
  transitionJournalEntry,
  type JournalTransition,
  type ErpJournalEntry,
  type ErpJournalType,
  type ErpDocumentStatus,
} from '@/lib/api/fin-journal-entries';
import { JournalTransactionForm } from './journal-transaction-form';
import {
  defaultJournalForm,
  fromJournalEntry,
  toJournalPayload,
  type JournalFormData,
} from './journal-form-model';

/** Per-journal-type wiring — the only thing that differs between GJ/AJ/JM/BB/RV. */
export interface JournalPageConfig {
  /** Trx base path, e.g. '/finance/general-journals'. */
  base: string;
  /** Form Builder + Kustomisasi Grid code, e.g. 'FIN.GJ'. */
  code: string;
  /** Scopes the list + new records, e.g. 'GENERAL'. */
  journalType: ErpJournalType;
  /** Document type for allowed-creation-status lookup, e.g. 'JOURNAL_ENTRY'. */
  documentType: string;
  /** Indonesian module label, e.g. 'Jurnal Umum'. */
  title: string;
  /** Short doc tag shown in the header, e.g. 'GJ'. */
  codeTag: string;
}

const STATUSES: ErpDocumentStatus[] = ['DRAFT', 'NEED_APPROVE', 'APPROVED', 'REJECTED', 'POSTED'];

const lineTotal = (r: ErpJournalEntry) => r.lines.reduce((s, l) => s + Number(l.debit || 0), 0);

export function JournalEntriesPage({
  config,
  formMode,
  recordId,
  onNavigate,
}: { config: JournalPageConfig } & TrxFormPageProps) {
  const { base, code, journalType, documentType, title, codeTag } = config;
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const [form, setForm] = React.useState<JournalFormData>(() => defaultJournalForm(journalType));
  const [saving, setSaving] = React.useState(false);
  const { statuses: allowedCreationStatuses } = useAllowedCreationStatuses(documentType);

  const formReady =
    formMode === 'create' ||
    (formMode === 'edit' && String(form.id ?? '') === String(recordId ?? ''));

  const goList = React.useCallback(() => onNavigate?.(base), [onNavigate, base]);

  const [search, setSearch] = React.useState('');
  const [statusFilter, setStatusFilter] = React.useState('');
  const { page, pageSize, setPage, setPageSize } = useListPagination(base);

  const [debouncedSearch, setDebouncedSearch] = React.useState(search);
  React.useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(search), 300);
    return () => clearTimeout(t);
  }, [search]);

  const { rows, meta, loading, error, reload } = useErpList(
    () =>
      listJournalEntries({
        page,
        limit: pageSize,
        search: debouncedSearch || undefined,
        journalType,
        status: (statusFilter || undefined) as ErpDocumentStatus | undefined,
        sortBy: 'entryDate',
        sortDir: 'desc',
      }),
    [page, pageSize, debouncedSearch, statusFilter],
  );

  React.useEffect(() => {
    setPage(1);
  }, [debouncedSearch, statusFilter, pageSize]);

  const [focused, setFocused] = React.useState(-1);
  const [selected, setSelected] = React.useState<Set<string>>(new Set());
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  const openCreate = () => onNavigate?.(trxNewRoute(base));
  const openEdit = (r: ErpJournalEntry) => onNavigate?.(trxEditRoute(base, r.id));

  const loadForm = React.useCallback(() => {
    if (formMode === 'create') {
      setForm(defaultJournalForm(journalType));
      return undefined;
    }
    if (formMode === 'edit' && recordId) {
      let alive = true;
      getJournalEntry(recordId)
        .then((full) => alive && setForm(fromJournalEntry(full)))
        .catch(() => {
          if (!alive) return;
          notify(`Gagal memuat ${title}`, 'danger');
          goList();
        });
      return () => { alive = false; };
    }
    return undefined;
  }, [formMode, recordId, goList, journalType, title]);
  React.useEffect(() => loadForm(), [loadForm]);

  const persist = async (closeAfter: boolean, newAfter = false) => {
    if (!form.branchId || !form.entryDate || !form.description) {
      notify('Cabang, Tanggal, dan Uraian wajib diisi.', 'warn');
      return;
    }
    setSaving(true);
    try {
      const payload = toJournalPayload(form);
      if (form.id) {
        await updateJournalEntry(form.id, payload);
        notify(`${title} diperbarui`, 'success');
      } else {
        await createJournalEntry(payload);
        notify(`${title} dibuat`, 'success');
      }
      reload();
      if (newAfter) {
        setForm(defaultJournalForm(journalType));
        onNavigate?.(trxNewRoute(base));
      } else if (closeAfter) {
        goList();
      }
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const runTransition = async (r: ErpJournalEntry, action: JournalTransition) => {
    let reason: string | undefined;
    if (action === 'REJECT') {
      reason = window.prompt('Alasan menolak dokumen ini?') ?? undefined;
      if (!reason) return;
    }
    const verb: Record<JournalTransition, string> = {
      SUBMIT: 'mengajukan', APPROVE: 'menyetujui', REJECT: 'menolak',
      POST: 'memposting', REOPEN: 'membuka kembali',
    };
    try {
      await transitionJournalEntry(r.id, action, reason);
      notify(`Berhasil ${verb[action]} ${r.docNumber}`, 'success');
      reload();
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal', 'danger');
    }
  };

  const handleDelete = (r: ErpJournalEntry) => {
    confirmAction({
      title: `Hapus ${title}?`,
      message: `${r.docNumber} akan dihapus permanen.`,
      variant: 'danger',
      confirmLabel: 'Hapus',
      confirmIcon: 'trash',
      onConfirm: async () => {
        try {
          await deleteJournalEntry(r.id);
          notify(`${title} dihapus`, 'success');
          reload();
        } catch (e: unknown) {
          notify(e instanceof Error ? e.message : 'Gagal', 'danger');
        }
      },
    });
  };

  const rowActions = (r: ErpJournalEntry): RowActionItem[] => [
    { label: 'Edit / Lihat', onSelect: () => openEdit(r) },
    ...journalWorkflowActions(r.status, (a) => runTransition(r, a)),
    { label: 'Hapus', onSelect: () => handleDelete(r), danger: true, separatorBefore: true },
  ];

  // ── form view ───────────────────────────────────────────────────────────────
  if (mode === 'form') {
    return (
      <div className="page">
        <div className="page-header">
          <h1 className="page-title flex items-center gap-2">
            <button className="iconbtn" onClick={goList} title="Kembali" style={{ fontSize: 18, lineHeight: 1 }}>←</button>
            {title}
            <span className="code-tag">{codeTag}</span>
          </h1>
        </div>
        <div className="page-body overflow-auto p-4">
          {formReady ? (
            <JournalTransactionForm
              data={form}
              onChange={setForm}
              transactionCode={code}
              saving={saving}
              allowedCreationStatuses={formMode === 'create' ? allowedCreationStatuses : undefined}
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
  const summary: SummaryConfig = { metricLabel: `Σ ${title}`, rowCount: rows.length, totalCount: totalRows };
  const pagination: ListPaginationConfig = {
    page, pageCount, pageSize, totalRows, onPage: setPage, onPageSize: setPageSize,
  };
  const toggleSel = (id: string) =>
    setSelected((s) => {
      const n = new Set(s);
      n.has(id) ? n.delete(id) : n.add(id);
      return n;
    });

  return (
    <ErpListLayout
      title={title}
      code={codeTag}
      loading={loading}
      error={error}
      search={search}
      onSearch={setSearch}
      onAdd={openCreate}
      onRefresh={reload}
      toolbar={
        <div className="flex items-center gap-2">
          <select
            className="select sm"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
          >
            <option value="">Semua status</option>
            {STATUSES.map((s) => (
              <option key={s} value={s}>{statusLabel(s)}</option>
            ))}
          </select>
        </div>
      }
      summary={summary}
      pagination={pagination}
      keyboardRows={{
        rowCount: rows.length,
        focusedIndex: focused,
        onFocusChange: setFocused,
        onToggle: (i) => rows[i] && toggleSel(rows[i].id),
        onOpen: (i) => rows[i] && openEdit(rows[i]),
      }}
    >
      {selected.size > 0 && (
        <div className="bulk-bar flex items-center gap-3 px-3 py-2 mb-2 rounded-md bg-secondary text-sm">
          <strong>{selected.size}</strong> baris dipilih
          <button
            className="btn sm danger"
            onClick={() =>
              confirmAction({
                title: 'Hapus terpilih?',
                message: `${selected.size} ${title} akan dihapus permanen.`,
                variant: 'danger',
                confirmLabel: 'Hapus',
                onConfirm: async () => {
                  await Promise.all([...selected].map((id) => deleteJournalEntry(id).catch(() => null)));
                  notify(`${selected.size} dokumen dihapus`, 'success');
                  setSelected(new Set());
                  reload();
                },
              })
            }
          >
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
            <TableHead>Uraian</TableHead>
            <TableHead style={{ textAlign: 'right' }}>Total</TableHead>
            <TableHead>Status</TableHead>
            <TableHead style={{ width: 44 }} />
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.length === 0 ? (
            <TableEmpty colSpan={7} />
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
                    <TableCell>{r.entryDate.slice(0, 10)}</TableCell>
                    <TableCell>{r.description}</TableCell>
                    <TableCell className="tabular-nums" style={{ textAlign: 'right' }}>
                      {formatNumber(lineTotal(r), 2)}
                    </TableCell>
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
