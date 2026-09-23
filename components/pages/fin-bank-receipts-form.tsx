'use client';

/**
 * Bank Masuk (RM) form — thin wrapper over the shared cash/bank transaction form
 * (RECEIPT against a bank account: "Terima Dari" + "Akun Bank [D]"), adding the
 * two bank-only bits via the form's extension slots: Cara Bayar (header extra)
 * and a Giro tab. Header/lines/Info live in cash-bank-transaction-form.tsx (§3).
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
import { CashBankGirosEditor } from '@/components/organisms/cash-bank-giros';
import {
  PAYMENT_METHODS,
  type BankReceiptFormData,
} from './fin-bank-receipts-form-model';
import type { ErpDocumentStatus, ErpPaymentMethod } from '@/lib/api/fin-bank-receipts';

export type { BankReceiptFormData } from './fin-bank-receipts-form-model';
export {
  defaultBankReceiptForm,
  fromBankReceipt,
  toBankReceiptPayload,
  paymentMethodLabel,
} from './fin-bank-receipts-form-model';

const BR_LABELS: CashBankFormLabels = { partner: 'Terima Dari', account: 'Akun Bank [D]' };
const EDITABLE: ErpDocumentStatus[] = ['DRAFT', 'NEED_APPROVE', 'REJECTED'];

export function BankReceiptForm({
  data,
  onChange,
  saving,
  allowedCreationStatuses,
  onSave,
  onSaveNew,
  onReset,
}: {
  data: BankReceiptFormData;
  onChange: (d: BankReceiptFormData) => void;
  saving?: boolean;
  allowedCreationStatuses?: string[];
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  const locked = !EDITABLE.includes(data.status);
  const giroCount = data.giros.filter((g) => g.giroNumber.trim()).length;

  const caraBayar = (
    <label className="flex items-center gap-2">
      <span className="text-xs text-muted-foreground w-24 shrink-0 text-left">
        Cara Bayar<span className="text-danger">&nbsp;*</span>
      </span>
      <div className="flex-1 min-w-0">
        <Select
          value={data.paymentMethod}
          disabled={locked}
          onValueChange={(v) => onChange({ ...data, paymentMethod: v as ErpPaymentMethod })}
        >
          <SelectTrigger><SelectValue /></SelectTrigger>
          <SelectContent>
            {PAYMENT_METHODS.map((m) => (
              <SelectItem key={m.value} value={m.value}>{m.label}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
    </label>
  );

  return (
    <CashBankTransactionForm
      data={data}
      onChange={(d) => onChange(d as BankReceiptFormData)}
      labels={BR_LABELS}
      transactionCode="FIN.RM"
      headerExtra={caraBayar}
      extraTabs={[
        {
          key: 'giro',
          label: `Giro${giroCount ? ` (${giroCount})` : ''}`,
          content: (
            <CashBankGirosEditor
              giros={data.giros}
              onChange={(giros) => onChange({ ...data, giros })}
              readOnly={locked}
            />
          ),
        },
      ]}
      saving={saving}
      allowedCreationStatuses={allowedCreationStatuses}
      onSave={onSave}
      onSaveNew={onSaveNew}
      onReset={onReset}
    />
  );
}
