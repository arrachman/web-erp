'use client';

/**
 * Sales Order (SO) form — thin wrapper over the shared sales transaction form,
 * pinned to transaction code "SLS.SO". Model + layout live in
 * sls-order-form-model.ts / sales-transaction-form.tsx (§3 reuse).
 */

import { SalesTransactionForm } from './sales-transaction-form';
import {
  defaultSlsOrderForm,
  fromSlsOrder,
  toSlsOrderPayload,
  type SlsOrderFormData,
} from './sls-order-form-model';

export type { SlsOrderFormData } from './sls-order-form-model';
export { defaultSlsOrderForm, fromSlsOrder, toSlsOrderPayload };

export function SlsOrderForm(props: {
  data: SlsOrderFormData;
  onChange: (d: SlsOrderFormData) => void;
  saving?: boolean;
  allowedCreationStatuses?: string[];
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  return <SalesTransactionForm {...props} transactionCode="SLS.SO" />;
}
