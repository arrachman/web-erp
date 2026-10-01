// Form-data model for the Return Receipt (RNR) transaction form. Header + item lines.
// Mirrors sls-order-form-model.ts with RNR-specific extra fields.

import { newSlsItemLine, type SlsItemLineRow } from '@/components/organisms/sls-item-lines';
import { TODAY_DEFAULT, type ErpFormField } from '@/lib/api/form-fields';
import type { FormFieldsConfig } from '@/lib/use-form-fields';
import type {
  CreateSlsReturnReceiptPayload,
  ErpDocumentStatus,
  ErpPriceMode,
  ErpSlsReturnReceipt,
} from '@/lib/api/sls-return-receipts';

export interface SlsReturnReceiptFormData {
  id?: string;
  docNumber: string;
  auto: boolean;
  docDate: string;
  dueDate: string;
  customerId: string;
  customerLabel?: string;
  branchId: string;
  branchLabel?: string;
  locationId: string;
  locationLabel?: string;
  warehouseId: string;
  warehouseLabel?: string;
  salesDeptId: string;
  salesDeptLabel?: string;
  paymentTermId: string;
  paymentTermLabel?: string;
  currencyId: string;
  exchangeRate: string;
  priceMode: ErpPriceMode;
  // RNR-specific fields
  invoiceId: string;
  returnId: string;
  settlementStatus: string;
  description: string;
  referenceNo: string;
  notes: string;
  discountPercent: string;
  discountAmount: string;
  tax1Amount: string;
  tax2Amount: string;
  otherCostAmount: string;
  receivableAccountId: string;
  status: ErpDocumentStatus;
  postedAt?: string | null;
  grandTotal?: string;
  lines: SlsItemLineRow[];
  customFields: Record<string, string | number | null>;
}

const todayIso = () => new Date().toISOString().slice(0, 10);

export function defaultSlsReturnReceiptForm(): SlsReturnReceiptFormData {
  return {
    docNumber: '',
    auto: true,
    docDate: '',
    dueDate: '',
    customerId: '',
    branchId: '',
    locationId: '',
    warehouseId: '',
    salesDeptId: '',
    paymentTermId: '',
    currencyId: '',
    exchangeRate: '1',
    priceMode: 'TAX_EXCLUSIVE',
    invoiceId: '',
    returnId: '',
    settlementStatus: '',
    description: '',
    referenceNo: '',
    notes: '',
    discountPercent: '',
    discountAmount: '',
    tax1Amount: '',
    tax2Amount: '',
    otherCostAmount: '',
    receivableAccountId: '',
    status: 'DRAFT',
    lines: [newSlsItemLine()],
    customFields: {},
  };
}

export const DEFAULT_SLS_RETURN_RECEIPT_FORM_FIELDS: ErpFormField[] = [
  { fieldKey: 'customerId', kind: 'STRUCTURAL', label: 'Pelanggan', fieldType: 'PARTNER', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'LEFT' },
  { fieldKey: 'description', kind: 'STRUCTURAL', label: 'Uraian', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'LEFT' },
  { fieldKey: 'referenceNo', kind: 'STRUCTURAL', label: 'No Referensi', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 2, columnSlot: 'LEFT' },
  { fieldKey: 'branchId', kind: 'STRUCTURAL', label: 'Cabang', fieldType: 'BRANCH', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'CENTER' },
  { fieldKey: 'locationId', kind: 'STRUCTURAL', label: 'Lokasi', fieldType: 'LOCATION', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'CENTER' },
  { fieldKey: 'warehouseId', kind: 'STRUCTURAL', label: 'Gudang', fieldType: 'LOOKUP', lookupSource: 'warehouses', isRequired: false, isVisible: true, sortOrder: 2, columnSlot: 'CENTER' },
  { fieldKey: 'docDate', kind: 'STRUCTURAL', label: 'Tanggal', fieldType: 'DATE', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'RIGHT' },
  { fieldKey: 'docNumber', kind: 'STRUCTURAL', label: 'No Transaksi', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'RIGHT' },
  { fieldKey: 'currencyId', kind: 'STRUCTURAL', label: 'Uang', fieldType: 'CURRENCY', isRequired: true, isVisible: true, sortOrder: 2, columnSlot: 'RIGHT' },
  { fieldKey: 'paymentTermId', kind: 'STRUCTURAL', label: 'Termin', fieldType: 'LOOKUP', lookupSource: 'payment-terms', isRequired: false, isVisible: true, sortOrder: 3, columnSlot: 'RIGHT' },
  { fieldKey: 'dueDate', kind: 'STRUCTURAL', label: 'Jatuh Tempo', fieldType: 'DATE', isRequired: false, isVisible: true, sortOrder: 4, columnSlot: 'RIGHT' },
];

