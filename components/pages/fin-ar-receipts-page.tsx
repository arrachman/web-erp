'use client';

/**
 * F2 Finance — AR Receipts page (skeleton CRUD).
 * Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { DateInput } from '@/components/ui/date-input';
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
  listArReceipts,
  createArReceipt,
  updateArReceipt,
  deleteArReceipt,
} from '@/lib/api/fin-ar-receipts';
import type {
  ErpArReceipt,
  ErpDocumentStatus,
  CreateArReceiptPayload,
} from '@/lib/api/fin-ar-receipts';

interface FormState {
  docNumber: string;
  branchId: string;
  transactionDate: string;
  fiscalPeriodId: string;
  partnerId: string;
  description: string;
  currencyId: string;
  exchangeRate: string;
  amount: string;
  allocatedAmount: string;
  notes: string;
}

const defaultForm = (): FormState => ({
  docNumber: '',
  branchId: '1',
  transactionDate: new Date().toISOString().slice(0, 10),
  fiscalPeriodId: '1',
  partnerId: '',
  description: '',
  currencyId: '1',
  exchangeRate: '1.000000',
  amount: '0.0000',
  allocatedAmount: '0.0000',
  notes: '',
});

function fromRow(r: ErpArReceipt): FormState {
  return {
    docNumber: r.docNumber,
    branchId: r.branchId,
    transactionDate: r.transactionDate.slice(0, 10),
    fiscalPeriodId: r.fiscalPeriodId,
    partnerId: r.partnerId,
    description: r.description,
    currencyId: r.currencyId,
    exchangeRate: r.exchangeRate,
    amount: r.amount,
    allocatedAmount: r.allocatedAmount,
    notes: r.notes ?? '',
  };
}

function toPayload(f: FormState): CreateArReceiptPayload {
  return {
    docNumber: f.docNumber,
    branchId: f.branchId,
    transactionDate: f.transactionDate,
    fiscalPeriodId: f.fiscalPeriodId,
    partnerId: f.partnerId,
    description: f.description,
    currencyId: f.currencyId,
    exchangeRate: f.exchangeRate,
    amount: f.amount,
    allocatedAmount: f.allocatedAmount,
    notes: f.notes || undefined,
  };
}

function Fields({
  data,
  onChange,
}: {
  data: FormState;
  onChange: (d: FormState) => void;
}) {
  const set = (k: keyof FormState, v: string) =>
    onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="No. Dokumen" htmlFor="ar-doc" required>
        <Input id="ar-doc" value={data.docNumber} onChange={(e) => set('docNumber', e.target.value)} placeholder="RCP-2026-000001" />
      </FormField>
      <FormField label="Tanggal" htmlFor="ar-date" required>
        <DateInput id="ar-date" value={data.transactionDate} onChange={(v) => set('transactionDate', v)} />
      </FormField>
      <FormField label="Branch ID" htmlFor="ar-branch" required>
        <Input id="ar-branch" value={data.branchId} onChange={(e) => set('branchId', e.target.value)} />
      </FormField>
      <FormField label="Fiscal Period ID" htmlFor="ar-fp" required>
        <Input id="ar-fp" value={data.fiscalPeriodId} onChange={(e) => set('fiscalPeriodId', e.target.value)} />
      </FormField>
      <FormField label="Partner ID" htmlFor="ar-partner" required>
        <Input id="ar-partner" value={data.partnerId} onChange={(e) => set('partnerId', e.target.value)} />
      </FormField>
      <FormField label="Deskripsi" htmlFor="ar-desc" required>
        <Input id="ar-desc" value={data.description} onChange={(e) => set('description', e.target.value)} />
      </FormField>
      <FormField label="Currency ID" htmlFor="ar-cur" required>
        <Input id="ar-cur" value={data.currencyId} onChange={(e) => set('currencyId', e.target.value)} />
      </FormField>
      <FormField label="Kurs" htmlFor="ar-rate" required>
        <Input id="ar-rate" value={data.exchangeRate} onChange={(e) => set('exchangeRate', e.target.value)} />
      </FormField>
      <FormField label="Jumlah" htmlFor="ar-amt" required>
        <Input id="ar-amt" value={data.amount} onChange={(e) => set('amount', e.target.value)} />
      </FormField>
      <FormField label="Allocated" htmlFor="ar-alloc" required>
        <Input id="ar-alloc" value={data.allocatedAmount} onChange={(e) => set('allocatedAmount', e.target.value)} />
      </FormField>
      <FormField label="Catatan" htmlFor="ar-notes">
        <Input id="ar-notes" value={data.notes} onChange={(e) => set('notes', e.target.value)} />
      </FormField>
    </div>
  );
}

export function ErpArReceiptsPage() {
  const [search, setSearch] = React.useState('');
  const [statusFilter, setStatusFilter] = React.useState('');
  const { page, pageSize, setPage, setPageSize } = useListPagination('fin-ar-receipts');

  const [debouncedSearch, setDebouncedSearch] = React.useState(search);
  React.useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(search), 300);
    return () => clearTimeout(t);
  }, [search]);

  const statusParam = (statusFilter || undefined) as ErpDocumentStatus | undefined;

  const { rows, meta, loading, error, reload } = useErpList(
    () =>
      listArReceipts({
        page,
        limit: pageSize,
        search: debouncedSearch || undefined,
        status: statusParam,
      }),
    [page, pageSize, debouncedSearch, statusParam],
  );

  React.useEffect(() => { setPage(1); }, [debouncedSearch, statusFilter, pageSize]);

  const [open, setOpen] = React.useState(false);
  const [editing, setEditing] = React.useState<ErpArReceipt | null>(null);
  const [form, setForm] = React.useState<FormState>(defaultForm);
  const [saving, setSaving] = React.useState(false);

  const paged = rows;
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  const ALL = { label: 'Semua', value: '' };
  const filters: FilterConfig[] = [
    { key: 'status', label: 'Status', value: statusFilter, onChange: setStatusFilter,
      options: [ALL,
        { label: 'Draft', value: 'DRAFT' },
        { label: 'Posted', value: 'POSTED' },
        { label: 'Void', value: 'VOID' },
        { label: 'Cancelled', value: 'CANCELLED' },
      ] },
  ];
  const summary: SummaryConfig = { metricLabel: 'Σ AR receipts', rowCount: totalRows, totalCount: totalRows };
  const pagination: ListPaginationConfig = { page, pageCount, pageSize, totalRows, onPage: setPage, onPageSize: setPageSize };

  const openCreate = () => {
    setEditing(null);
    setForm(defaultForm());
    setOpen(true);
  };

  const openEdit = (r: ErpArReceipt) => {
    setEditing(r);
    setForm(fromRow(r));
    setOpen(true);
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      if (editing) {
        await updateArReceipt(editing.id, toPayload(form));
        notify('AR receipt diperbarui', 'success');
      } else {
        await createArReceipt(toPayload(form));
        notify('AR receipt dibuat', 'success');
      }
      setOpen(false);
      reload();
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = (r: ErpArReceipt) => {
    confirmAction({
      title: 'Hapus AR receipt?',
      message: `${r.docNumber} akan dihapus.`,
      variant: 'danger',
      confirmLabel: 'Hapus',
      confirmIcon: 'trash',
      onConfirm: async () => {
        try {
          await deleteArReceipt(r.id);
          notify('AR receipt dihapus', 'success');
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
        title="AR Receipts"
        code="RCP"
        loading={loading}
        error={error}
        search={search}
        onSearch={setSearch}
        onAdd={openCreate}
        onRefresh={reload}
        filters={filters}
        summary={summary}
        pagination={pagination}
      >
        <div className="lines">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>No. Dokumen</TableHead>
                <TableHead>Tanggal</TableHead>
                <TableHead>Deskripsi</TableHead>
                <TableHead>Jumlah</TableHead>
                <TableHead>Status</TableHead>
                <TableHead />
              </TableRow>
            </TableHeader>
            <TableBody>
              {paged.length === 0 ? (
                <TableEmpty colSpan={6} />
              ) : (
                paged.map((r) => (
                  <TableRow key={r.id}>
                    <TableCell className="mono">{r.docNumber}</TableCell>
                    <TableCell>{r.transactionDate.slice(0, 10)}</TableCell>
                    <TableCell>{r.description}</TableCell>
                    <TableCell className="mono">{r.amount}</TableCell>
                    <TableCell>
                      <Badge variant={r.status === 'POSTED' ? 'success' : 'default'} dot>
                        {r.status}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <div style={{ display: 'flex', gap: 4 }}>
                        <button className="btn sm" onClick={() => openEdit(r)}>Edit</button>
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
            <ModalTitle>{editing ? 'Edit AR Receipt' : 'Tambah AR Receipt'}</ModalTitle>
          </ModalHeader>
          <Fields data={form} onChange={setForm} />
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
