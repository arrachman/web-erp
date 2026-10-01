'use client';

/**
 * Purchase Order (PO) form — thin wrapper over the shared purchasing transaction
 * form, pinned to transaction code "PUR.PO". Model + layout live in
 * pur-order-form-model.ts / purchase-transaction-form.tsx (§3 reuse).
 */

import { PurchaseTransactionForm } from './purchase-transaction-form';
import {
  defaultPurOrderForm,
  fromPurOrder,
  toPurOrderPayload,
  type PurOrderFormData,
} from './pur-order-form-model';

export type { PurOrderFormData } from './pur-order-form-model';
export { defaultPurOrderForm, fromPurOrder, toPurOrderPayload };

export function PurOrderForm(props: {
  data: PurOrderFormData;
  onChange: (d: PurOrderFormData) => void;
  saving?: boolean;
  allowedCreationStatuses?: string[];
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  return <PurchaseTransactionForm {...props} transactionCode="PUR.PO" />;
}
