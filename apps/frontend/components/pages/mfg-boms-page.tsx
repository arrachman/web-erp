'use client';

/**
 * Bill of Materials (BOM) — list (§2.7) + master-detail form. Atomic tier: Page.
 * URL-driven list↔form via trx sub-routes (§2.3.1): /manufacturing/boms · /new · /:id.
 * Table extracted to mfg-boms-table.tsx (§3 400-line limit).
 */

import * as React from 'react';
import {
  ErpListLayout,
  type ListPaginationConfig,
  type SummaryConfig,
} from '@/components/organisms/erp-list-layout';
import {
  MfgBomFilters,
  emptyMfgBomFilters,
  type MfgBomFilters as MfgBomFiltersType,
} from './mfg-boms-filters';
import { MfgBomTable } from './mfg-boms-table';
import { type RowActionItem } from '@/components/molecules/row-actions-menu';
import { confirmAction, notify } from '@/lib/feedback';
import { trxNewRoute, trxEditRoute, type TrxFormPageProps } from '@/lib/trx-route';
import { useErpList } from '@/lib/use-erp-list';
import { useListPagination } from '@/lib/use-list-pagination';
import {
  listMfgBoms,
  createMfgBom,
  updateMfgBom,
  deleteMfgBom,
  getMfgBom,
  transitionMfgBom,
  type MfgBomTransition,
  type ErpMfgBom,
  type ErpDocumentStatus,
} from '@/lib/api/mfg-boms';
import {
  MfgBomForm,
  defaultMfgBomForm,
  fromMfgBom,
  toMfgBomPayload,
  type MfgBomFormData,
} from './mfg-bom-form';

const BOM_BASE = '/manufacturing/boms';

function bomWorkflowActions(
  status: ErpDocumentStatus,
  run: (a: MfgBomTransition) => void,
): RowActionItem[] {
  switch (status) {
    case 'DRAFT':
    case 'REJECTED':
      return [{ label: 'Ajukan', onSelect: () => run('SUBMIT') }];
    case 'NEED_APPROVE':
      return [
        { label: 'Setujui', onSelect: () => run('APPROVE') },
        { label: 'Tolak', onSelect: () => run('REJECT') },
      ];
    case 'APPROVED':
      return [{ label: 'Reopen', onSelect: () => run('REOPEN') }];
    default:
      return [];
  }
}

