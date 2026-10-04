'use client';

// Freight Payable (PP) form — header-only document on its own backend
// (erp-pur-freight-payables, stored in fin_ap_payments with source='PP',
// DOC_CODE 'PP'). Biaya angkut pembelian dibayar ke ekspedisi/supplier dan
// dicatat terpisah dari HPP: Dr Beban Angkut Pembelian / Cr Kas-Bank saat POST.

import * as React from 'react';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { SearchSelect } from '@/components/molecules/search-select';
import { NumInput } from '@/components/molecules/num-input';
import { listAccounts, type ErpAccountType } from '@/lib/api/accounts';
import { listBranches } from '@/lib/api/branches';
import { listCurrencies } from '@/lib/api/currencies';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import type {
  CreateFreightPayablePayload,
  ErpFreightPayable,
} from '@/lib/api/pur-freight-payables';
import { loadCurrencyOptions } from './partners-lookups';
import { loadBranchOptions, loadSupplierOptions } from './items-form-lookups';

// ─── Form model ─────────────────────────────────────────────────────────────

export interface PurFreightPayableFormData {
  id?: string;
  docNumber: string;
  auto: boolean;
  transactionDate: string;
  branchId: string;
  branchLabel: string;
  partnerId: string;
  partnerLabel: string;
  currencyId: string;
  currencyLabel: string;
  exchangeRate: string;
  amount: string;
  description: string;
  notes: string;
  bankAccountId: string;
  bankAccountLabel: string;
  expenseAccountId: string;
  expenseAccountLabel: string;
  status: string;
}

export function defaultPurFreightPayableForm(): PurFreightPayableFormData {
  return {
    docNumber: '',
    auto: true,
    transactionDate: new Date().toISOString().slice(0, 10),
    branchId: '',
    branchLabel: '',
    partnerId: '',
    partnerLabel: '',
    currencyId: '',
    currencyLabel: '',
    exchangeRate: '1',
    amount: '',
    description: '',
    notes: '',
    bankAccountId: '',
    bankAccountLabel: '',
    expenseAccountId: '',
    expenseAccountLabel: '',
    status: 'DRAFT',
  };
}

const refLabel = (r?: { code: string; name: string } | null) => (r ? r.name : '');
const currencyLabel = (r?: { code: string; name: string } | null) =>
  r ? `${r.code} — ${r.name}` : '';

export function fromFreightPayable(r: ErpFreightPayable): PurFreightPayableFormData {
  return {
    id: r.id,
    docNumber: r.docNumber,
    auto: false,
    transactionDate: r.transactionDate.slice(0, 10),
    branchId: r.branchId,
    branchLabel: refLabel(r.branch),
    partnerId: r.partnerId,
    partnerLabel: refLabel(r.partner),
    currencyId: r.currencyId,
    currencyLabel: currencyLabel(r.currency),
    exchangeRate: r.exchangeRate,
    amount: r.amount,
    description: r.description ?? '',
    notes: r.notes ?? '',
    bankAccountId: r.bankAccountId ?? '',
    bankAccountLabel: refLabel(r.bankAccount),
    expenseAccountId: r.expenseAccountId ?? '',
    expenseAccountLabel: refLabel(r.expenseAccount),
    status: r.status,
  };
}

export function toFreightPayablePayload(
  form: PurFreightPayableFormData,
): CreateFreightPayablePayload {
  return {
    docNumber: form.auto ? undefined : form.docNumber || undefined,
    transactionDate: form.transactionDate,
    branchId: form.branchId,
    partnerId: form.partnerId,
    description: form.description,
    currencyId: form.currencyId,
    exchangeRate: form.exchangeRate || '1',
    amount: form.amount || '0',
    notes: form.notes || undefined,
    bankAccountId: form.bankAccountId || undefined,
    expenseAccountId: form.expenseAccountId || undefined,
  };
}

// ─── Option loaders ─────────────────────────────────────────────────────────

const accountLoader =
  (accountType: ErpAccountType) => async (search: string, page: number, limit: number) => {
    const res = await listAccounts({
      page,
      limit,
      search: search || undefined,
      accountType,
      accountKind: 'POSTABLE',
      isActive: true,
    });
    return {
      data: res.data.map((a) => ({ value: a.id, label: a.name, code: a.code })),
      total: res.meta.total,
    };
  };

const loadCashBankAccounts = accountLoader('ASSET');
const loadExpenseAccounts = accountLoader('EXPENSE');

// ─── Form component ─────────────────────────────────────────────────────────

const EDITABLE = ['DRAFT', 'REJECTED'];

