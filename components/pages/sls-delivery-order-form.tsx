'use client';

/**
 * Delivery Order (DO) form — thin wrapper over the shared sales transaction form,
 * pinned to transaction code "SLS.DO". Model + layout live in
 * sls-delivery-order-form-model.ts / sales-transaction-form.tsx (§3 reuse).
 */

import { SalesTransactionForm } from './sales-transaction-form';
import {
  defaultSlsDeliveryOrderForm,
  fromSlsDeliveryOrder,
  toSlsDeliveryOrderPayload,
  type SlsDeliveryOrderFormData,
} from './sls-delivery-order-form-model';

export type { SlsDeliveryOrderFormData } from './sls-delivery-order-form-model';
export { defaultSlsDeliveryOrderForm, fromSlsDeliveryOrder, toSlsDeliveryOrderPayload };

export function SlsDeliveryOrderForm(props: {
  data: SlsDeliveryOrderFormData;
  onChange: (d: SlsDeliveryOrderFormData) => void;
  saving?: boolean;
  allowedCreationStatuses?: string[];
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  return <SalesTransactionForm {...props} transactionCode="SLS.DO" />;
}
