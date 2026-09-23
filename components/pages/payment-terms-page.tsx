'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { NumInput } from '@/components/molecules/num-input';
import { DiscountInput } from '@/components/molecules/discount-input';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import {
  listPaymentTerms, createPaymentTerm, updatePaymentTerm, deletePaymentTerm,
  bulkUpdateErpPaymentTermStatus, bulkDeleteErpPaymentTerms,
  type ErpPaymentTerm, type CreatePaymentTermPayload,
} from '@/lib/api/payment-terms';
import { validateForm, type FormErrors } from '@/lib/form-validation';

interface FormData {
  code: string;
  name: string;
  netDays: string;
  discountDays1: string;
  discountPercent1: string;
  discountDays2: string;
  discountPercent2: string;
  penaltyPercent: string;
  penaltyPeriod: string;
  isActive: boolean;
}

const defaultForm = (): FormData => ({
  code: '',
  name: '',
  netDays: '0',
  discountDays1: '',
  discountPercent1: '',
  discountDays2: '',
  discountPercent2: '',
  penaltyPercent: '',
  penaltyPeriod: '',
  isActive: true,
});

const fromRecord = (r: ErpPaymentTerm): FormData => ({
  code: r.code,
  name: r.name,
  netDays: String(r.netDays),
  discountDays1: r.discountDays1 != null ? String(r.discountDays1) : '',
  discountPercent1: r.discountPercent1 ?? '',
  discountDays2: r.discountDays2 != null ? String(r.discountDays2) : '',
  discountPercent2: r.discountPercent2 ?? '',
  penaltyPercent: r.penaltyPercent ?? '',
  penaltyPeriod: r.penaltyPeriod ?? '',
  isActive: r.isActive,
});

const toPayload = (f: FormData): CreatePaymentTermPayload => {
  const num = (v: string) => (v.trim() === '' ? undefined : Number(v));
  const str = (v: string) => (v.trim() === '' ? undefined : v);
  return {
    code: f.code,
    name: f.name,
    netDays: Number(f.netDays || '0'),
    discountDays1: num(f.discountDays1),
    discountPercent1: str(f.discountPercent1),
    discountDays2: num(f.discountDays2),
    discountPercent2: str(f.discountPercent2),
    penaltyPercent: str(f.penaltyPercent),
    penaltyPeriod: str(f.penaltyPeriod),
    isActive: f.isActive,
  };
};

const validatePaymentTerm = (form: FormData) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
    {
      field: 'netDays',
      label: 'Jatuh Tempo',
      validate: (v) => (v === '' || v === null || v === undefined) ? 'Jatuh Tempo wajib diisi' : undefined,
    },
  ]);

// Periode denda — descriptor string ke backend (English canonical), label ID.
const PENALTY_PERIOD_OPTIONS = [
  { value: 'daily', label: 'Per hari' },
  { value: 'weekly', label: 'Per minggu' },
  { value: 'monthly', label: 'Per bulan' },
  { value: 'yearly', label: 'Per tahun' },
] as const;

function SectionTitle({ children, hint }: { children: React.ReactNode; hint?: string }) {
  return (
    <div className="mt-5 mb-1 first:mt-0">
      <h3 className="text-[11px] font-semibold uppercase tracking-wide text-[var(--fg-muted)]">
        {children}
      </h3>
      {hint && <p className="mt-0.5 text-[11px] text-[var(--fg-subtle)]">{hint}</p>}
    </div>
  );
}

/** Satu baris tier: "Bayar dalam [hari] hari → diskon [%]". */
function DiscountTierRow({
  label, idPrefix, days, percent, onDays, onPercent,
}: {
  label: string;
  idPrefix: string;
  days: string;
  percent: string;
  onDays: (v: string) => void;
  onPercent: (v: string) => void;
}) {
  return (
    <FormField label={label} htmlFor={`${idPrefix}-days`}>
      <div className="flex w-full items-center gap-2">
        <span className="whitespace-nowrap text-[11px] text-[var(--fg-subtle)]">dalam</span>
        <NumInput id={`${idPrefix}-days`} value={days} onChange={onDays} decimals={0} placeholder="10" className="w-16" />
        <span className="whitespace-nowrap text-[11px] text-[var(--fg-subtle)]">hari → diskon</span>
        <div className="w-24"><DiscountInput id={`${idPrefix}-pct`} value={percent} onChange={onPercent} placeholder="2" /></div>
      </div>
    </FormField>
  );
}

