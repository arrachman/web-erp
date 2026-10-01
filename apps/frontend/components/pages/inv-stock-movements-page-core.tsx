'use client';

/**
 * Shared list (§2.7) + master-detail form for inventory stock movements. One
 * page component parameterized by `{ movementType, transactionCode, base, title,
 * code }`; the four movement faces (MR/TS/RS/RF) are thin wrappers in
 * inv-stock-movements-page.tsx. Atomic tier: Page.
 * URL-driven list↔form via trx sub-routes (§2.3.1): <base> · /new · /:id.
 * Route = SSOT, no internal mode state.
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
  InvMovementFilters,
  emptyInvMovementFilters,
  type InvMovementFilters as InvFilters,
} from './inv-stock-movements-filters';
import {
  RowActionsMenu,
  RowContextMenu,
  type RowActionItem,
} from '@/components/molecules/row-actions-menu';
import { confirmAction, notify } from '@/lib/feedback';
import { invStockMovementWorkflowActions } from '@/lib/inv-stock-movement-workflow';
import {
  trxNewRoute,
  trxEditRoute,
  type TrxFormPageProps,
} from '@/lib/trx-route';
import { useErpList } from '@/lib/use-erp-list';
import { useListPagination } from '@/lib/use-list-pagination';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import {
  listInvStockMovements,
  createInvStockMovement,
  updateInvStockMovement,
  deleteInvStockMovement,
  getInvStockMovement,
  transitionInvStockMovement,
  type InvStockMovementTransition,
  type ErpInvMovementType,
  type ErpInvStockMovement,
  type ErpDocumentStatus,
} from '@/lib/api/inv-stock-movements';
import { InvStockMovementForm } from './inv-stock-movement-form';
import {
  defaultInvStockMovementForm,
  fromInvStockMovement,
  toInvStockMovementPayload,
  type InvStockMovementFormData,
} from './inv-stock-movement-form-model';

/** Per-movement-face configuration injected into the shared page. */
export interface InvStockMovementPageConfig {
  movementType: ErpInvMovementType;
  /** Form Builder / Kustomisasi Grid code, e.g. "INV.MR". */
  transactionCode: string;
  /** Canonical list path (seeded `sys_menus.path`); base for /new and /:id. */
  base: string;
  /** Page + breadcrumb title, e.g. "Material Request". */
  title: string;
  /** Short code tag shown beside the title (e.g. "MR"). */
  code: string;
}

const VERB: Record<InvStockMovementTransition, string> = {
  SUBMIT: 'mengajukan',
  APPROVE: 'menyetujui',
  REJECT: 'menolak',
  POST: 'memposting',
  REOPEN: 'membuka kembali',
};

