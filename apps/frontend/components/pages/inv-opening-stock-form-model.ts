// Form-data model for the inventory opening-stock transaction form. Header +
// opening-stock lines. Mirrors the stock-movement form model, but value-based:
// it carries a currency + exchange rate, lines require unitCost, and the
// inventory GL account is resolved server-side (never in the form/payload).

import {
  newInvOpeningStockLine,
  type InvOpeningStockLineRow,
} from '@/components/organisms/inv-opening-stock-lines';
import { TODAY_DEFAULT, type ErpFormField } from '@/lib/api/form-fields';
import type { FormFieldsConfig } from '@/lib/use-form-fields';
import type {
  CreateInvOpeningStockPayload,
  ErpDocumentStatus,
  ErpInvOpeningStock,
} from '@/lib/api/inv-opening-stocks';

export interface InvOpeningStockFormData {
  id?: string;
  docNumber: string;
  auto: boolean;
  openingDate: string;
  branchId: string;
  branchLabel?: string;
  warehouseId: string;
  warehouseLabel?: string;
  locationId: string;
  locationLabel?: string;
  currencyId: string;
  currencyLabel?: string;
  exchangeRate: string;
  description: string;
  notes: string;
  status: ErpDocumentStatus;
  postedAt?: string | null;
  lines: InvOpeningStockLineRow[];
  /** Values for custom header fields added via Form Builder, keyed by fieldKey. */
  customFields: Record<string, string | number | null>;
}

const todayIso = () => new Date().toISOString().slice(0, 10);

export function defaultInvOpeningStockForm(): InvOpeningStockFormData {
  return {
    docNumber: '',
    auto: true,
    // Date driven by Form Builder default (Kosong / Hari ini / Tanggal tetap).
    openingDate: '',
    branchId: '',
    warehouseId: '',
    locationId: '',
    currencyId: '',
    exchangeRate: '1',
    description: '',
    notes: '',
    status: 'DRAFT',
    lines: [newInvOpeningStockLine()],
    customFields: {},
  };
}

/**
 * Built-in fallback header layout used until Form Builder config loads (or when
 * the transaction has no saved config) — prevents an empty-header flash. The
 * authoritative layout always comes from the API (seeded for INV.IB).
 */
export const DEFAULT_INV_OPENING_FORM_FIELDS: ErpFormField[] = [
  { fieldKey: 'description', kind: 'STRUCTURAL', label: 'Uraian', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 0, columnSlot: 'LEFT' },
  { fieldKey: 'branchId', kind: 'STRUCTURAL', label: 'Cabang', fieldType: 'BRANCH', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'CENTER' },
  { fieldKey: 'warehouseId', kind: 'STRUCTURAL', label: 'Gudang', fieldType: 'LOOKUP', lookupSource: 'warehouses', isRequired: true, isVisible: true, sortOrder: 1, columnSlot: 'CENTER' },
  { fieldKey: 'locationId', kind: 'STRUCTURAL', label: 'Lokasi', fieldType: 'LOCATION', isRequired: false, isVisible: true, sortOrder: 2, columnSlot: 'CENTER' },
  { fieldKey: 'openingDate', kind: 'STRUCTURAL', label: 'Tanggal', fieldType: 'DATE', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'RIGHT', defaultValue: TODAY_DEFAULT },
  { fieldKey: 'docNumber', kind: 'STRUCTURAL', label: 'No Transaksi', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'RIGHT' },
  { fieldKey: 'currencyId', kind: 'STRUCTURAL', label: 'Uang', fieldType: 'CURRENCY', isRequired: true, isVisible: true, sortOrder: 2, columnSlot: 'RIGHT' },
];

/** Structural keys whose default value maps straight onto InvOpeningStockFormData. */
const STRUCTURAL_DEFAULT_KEYS = [
  'branchId', 'warehouseId', 'locationId', 'currencyId',
  'openingDate', 'docNumber', 'description',
] as const;

const STRUCTURAL_LABEL_KEYS: Record<string, keyof InvOpeningStockFormData> = {
  branchId: 'branchLabel',
  warehouseId: 'warehouseLabel',
  locationId: 'locationLabel',
  currencyId: 'currencyLabel',
};

/** Patch of default values for a NEW form, derived from Form Builder config (fill-empty only). */
export function formDefaultsPatch(
  data: InvOpeningStockFormData,
  config: FormFieldsConfig,
): Partial<InvOpeningStockFormData> {
  const patch: Partial<InvOpeningStockFormData> = {};
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

export function fromInvOpeningStock(r: ErpInvOpeningStock): InvOpeningStockFormData {
  return {
    id: r.id,
    docNumber: r.docNumber,
    auto: !!r.autoNumber,
    openingDate: r.openingDate.slice(0, 10),
    branchId: r.branchId,
    branchLabel: r.branch?.name,
    warehouseId: r.warehouseId,
    warehouseLabel: r.warehouse?.name,
    locationId: r.locationId ?? '',
    locationLabel: r.location?.name,
    currencyId: r.currencyId,
    currencyLabel: r.currency?.name,
    exchangeRate: r.exchangeRate ?? '1',
    description: r.description ?? '',
    notes: r.notes ?? '',
    status: r.status,
    postedAt: r.postedAt,
    customFields: {},
    lines: r.lines.map((l) => ({
      key: `iob-${l.id ?? l.lineNo}`,
      itemId: l.itemId,
      itemLabel: l.item?.name,
      quantity: l.quantity,
      unitId: l.unitId,
      unitLabel: l.unit?.name,
      unitCost: l.unitCost ?? '',
      warehouseId: l.warehouseId ?? undefined,
      warehouseLabel: l.warehouse?.name,
      inventoryAccountId: l.inventoryAccountId != null ? String(l.inventoryAccountId) : undefined,
      inventoryAccountLabel: l.inventoryAccount?.name,
      costCenterId: l.costCenterId ?? undefined,
      notes: l.notes ?? undefined,
    })),
  };
}

export function toInvOpeningStockPayload(d: InvOpeningStockFormData): CreateInvOpeningStockPayload {
  return {
    auto: d.auto,
    docNumber: d.auto ? undefined : d.docNumber || undefined,
    openingDate: d.openingDate,
    branchId: d.branchId,
    warehouseId: d.warehouseId,
    locationId: d.locationId || undefined,
    currencyId: d.currencyId,
    exchangeRate: d.exchangeRate || '1',
    description: d.description || undefined,
    notes: d.notes || undefined,
    lines: d.lines
      .filter((l) => l.itemId && Number(l.quantity) > 0)
      .map((l, i) => ({
        itemId: l.itemId,
        quantity: l.quantity,
        unitId: l.unitId,
        unitCost: l.unitCost || '0',
        warehouseId: l.warehouseId || undefined,
        inventoryAccountId: l.inventoryAccountId || undefined,
        costCenterId: l.costCenterId || undefined,
        notes: l.notes || undefined,
        lineNo: i + 1,
      })),
  };
}
