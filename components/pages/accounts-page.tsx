'use client';

/**
 * F3 Master Data — Chart of Accounts page.
 * Hierarchical list by parentId: parent rows expand/collapse; children nest
 * with depth indent. Lazy tree: roots first, children loaded on expand.
 * Atomic tier: Page.
 */

import * as React from 'react';
import { FormErrorSummary } from '@/components/molecules/form-error-summary';
import { BulkActionBar } from '@/components/molecules/bulk-action-bar';
import { AuditModal } from '@/components/molecules/audit-modal';
import {
  Modal,
  ModalContent,
  ModalHeader,
  ModalTitle,
  ModalFooter,
} from '@/components/organisms/modal';
import {
  ErpListLayout,
  type FilterConfig,
  type SummaryConfig,
  type KeyboardRowConfig,
} from '@/components/organisms/erp-list-layout';
import {
  ACCOUNT_TYPES,
  type ErpAccount,
} from '@/lib/api/accounts';
import {
  filterAccountTreeIds,
  flattenAccountTree,
  type FlatAccountRow,
} from '@/lib/accounts-tree';
import { notify } from '@/lib/feedback';
import type { FormErrors } from '@/lib/form-validation';
import { tGlobal } from '@/lib/mock';
import { useAccountTree } from '@/lib/use-account-tree';
import { useModalShortcuts } from '@/lib/use-modal-shortcuts';
import {
  AccountFormFields,
  defaultAccountForm,
  fromAccount,
  type AccountFormData,
} from './accounts-form';
import {
  saveAccount,
  confirmDeleteAccount,
  confirmBulkStatus,
  confirmBulkDelete,
} from './accounts-page-actions';
import { AccountsTreeTable } from './accounts-tree-table';

