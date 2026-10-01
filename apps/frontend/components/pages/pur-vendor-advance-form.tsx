'use client';

import * as React from 'react';
import { Input } from '@/components/ui/input';
import { DateInput } from '@/components/ui/date-input';
import { SearchSelect } from '@/components/molecules/search-select';
import { NumInput } from '@/components/molecules/num-input';
import { loadSupplierOptions } from './pur-form-lookups';
import { loadBranchOptions, loadCurrencyOptions } from './items-form-lookups';

export interface VendorAdvanceFormData {
  id?: string;
  docNumber: string;
  autoNumber: boolean;
  transactionDate: string;
  fiscalPeriodId: string;
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
  status?: string;
}

export function emptyVendorAdvanceForm(): VendorAdvanceFormData {
  return {
    docNumber: '', autoNumber: true,
    transactionDate: new Date().toISOString().slice(0, 10),
    fiscalPeriodId: '', branchId: '', partnerId: '',
    description: '', currencyId: '', exchangeRate: '1',
    amount: '', notes: '',
  };
}

interface FormProps {
  data: VendorAdvanceFormData;
  onChange: (d: VendorAdvanceFormData) => void;
  saving: boolean;
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}

export function VendorAdvanceForm({ data, onChange, saving, onSave, onSaveNew, onReset }: FormProps) {
  const set = <K extends keyof VendorAdvanceFormData>(k: K, v: VendorAdvanceFormData[K]) =>
    onChange({ ...data, [k]: v });
  const isPosted = data.status === 'POSTED' || data.status === 'VOID';

  return (
    <div className="trx-form-layout">
      <div className="trx-form-left">
        <div className="form-field">
          <label className="form-label">Supplier *</label>
          <SearchSelect
            value={data.partnerId}
            onValueChange={(v) => set('partnerId', v)}
            onPick={(opt) => onChange({ ...data, partnerId: opt.value, partnerLabel: opt.label })}
            initialLabel={data.partnerLabel}
            loadOptions={loadSupplierOptions}
            placeholder="Pilih Supplier…"
            disabled={isPosted}
          />
        </div>
        <div className="form-field">
          <label className="form-label">Cabang *</label>
          <SearchSelect
            value={data.branchId}
            onValueChange={(v) => set('branchId', v)}
            onPick={(opt) => onChange({ ...data, branchId: opt.value, branchLabel: opt.label })}
            initialLabel={data.branchLabel}
            loadOptions={loadBranchOptions}
            placeholder="Pilih Cabang…"
            disabled={isPosted}
          />
        </div>
        <div className="form-field">
          <label className="form-label">Uraian *</label>
          <Input value={data.description} onChange={(e) => set('description', e.target.value)}
            placeholder="Uraian transaksi" disabled={isPosted} />
        </div>
        <div className="form-field">
          <label className="form-label">Catatan</label>
          <Input value={data.notes} onChange={(e) => set('notes', e.target.value)}
            placeholder="Catatan opsional" disabled={isPosted} />
        </div>
      </div>

      <div className="trx-form-right">
        <div className="form-field">
          <label className="form-label">Tanggal *</label>
          <DateInput value={data.transactionDate} onChange={(v) => set('transactionDate', v ?? '')}
            disabled={isPosted} />
        </div>
        <div className="form-field">
          <label className="form-label">No Transaksi</label>
          <div className="flex items-center gap-2">
            <Input value={data.autoNumber ? '(Otomatis)' : data.docNumber}
              onChange={(e) => set('docNumber', e.target.value)}
              disabled={data.autoNumber || isPosted} className="flex-1" />
            <label className="flex items-center gap-1 text-sm cursor-pointer">
              <input type="checkbox" checked={data.autoNumber}
                onChange={(e) => set('autoNumber', e.target.checked)} disabled={isPosted} />
              Auto
            </label>
          </div>
        </div>
        <div className="form-field">
          <label className="form-label">Mata Uang *</label>
          <div className="flex items-center gap-2">
            <div className="flex-1">
              <SearchSelect value={data.currencyId}
                onValueChange={(v) => set('currencyId', v)}
                onPick={(opt) => onChange({ ...data, currencyId: opt.value, currencyLabel: opt.label })}
                initialLabel={data.currencyLabel}
                loadOptions={loadCurrencyOptions}
                placeholder="Pilih Mata Uang…" disabled={isPosted} />
            </div>
            <NumInput value={data.exchangeRate} onChange={(v) => set('exchangeRate', v)}
              disabled={isPosted} className="w-24" placeholder="Kurs" decimals={4} />
          </div>
        </div>
        <div className="form-field">
          <label className="form-label">Nominal *</label>
          <NumInput value={data.amount} onChange={(v) => set('amount', v)}
            disabled={isPosted} decimals={2} />
        </div>
      </div>

      {!isPosted && (
        <div className="trx-form-actions">
          <button className="btn primary" onClick={onSave} disabled={saving}>Simpan &amp; Tutup</button>
          <button className="btn secondary" onClick={onSaveNew} disabled={saving}>Simpan &amp; Baru</button>
          <button className="btn ghost" onClick={onReset} disabled={saving}>Reset</button>
        </div>
      )}
    </div>
  );
}
