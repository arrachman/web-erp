'use client';

/**
 * Proforma Invoice (PI) form — thin wrapper over the shared sales transaction form,
 * pinned to transaction code "SLS.PI". Model + layout live in
 * sls-proforma-invoice-form-model.ts / sales-transaction-form.tsx (§3 reuse).
 */

import { SalesTransactionForm } from './sales-transaction-form';
import {
  defaultSlsProformaInvoiceForm,
  fromSlsProformaInvoice,
  toSlsProformaInvoicePayload,
  type SlsProformaInvoiceFormData,
} from './sls-proforma-invoice-form-model';

export type { SlsProformaInvoiceFormData } from './sls-proforma-invoice-form-model';
export { defaultSlsProformaInvoiceForm, fromSlsProformaInvoice, toSlsProformaInvoicePayload };

export function SlsProformaInvoiceForm(props: {
  data: SlsProformaInvoiceFormData;
  onChange: (d: SlsProformaInvoiceFormData) => void;
  saving?: boolean;
  allowedCreationStatuses?: string[];
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  return <SalesTransactionForm {...props} transactionCode="SLS.PI" />;
}