export function InvStockMovementsPage(
  cfg: InvStockMovementPageConfig,
  { formMode, recordId, onNavigate }: TrxFormPageProps = {},
) {
  const { movementType, transactionCode, base, title, code } = cfg;
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const [form, setForm] = React.useState<InvStockMovementFormData>(() =>
    defaultInvStockMovementForm(movementType),
  );
  const [saving, setSaving] = React.useState(false);

  const formReady =
    formMode === 'create' ||
    (formMode === 'edit' && String(form.id ?? '') === String(recordId ?? ''));

  const goList = React.useCallback(() => onNavigate?.(base), [onNavigate, base]);

  const [search, setSearch] = React.useState('');
  const [filters, setFilters] = React.useState<InvFilters>(emptyInvMovementFilters);
  const { page, pageSize, setPage, setPageSize } = useListPagination(`inv-movements-${transactionCode}`);

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
      listInvStockMovements({
        movementType,
        page,
        limit: pageSize,
        search: debouncedSearch || undefined,
        status: (debF.status || undefined) as ErpDocumentStatus | undefined,
        dateFrom: debF.dateFrom || undefined,
        dateTo: debF.dateTo || undefined,
        sortBy: 'movementDate',
        sortDir: 'desc',
      }),
    [movementType, page, pageSize, debouncedSearch, debF],
  );

  React.useEffect(() => {
    setPage(1);
  }, [debouncedSearch, debF, pageSize]);

  const [focused, setFocused] = React.useState(-1);
  const [selected, setSelected] = React.useState<Set<string>>(new Set());
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  const openCreate = () => onNavigate?.(trxNewRoute(base));
  const openEdit = (r: ErpInvStockMovement) => onNavigate?.(trxEditRoute(base, r.id));

  const loadForm = React.useCallback(() => {
    if (formMode === 'create') {
      setForm(defaultInvStockMovementForm(movementType));
      return undefined;
    }
    if (formMode === 'edit' && recordId) {
      let alive = true;
      getInvStockMovement(recordId)
        .then((full) => alive && setForm(fromInvStockMovement(full)))
        .catch(() => {
          if (!alive) return;
          notify(`Gagal memuat ${title}`, 'danger');
          goList();
        });
      return () => {
        alive = false;
      };
    }
    return undefined;
  }, [formMode, recordId, goList, movementType, title]);
  React.useEffect(() => loadForm(), [loadForm]);

  const persist = async (closeAfter: boolean, newAfter = false) => {
    if (!form.branchId || !form.movementDate) {
      notify('Cabang dan Tanggal wajib diisi.', 'warn');
      return;
    }
    if (!form.lines.some((l) => l.itemId && Number(l.quantity) > 0)) {
      notify('Minimal satu baris item dengan qty > 0.', 'warn');
      return;
    }
    setSaving(true);
    try {
      const payload = toInvStockMovementPayload(form);
      if (form.id) {
        await updateInvStockMovement(form.id, payload);
        notify(`${title} diperbarui`, 'success');
      } else {
        await createInvStockMovement(payload);
        notify(`${title} dibuat`, 'success');
      }
      reload();
      if (newAfter) {
        setForm(defaultInvStockMovementForm(movementType));
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

  const runTransition = async (r: ErpInvStockMovement, action: InvStockMovementTransition) => {
    let reason: string | undefined;
    if (action === 'REJECT') {
      reason = window.prompt('Alasan menolak dokumen ini?') ?? undefined;
      if (!reason) return;
    }
    try {
      await transitionInvStockMovement(r.id, action, reason);
      notify(`Berhasil ${VERB[action]} ${r.docNumber}`, 'success');
      reload();
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal', 'danger');
    }
  };

  const handleDelete = (r: ErpInvStockMovement) => {
    confirmAction({
      title: `Hapus ${title}?`,
      message: `${r.docNumber} akan dihapus permanen.`,
      variant: 'danger',
      confirmLabel: 'Hapus',
      confirmIcon: 'trash',
      onConfirm: async () => {
        try {
          await deleteInvStockMovement(r.id);
          notify(`${title} dihapus`, 'success');
          reload();
        } catch (e: unknown) {
          notify(e instanceof Error ? e.message : 'Gagal', 'danger');
        }
      },
    });
  };

  const rowActions = (r: ErpInvStockMovement): RowActionItem[] => [
    { label: 'Edit / Lihat', onSelect: () => openEdit(r) },
    ...invStockMovementWorkflowActions(r.status, (a) => runTransition(r, a)),
    { label: 'Hapus', onSelect: () => handleDelete(r), danger: true, separatorBefore: true },
  ];

  // ── form view ───────────────────────────────────────────────────────────────
  if (mode === 'form') {
    return (
      <div className="page">
        <div className="page-header">
          <h1 className="page-title flex items-center gap-2">
            <button className="iconbtn" onClick={goList} title="Kembali" style={{ fontSize: 18, lineHeight: 1 }}>
              ←
            </button>
            {title}
            <span className="code-tag">{code}</span>
          </h1>
        </div>
        <div className="page-body overflow-auto p-4">
          {formReady ? (
            <InvStockMovementForm
              data={form}
              onChange={setForm}
              transactionCode={transactionCode}
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
  const summary: SummaryConfig = { metricLabel: `Σ ${title}`, rowCount: rows.length, totalCount: totalRows };
  const pagination: ListPaginationConfig = {
    page,
    pageCount,
    pageSize,
    totalRows,
    onPage: setPage,
    onPageSize: setPageSize,
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
      code={code}
      loading={loading}
      error={error}
      search={search}
      onSearch={setSearch}
      onAdd={openCreate}
      onRefresh={reload}
      toolbar={<InvMovementFilters value={filters} onChange={setFilters} />}
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
                  await Promise.all([...selected].map((id) => deleteInvStockMovement(id).catch(() => null)));
                  notify(`${selected.size} dokumen dihapus`, 'success');
                  setSelected(new Set());
                  reload();
                },
              })
            }
          >
            <Icon name="trash" size={12} /> Hapus
          </button>
          <button className="btn ghost sm" onClick={() => setSelected(new Set())}>
            Batal pilihan
          </button>
        </div>
      )}
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead style={{ width: 36 }} />
            <TableHead>No Transaksi</TableHead>
            <TableHead>Tanggal</TableHead>
            <TableHead>Gudang Asal</TableHead>
            <TableHead>Gudang Tujuan</TableHead>
            <TableHead>Uraian</TableHead>
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
                    <TableCell>{r.movementDate.slice(0, 10)}</TableCell>
                    <TableCell>{r.sourceWarehouse?.name ?? '—'}</TableCell>
                    <TableCell>{r.destinationWarehouse?.name ?? '—'}</TableCell>
                    <TableCell>{r.description ?? '—'}</TableCell>
                    <TableCell>
                      <Badge variant={statusBadgeVariant(r.status)} dot>
                        {statusLabel(r.status)}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <RowActionsMenu items={actions} />
                    </TableCell>
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