export function ErpMfgBomsPage({ formMode, recordId, onNavigate }: TrxFormPageProps = {}) {
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const [form, setForm] = React.useState<MfgBomFormData>(defaultMfgBomForm);
  const [saving, setSaving] = React.useState(false);

  const formReady =
    formMode === 'create' ||
    (formMode === 'edit' && String(form.id ?? '') === String(recordId ?? ''));

  const goList = React.useCallback(() => onNavigate?.(BOM_BASE), [onNavigate]);

  const [search, setSearch] = React.useState('');
  const [filters, setFilters] = React.useState<MfgBomFiltersType>(emptyMfgBomFilters);
  const { page, pageSize, setPage, setPageSize } = useListPagination('mfg-boms');

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
      listMfgBoms({
        page,
        limit: pageSize,
        search: debouncedSearch || undefined,
        status: (debF.status || undefined) as ErpDocumentStatus | undefined,
        dateFrom: debF.dateFrom || undefined,
        dateTo: debF.dateTo || undefined,
        sortBy: 'docDate',
        sortDir: 'desc',
      }),
    [page, pageSize, debouncedSearch, debF],
  );

  React.useEffect(() => {
    setPage(1);
  }, [debouncedSearch, debF, pageSize]);

  const [focused, setFocused] = React.useState(-1);
  const [selected, setSelected] = React.useState<Set<string>>(new Set());
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  const openCreate = () => onNavigate?.(trxNewRoute(BOM_BASE));
  const openEdit = (r: ErpMfgBom) => onNavigate?.(trxEditRoute(BOM_BASE, r.id));

  const loadForm = React.useCallback(() => {
    if (formMode === 'create') {
      setForm(defaultMfgBomForm());
      return undefined;
    }
    if (formMode === 'edit' && recordId) {
      let alive = true;
      getMfgBom(recordId)
        .then((full) => alive && setForm(fromMfgBom(full)))
        .catch(() => {
          if (!alive) return;
          notify('Gagal memuat Bill of Materials', 'danger');
          goList();
        });
      return () => { alive = false; };
    }
    return undefined;
  }, [formMode, recordId, goList]);
  React.useEffect(() => loadForm(), [loadForm]);

  const persist = async (closeAfter: boolean, newAfter = false) => {
    if (!form.branchId || !form.docDate || !form.currencyId) {
      notify('Cabang, Tanggal, dan Mata Uang wajib diisi.', 'warn');
      return;
    }
    setSaving(true);
    try {
      const payload = toMfgBomPayload(form);
      if (form.id) {
        await updateMfgBom(form.id, payload);
        notify('Bill of Materials diperbarui', 'success');
      } else {
        await createMfgBom(payload);
        notify('Bill of Materials dibuat', 'success');
      }
      reload();
      if (newAfter) {
        setForm(defaultMfgBomForm());
        onNavigate?.(trxNewRoute(BOM_BASE));
      } else if (closeAfter) {
        goList();
      }
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const runTransition = async (r: ErpMfgBom, action: MfgBomTransition) => {
    let reason: string | undefined;
    if (action === 'REJECT') {
      reason = window.prompt('Alasan menolak dokumen ini?') ?? undefined;
      if (!reason) return;
    }
    const verb: Record<MfgBomTransition, string> = {
      SUBMIT: 'mengajukan',
      APPROVE: 'menyetujui',
      REJECT: 'menolak',
      REOPEN: 'membuka kembali',
    };
    try {
      await transitionMfgBom(r.id, action, reason);
      notify(`Berhasil ${verb[action]} ${r.docNumber}`, 'success');
      reload();
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal', 'danger');
    }
  };

  const handleDelete = (r: ErpMfgBom) => {
    confirmAction({
      title: 'Hapus Bill of Materials?',
      message: `${r.docNumber} akan dihapus permanen.`,
      variant: 'danger',
      confirmLabel: 'Hapus',
      confirmIcon: 'trash',
      onConfirm: async () => {
        try {
          await deleteMfgBom(r.id);
          notify('Bill of Materials dihapus', 'success');
          reload();
        } catch (e: unknown) {
          notify(e instanceof Error ? e.message : 'Gagal', 'danger');
        }
      },
    });
  };

  const rowActions = (r: ErpMfgBom): RowActionItem[] => [
    { label: 'Edit / Lihat', onSelect: () => openEdit(r) },
    ...bomWorkflowActions(r.status, (a) => runTransition(r, a)),
    { label: 'Hapus', onSelect: () => handleDelete(r), danger: true, separatorBefore: true },
  ];

  const toggleSel = (id: string) =>
    setSelected((s) => {
      const n = new Set(s);
      n.has(id) ? n.delete(id) : n.add(id);
      return n;
    });

  // ── form view ──────────────────────────────────────────────────────────────
  if (mode === 'form') {
    return (
      <div className="page">
        <div className="page-header">
          <h1 className="page-title flex items-center gap-2">
            <button
              className="iconbtn"
              onClick={goList}
              title="Kembali"
              style={{ fontSize: 18, lineHeight: 1 }}
            >
              ←
            </button>
            Bill of Materials
            <span className="code-tag">BOM</span>
          </h1>
        </div>
        <div className="page-body overflow-auto p-4">
          {formReady ? (
            <MfgBomForm
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

  // ── list view ──────────────────────────────────────────────────────────────
  const summary: SummaryConfig = {
    metricLabel: 'Σ Bill of Materials',
    rowCount: rows.length,
    totalCount: totalRows,
  };
  const pagination: ListPaginationConfig = {
    page,
    pageCount,
    pageSize,
    totalRows,
    onPage: setPage,
    onPageSize: setPageSize,
  };

  return (
    <ErpListLayout
      title="Bill of Materials"
      code="BOM"
      loading={loading}
      error={error}
      search={search}
      onSearch={setSearch}
      onAdd={openCreate}
      onRefresh={reload}
      toolbar={<MfgBomFilters value={filters} onChange={setFilters} />}
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
      <MfgBomTable
        rows={rows}
        focused={focused}
        selected={selected}
        onSelect={toggleSel}
        onSelectAll={(checked) =>
          setSelected(checked ? new Set(rows.map((r) => r.id)) : new Set())
        }
        onOpen={openEdit}
        rowActions={rowActions}
        onBulkDelete={handleDelete as unknown as () => void}
        onBulkClearSel={() => setSelected(new Set())}
        reload={reload}
      />
    </ErpListLayout>
  );
}
