'use client';

/**
 * Bank Keluar (SM) form — bank-flavoured wrapper over the shared cash/bank
 * transaction form. Differs from Kas Keluar (CD) only in: bank labels
 * ("Bayar Ke" + "Akun Bank [K]"), a Cara Bayar (paymentMethod) header field,
 * and a functional Giro tab. Model + layout live in cash-bank-form-model.ts /
 * cash-bank-transaction-form.tsx (§3 reuse — header/lines identical).
 */

import * as React from 'react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  CashBankTransactionForm,
  type CashBankFormLabels,
} from './cash-bank-transaction-form';
import {
  defaultCashBankForm,
  fromCashBankTransaction,
  toCashBankPayload,
  type CashBankFormData,
} from './cash-bank-form-model';
import { CashBankGirosEditor } from '@/components/organisms/cash-bank-giros';
import type {
  CreateBankDisbursementPayload,
  ErpBankDisbursement,
  ErpPaymentMethod,
} from '@/lib/api/fin-bank-disbursements';

export type BankDisbursementFormData = CashBankFormData;

export const defaultBankDisbursementForm = (): BankDisbursementFormData =>
  defaultCashBankForm('BANK');
export const fromBankDisbursement = (r: ErpBankDisbursement): BankDisbursementFormData =>
  fromCashBankTransaction(r);
export const toBankDisbursementPayload = (
  d: BankDisbursementFormData,
): CreateBankDisbursementPayload => toCashBankPayload(d, 'DISBURSEMENT');

const SM_LABELS: CashBankFormLabels = { partner: 'Bayar Ke', account: 'Akun Bank [K]' };

/** Cara Bayar options for bank transactions (CASH excluded — that's Kas). */
const PAYMENT_METHODS: { value: ErpPaymentMethod; label: string }[] = [
  { value: 'TRANSFER', label: 'Transfer' },
  { value: 'GIRO', label: 'Giro' },
  { value: 'CHEQUE', label: 'Cek' },
  { value: 'CARD', label: 'Kartu' },
  { value: 'OTHER', label: 'Lainnya' },
];

/** Cara Bayar value → label (covers CASH for completeness, used by the list column). */
export const paymentMethodLabel = (m?: ErpPaymentMethod | null): string => {
  if (!m) return '—';
  if (m === 'CASH') return 'Tunai';
  return PAYMENT_METHODS.find((p) => p.value === m)?.label ?? m;
};

const EDITABLE = ['DRAFT', 'NEED_APPROVE', 'APPROVE_1', 'APPROVE_2', 'APPROVE_3', 'APPROVE_4', 'REJECTED'];

export function BankDisbursementForm({
  data,
  onChange,
  saving,
  allowedCreationStatuses,
  onSave,
  onSaveNew,
  onReset,
}: {
  data: BankDisbursementFormData;
  onChange: (d: BankDisbursementFormData) => void;
  saving?: boolean;
  allowedCreationStatuses?: string[];
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  const locked = !EDITABLE.includes(data.status);

  // Cara Bayar — rendered atop the left header column (matches Field markup).
  const caraBayar = (
    <label className="flex items-center gap-2">
      <span className="text-xs text-muted-foreground w-24 shrink-0 text-left">
        Cara Bayar<span className="text-danger">&nbsp;*</span>
      </span>
      <div className="flex-1 min-w-0">
        <Select
          value={data.paymentMethod || 'TRANSFER'}
          disabled={locked}
          onValueChange={(v) => onChange({ ...data, paymentMethod: v as ErpPaymentMethod })}
        >
          <SelectTrigger>
            <SelectValue placeholder="Pilih cara bayar" />
          </SelectTrigger>
          <SelectContent>
            {PAYMENT_METHODS.map((m) => (
              <SelectItem key={m.value} value={m.value}>
                {m.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
    </label>
  );

  const giroTab = (
    <CashBankGirosEditor
      giros={data.giros}
      onChange={(giros) => onChange({ ...data, giros })}
      readOnly={locked}
    />
  );

  return (
    <CashBankTransactionForm
      data={data}
      onChange={onChange}
      labels={SM_LABELS}
      transactionCode="FIN.SM"
      headerExtra={caraBayar}
      extraTabs={[{ key: 'giro', label: 'Giro', content: giroTab }]}
      saving={saving}
      allowedCreationStatuses={allowedCreationStatuses}
      onSave={onSave}
      onSaveNew={onSaveNew}
      onReset={onReset}
    />
  );
}