function FormFields({ data, onChange, errors = {} }: { data: FormData; onChange: (d: FormData) => void; errors?: FormErrors<FormData> }) {
  const set = (k: keyof FormData, v: string | boolean) => onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <SectionTitle>Identitas</SectionTitle>
      <FormField label="Kode" htmlFor="pt-code" required error={errors.code}>
        <Input id="pt-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder="NET30" aria-invalid={!!errors.code} />
      </FormField>
      <FormField label="Nama" htmlFor="pt-name" required error={errors.name}>
        <Input id="pt-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="Pembayaran 30 Hari" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Jatuh Tempo" htmlFor="pt-net" required error={errors.netDays} help="Batas pembayaran dihitung dari tanggal faktur. Isi 0 untuk tunai/COD.">
        <div className="flex w-full items-center gap-2">
          <NumInput id="pt-net" value={data.netDays} onChange={(v) => set('netDays', v)} decimals={0} placeholder="30" className="w-24" aria-invalid={!!errors.netDays} />
          <span className="whitespace-nowrap text-[11px] text-[var(--fg-subtle)]">hari</span>
        </div>
      </FormField>

      <SectionTitle hint="Insentif bila pelanggan membayar sebelum jatuh tempo. Kosongkan bila tidak ada diskon.">
        Diskon Pembayaran Awal
      </SectionTitle>
      <DiscountTierRow
        label="Tier 1" idPrefix="pt-d1"
        days={data.discountDays1} percent={data.discountPercent1}
        onDays={(v) => set('discountDays1', v)} onPercent={(v) => set('discountPercent1', v)}
      />
      <DiscountTierRow
        label="Tier 2" idPrefix="pt-d2"
        days={data.discountDays2} percent={data.discountPercent2}
        onDays={(v) => set('discountDays2', v)} onPercent={(v) => set('discountPercent2', v)}
      />

      <SectionTitle hint="Denda yang dikenakan bila pembayaran melewati jatuh tempo. Kosongkan bila tidak ada denda.">
        Denda Keterlambatan
      </SectionTitle>
      <FormField label="Besar Denda" htmlFor="pt-pen">
        <div className="w-24"><DiscountInput id="pt-pen" value={data.penaltyPercent} onChange={(v) => set('penaltyPercent', v)} placeholder="1.5" /></div>
      </FormField>
      <FormField label="Periode" htmlFor="pt-penp">
        <Select value={data.penaltyPeriod || undefined} onValueChange={(v) => set('penaltyPeriod', v)}>
          <SelectTrigger id="pt-penp"><SelectValue placeholder="Pilih periode denda" /></SelectTrigger>
          <SelectContent>
            {PENALTY_PERIOD_OPTIONS.map((o) => (
              <SelectItem key={o.value} value={o.value}>{o.label}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </FormField>

      <SectionTitle>Status</SectionTitle>
      <FormField label="Status" htmlFor="pt-active">
        <BooleanRadio id="pt-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}

export function ErpPaymentTermsPage() {
  return (
    <SimpleMasterPage<ErpPaymentTerm, FormData>
      title="Termin Pembayaran"
      code="TERM"
      entityLabel="termin"
      storageKey="payment-terms"
      auditEntityName="ErpPaymentTerm"
      list={listPaymentTerms}
      create={createPaymentTerm}
      update={updatePaymentTerm}
      remove={deletePaymentTerm}
      bulkStatus={bulkUpdateErpPaymentTermStatus}
      bulkDelete={bulkDeleteErpPaymentTerms}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={FormFields}
      validate={validatePaymentTerm}
      extraColumns={[
        { key: 'netDays', label: 'Jatuh Tempo (hari)', sortable: true, render: (row) => `${row.netDays} hari` },
      ]}
      defaultSortBy="code"
      defaultSortDir="asc"
    />
  );
}
