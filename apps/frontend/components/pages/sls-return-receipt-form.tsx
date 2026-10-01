'use client';

/**
 * Return Receipt (RNR) form — thin wrapper over the shared sales transaction form,
 * pinned to transaction code "SLS.RNR". Model + layout live in
 * sls-return-receipt-form-model.ts / sales-transaction-form.tsx (§3 reuse).
 */

import { SalesTransactionForm } from './sales-transaction-form';
import {
  defaultSlsReturnReceiptForm,
  fromSlsReturnReceipt,
  toSlsReturnReceiptPayload,
  type SlsReturnReceiptFormData,
} from './sls-return-receipt-form-model';

export type { SlsReturnReceiptFormData } from './sls-return-receipt-form-model';
export { defaultSlsReturnReceiptForm, fromSlsReturnReceipt, toSlsReturnReceiptPayload };

export function SlsReturnReceiptForm(props: {
  data: SlsReturnReceiptFormData;
  onChange: (d: SlsReturnReceiptFormData) => void;
  saving?: boolean;
  allowedCreationStatuses?: string[];
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  return <SalesTransactionForm {...props} transactionCode="SLS.RNR" />;
}
