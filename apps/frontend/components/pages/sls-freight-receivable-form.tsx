'use client';

// Freight Receivable (RP) form — header-only document on its own backend
// (erp-sls-freight-receivables, table sls_freight_receivables, DOC_CODE 'RP').
// Ongkos kirim ditagihkan ke customer dan dicatat terpisah dari penjualan
// barang: Dr Piutang Usaha / Cr Pendapatan Jasa Angkut saat POST.

import * as React from 'react';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { SearchSelect } from '@/components/molecules/search-select';
import { NumInput } from '@/components/molecules/num-input';
import { listAccounts } from '@/lib/api/accounts';
import { listBranches } from '@/lib/api/branches';
import { listCurrencies } from '@/lib/api/currencies';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import type {
  CreateSlsFreightReceivablePayload,
  ErpSlsFreightReceivable,
} from '@/lib/api/sls-freight-receivables';
import { loadReceivableAccounts, loadCurrencyOptions } from './partners-lookups';
import { loadBranchOptions, loadCustomerPartnerOptions } from './items-form-lookups';

// ─── Form model ─────────────────────────────────────────────────────────────

export interface SlsFreightReceivableFormData {
  id?: string;
  docNumber: string;
  auto: boolean;
  transactionDate: string;
  branchId: string;
  branchLabel: string;
  customerId: string;
  customerLabel: string;
  currencyId: string;
  currencyLabel: string;
  exchangeRate: string;
  amount: string;
  description: string;
  notes: string;
  receivableAccountId: string;
  receivableAccountLabel: string;
  incomeAccountId: string;
  incomeAccountLabel: string;
  status: string;
}

export function defaultSlsFreightReceivableForm(): SlsFreightReceivableFormData {
  return {
    docNumber: '',
    auto: true,
    transactionDate: new Date().toISOString().slice(0, 10),
    branchId: '',
    branchLabel: '',
    customerId: '',
    customerLabel: '',
    currencyId: '',
    currencyLabel: '',
    exchangeRate: '1',
    amount: '',
    description: '',
    notes: '',
    receivableAccountId: '',
    receivableAccountLabel: '',
    incomeAccountId: '',
    incomeAccountLabel: '',
    status: 'DRAFT',
  };
}

const refLabel = (r?: { code: string; name: string } | null) => (r ? r.name : '');
const currencyLabel = (r?: { code: string; name: string } | null) =>
  r ? `${r.code} — ${r.name}` : '';

export function fromSlsFreightReceivable(
  r: ErpSlsFreightReceivable,
): SlsFreightReceivableFormData {
  return {
    id: r.id,
    docNumber: r.docNumber,
    auto: false,
    transactionDate: r.transactionDate.slice(0, 10),
    branchId: r.branchId,
    branchLabel: refLabel(r.branch),
    customerId: r.customerId,
    customerLabel: refLabel(r.customer),
    currencyId: r.currencyId,
    currencyLabel: currencyLabel(r.currency),
    exchangeRate: r.exchangeRate,
    amount: r.amount,
    description: r.description ?? '',
    notes: r.notes ?? '',
    receivableAccountId: r.receivableAccountId ?? '',
    receivableAccountLabel: refLabel(r.receivableAccount),
    incomeAccountId: r.incomeAccountId ?? '',
    incomeAccountLabel: refLabel(r.incomeAccount),
    status: r.status,
  };
}

export function toSlsFreightReceivablePayload(
  form: SlsFreightReceivableFormData,
): CreateSlsFreightReceivablePayload {
  return {
    docNumber: form.auto ? undefined : form.docNumber || undefined,
    transactionDate: form.transactionDate,
    branchId: form.branchId,
    partnerId: form.customerId,
    description: form.description,
    currencyId: form.currencyId,
    exchangeRate: form.exchangeRate || '1',
    amount: form.amount || '0',
    notes: form.notes || undefined,
    receivableAccountId: form.receivableAccountId || undefined,
    incomeAccountId: form.incomeAccountId || undefined,
  };
}

// ─── Option loaders ─────────────────────────────────────────────────────────

const loadIncomeAccounts = async (search: string, page: number, limit: number) => {
  const res = await listAccounts({
    page,
    limit,
    search: search || undefined,
    accountType: 'REVENUE',
    accountKind: 'POSTABLE',
    isActive: true,
  });
  return {
    data: res.data.map((a) => ({ value: a.id, label: a.name, code: a.code })),
    total: res.meta.total,
  };
};

// ─── Form component ─────────────────────────────────────────────────────────

const EDITABLE = ['DRAFT', 'REJECTED'];

export function SlsFreightReceivableForm({
  data,
  onChange,
  saving,
  onSave,
  onSaveNew,
  onReset,
}: {
  data: SlsFreightReceivableFormData;
  onChange: (d: SlsFreightReceivableFormData) => void;
  saving?: boolean;
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  const set = (patch: Partial<SlsFreightReceivableFormData>) => onChange({ ...data, ...patch });
  const locked = !!data.id && !EDITABLE.includes(data.status);

  // Defaults for a new document: first branch + IDR currency.
  const defaultsApplied = React.useRef(false);
  React.useEffect(() => {
    if (data.id || defaultsApplied.current) return;
    if (data.branchId && data.currencyId) return;
    defaultsApplied.current = true;
    (async () => {
      const patch: Partial<SlsFreightReceivableFormData> = {};
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
                placeholder="Otomatis (RP000001)"
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
              id="rp-branch"
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
            <label className="form-label">Customer *</label>
            <SearchSelect
              id="rp-customer"
              value={data.customerId}
              onValueChange={(v) => set({ customerId: v })}
              placeholder="Cari customer…"
              loadOptions={loadCustomerPartnerOptions}
              initialLabel={data.customerLabel}
              title="Customer"
              disabled={locked}
            />
          </div>
          <div>
            <label className="form-label">Keterangan *</label>
            <Input
              value={data.description}
              disabled={locked}
              onChange={(e) => set({ description: e.target.value })}
              placeholder="Tagihan ongkos kirim…"
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
              id="rp-currency"
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
              id="rp-rate"
              value={data.exchangeRate}
              onChange={(v) => set({ exchangeRate: v })}
              decimals={6}
              placeholder="1"
              disabled={locked}
            />
          </div>
          <div>
            <label className="form-label">Jumlah Tagihan *</label>
            <NumInput
              id="rp-amount"
              value={data.amount}
              onChange={(v) => set({ amount: v })}
              decimals={2}
              placeholder="0,00"
              disabled={locked}
            />
          </div>
          <div>
            <label className="form-label">Akun Piutang</label>
            <SearchSelect
              id="rp-recv-acct"
              value={data.receivableAccountId}
              onValueChange={(v) => set({ receivableAccountId: v })}
              placeholder="Cari akun piutang…"
              loadOptions={loadReceivableAccounts}
              initialLabel={data.receivableAccountLabel}
              title="Akun Piutang"
              disabled={locked}
            />
          </div>
          <div>
            <label className="form-label">Akun Pendapatan Jasa Angkut</label>
            <SearchSelect
              id="rp-income-acct"
              value={data.incomeAccountId}
              onValueChange={(v) => set({ incomeAccountId: v })}
              placeholder="Cari akun pendapatan…"
              loadOptions={loadIncomeAccounts}
              initialLabel={data.incomeAccountLabel}
              title="Akun Pendapatan Jasa Angkut"
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
