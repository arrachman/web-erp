'use client';

/**
 * F2 Finance — Journal Entries page (master + nested lines, skeleton CRUD).
 * Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import {
  Modal,
  ModalContent,
  ModalHeader,
  ModalTitle,
  ModalFooter,
} from '@/components/organisms/modal';
import {
  ErpListLayout,
  type ListPaginationConfig,
  type SummaryConfig,
  type FilterConfig,
} from '@/components/organisms/erp-list-layout';
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableCell,
  TableEmpty,
} from '@/components/organisms/table';
import { confirmAction, notify } from '@/lib/feedback';
import { useErpList } from '@/lib/use-erp-list';
import { useListPagination } from '@/lib/use-list-pagination';
import {
  listJournalEntries,
  createJournalEntry,
  updateJournalEntry,
  deleteJournalEntry,
  getJournalEntry,
} from '@/lib/api/fin-journal-entries';
import type {
  ErpJournalEntry,
  ErpJournalType,
  ErpDocumentStatus,
} from '@/lib/api/fin-journal-entries';
import {
  JournalFormFields,
  defaultJournalForm,
  fromEntry,
  toJournalPayload,
  type JournalFormData,
} from './fin-journal-entries-form';

const JOURNAL_TYPES: ErpJournalType[] = [
  'GENERAL', 'MEMORIAL', 'ADJUSTMENT', 'OPENING_BALANCE', 'CLOSING',
];

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}

function defaultDateFrom(): string {
  const d = new Date();
  d.setDate(d.getDate() - 30);
  return d.toISOString().slice(0, 10);
}

export function ErpJournalEntriesPage() {
  const [search, setSearch] = React.useState('');
  const [typeFilter, setTypeFilter] = React.useState('');
  const [statusFilter, setStatusFilter] = React.useState('');
  const [dateFrom, setDateFrom] = React.useState(() => defaultDateFrom());
  const [dateTo, setDateTo] = React.useState(() => todayIso());
  const { page, pageSize, setPage, setPageSize } = useListPagination('fin-journal-entries');

  const [debouncedSearch, setDebouncedSearch] = React.useState(search);
  React.useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(search), 300);
    return () => clearTimeout(t);
  }, [search]);

  const typeParam = (typeFilter || undefined) as ErpJournalType | undefined;
  const statusParam = (statusFilter || undefined) as ErpDocumentStatus | undefined;

  const { rows, meta, loading, error, reload } = useErpList(
    () =>
      listJournalEntries({
        page,
        limit: pageSize,
        search: debouncedSearch || undefined,
        journalType: typeParam,
        status: statusParam,
        dateFrom,
        dateTo,
      }),
    [page, pageSize, debouncedSearch, typeParam, statusParam, dateFrom, dateTo],
  );

  React.useEffect(() => { setPage(1); }, [debouncedSearch, typeFilter, statusFilter, dateFrom, dateTo, pageSize]);

  const [open, setOpen] = React.useState(false);
  const [editing, setEditing] = React.useState<ErpJournalEntry | null>(null);
  const [form, setForm] = React.useState<JournalFormData>(defaultJournalForm);
  const [saving, setSaving] = React.useState(false);

  const paged = rows;
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  const ALL = { label: 'Semua', value: '' };
  const filters: FilterConfig[] = [
    { key: 'journalType', label: 'Tipe', value: typeFilter, onChange: setTypeFilter,
      options: [ALL, ...JOURNAL_TYPES.map((t) => ({ label: t, value: t }))] },
    { key: 'status', label: 'Status', value: statusFilter, onChange: setStatusFilter,
      options: [ALL,
        { label: 'Draft', value: 'DRAFT' },
        { label: 'Posted', value: 'POSTED' },
        { label: 'Void', value: 'VOID' },
        { label: 'Cancelled', value: 'CANCELLED' },
      ] },
    { key: 'dateFrom', label: 'Dari', value: dateFrom, onChange: setDateFrom,
      options: [{ label: dateFrom, value: dateFrom }] },
    { key: 'dateTo', label: 'Sampai', value: dateTo, onChange: setDateTo,
      options: [{ label: dateTo, value: dateTo }] },
  ];
  const summary: SummaryConfig = { metricLabel: 'Σ jurnal', rowCount: totalRows, totalCount: totalRows };
  const pagination: ListPaginationConfig = { page, pageCount, pageSize, totalRows, onPage: setPage, onPageSize: setPageSize };

  const openCreate = () => {
    setEditing(null);
    setForm(defaultJournalForm());
    setOpen(true);
  };

  const openEdit = async (r: ErpJournalEntry) => {
    try {
      const detail = await getJournalEntry(r.id);
      setEditing(detail);
      setForm(fromEntry(detail));
      setOpen(true);
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal memuat detail jurnal', 'danger');
    }
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      if (editing) {
        await updateJournalEntry(editing.id, toJournalPayload(form));
        notify('Jurnal diperbarui', 'success');
      } else {
        await createJournalEntry(toJournalPayload(form));
        notify('Jurnal dibuat', 'success');
      }
      setOpen(false);
      reload();
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = (r: ErpJournalEntry) => {
    confirmAction({
      title: 'Hapus jurnal?',
      message: `${r.docNumber} akan dihapus permanen.`,
      variant: 'danger',
      confirmLabel: 'Hapus',
      confirmIcon: 'trash',
      onConfirm: async () => {
        try {
          await deleteJournalEntry(r.id);
          notify('Jurnal dihapus', 'success');
          reload();
        } catch (e: unknown) {
          notify(e instanceof Error ? e.message : 'Gagal', 'danger');
        }
      },
    });
  };

  return (
    <>
      <ErpListLayout
        title="Journal Entries"
        code="JV"
        loading={loading}
        error={error}
        search={search}
        onSearch={setSearch}
        onAdd={openCreate}
        onRefresh={reload}
      >
        <div className="lines">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>No. Dokumen</TableHead>
                <TableHead>Tipe</TableHead>
                <TableHead>Tanggal</TableHead>
                <TableHead>Deskripsi</TableHead>
                <TableHead>Lines</TableHead>
                <TableHead>Status</TableHead>
                <TableHead />
              </TableRow>
            </TableHeader>
            <TableBody>
              {paged.length === 0 ? (
                <TableEmpty colSpan={7} />
              ) : (
                paged.map((r) => (
                  <TableRow key={r.id}>
                    <TableCell className="mono">{r.docNumber}</TableCell>
                    <TableCell>{r.journalType}</TableCell>
                    <TableCell>{r.entryDate.slice(0, 10)}</TableCell>
                    <TableCell>{r.description}</TableCell>
                    <TableCell className="muted">{r.lines.length}</TableCell>
                    <TableCell>
                      <Badge variant={r.status === 'POSTED' ? 'success' : 'default'} dot>
                        {r.status}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <div style={{ display: 'flex', gap: 4 }}>
                        <button className="btn sm" onClick={() => void openEdit(r)}>Edit</button>
                        <button className="btn sm danger" onClick={() => handleDelete(r)}>Hapus</button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </div>
      </ErpListLayout>

      <Modal open={open} onOpenChange={setOpen}>
        <ModalContent>
          <ModalHeader>
            <ModalTitle>{editing ? 'Edit Jurnal' : 'Tambah Jurnal'}</ModalTitle>
          </ModalHeader>
          <JournalFormFields data={form} onChange={setForm} />
          <ModalFooter>
            <button className="btn ghost" onClick={() => setOpen(false)}>Batal</button>
            <button className="btn primary" onClick={handleSave} disabled={saving}>
              {saving ? 'Menyimpan...' : 'Simpan'}
            </button>
          </ModalFooter>
        </ModalContent>
      </Modal>
    </>
  );
}
