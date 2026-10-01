'use client';

// Freight Receivable (RP) form — thin wrapper over shared SalesTransactionForm,
// pinned to transaction code "SLS.RP". Uses same model as SI (same sls_invoices table).

import { SalesTransactionForm } from './sales-transaction-form';
import type { SlsOrderFormData } from './sls-order-form-model';
import {
  defaultSlsInvoiceForm,
  fromSlsInvoice,
  toSlsInvoicePayload,
  type SlsInvoiceFormData,
} from './sls-invoice-form-model';

export type { SlsInvoiceFormData } from './sls-invoice-form-model';
export { defaultSlsInvoiceForm, fromSlsInvoice, toSlsInvoicePayload };

export function SlsFreightReceivableForm({
  data,
  onChange,
  ...rest
}: {
  data: SlsInvoiceFormData;
  onChange: (d: SlsInvoiceFormData) => void;
  saving?: boolean;
  allowedCreationStatuses?: string[];
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  return (
    <SalesTransactionForm
      {...rest}
      data={data}
      onChange={(d: SlsOrderFormData) => onChange(d as SlsInvoiceFormData)}
      transactionCode="SLS.RP"
    />
  );
}