export function PurFreightPayableForm({
  data,
  onChange,
  saving,
  onSave,
  onSaveNew,
  onReset,
}: {
  data: PurFreightPayableFormData;
  onChange: (d: PurFreightPayableFormData) => void;
  saving?: boolean;
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  const set = (patch: Partial<PurFreightPayableFormData>) => onChange({ ...data, ...patch });
  const locked = !!data.id && !EDITABLE.includes(data.status);

  // Defaults for a new document: first branch + IDR currency.
  const defaultsApplied = React.useRef(false);
  React.useEffect(() => {
    if (data.id || defaultsApplied.current) return;
    if (data.branchId && data.currencyId) return;
    defaultsApplied.current = true;
    (async () => {
      const patch: Partial<PurFreightPayableFormData> = {};
      try {
        if (!data.branchId) {
          const res = await listBranches({ page: 1, limit: 100 });
          const first = res.data[0];
          if (first) {
            patch.branchId = first.id;
            patch.branchLabel = first.name;
          }
        }
        if (!data.currencyId) {
          const res = await listCurrencies({ page: 1, limit: 100, isActive: true });
          const idr = res.data.find((c) => c.code === 'IDR') ?? res.data[0];
          if (idr) {
            patch.currencyId = idr.id;
            patch.currencyLabel = `${idr.code} — ${idr.name}`;
          }
        }
      } catch {
        /* leave empty — user picks manually */
      }
      if (Object.keys(patch).length) onChange({ ...data, ...patch });
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [data.id]);

  return (
    <div className="flex flex-col gap-4">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="flex flex-col gap-3">
          <div className="flex items-end gap-2">
            <div className="flex-1">
              <label className="form-label">No Transaksi</label>
              <Input
                value={data.docNumber}
                onChange={(e) => set({ docNumber: e.target.value })}
                placeholder="Otomatis (PP000001)"
                disabled={data.auto || locked}
              />
            </div>
            <label className="flex items-center gap-1.5 pb-1.5 whitespace-nowrap cursor-pointer text-sm">
              <input
                type="checkbox"
                checked={data.auto}
                disabled={locked || !!data.id}
                onChange={(e) =>
                  set({ auto: e.target.checked, docNumber: e.target.checked ? '' : data.docNumber })
                }
              />
              Auto
            </label>
          </div>
          <div>
            <label className="form-label">Tanggal *</label>
            <Input
              type="date"
              value={data.transactionDate}
              disabled={locked}
              onChange={(e) => set({ transactionDate: e.target.value })}
            />
          </div>
          <div>
            <label className="form-label">Cabang *</label>
            <SearchSelect
              id="pp-branch"
              value={data.branchId}
              onValueChange={(v) => set({ branchId: v })}
              placeholder="Pilih cabang…"
              loadOptions={loadBranchOptions}
              initialLabel={data.branchLabel}
              title="Cabang"
              disabled={locked}
            />
          </div>
          <div>
            <label className="form-label">Supplier / Ekspedisi *</label>
            <SearchSelect
              id="pp-partner"
              value={data.partnerId}
              onValueChange={(v) => set({ partnerId: v })}
              placeholder="Cari supplier…"
              loadOptions={loadSupplierOptions}
              initialLabel={data.partnerLabel}
              title="Supplier / Ekspedisi"
              disabled={locked}
            />
          </div>
          <div>
            <label className="form-label">Keterangan *</label>
            <Input
              value={data.description}
              disabled={locked}
              onChange={(e) => set({ description: e.target.value })}
              placeholder="Biaya pengiriman barang…"
            />
          </div>
          <div>
            <label className="form-label">Catatan</label>
            <Input
              value={data.notes}
              disabled={locked}
              onChange={(e) => set({ notes: e.target.value })}
              placeholder="—"
            />
          </div>
        </div>

        <div className="flex flex-col gap-3">
          <div>
            <label className="form-label">Status</label>
            <div>
              <Badge variant={statusBadgeVariant(data.status)} dot>
                {statusLabel(data.status)}
              </Badge>
            </div>
          </div>
          <div>
            <label className="form-label">Mata Uang *</label>
            <SearchSelect
              id="pp-currency"
              value={data.currencyId}
              onValueChange={(v) => set({ currencyId: v })}
              placeholder="Pilih mata uang…"
              loadOptions={loadCurrencyOptions}
              initialLabel={data.currencyLabel}
              title="Mata Uang"
              disabled={locked}
            />
          </div>
          <div>
            <label className="form-label">Kurs</label>
            <NumInput
              id="pp-rate"
              value={data.exchangeRate}
              onChange={(v) => set({ exchangeRate: v })}
              decimals={6}
              placeholder="1"
              disabled={locked}
            />
          </div>
          <div>
            <label className="form-label">Jumlah Biaya *</label>
            <NumInput
              id="pp-amount"
              value={data.amount}
              onChange={(v) => set({ amount: v })}
              decimals={2}
              placeholder="0,00"
              disabled={locked}
            />
          </div>
          <div>
            <label className="form-label">Akun Kas / Bank</label>
            <SearchSelect
              id="pp-bank-acct"
              value={data.bankAccountId}
              onValueChange={(v) => set({ bankAccountId: v })}
              placeholder="Cari akun kas/bank…"
              loadOptions={loadCashBankAccounts}
              initialLabel={data.bankAccountLabel}
              title="Akun Kas / Bank"
              disabled={locked}
            />
          </div>
          <div>
            <label className="form-label">Akun Beban Angkut Pembelian</label>
            <SearchSelect
              id="pp-expense-acct"
              value={data.expenseAccountId}
              onValueChange={(v) => set({ expenseAccountId: v })}
              placeholder="Cari akun beban…"
              loadOptions={loadExpenseAccounts}
              initialLabel={data.expenseAccountLabel}
              title="Akun Beban Angkut Pembelian"
              disabled={locked}
            />
          </div>
        </div>
      </div>

      <div className="flex items-center gap-2 pt-2 border-t border-border">
        <button type="button" className="btn primary" onClick={onSave} disabled={saving || locked}>
          {saving ? 'Menyimpan…' : 'Simpan'}
        </button>
        <button type="button" className="btn secondary" onClick={onSaveNew} disabled={saving || locked}>
          Simpan &amp; Baru
        </button>
        <button type="button" className="btn ghost" onClick={onReset} disabled={saving}>
          Reset
        </button>
      </div>
    </div>
  );
}
