'use client';

/**
 * Packing List (PL) form — thin wrapper over the shared sales transaction form,
 * pinned to transaction code "SLS.PL". Model + layout live in
 * sls-packing-list-form-model.ts / sales-transaction-form.tsx (§3 reuse).
 */

import { SalesTransactionForm } from './sales-transaction-form';
import {
  defaultSlsPackingListForm,
  fromSlsPackingList,
  toSlsPackingListPayload,
  type SlsPackingListFormData,
} from './sls-packing-list-form-model';

export type { SlsPackingListFormData } from './sls-packing-list-form-model';
export { defaultSlsPackingListForm, fromSlsPackingList, toSlsPackingListPayload };

export function SlsPackingListForm(props: {
  data: SlsPackingListFormData;
  onChange: (d: SlsPackingListFormData) => void;
  saving?: boolean;
  allowedCreationStatuses?: string[];
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  return <SalesTransactionForm {...props} transactionCode="SLS.PL" />;
}
