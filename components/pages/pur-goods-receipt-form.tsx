'use client';

/**
 * Goods Receipt (GRN) — thin wrapper. Pins transactionCode="PUR.GRN" so the
 * shared PurchaseTransactionForm loads the correct Form Builder header config and
 * the Kustomisasi Grid columns (which expose acceptedQty/rejectedQty/quarantineQty).
 * Mirrors sls-order-form.tsx / pur-order-form.tsx pattern.
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

export function PurGoodsReceiptForm(props: Props) {
  return <PurchaseTransactionForm {...props} transactionCode="PUR.GRN" />;
}
