'use client';

/**
 * Purchase Invoice (PI) form — thin wrapper over the shared purchasing
 * transaction form, pinned to transaction code "PUR.PI". The form-data shape,
 * defaults, and layout are reused from the Purchase Order model (§3 reuse);
 * only the from/to mapping is PI-specific (pur-invoice-form-model.ts).
 */

import { PurchaseTransactionForm } from './purchase-transaction-form';
import { defaultPurOrderForm, type PurOrderFormData } from './pur-order-form-model';
import { fromPurInvoice, toPurInvoicePayload } from './pur-invoice-form-model';

export type { PurOrderFormData } from './pur-order-form-model';
export { defaultPurOrderForm, fromPurInvoice, toPurInvoicePayload };

export function PurInvoiceForm(props: {
  data: PurOrderFormData;
  onChange: (d: PurOrderFormData) => void;
  saving?: boolean;
  allowedCreationStatuses?: string[];
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  return <PurchaseTransactionForm {...props} transactionCode="PUR.PI" />;
}
