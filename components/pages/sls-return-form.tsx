'use client';

/**
 * Sales Return (SR) form — thin wrapper over the shared sales transaction form,
 * pinned to transaction code "SLS.SR". Model + layout live in
 * sls-return-form-model.ts / sales-transaction-form.tsx (§3 reuse).
 */

import { SalesTransactionForm } from './sales-transaction-form';
import {
  defaultSlsReturnForm,
  fromSlsReturn,
  toSlsReturnPayload,
  type SlsReturnFormData,
} from './sls-return-form-model';

export type { SlsReturnFormData } from './sls-return-form-model';
export { defaultSlsReturnForm, fromSlsReturn, toSlsReturnPayload };

export function SlsReturnForm(props: {
  data: SlsReturnFormData;
  onChange: (d: SlsReturnFormData) => void;
  saving?: boolean;
  allowedCreationStatuses?: string[];
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  return <SalesTransactionForm {...props} transactionCode="SLS.SR" />;
}