export function ErpAccountsPage() {
  const [search, setSearch] = React.useState('');
  const [debouncedSearch, setDebouncedSearch] = React.useState('');
  const [statusFilter, setStatusFilter] = React.useState('active');
  const [accountType, setAccountType] = React.useState('');
  const [accountKind, setAccountKind] = React.useState('');
  const [expanded, setExpanded] = React.useState<Set<string>>(() => new Set());
  const knownIdsRef = React.useRef<Set<string>>(new Set());

  React.useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(search), 300);
    return () => clearTimeout(t);
  }, [search]);

  const isActiveParam =
    statusFilter === 'active' ? true : statusFilter === 'inactive' ? false : undefined;

  const {
    rows,
    hasChildrenMap,
    loading,
    fetching,
    error,
    reload,
    ensureChildren,
  } = useAccountTree({
    isActive: isActiveParam,
    accountType: accountType || undefined,
    accountKind: accountKind || undefined,
  });

  // Roots start collapsed; first expand fetches children.
  React.useEffect(() => {
    if (rows.length === 0) return;
    if (knownIdsRef.current.size === 0) {
      setExpanded(new Set());
      knownIdsRef.current = new Set(rows.map((r) => r.id));
      return;
    }
    knownIdsRef.current = new Set(rows.map((r) => r.id));
  }, [rows]);

  const toggleExpand = (id: string) => {
    setExpanded((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
        void ensureChildren(id);
      }
      return next;
    });
  };

  const visibleFlat = React.useMemo(() => {
    const q = debouncedSearch.trim();
    let source = rows;
    let expandSet = expanded;
    if (q) {
      // Server search not used for tree coherence; filter loaded subset only.
      const keep = filterAccountTreeIds(rows, q);
      source = rows.filter((r) => keep.has(r.id));
      expandSet = new Set([...expanded, ...keep]);
    }
    const out: FlatAccountRow<ErpAccount>[] = [];
    flattenAccountTree(source, null, 0, out, expandSet);
    return out.map((n) => ({
      ...n,
      hasChildren: hasChildrenMap.get(n.row.id) ?? n.hasChildren,
    }));
  }, [rows, expanded, debouncedSearch, hasChildrenMap]);

  const [selectedIds, setSelectedIds] = React.useState<Set<string>>(new Set());
  const [focusedIndex, setFocusedIndex] = React.useState(-1);
  const [open, setOpen] = React.useState(false);
  const [editing, setEditing] = React.useState<ErpAccount | null>(null);
  const [form, setForm] = React.useState<AccountFormData>(defaultAccountForm);
  const [saving, setSaving] = React.useState(false);
  const [formErrors, setFormErrors] = React.useState<FormErrors<AccountFormData>>({});
  const [auditTarget, setAuditTarget] = React.useState<ErpAccount | null>(null);

  React.useEffect(() => {
    setFocusedIndex(-1);
  }, [debouncedSearch, statusFilter, accountType, accountKind]);

  React.useEffect(() => {
    if (focusedIndex < 0) return;
    document
      .querySelector('[data-focused="true"]')
      ?.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
  }, [focusedIndex]);

  const visibleRows = visibleFlat.map((f) => f.row);
  const totalRows = rows.length;

  const toggleRow = (id: string) =>
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  const toggleAll = (checked: boolean) =>
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (checked) visibleRows.forEach((r) => next.add(r.id));
      else visibleRows.forEach((r) => next.delete(r.id));
      return next;
    });

  const ALL = { label: 'Semua', value: '' };
  const filters: FilterConfig[] = [
    {
      key: 'status',
      label: 'Status',
      value: statusFilter,
      onChange: setStatusFilter,
      options: [
        ALL,
        { label: 'Aktif', value: 'active' },
        { label: 'Nonaktif', value: 'inactive' },
      ],
    },
    {
      key: 'accountType',
      label: 'Tipe',
      value: accountType,
      onChange: setAccountType,
      options: [ALL, ...ACCOUNT_TYPES.map((t) => ({ label: t, value: t }))],
    },
    {
      key: 'accountKind',
      label: 'Jenis',
      value: accountKind,
      onChange: setAccountKind,
      options: [
        ALL,
        { label: 'Header', value: 'HEADER' },
        { label: 'Postable', value: 'POSTABLE' },
      ],
    },
  ];
  const hasActiveFilter =
    search !== '' ||
    statusFilter !== 'active' ||
    accountType !== '' ||
    accountKind !== '';
  const summary: SummaryConfig = {
    metricLabel: `Σ ${tGlobal('akun')}`,
    rowCount: visibleFlat.length,
    totalCount: totalRows,
  };

  const openCreate = () => {
    setEditing(null);
    setForm(defaultAccountForm());
    setFormErrors({});
    setOpen(true);
  };
  const openEdit = (row: ErpAccount) => {
    setEditing(row);
    setForm(fromAccount(row));
    setFormErrors({});
    setOpen(true);
  };
  const openDuplicate = (row: ErpAccount) => {
    setEditing(null);
    setForm({ ...fromAccount(row), code: '' });
    setFormErrors({});
    setOpen(true);
  };

  const keyboardCfg: KeyboardRowConfig = {
    rowCount: visibleRows.length,
    focusedIndex,
    onFocusChange: setFocusedIndex,
    onToggle: (i) => toggleRow(visibleRows[i].id),
    onOpen: (i) => openEdit(visibleRows[i]),
  };

  const handleSave = (keepOpen = false) =>
    saveAccount({
      form,
      editing,
      setFormErrors,
      setSaving,
      setOpen,
      setForm,
      defaultForm: defaultAccountForm,
      reload,
      keepOpen,
    });

  useModalShortcuts({
    open,
    editing: !!editing,
    onSave: () => handleSave(false),
    onSaveAndNew: () => handleSave(true),
  });

  const clearSelection = () => setSelectedIds(new Set());
  const selectedArr = Array.from(selectedIds);

  const expandAll = () => {
    const parents = [...hasChildrenMap.entries()]
      .filter(([, h]) => h)
      .map(([id]) => id);
    setExpanded(new Set(parents));
    for (const id of parents) void ensureChildren(id);
  };

  const treeToolbar = (
    <div className="flex items-center gap-1">
      <button
        type="button"
        className="btn ghost"
        onClick={expandAll}
        title={tGlobal('Expand semua')}
      >
        {tGlobal('Expand')}
      </button>
      <button
        type="button"
        className="btn ghost"
        onClick={() => setExpanded(new Set())}
        title={tGlobal('Collapse semua')}
      >
        {tGlobal('Collapse')}
      </button>
    </div>
  );

  return (
    <>
      <ErpListLayout
        title="Bagan Akun"
        code="COA"
        loading={loading}
        fetching={fetching}
        error={error}
        search={search}
        onSearch={setSearch}
        onAdd={openCreate}
        onRefresh={reload}
        onExport={() => notify(tGlobal('Export belum tersedia'), 'warn')}
        filters={filters}
        summary={summary}
        footerSummary={{ rowCount: visibleFlat.length, totalRows }}
        keyboardRows={keyboardCfg}
        toolbar={treeToolbar}
      >
        <AccountsTreeTable
          nodes={visibleFlat}
          expanded={expanded}
          selectedIds={selectedIds}
          focusedIndex={focusedIndex}
          hasActiveFilter={hasActiveFilter}
          searchTerm={debouncedSearch}
          onToggleExpand={toggleExpand}
          onToggleRow={toggleRow}
          onToggleAll={toggleAll}
          onFocus={setFocusedIndex}
          onOpenEdit={openEdit}
          onOpenDuplicate={openDuplicate}
          onOpenAudit={setAuditTarget}
          onDelete={(row) => confirmDeleteAccount(row, reload)}
          onResetFilters={() => {
            setSearch('');
            setStatusFilter('active');
            setAccountType('');
            setAccountKind('');
          }}
          onCreate={openCreate}
        />
      </ErpListLayout>

      <BulkActionBar
        count={selectedIds.size}
        onActivate={() =>
          confirmBulkStatus(selectedArr, true, reload, clearSelection)
        }
        onDeactivate={() =>
          confirmBulkStatus(selectedArr, false, reload, clearSelection)
        }
        onDelete={() => confirmBulkDelete(selectedArr, reload, clearSelection)}
        onCancel={clearSelection}
      />

      <Modal open={open} onOpenChange={setOpen}>
        <ModalContent size="lg" className="max-h-[84vh]">
          <ModalHeader className="shrink-0">
            <ModalTitle>
              {editing
                ? `${tGlobal('Edit')} ${tGlobal('Bagan Akun')}`
                : `${tGlobal('Tambah')} ${tGlobal('Bagan Akun')}`}
            </ModalTitle>
          </ModalHeader>
          <div className="min-h-0 flex-1 overflow-y-auto">
            <FormErrorSummary errors={formErrors} />
            <AccountFormFields data={form} onChange={setForm} errors={formErrors} />
          </div>
          <ModalFooter className="shrink-0">
            <button className="btn ghost" onClick={() => setOpen(false)}>
              {tGlobal('Batal')}
            </button>
            {!editing && (
              <button
                className="btn ghost"
                onClick={() => handleSave(true)}
                disabled={saving}
                title="Ctrl+Enter"
              >
                {tGlobal('Simpan & Tambah Baru')}
              </button>
            )}
            <button
              className="btn primary"
              onClick={() => handleSave(false)}
              disabled={saving}
              title="Ctrl+S"
            >
              {saving ? tGlobal('Menyimpan...') : tGlobal('Simpan')}
            </button>
          </ModalFooter>
        </ModalContent>
      </Modal>

      <AuditModal
        target={auditTarget}
        onClose={() => setAuditTarget(null)}
        entityName="ErpAccount"
      />
    </>
  );
}