const STRUCTURAL_DEFAULT_KEYS = [
  'customerId', 'branchId', 'locationId', 'warehouseId', 'paymentTermId',
  'currencyId', 'docDate', 'docNumber', 'description', 'referenceNo', 'dueDate',
] as const;

const STRUCTURAL_LABEL_KEYS: Record<string, keyof SlsReturnReceiptFormData> = {
  customerId: 'customerLabel',
  branchId: 'branchLabel',
  locationId: 'locationLabel',
  warehouseId: 'warehouseLabel',
  paymentTermId: 'paymentTermLabel',
};

export function formDefaultsPatch(
  data: SlsReturnReceiptFormData,
  config: FormFieldsConfig,
): Partial<SlsReturnReceiptFormData> {
  const patch: Partial<SlsReturnReceiptFormData> = {};
  const customPatch: Record<string, string | number | null> = {};
  const isEmpty = (v: unknown) => v == null || v === '';

  for (const key of Object.keys(config.byKey)) {
    const f = config.byKey[key];
    if (isEmpty(f.defaultValue)) continue;
    const val = f.fieldType === 'DATE' && f.defaultValue === TODAY_DEFAULT ? todayIso() : f.defaultValue!;
    if (f.kind === 'CUSTOM') {
      if (isEmpty(data.customFields[key])) customPatch[key] = val;
    } else if ((STRUCTURAL_DEFAULT_KEYS as readonly string[]).includes(key)) {
      if (isEmpty((data as unknown as Record<string, unknown>)[key])) {
        (patch as Record<string, unknown>)[key] = val;
        const labelKey = STRUCTURAL_LABEL_KEYS[key];
        if (labelKey && f.defaultValueLabel) {
          (patch as Record<string, unknown>)[labelKey] = f.defaultValueLabel;
        }
      }
    }
  }
  if (Object.keys(customPatch).length > 0) {
    patch.customFields = { ...data.customFields, ...customPatch };
  }
  return patch;
}

