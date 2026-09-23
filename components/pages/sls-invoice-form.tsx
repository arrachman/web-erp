'use client';

/**
 * Sales Invoice (SI) form — thin wrapper over the shared sales transaction form,
 * pinned to transaction code "SLS.SI". Model + layout live in
 * sls-invoice-form-model.ts / sales-transaction-form.tsx (§3 reuse).
 */

import { SalesTransactionForm } from './sales-transaction-form';
import {
  defaultSlsInvoiceForm,
  fromSlsInvoice,
  toSlsInvoicePayload,
  type SlsInvoiceFormData,
} from './sls-invoice-form-model';

export type { SlsInvoiceFormData } from './sls-invoice-form-model';
export { defaultSlsInvoiceForm, fromSlsInvoice, toSlsInvoicePayload };

export function SlsInvoiceForm(props: {
  data: SlsInvoiceFormData;
  onChange: (d: SlsInvoiceFormData) => void;
  saving?: boolean;
  allowedCreationStatuses?: string[];
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  return <SalesTransactionForm {...props} transactionCode="SLS.SI" />;
}
