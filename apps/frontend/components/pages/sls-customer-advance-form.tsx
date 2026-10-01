'use client';

/**
 * Customer Advance (AS) — header-only form (no item lines).
 * Fields: customerId, branchId, docDate, docNumber+auto, currencyId+exchangeRate,
 *         paymentTermId, dueDate, amount, description, notes.
 * Atomic tier: Page-level form organism.
 */

import * as React from 'react';
import { FormFieldRow } from '@/components/molecules/form-field-row';
import { SearchSelect } from '@/components/molecules/search-select';
import { NumInput } from '@/components/molecules/num-input';
import { Input } from '@/components/ui/input';
import { DateInput } from '@/components/ui/date-input';
import { loadBranchOptions, loadCurrencyOptions } from '@/components/pages/items-form-lookups';
import {
  loadCustomerOptions,
  loadPaymentTermOptions,
} from '@/components/pages/sls-form-lookups';
import type { ErpSlsCustomerAdvance } from '@/lib/api/sls-customer-advances';
import type { ErpDocumentStatus } from '@/lib/api/sls-orders';

export interface SlsCustomerAdvanceFormData {
  id?: string;
  docNumber: string;
  auto: boolean;
  docDate: string;
  dueDate: string;
  customerId: string;
  customerLabel?: string;
  branchId: string;
  branchLabel?: string;
  currencyId: string;
  currencyLabel?: string;
  exchangeRate: string;
  paymentTermId: string;
  paymentTermLabel?: string;
  amount: string;
  description: string;
  notes: string;
  status: ErpDocumentStatus;
  postedAt?: string | null;
}

export function defaultSlsCustomerAdvanceForm(): SlsCustomerAdvanceFormData {
  return {
    docNumber: '',
    auto: true,
    docDate: '',
    dueDate: '',
    customerId: '',
    branchId: '',
    currencyId: '',
    exchangeRate: '1',
    paymentTermId: '',
    amount: '',
    description: '',
    notes: '',
    status: 'DRAFT',
  };
}

export function fromSlsCustomerAdvance(r: ErpSlsCustomerAdvance): SlsCustomerAdvanceFormData {
  return {
    id: r.id,
    docNumber: r.docNumber,
    auto: !!r.autoNumber,
    docDate: r.docDate.slice(0, 10),
    dueDate: r.dueDate ? r.dueDate.slice(0, 10) : '',
    customerId: r.customerId ?? '',
    customerLabel: r.customer?.name,
    branchId: r.branchId,
    branchLabel: r.branch?.name,
    currencyId: r.currencyId,
    currencyLabel: r.currency?.code,
    exchangeRate: r.exchangeRate,
    paymentTermId: r.paymentTermId ?? '',
    paymentTermLabel: r.paymentTerm?.name,
    amount: r.amount,
    description: r.description ?? '',
    notes: r.notes ?? '',
    status: r.status,
    postedAt: r.postedAt,
  };
}

function SectionTitle({ children }: { children: React.ReactNode }) {
  return (
    <h3 className="text-xs font-semibold text-muted-foreground uppercase tracking-wide mb-2 mt-4 first:mt-0">
      {children}
    </h3>
  );
}