export function fromSlsReturnReceipt(r: ErpSlsReturnReceipt): SlsReturnReceiptFormData {
  return {
    id: r.id,
    docNumber: r.docNumber,
    auto: !!r.autoNumber,
    docDate: r.docDate.slice(0, 10),
    dueDate: r.dueDate ? r.dueDate.slice(0, 10) : '',
    customerId: r.customerId ?? '',
    customerLabel: r.customer?.name,
    branchId: r.branchId,
    branchLabel: r.branch?.name,
    locationId: r.locationId ?? '',
    locationLabel: r.location?.name,
    warehouseId: r.warehouseId ?? '',
    warehouseLabel: r.warehouse?.name,
    salesDeptId: (r as unknown as { salesDeptId?: string }).salesDeptId ?? '',
    paymentTermId: r.paymentTermId ?? '',
    paymentTermLabel: r.paymentTerm?.name,
    currencyId: r.currencyId,
    exchangeRate: r.exchangeRate,
    priceMode: r.priceMode,
    invoiceId: r.invoiceId ?? '',
    returnId: r.returnId ?? '',
    settlementStatus: r.settlementStatus ?? '',
    description: r.description ?? '',
    referenceNo: r.referenceNo ?? '',
    notes: r.notes ?? '',
    discountPercent: r.discountPercent ?? '',
    discountAmount: r.discountAmount ?? '',
    tax1Amount: r.tax1Amount ?? '',
    tax2Amount: r.tax2Amount ?? '',
    otherCostAmount: r.otherCostAmount ?? '',
    receivableAccountId: r.receivableAccountId ?? '',
    status: r.status,
    postedAt: r.postedAt,
    grandTotal: r.grandTotal,
    customFields: (r.customFields as Record<string, string | number | null>) ?? {},
    lines: r.lines.map((l) => ({
      key: `sl-${l.id ?? l.lineNo}`,
      itemId: l.itemId,
      itemLabel: l.item?.name,
      quantity: l.quantity,
      unitId: l.unitId,
      unitLabel: l.unit?.name,
      unitPrice: l.unitPrice,
      discountPercent: l.discountPercent ?? undefined,
      discountAmount: l.discountAmount ?? undefined,
      tax1Id: l.tax1Id ?? undefined,
      tax1Label: l.tax1?.name,
      tax2Id: l.tax2Id ?? undefined,
      warehouseId: l.warehouseId ?? undefined,
      warehouseLabel: l.warehouse?.name,
      notes: l.notes ?? undefined,
      costCenterId: l.costCenterId ?? undefined,
      divisionId: l.divisionId ?? undefined,
      subdivisionId: l.subdivisionId ?? undefined,
      projectId: l.projectId ?? undefined,
      customFields: (l.customFields as Record<string, unknown>) ?? undefined,
    })),
  };
}

export function toSlsReturnReceiptPayload(d: SlsReturnReceiptFormData): CreateSlsReturnReceiptPayload {
  const hasCustomFields = Object.keys(d.customFields).length > 0;
  return {
    auto: d.auto,
    docNumber: d.auto ? undefined : d.docNumber || undefined,
    docDate: d.docDate,
    dueDate: d.dueDate || undefined,
    branchId: d.branchId,
    locationId: d.locationId || undefined,
    warehouseId: d.warehouseId || undefined,
    customerId: d.customerId || undefined,
    paymentTermId: d.paymentTermId || undefined,
    receivableAccountId: d.receivableAccountId || undefined,
    currencyId: d.currencyId,
    exchangeRate: d.exchangeRate || '1',
    priceMode: d.priceMode,
    invoiceId: d.invoiceId || undefined,
    returnId: d.returnId || undefined,
    description: d.description || undefined,
    referenceNo: d.referenceNo || undefined,
    notes: d.notes || undefined,
    discountPercent: d.discountPercent || undefined,
    discountAmount: d.discountAmount || undefined,
    tax1Amount: d.tax1Amount || undefined,
    tax2Amount: d.tax2Amount || undefined,
    otherCostAmount: d.otherCostAmount || undefined,
    customFields: hasCustomFields ? (d.customFields as Record<string, unknown>) : undefined,
    lines: d.lines
      .filter((l) => l.itemId && Number(l.quantity) > 0)
      .map((l, i) => ({
        itemId: l.itemId,
        quantity: l.quantity,
        unitId: l.unitId,
        unitPrice: l.unitPrice || '0',
        discountPercent: l.discountPercent || undefined,
        discountAmount: l.discountAmount || undefined,
        tax1Id: l.tax1Id || undefined,
        tax2Id: l.tax2Id || undefined,
        warehouseId: l.warehouseId || undefined,
        costCenterId: l.costCenterId || undefined,
        divisionId: l.divisionId || undefined,
        subdivisionId: l.subdivisionId || undefined,
        projectId: l.projectId || undefined,
        notes: l.notes || undefined,
        customFields: l.customFields ? (l.customFields as Record<string, unknown>) : undefined,
        lineNo: i + 1,
      })),
  };
}
