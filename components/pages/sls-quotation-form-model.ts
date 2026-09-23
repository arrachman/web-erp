// Form-data model for the Sales Quotation transaction form. Header + item lines.
// One default/from/to mapping reused by the SQ wrapper. Mirrors SO form model pattern.

import { newSlsItemLine, type SlsItemLineRow } from '@/components/organisms/sls-item-lines';
import { TODAY_DEFAULT, type ErpFormField } from '@/lib/api/form-fields';
import type { FormFieldsConfig } from '@/lib/use-form-fields';
import type {
  CreateSlsQuotationPayload,
  ErpDocumentStatus,
  ErpPriceMode,
  ErpSlsQuotation,
} from '@/lib/api/sls-quotations';

export interface SlsQuotationFormData {
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
  description: string;
  referenceNo: string;
  notes: string;
  /** Header discount — percent takes priority if both set. */
  discountPercent: string;
  discountAmount: string;
  /** Header-level tax amounts (separate from per-line tax). */
  tax1Amount: string;
  tax2Amount: string;
  otherCostAmount: string;
  receivableAccountId: string;
  status: ErpDocumentStatus;
  postedAt?: string | null;
  grandTotal?: string;
  lines: SlsItemLineRow[];
  /** Values for custom header fields added via Form Builder, keyed by fieldKey. */
  customFields: Record<string, string | number | null>;
}

const todayIso = () => new Date().toISOString().slice(0, 10);

export function defaultSlsQuotationForm(): SlsQuotationFormData {
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

/**
 * Built-in fallback header layout used until Form Builder config loads (or when
 * the transaction type has no saved config) — prevents an empty-header flash.
 */
export const DEFAULT_SLS_QUOT_FORM_FIELDS: ErpFormField[] = [
  { fieldKey: 'customerId', kind: 'STRUCTURAL', label: 'Pelanggan', fieldType: 'PARTNER', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'LEFT' },
  { fieldKey: 'description', kind: 'STRUCTURAL', label: 'Uraian', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'LEFT' },
  { fieldKey: 'referenceNo', kind: 'STRUCTURAL', label: 'No Referensi', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 2, columnSlot: 'LEFT' },
  { fieldKey: 'branchId', kind: 'STRUCTURAL', label: 'Cabang', fieldType: 'BRANCH', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'CENTER' },
  { fieldKey: 'locationId', kind: 'STRUCTURAL', label: 'Lokasi', fieldType: 'LOCATION', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'CENTER' },
  { fieldKey: 'warehouseId', kind: 'STRUCTURAL', label: 'Gudang', fieldType: 'LOOKUP', lookupSource: 'warehouses', isRequired: false, isVisible: true, sortOrder: 2, columnSlot: 'CENTER' },
  { fieldKey: 'salesDeptId', kind: 'STRUCTURAL', label: 'Divisi Jual', fieldType: 'LOOKUP', lookupSource: 'divisions', isRequired: false, isVisible: true, sortOrder: 3, columnSlot: 'CENTER' },
  { fieldKey: 'docDate', kind: 'STRUCTURAL', label: 'Tanggal', fieldType: 'DATE', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'RIGHT' },
  { fieldKey: 'docNumber', kind: 'STRUCTURAL', label: 'No Transaksi', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'RIGHT' },
  { fieldKey: 'currencyId', kind: 'STRUCTURAL', label: 'Uang', fieldType: 'CURRENCY', isRequired: true, isVisible: true, sortOrder: 2, columnSlot: 'RIGHT' },
  { fieldKey: 'paymentTermId', kind: 'STRUCTURAL', label: 'Termin', fieldType: 'LOOKUP', lookupSource: 'payment-terms', isRequired: false, isVisible: true, sortOrder: 3, columnSlot: 'RIGHT' },
  { fieldKey: 'dueDate', kind: 'STRUCTURAL', label: 'Jatuh Tempo', fieldType: 'DATE', isRequired: false, isVisible: true, sortOrder: 4, columnSlot: 'RIGHT' },
];

/** Structural keys whose default value maps straight onto SlsQuotationFormData. */
const STRUCTURAL_DEFAULT_KEYS = [
  'customerId', 'branchId', 'locationId', 'warehouseId', 'salesDeptId', 'paymentTermId',
  'currencyId', 'docDate', 'docNumber', 'description', 'referenceNo', 'dueDate',
] as const;

const STRUCTURAL_LABEL_KEYS: Record<string, keyof SlsQuotationFormData> = {
  customerId: 'customerLabel',
  branchId: 'branchLabel',
  locationId: 'locationLabel',
  warehouseId: 'warehouseLabel',
  salesDeptId: 'salesDeptLabel',
  paymentTermId: 'paymentTermLabel',
};

/** Patch of default values for a NEW form, derived from Form Builder config (fill-empty only). */
export function formDefaultsPatch(
  data: SlsQuotationFormData,
  config: FormFieldsConfig,
): Partial<SlsQuotationFormData> {
  const patch: Partial<SlsQuotationFormData> = {};
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

export function fromSlsQuotation(r: ErpSlsQuotation): SlsQuotationFormData {
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
    salesDeptId: r.salesDeptId ?? '',
    salesDeptLabel: r.salesDept?.name,
    paymentTermId: r.paymentTermId ?? '',
    paymentTermLabel: r.paymentTerm?.name,
    currencyId: r.currencyId,
    exchangeRate: r.exchangeRate,
    priceMode: r.priceMode,
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

export function toSlsQuotationPayload(d: SlsQuotationFormData): CreateSlsQuotationPayload {
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
    salesDeptId: d.salesDeptId || undefined,
    receivableAccountId: d.receivableAccountId || undefined,
    currencyId: d.currencyId,
    exchangeRate: d.exchangeRate || '1',
    priceMode: d.priceMode,
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