export function SlsCustomerAdvanceForm({
  data,
  onChange,
  saving,
  onSave,
  onSaveNew,
  onReset,
}: {
  data: SlsCustomerAdvanceFormData;
  onChange: (d: SlsCustomerAdvanceFormData) => void;
  saving?: boolean;
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  const set = <K extends keyof SlsCustomerAdvanceFormData>(
    key: K,
    value: SlsCustomerAdvanceFormData[K],
  ) => onChange({ ...data, [key]: value });

  const isPosted = data.status === 'POSTED';

  return (
    <div className="flex flex-col gap-6 max-w-3xl">
      {/* Left section: partner + description */}
      <div className="flex flex-col gap-2">
        <SectionTitle>Identitas</SectionTitle>

        <FormFieldRow label="Pelanggan" required>
          <SearchSelect
            id="as-customer"
            value={data.customerId}
            initialLabel={data.customerLabel}
            loadOptions={loadCustomerOptions}
            placeholder="Cari pelanggan…"
            disabled={isPosted}
            onValueChange={(v) => onChange({ ...data, customerId: v })}
            onPick={(o) => onChange({ ...data, customerId: o.value, customerLabel: o.label })}
          />
        </FormFieldRow>

        <FormFieldRow label="Cabang" required>
          <SearchSelect
            id="as-branch"
            value={data.branchId}
            initialLabel={data.branchLabel}
            loadOptions={loadBranchOptions}
            placeholder="Pilih cabang…"
            disabled={isPosted}
            onValueChange={(v) => onChange({ ...data, branchId: v })}
            onPick={(o) => onChange({ ...data, branchId: o.value, branchLabel: o.label })}
          />
        </FormFieldRow>

        <FormFieldRow label="Keterangan">
          <Input
            value={data.description}
            onChange={(e) => set('description', e.target.value)}
            placeholder="Keterangan advance…"
            disabled={isPosted}
          />
        </FormFieldRow>
      </div>

      {/* Right section: dates + doc number + currency */}
      <div className="flex flex-col gap-2">
        <SectionTitle>Dokumen</SectionTitle>

        <FormFieldRow label="Tanggal" required>
          <DateInput
            value={data.docDate}
            onChange={(v) => set('docDate', v)}
            disabled={isPosted}
          />
        </FormFieldRow>

        <FormFieldRow label="No Transaksi">
          <div className="flex items-center gap-2">
            <Input
              value={data.auto ? '' : data.docNumber}
              onChange={(e) => set('docNumber', e.target.value)}
              placeholder={data.auto ? '(otomatis)' : 'No. dokumen…'}
              disabled={data.auto || isPosted}
              className="flex-1"
            />
            <label className="flex items-center gap-1 text-xs text-muted-foreground whitespace-nowrap cursor-pointer">
              <input
                type="checkbox"
                checked={data.auto}
                onChange={(e) => set('auto', e.target.checked)}
                disabled={isPosted}
              />
              Auto
            </label>
          </div>
        </FormFieldRow>

        <FormFieldRow label="Mata Uang" required>
          <div className="flex items-center gap-2">
            <SearchSelect
              id="as-currency"
              value={data.currencyId}
              initialLabel={data.currencyLabel}
              loadOptions={loadCurrencyOptions}
              placeholder="Pilih uang…"
              disabled={isPosted}
              onValueChange={(v) => onChange({ ...data, currencyId: v })}
              onPick={(o) => onChange({ ...data, currencyId: o.value, currencyLabel: o.label })}
            />
            <Input
              value={data.exchangeRate}
              onChange={(e) => set('exchangeRate', e.target.value)}
              placeholder="Kurs"
              className="w-24"
              disabled={isPosted}
            />
          </div>
        </FormFieldRow>

        <FormFieldRow label="Termin">
          <SearchSelect
            id="as-payment-term"
            value={data.paymentTermId}
            initialLabel={data.paymentTermLabel}
            loadOptions={loadPaymentTermOptions}
            placeholder="Pilih termin…"
            disabled={isPosted}
            onValueChange={(v) => onChange({ ...data, paymentTermId: v })}
            onPick={(o) => onChange({ ...data, paymentTermId: o.value, paymentTermLabel: o.label })}
          />
        </FormFieldRow>

        <FormFieldRow label="Jatuh Tempo">
          <DateInput
            value={data.dueDate}
            onChange={(v) => set('dueDate', v)}
            disabled={isPosted}
          />
        </FormFieldRow>
      </div>

      {/* Amount */}
      <div className="flex flex-col gap-2">
        <SectionTitle>Jumlah</SectionTitle>

        <FormFieldRow label="Jumlah Advance" required>
          <NumInput
            value={data.amount}
            onChange={(v) => set('amount', v)}
            placeholder="0"
            disabled={isPosted}
            className="text-right text-lg font-semibold"
          />
        </FormFieldRow>
      </div>

      {/* Notes */}
      <FormFieldRow label="Catatan">
        <Input
          value={data.notes}
          onChange={(e) => set('notes', e.target.value)}
          placeholder="Catatan tambahan…"
          disabled={isPosted}
        />
      </FormFieldRow>

      {/* Action buttons */}
      {!isPosted && (
        <div className="flex items-center gap-2 pt-2">
          <button type="button" className="btn primary sm" onClick={onSave} disabled={saving}>
            {saving ? 'Menyimpan…' : 'Simpan + Tutup'}
          </button>
          <button type="button" className="btn secondary sm" onClick={onSaveNew} disabled={saving}>
            Simpan + Baru
          </button>
          <button type="button" className="btn ghost sm" onClick={onReset} disabled={saving}>
            Reset
          </button>
        </div>
      )}
    </div>
  );
}
