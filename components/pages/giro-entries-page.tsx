'use client';

/**
 * Generic giro-entry transaction page (list §2.7 + master-detail form) shared by
 * the whole giro family — Receipt Giro (RG), Send Giro (SG), Receipt Giro Clearing
 * (RGC), Send Giro Clearing (SGC). One implementation, parameterized by
 * {@link GiroPageConfig}; each menu item is a thin wrapper supplying its base path,
 * transaction code (Form Builder + Kustomisasi Grid key) and kind×type. URL-driven
 * list↔form via trx sub-routes (§2.3.1): <base> · <base>/new · <base>/:id.
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
import { giroWorkflowActions } from '@/lib/fin-giro-workflow';
import { trxNewRoute, trxEditRoute, type TrxFormPageProps } from '@/lib/trx-route';
import { useAllowedCreationStatuses } from '@/lib/use-allowed-creation-statuses';
import { useErpList } from '@/lib/use-erp-list';
import { useListPagination } from '@/lib/use-list-pagination';
import { formatNumber } from '@/lib/format';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import type { ErpDocumentStatus } from '@/lib/api/fin-journal-entries';
import {
  listGiroEntries,
  createGiroEntry,
  updateGiroEntry,
  deleteGiroEntry,
  getGiroEntry,
  transitionGiroEntry,
  type GiroTransition,
  type ErpGiroEntry,
  type GiroKind,
  type GiroType,
} from '@/lib/api/fin-giro-entries';
import { GiroTransactionForm } from './giro-transaction-form';
import {
  defaultGiroForm,
  fromGiroEntry,
  toGiroPayload,
  type GiroFormData,
} from './giro-form-model';

/** Per-giro-type wiring — the only thing that differs between RG/SG/RGC/SGC. */
export interface GiroPageConfig {
  /** Trx base path, e.g. '/finance/receipt-giros'. */
  base: string;
  /** Form Builder + Kustomisasi Grid code, e.g. 'FIN.RG'. */
  code: string;
  /** Scopes the list + new records (REGISTER|CLEAR). */
  kind: GiroKind;
  /** Scopes the list + new records (INCOMING|OUTGOING). */
  type: GiroType;
  /** Document type for allowed-creation-status lookup, e.g. 'INCOMING_GIRO'. */
  documentType: string;
  /** Indonesian module label, e.g. 'Giro Masuk'. */
  title: string;
  /** Short doc tag shown in the header, e.g. 'RG'. */
  codeTag: string;
}

const STATUSES: ErpDocumentStatus[] = ['DRAFT', 'NEED_APPROVE', 'APPROVED', 'REJECTED', 'POSTED'];

const giroTotal = (r: ErpGiroEntry): number => {
  const rows = r.kind === 'REGISTER' ? r.registeredGiros : r.clearedGiros;
  return rows.reduce((s, g) => s + Number(g.amount || 0), 0);
};

export function GiroEntriesPage({
  config,
  formMode,
  recordId,
  onNavigate,
}: { config: GiroPageConfig } & TrxFormPageProps) {
  const { base, code, kind, type, documentType, title, codeTag } = config;
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const [form, setForm] = React.useState<GiroFormData>(() => defaultGiroForm(kind, type));
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
      listGiroEntries({
        page,
        limit: pageSize,
        search: debouncedSearch || undefined,
        kind,
        type,
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
  const openEdit = (r: ErpGiroEntry) => onNavigate?.(trxEditRoute(base, r.id));

  const loadForm = React.useCallback(() => {
    if (formMode === 'create') {
      setForm(defaultGiroForm(kind, type));
      return undefined;
    }
    if (formMode === 'edit' && recordId) {
      let alive = true;
      getGiroEntry(recordId)
        .then((full) => alive && setForm(fromGiroEntry(full)))
        .catch(() => {
          if (!alive) return;
          notify(`Gagal memuat ${title}`, 'danger');
          goList();
        });
      return () => { alive = false; };
    }
    return undefined;
  }, [formMode, recordId, goList, kind, type, title]);
  React.useEffect(() => loadForm(), [loadForm]);

  const persist = async (closeAfter: boolean, newAfter = false) => {
    if (!form.branchId || !form.entryDate) {
      notify('Cabang dan Tanggal wajib diisi.', 'warn');
      return;
    }
    if (kind === 'CLEAR' && !form.bankAccountId) {
      notify('Bank pencairan wajib diisi untuk kliring.', 'warn');
      return;
    }
    setSaving(true);
    try {
      const payload = toGiroPayload(form);
      if (form.id) {
        await updateGiroEntry(form.id, payload);
        notify(`${title} diperbarui`, 'success');
      } else {
        await createGiroEntry(payload);
        notify(`${title} dibuat`, 'success');
      }
      reload();
      if (newAfter) {
        setForm(defaultGiroForm(kind, type));
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

  const runTransition = async (r: ErpGiroEntry, action: GiroTransition) => {
    let reason: string | undefined;
    if (action === 'REJECT') {
      reason = window.prompt('Alasan menolak dokumen ini?') ?? undefined;
      if (!reason) return;
    }
    const verb: Record<GiroTransition, string> = {
      SUBMIT: 'mengajukan', APPROVE: 'menyetujui', REJECT: 'menolak',
      POST: 'memposting', REOPEN: 'membuka kembali',
    };
    try {
      await transitionGiroEntry(r.id, action, reason);
      notify(`Berhasil ${verb[action]} ${r.docNumber}`, 'success');
      reload();
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal', 'danger');
    }
  };

  const handleDelete = (r: ErpGiroEntry) => {
    confirmAction({
      title: `Hapus ${title}?`,
      message: `${r.docNumber} akan dihapus permanen.`,
      variant: 'danger',
      confirmLabel: 'Hapus',
      confirmIcon: 'trash',
      onConfirm: async () => {
        try {
          await deleteGiroEntry(r.id);
          notify(`${title} dihapus`, 'success');
          reload();
        } catch (e: unknown) {
          notify(e instanceof Error ? e.message : 'Gagal', 'danger');
        }
      },
    });
  };

  const rowActions = (r: ErpGiroEntry): RowActionItem[] => [
    { label: 'Edit / Lihat', onSelect: () => openEdit(r) },
    ...giroWorkflowActions(r.status, (a) => runTransition(r, a)),
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
            <GiroTransactionForm
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
                  await Promise.all([...selected].map((id) => deleteGiroEntry(id).catch(() => null)));
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
            <TableHead>Partner / Uraian</TableHead>
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
                    <TableCell>{r.description || '—'}</TableCell>
                    <TableCell className="tabular-nums" style={{ textAlign: 'right' }}>
                      {formatNumber(giroTotal(r), 2)}
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
