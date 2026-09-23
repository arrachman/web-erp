'use client';

/**
 * Kas Masuk (CR) form — thin direction wrapper over the shared cash/bank
 * transaction form (RECEIPT: "Terima Dari" + "Akun Kas [D]"). Model + layout
 * live in cash-bank-form-model.ts / cash-bank-transaction-form.tsx (§3 reuse).
 */

import * as React from 'react';
import {
  CashBankTransactionForm,
  type CashBankFormLabels,
  type CashBankTransactionFormHandle,
} from './cash-bank-transaction-form';
import {
  defaultCashBankForm,
  fromCashBankTransaction,
  toCashBankPayload,
  type CashBankFormData,
} from './cash-bank-form-model';
import type {
  CreateCashReceiptPayload,
  ErpCashReceipt,
} from '@/lib/api/fin-cash-receipts';

export type CashReceiptFormData = CashBankFormData;

export const defaultCashReceiptForm = defaultCashBankForm;
export const fromCashReceipt = (r: ErpCashReceipt): CashReceiptFormData =>
  fromCashBankTransaction(r);
export const toCashReceiptPayload = (d: CashReceiptFormData): CreateCashReceiptPayload =>
  toCashBankPayload(d, 'RECEIPT');

export type { CashBankTransactionFormHandle as CashReceiptFormHandle };

const CR_LABELS: CashBankFormLabels = { partner: 'Terima Dari', account: 'Akun Kas [D]' };

export const CashReceiptForm = React.forwardRef<
  CashBankTransactionFormHandle,
  {
    data: CashReceiptFormData;
    onChange: (d: CashReceiptFormData) => void;
    saving?: boolean;
    allowedCreationStatuses?: string[];
    onSave: () => void;
    onSaveNew: () => void;
    onReset: () => void;
  }
>(function CashReceiptForm(props, ref) {
  return <CashBankTransactionForm ref={ref} {...props} labels={CR_LABELS} transactionCode="FIN.CR" />;
});
