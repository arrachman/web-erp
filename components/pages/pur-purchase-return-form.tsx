'use client';

/**
 * Purchase Return (PRT) — thin wrapper. Pins transactionCode="PUR.PRT" so the
 * shared PurchaseTransactionForm loads the correct Form Builder header config and
 * Kustomisasi Grid columns. Business logic (returnType=RETURN_TO_VENDOR) is
 * injected at the page level, not here. Mirrors sls-order-form.tsx pattern.
 */

import * as React from 'react';
import { PurchaseTransactionForm } from './purchase-transaction-form';
import type { PurOrderFormData } from './pur-order-form-model';

interface Props {
  data: PurOrderFormData;
  onChange: (d: PurOrderFormData) => void;
  saving?: boolean;
  allowedCreationStatuses?: string[];
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}

export function PurPurchaseReturnForm(props: Props) {
  return <PurchaseTransactionForm {...props} transactionCode="PUR.PRT" />;
}
