'use client';

/**
 * Kas Keluar (CD) form — thin direction wrapper over the shared cash/bank
 * transaction form (DISBURSEMENT: "Bayar Ke" + "Akun Kas [K]"). Model + layout
 * live in cash-bank-form-model.ts / cash-bank-transaction-form.tsx (§3 reuse).
 */

import * as React from 'react';
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
import type {
  CreateCashDisbursementPayload,
  ErpCashDisbursement,
} from '@/lib/api/fin-cash-disbursements';

export type CashDisbursementFormData = CashBankFormData;

export const defaultCashDisbursementForm = defaultCashBankForm;
export const fromCashDisbursement = (r: ErpCashDisbursement): CashDisbursementFormData =>
  fromCashBankTransaction(r);
export const toCashDisbursementPayload = (
  d: CashDisbursementFormData,
): CreateCashDisbursementPayload => toCashBankPayload(d, 'DISBURSEMENT');

const CD_LABELS: CashBankFormLabels = { partner: 'Bayar Ke', account: 'Akun Kas [K]' };

export function CashDisbursementForm(props: {
  data: CashDisbursementFormData;
  onChange: (d: CashDisbursementFormData) => void;
  saving?: boolean;
  allowedCreationStatuses?: string[];
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  return <CashBankTransactionForm {...props} labels={CD_LABELS} transactionCode="FIN.CD" />;
}
