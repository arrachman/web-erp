'use client';

/**
 * Delivery Report (DR) form — thin wrapper over the shared sales transaction form,
 * pinned to transaction code "SLS.DR". Model + layout live in
 * sls-delivery-report-form-model.ts / sales-transaction-form.tsx (§3 reuse).
 */

import { SalesTransactionForm } from './sales-transaction-form';
import {
  defaultSlsDeliveryReportForm,
  fromSlsDeliveryReport,
  toSlsDeliveryReportPayload,
  type SlsDeliveryReportFormData,
} from './sls-delivery-report-form-model';

export type { SlsDeliveryReportFormData } from './sls-delivery-report-form-model';
export { defaultSlsDeliveryReportForm, fromSlsDeliveryReport, toSlsDeliveryReportPayload };

export function SlsDeliveryReportForm(props: {
  data: SlsDeliveryReportFormData;
  onChange: (d: SlsDeliveryReportFormData) => void;
  saving?: boolean;
  allowedCreationStatuses?: string[];
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  return <SalesTransactionForm {...props} transactionCode="SLS.DR" />;
}
