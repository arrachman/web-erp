'use client';

/**
 * Purchase Requisition (PR) form — thin wrapper over the shared purchasing
 * transaction form, pinned to transaction code "PUR.PR". The form-data shape,
 * defaults, and layout are reused from the Purchase Order model (§3 reuse);
 * only the from/to mapping is PR-specific (pur-requisition-form-model.ts).
 */

import { PurchaseTransactionForm } from './purchase-transaction-form';
import { defaultPurOrderForm, type PurOrderFormData } from './pur-order-form-model';
import {
  fromPurRequisition,
  toPurRequisitionPayload,
} from './pur-requisition-form-model';

export type { PurOrderFormData } from './pur-order-form-model';
export { defaultPurOrderForm, fromPurRequisition, toPurRequisitionPayload };

export function PurRequisitionForm(props: {
  data: PurOrderFormData;
  onChange: (d: PurOrderFormData) => void;
  saving?: boolean;
  allowedCreationStatuses?: string[];
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  return <PurchaseTransactionForm {...props} transactionCode="PUR.PR" />;
}
