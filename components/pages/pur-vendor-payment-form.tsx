'use client';

/**
 * Vendor Payment (VP) / Payment Schedule (VPP) form.
 * Reuses `fin_ap_payments` endpoint — same data as Finance AP Payments but
 * accessed from the Purchasing sidebar with type codes PUR.VP / PUR.VPP.
 * Status machine: DRAFT → POST → POSTED; POSTED → VOID → VOID.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Input } from '@/components/ui/input';
import { DateInput } from '@/components/ui/date-input';
import { Badge } from '@/components/ui/badge';
import { FormFieldRow } from '@/components/molecules/form-field-row';
import { SearchSelect } from '@/components/molecules/search-select';
import { notify } from '@/lib/feedback';
import { formatNumber } from '@/lib/format';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import { loadBranchOptions, loadCurrencyOptions } from '@/components/pages/items-form-lookups';
import { loadSupplierOptions } from '@/components/pages/pur-form-lookups';
import {
  createApPayment,
  updateApPayment,
  getApPayment,
  transitionApPayment,
  type ErpApPayment,
  type CreateApPaymentPayload,
} from '@/lib/api/fin-ap-payments';

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

const todayIso = () => new Date().toISOString().slice(0, 10);

const defaultForm = (): FormState => ({
  docNumber: '',
  auto: true,
  transactionDate: todayIso(),
  branchId: '',
  description: '',
  partnerId: '',
  currencyId: '',
  exchangeRate: '1',
  amount: '0',
  notes: '',
});

function fromRecord(r: ErpApPayment): FormState {
  return {
    docNumber: r.docNumber,
    auto: false,
    transactionDate: r.transactionDate.slice(0, 10),
    branchId: r.branchId,
    partnerId: r.partnerId,
    description: r.description,
    currencyId: r.currencyId,
    exchangeRate: r.exchangeRate,
    amount: r.amount,
    notes: r.notes ?? '',
  };
}

function toPayload(f: FormState): CreateApPaymentPayload {
  return {
    docNumber: f.auto ? undefined : f.docNumber || undefined,
    branchId: f.branchId,
    transactionDate: f.transactionDate,
    fiscalPeriodId: '1',
    partnerId: f.partnerId,
    description: f.description,
    currencyId: f.currencyId || '1',
    exchangeRate: f.exchangeRate || '1',
    amount: f.amount || '0',
    allocatedAmount: '0',
    notes: f.notes || undefined,
  };
}

export function PurVendorPaymentForm({
  transactionCode,
  formMode,
  recordId,
  onBack,
}: {
  transactionCode: 'PUR.VP' | 'PUR.VPP';
  formMode?: 'create' | 'edit';
  recordId?: string;
  onBack: () => void;
}) {
  const title = transactionCode === 'PUR.VP' ? 'Pembayaran Vendor (VP)' : 'Jadwal Pembayaran (VPP)';
  const [record, setRecord] = React.useState<ErpApPayment | null>(null);
  const [form, setForm] = React.useState<FormState>(defaultForm());
  const [saving, setSaving] = React.useState(false);
  const set = (p: Partial<FormState>) => setForm((f) => ({ ...f, ...p }));

  React.useEffect(() => {
    if (formMode === 'edit' && recordId) {
      getApPayment(recordId)
        .then((r) => { setRecord(r); setForm(fromRecord(r)); })
        .catch(() => { notify('Gagal memuat data', 'danger'); onBack(); });
    } else {
      setRecord(null);
      setForm(defaultForm());
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formMode, recordId]);

  const status = record?.status ?? 'DRAFT';
  const locked = status !== 'DRAFT';

  const persist = async (closeAfter: boolean) => {
    if (!form.branchId || !form.transactionDate || !form.partnerId) {
      notify('Cabang, Tanggal, dan Supplier wajib diisi.', 'warn');
      return;
    }
    setSaving(true);
    try {
      const payload = toPayload(form);
      if (record) {
        await updateApPayment(record.id, payload);
        notify('Berhasil diperbarui', 'success');
      } else {
        await createApPayment(payload);
        notify('Berhasil disimpan', 'success');
      }
      if (closeAfter) onBack();
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const runTransition = async (action: 'POST' | 'VOID') => {
    if (!record) return;
    const label = action === 'POST' ? 'posting' : 'void';
    try {
      const updated = await transitionApPayment(record.id, action);
      setRecord(updated);
      notify(`Dokumen berhasil di-${label}`, 'success');
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : `Gagal ${label}`, 'danger');
    }
  };

  return (
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
        {record && status === 'DRAFT' && (
          <button type="button" className="btn" onClick={() => runTransition('POST')} disabled={saving}>
            <Icon name="check" size={13} /> Post
          </button>
        )}
        {record && status === 'POSTED' && (
          <button type="button" className="btn danger" onClick={() => runTransition('VOID')} disabled={saving}>
            Void
          </button>
        )}
        <button type="button" className="btn ghost" onClick={onBack} disabled={saving}>
          Batal
        </button>
        <div className="flex-1" />
        <Badge variant={statusBadgeVariant(status as never)} dot>
          {statusLabel(status as never)}
        </Badge>
      </div>

      {/* Header grid */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-x-6 gap-y-3 rounded-lg border border-border p-4">
        {/* LEFT */}
        <div className="flex flex-col gap-3">
          <FormFieldRow label="Supplier" required>
            <SearchSelect placeholder="Pilih supplier…" value={form.partnerId}
              initialLabel={form.partnerLabel} disabled={locked}
              loadOptions={loadSupplierOptions}
              onValueChange={(v) => set({ partnerId: v })}
              onPick={(o) => set({ partnerId: o.value, partnerLabel: o.label })} />
          </FormFieldRow>
          <FormFieldRow label="Uraian">
            <Input value={form.description} placeholder="Keterangan…" disabled={locked}
              onChange={(e) => set({ description: e.target.value })} />
          </FormFieldRow>
        </div>

        {/* CENTER */}
        <div className="flex flex-col gap-3">
          <FormFieldRow label="Cabang" required>
            <SearchSelect placeholder="Pilih cabang…" value={form.branchId}
              initialLabel={form.branchLabel} disabled={locked}
              loadOptions={loadBranchOptions}
              onValueChange={(v) => set({ branchId: v })}
              onPick={(o) => set({ branchId: o.value, branchLabel: o.label })} />
          </FormFieldRow>
          <FormFieldRow label="Mata Uang">
            <SearchSelect placeholder="Mata uang…" value={form.currencyId}
              initialLabel={form.currencyLabel} disabled={locked}
              loadOptions={loadCurrencyOptions}
              onValueChange={(v) => set({ currencyId: v })}
              onPick={(o) => set({ currencyId: o.value, currencyLabel: o.label })} />
          </FormFieldRow>
          <FormFieldRow label="Jumlah" required>
            <Input type="number" value={form.amount} placeholder="0" disabled={locked}
              onChange={(e) => set({ amount: e.target.value })}
              className="tabular-nums text-right" />
          </FormFieldRow>
        </div>

        {/* RIGHT */}
        <div className="flex flex-col gap-3">
          <FormFieldRow label="Tanggal" required>
            <DateInput value={form.transactionDate} disabled={locked}
              onChange={(v) => set({ transactionDate: v })} />
          </FormFieldRow>
          <FormFieldRow label="No Transaksi">
            <div className="flex items-center gap-2">
              <Input className="flex-1 min-w-0"
                value={form.auto ? '(otomatis saat simpan)' : form.docNumber}
                placeholder="No transaksi" disabled={form.auto || locked}
                onChange={(e) => set({ docNumber: e.target.value })} />
              <label className="flex items-center gap-1 text-xs text-muted-foreground shrink-0 cursor-pointer">
                <input type="checkbox" checked={form.auto} disabled={locked}
                  onChange={(e) => set({ auto: e.target.checked })} />
                Auto
              </label>
            </div>
          </FormFieldRow>
          <FormFieldRow label="Catatan">
            <Input value={form.notes} placeholder="Catatan opsional…" disabled={locked}
              onChange={(e) => set({ notes: e.target.value })} />
          </FormFieldRow>
        </div>
      </div>

      {/* Footer total */}
      {Number(form.amount) > 0 && (
        <div className="flex justify-end items-center gap-3 border-t border-border pt-3">
          <span className="text-sm text-muted-foreground">Jumlah</span>
          <span className="text-lg font-semibold tabular-nums">
            {formatNumber(Number(form.amount), 2)}
          </span>
        </div>
      )}
    </div>
  );
}
