// Form-data model for the inventory price-adjustment transaction form. Header-
// only (no item lines). PA triggers a backend re-costing job; status is
// backend-managed (PENDING → COMPLETED | FAILED). Create-only from the UI.

import { TODAY_DEFAULT, type ErpFormField } from '@/lib/api/form-fields';
import type {
  CreateInvPriceAdjustmentPayload,
  ErpInvCostingMethod,
  ErpInvPriceAdjustment,
  ErpInvPriceAdjustmentStatus,
} from '@/lib/api/inv-price-adjustments';

export interface InvPriceAdjustmentFormData {
  id?: string;
  docNumber: string;
  fromDate: string;
  toDate: string;
  fiscalPeriodId: string;
  warehouseId: string;
  warehouseLabel?: string;
  itemId: string;
  itemLabel?: string;
  costingMethod: ErpInvCostingMethod;
  notes: string;
  status: ErpInvPriceAdjustmentStatus;
}

export function defaultInvPriceAdjustmentForm(): InvPriceAdjustmentFormData {
  return {
    docNumber: '',
    fromDate: '',
    toDate: '',
    fiscalPeriodId: '',
    warehouseId: '',
    itemId: '',
    costingMethod: 'AVG',
    notes: '',
    status: 'PENDING',
  };
}

/**
 * Fallback header layout until Form Builder config loads (or no config saved for
 * INV.PA). CENTER: warehouseId + itemId lookups; RIGHT: fromDate (today default),
 * docNumber (manual), toDate.
 */
export const DEFAULT_INV_PA_FORM_FIELDS: ErpFormField[] = [
  { fieldKey: 'warehouseId', kind: 'STRUCTURAL', label: 'Gudang', fieldType: 'LOOKUP', lookupSource: 'warehouses', isRequired: false, isVisible: true, sortOrder: 0, columnSlot: 'CENTER' },
  { fieldKey: 'itemId', kind: 'STRUCTURAL', label: 'Item', fieldType: 'LOOKUP', lookupSource: 'items', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'CENTER' },
  { fieldKey: 'fromDate', kind: 'STRUCTURAL', label: 'Dari Tanggal', fieldType: 'DATE', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'RIGHT', defaultValue: TODAY_DEFAULT },
  { fieldKey: 'docNumber', kind: 'STRUCTURAL', label: 'No Transaksi', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'RIGHT' },
  { fieldKey: 'toDate', kind: 'STRUCTURAL', label: 'Sampai Tanggal', fieldType: 'DATE', isRequired: false, isVisible: true, sortOrder: 2, columnSlot: 'RIGHT' },
];

export function fromInvPriceAdjustment(r: ErpInvPriceAdjustment): InvPriceAdjustmentFormData {
  return {
    id: r.id,
    docNumber: r.docNumber,
    fromDate: r.fromDate.slice(0, 10),
    toDate: r.toDate ? r.toDate.slice(0, 10) : '',
    fiscalPeriodId: r.fiscalPeriodId ?? '',
    warehouseId: r.warehouseId ?? '',
    warehouseLabel: r.warehouse?.name,
    itemId: r.itemId ?? '',
    itemLabel: r.item?.name,
    costingMethod: r.costingMethod,
    notes: r.notes ?? '',
    status: r.status,
  };
}

export function toInvPriceAdjustmentPayload(
  d: InvPriceAdjustmentFormData,
): CreateInvPriceAdjustmentPayload {
  return {
    docNumber: d.docNumber || undefined,
    fromDate: d.fromDate,
    toDate: d.toDate || undefined,
    fiscalPeriodId: d.fiscalPeriodId || undefined,
    warehouseId: d.warehouseId || undefined,
    itemId: d.itemId || undefined,
    costingMethod: d.costingMethod || undefined,
    notes: d.notes || undefined,
  };
}
