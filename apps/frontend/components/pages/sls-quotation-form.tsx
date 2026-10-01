'use client';

/**
 * Sales Quotation (SQ) form — thin wrapper over the shared sales transaction form,
 * pinned to transaction code "SLS.SQ". Model + layout live in
 * sls-quotation-form-model.ts / sales-transaction-form.tsx (§3 reuse).
 */

import { SalesTransactionForm } from './sales-transaction-form';
import {
  defaultSlsQuotationForm,
  fromSlsQuotation,
  toSlsQuotationPayload,
  type SlsQuotationFormData,
} from './sls-quotation-form-model';

export type { SlsQuotationFormData } from './sls-quotation-form-model';
export { defaultSlsQuotationForm, fromSlsQuotation, toSlsQuotationPayload };

export function SlsQuotationForm(props: {
  data: SlsQuotationFormData;
  onChange: (d: SlsQuotationFormData) => void;
  saving?: boolean;
  allowedCreationStatuses?: string[];
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  return <SalesTransactionForm {...props} transactionCode="SLS.SQ" />;
}
