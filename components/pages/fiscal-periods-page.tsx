'use client';

/**
 * F2 Admin — Fiscal Periods page.
 * Lists sys_fiscal_periods; supports create, edit, delete + open/close.
 * Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { DateInput } from '@/components/ui/date-input';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import {
  Modal,
  ModalContent,
  ModalHeader,
  ModalTitle,
  ModalFooter,
} from '@/components/organisms/modal';
import { ErpListLayout } from '@/components/organisms/erp-list-layout';
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
import {
  listFiscalPeriods,
  createFiscalPeriod,
  updateFiscalPeriod,
  deleteFiscalPeriod,
  openFiscalPeriod,
  closeFiscalPeriod,
} from '@/lib/api/fiscal-periods';
import type {
  ErpFiscalPeriod,
  ErpFiscalPeriodStatus,
  CreateFiscalPeriodPayload,
} from '@/lib/api/fiscal-periods';

// ─── Helpers ──────────────────────────────────────────────────────────────────

interface PeriodForm {
  year: string;
  periodNo: string;
  name: string;
  startDate: string;
  endDate: string;
}

const defaultForm = (): PeriodForm => ({
  year: String(new Date().getFullYear()),
  periodNo: '1',
  name: '',
  startDate: '',
  endDate: '',
});

function fromPeriod(p: ErpFiscalPeriod): PeriodForm {
  return {
    year: String(p.year),
    periodNo: String(p.periodNo),
    name: p.name,
    startDate: p.startDate.slice(0, 10),
    endDate: p.endDate.slice(0, 10),
  };
}

function toPayload(f: PeriodForm): CreateFiscalPeriodPayload {
  return {
    year: Number(f.year),
    periodNo: Number(f.periodNo),
    name: f.name,
    startDate: f.startDate,
    endDate: f.endDate,
  };
}

const STATUS_VARIANT: Record<
  ErpFiscalPeriodStatus,
  'success' | 'default' | 'warn' | 'danger'
> = {
  OPEN: 'success',
  REOPENED: 'warn',
  SOFT_CLOSED: 'warn',
  CLOSED: 'danger',
};

const STATUS_LABEL: Record<ErpFiscalPeriodStatus, string> = {
  OPEN: 'Terbuka',
  REOPENED: 'Dibuka Ulang',
  SOFT_CLOSED: 'Soft Closed',
  CLOSED: 'Ditutup',
};

// ─── Form ─────────────────────────────────────────────────────────────────────

function PeriodFormFields({
  data,
  onChange,
}: {
  data: PeriodForm;
  onChange: (d: PeriodForm) => void;
}) {
  const set = (k: keyof PeriodForm, v: string) =>
    onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Tahun" htmlFor="fp-year" required>
        <Input
          id="fp-year"
          type="number"
          value={data.year}
          onChange={(e) => set('year', e.target.value)}
          placeholder="2025"
        />
      </FormField>
      <FormField label="Periode (1–12)" htmlFor="fp-periodno" required>
        <Input
          id="fp-periodno"
          type="number"
          value={data.periodNo}
          onChange={(e) => set('periodNo', e.target.value)}
          placeholder="1"
        />
      </FormField>
      <FormField label="Nama" htmlFor="fp-name" required>
        <Input
          id="fp-name"
          value={data.name}
          onChange={(e) => set('name', e.target.value)}
          placeholder="Jan 2025"
        />
      </FormField>
      <FormField label="Tanggal Mulai" htmlFor="fp-start" required>
        <DateInput
          id="fp-start"
          value={data.startDate}
          onChange={(v) => set('startDate', v)}
        />
      </FormField>
      <FormField label="Tanggal Akhir" htmlFor="fp-end" required>
        <DateInput
          id="fp-end"
          value={data.endDate}
          onChange={(v) => set('endDate', v)}
        />
      </FormField>
    </div>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export function ErpFiscalPeriodsPage() {
  const { rows, loading, error, reload } = useErpList(() =>
    listFiscalPeriods(),
  );
  const [search, setSearch] = React.useState('');
  const [open, setOpen] = React.useState(false);
  const [editing, setEditing] = React.useState<ErpFiscalPeriod | null>(null);
  const [form, setForm] = React.useState<PeriodForm>(defaultForm);
  const [saving, setSaving] = React.useState(false);

  const filtered = React.useMemo(() => {
    const q = search.toLowerCase();
    return q
      ? rows.filter(
          (r) =>
            r.name.toLowerCase().includes(q) ||
            String(r.year).includes(q),
        )
      : rows;
  }, [rows, search]);

  const openCreate = () => {
    setEditing(null);
    setForm(defaultForm());
    setOpen(true);
  };

  const openEdit = (p: ErpFiscalPeriod) => {
    setEditing(p);
    setForm(fromPeriod(p));
    setOpen(true);
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      if (editing) {
        await updateFiscalPeriod(editing.id, toPayload(form));
        notify('Periode diperbarui', 'success');
      } else {
        await createFiscalPeriod(toPayload(form));
        notify('Periode dibuat', 'success');
      }
      setOpen(false);
      reload();
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = (p: ErpFiscalPeriod) => {
    confirmAction({
      title: 'Hapus periode?',
      message: `${p.name} akan dihapus permanen.`,
      variant: 'danger',
      confirmLabel: 'Hapus',
      confirmIcon: 'trash',
      onConfirm: async () => {
        try {
          await deleteFiscalPeriod(p.id);
          notify('Periode dihapus', 'success');
          reload();
        } catch (e: unknown) {
          notify(e instanceof Error ? e.message : 'Gagal', 'danger');
        }
      },
    });
  };

  const handleOpen = async (p: ErpFiscalPeriod) => {
    try {
      await openFiscalPeriod(p.id);
      notify('Periode dibuka kembali', 'success');
      reload();
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal', 'danger');
    }
  };

  const handleClose = (p: ErpFiscalPeriod) => {
    confirmAction({
      title: 'Tutup periode?',
      message: `${p.name} akan ditutup. Transaksi pada periode ini akan terkunci.`,
      variant: 'warn',
      confirmLabel: 'Tutup',
      confirmIcon: 'shield',
      onConfirm: async () => {
        try {
          await closeFiscalPeriod(p.id);
          notify('Periode ditutup', 'success');
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
        title="Periode Fiskal"
        code="FISCAL"
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
                <TableHead>Tahun</TableHead>
                <TableHead>Periode</TableHead>
                <TableHead>Nama</TableHead>
                <TableHead>Mulai</TableHead>
                <TableHead>Akhir</TableHead>
                <TableHead>Status</TableHead>
                <TableHead />
              </TableRow>
            </TableHeader>
            <TableBody>
              {filtered.length === 0 ? (
                <TableEmpty colSpan={7} />
              ) : (
                filtered.map((p) => {
                  const isClosed =
                    p.status === 'CLOSED' || p.status === 'SOFT_CLOSED';
                  return (
                    <TableRow key={p.id}>
                      <TableCell className="mono">{p.year}</TableCell>
                      <TableCell className="mono">{p.periodNo}</TableCell>
                      <TableCell>{p.name}</TableCell>
                      <TableCell>{p.startDate.slice(0, 10)}</TableCell>
                      <TableCell>{p.endDate.slice(0, 10)}</TableCell>
                      <TableCell>
                        <Badge variant={STATUS_VARIANT[p.status]} dot>
                          {STATUS_LABEL[p.status]}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <div style={{ display: 'flex', gap: 4 }}>
                          {isClosed ? (
                            <button
                              className="btn sm"
                              onClick={() => handleOpen(p)}
                            >
                              Buka
                            </button>
                          ) : (
                            <button
                              className="btn sm"
                              onClick={() => handleClose(p)}
                            >
                              Tutup
                            </button>
                          )}
                          <button
                            className="btn sm"
                            onClick={() => openEdit(p)}
                          >
                            Edit
                          </button>
                          <button
                            className="btn sm danger"
                            onClick={() => handleDelete(p)}
                          >
                            Hapus
                          </button>
                        </div>
                      </TableCell>
                    </TableRow>
                  );
                })
              )}
            </TableBody>
          </Table>
        </div>
      </ErpListLayout>

      <Modal open={open} onOpenChange={setOpen}>
        <ModalContent>
          <ModalHeader>
            <ModalTitle>
              {editing ? 'Edit Periode Fiskal' : 'Tambah Periode Fiskal'}
            </ModalTitle>
          </ModalHeader>
          <PeriodFormFields data={form} onChange={setForm} />
          <ModalFooter>
            <button className="btn ghost" onClick={() => setOpen(false)}>
              Batal
            </button>
            <button
              className="btn primary"
              onClick={handleSave}
              disabled={saving}
            >
              {saving ? 'Menyimpan...' : 'Simpan'}
            </button>
          </ModalFooter>
        </ModalContent>
      </Modal>
    </>
  );
}
