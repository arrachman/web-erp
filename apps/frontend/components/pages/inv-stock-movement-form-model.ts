// Form-data model for the inventory stock-movement transaction form. Header +
// movement lines. One default/from/to mapping reused by all four movement-type
// wrappers (MR / TS / RS / RF). Mirrors the sales order form model pattern, but
// quantity-based: no currency / price / discount / tax / total.

import {
  newInvStockMovementLine,
  type InvStockMovementLineRow,
} from '@/components/organisms/inv-stock-movement-lines';
import { TODAY_DEFAULT, type ErpFormField } from '@/lib/api/form-fields';
import type { FormFieldsConfig } from '@/lib/use-form-fields';
import type {
  CreateInvStockMovementPayload,
  ErpDocumentStatus,
  ErpInvMovementType,
  ErpInvStockMovement,
} from '@/lib/api/inv-stock-movements';

export interface InvStockMovementFormData {
  id?: string;
  docNumber: string;
  auto: boolean;
  movementType: ErpInvMovementType;
  movementDate: string;
  neededDate: string;
  branchId: string;
  branchLabel?: string;
  locationId: string;
  locationLabel?: string;
  sourceWarehouseId: string;
  sourceWarehouseLabel?: string;
  transitWarehouseId: string;
  transitWarehouseLabel?: string;
  destinationWarehouseId: string;
  destinationWarehouseLabel?: string;
  requestedPartnerId: string;
  requestedPartnerLabel?: string;
  requestedTo: string;
  referenceNo: string;
  referenceDate: string;
  description: string;
  notes: string;
  status: ErpDocumentStatus;
  postedAt?: string | null;
  lines: InvStockMovementLineRow[];
  /** Values for custom header fields added via Form Builder, keyed by fieldKey. */
  customFields: Record<string, string | number | null>;
}

const todayIso = () => new Date().toISOString().slice(0, 10);

export function defaultInvStockMovementForm(movementType: ErpInvMovementType): InvStockMovementFormData {
  return {
    docNumber: '',
    auto: true,
    movementType,
    // Date driven by Form Builder default (Kosong / Hari ini / Tanggal tetap).
    movementDate: '',
    neededDate: '',
    branchId: '',
    locationId: '',
    sourceWarehouseId: '',
    transitWarehouseId: '',
    destinationWarehouseId: '',
    requestedPartnerId: '',
    requestedTo: '',
    referenceNo: '',
    referenceDate: '',
    description: '',
    notes: '',
    status: 'DRAFT',
    lines: [newInvStockMovementLine()],
    customFields: {},
  };
}

/**
 * Built-in fallback header layout used until Form Builder config loads (or when
 * the movement type has no saved config) — prevents an empty-header flash. The
 * authoritative layout always comes from the API (seeded per movement code).
 */
export const DEFAULT_INV_MOVEMENT_FORM_FIELDS: ErpFormField[] = [
  { fieldKey: 'description', kind: 'STRUCTURAL', label: 'Uraian', fieldType: 'TEXT', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'LEFT' },
  { fieldKey: 'requestedTo', kind: 'STRUCTURAL', label: 'Diminta Ke', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'LEFT' },
  { fieldKey: 'branchId', kind: 'STRUCTURAL', label: 'Cabang', fieldType: 'BRANCH', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'CENTER' },
  { fieldKey: 'sourceWarehouseId', kind: 'STRUCTURAL', label: 'Gudang Asal', fieldType: 'LOOKUP', lookupSource: 'warehouses', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'CENTER' },
  { fieldKey: 'destinationWarehouseId', kind: 'STRUCTURAL', label: 'Gudang Tujuan', fieldType: 'LOOKUP', lookupSource: 'warehouses', isRequired: false, isVisible: true, sortOrder: 2, columnSlot: 'CENTER' },
  { fieldKey: 'locationId', kind: 'STRUCTURAL', label: 'Lokasi', fieldType: 'LOCATION', isRequired: false, isVisible: true, sortOrder: 3, columnSlot: 'CENTER' },
  { fieldKey: 'movementDate', kind: 'STRUCTURAL', label: 'Tanggal', fieldType: 'DATE', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'RIGHT', defaultValue: TODAY_DEFAULT },
  { fieldKey: 'docNumber', kind: 'STRUCTURAL', label: 'No Transaksi', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'RIGHT' },
  { fieldKey: 'neededDate', kind: 'STRUCTURAL', label: 'Tgl Dibutuhkan', fieldType: 'DATE', isRequired: false, isVisible: true, sortOrder: 2, columnSlot: 'RIGHT' },
];

/** Structural keys whose default value maps straight onto InvStockMovementFormData. */
const STRUCTURAL_DEFAULT_KEYS = [
  'branchId', 'locationId', 'sourceWarehouseId', 'transitWarehouseId', 'destinationWarehouseId',
  'requestedPartnerId', 'requestedTo', 'movementDate', 'neededDate', 'docNumber',
  'description', 'referenceNo', 'referenceDate',
] as const;

const STRUCTURAL_LABEL_KEYS: Record<string, keyof InvStockMovementFormData> = {
  branchId: 'branchLabel',
  locationId: 'locationLabel',
  sourceWarehouseId: 'sourceWarehouseLabel',
  transitWarehouseId: 'transitWarehouseLabel',
  destinationWarehouseId: 'destinationWarehouseLabel',
  requestedPartnerId: 'requestedPartnerLabel',
};

/** Patch of default values for a NEW form, derived from Form Builder config (fill-empty only). */
export function formDefaultsPatch(
  data: InvStockMovementFormData,
  config: FormFieldsConfig,
): Partial<InvStockMovementFormData> {
  const patch: Partial<InvStockMovementFormData> = {};
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

export function fromInvStockMovement(r: ErpInvStockMovement): InvStockMovementFormData {
  return {
    id: r.id,
    docNumber: r.docNumber,
    auto: !!r.autoNumber,
    movementType: r.movementType,
    movementDate: r.movementDate.slice(0, 10),
    neededDate: r.neededDate ? r.neededDate.slice(0, 10) : '',
    branchId: r.branchId,
    branchLabel: r.branch?.name,
    locationId: r.locationId ?? '',
    locationLabel: r.location?.name,
    sourceWarehouseId: r.sourceWarehouseId ?? '',
    sourceWarehouseLabel: r.sourceWarehouse?.name,
    transitWarehouseId: r.transitWarehouseId ?? '',
    transitWarehouseLabel: r.transitWarehouse?.name,
    destinationWarehouseId: r.destinationWarehouseId ?? '',
    destinationWarehouseLabel: r.destinationWarehouse?.name,
    requestedPartnerId: r.requestedPartnerId ?? '',
    requestedPartnerLabel: r.requestedPartner?.name,
    requestedTo: r.requestedTo ?? '',
    referenceNo: r.referenceNo ?? '',
    referenceDate: r.referenceDate ? r.referenceDate.slice(0, 10) : '',
    description: r.description ?? '',
    notes: r.notes ?? '',
    status: r.status,
    postedAt: r.postedAt,
    customFields: {},
    lines: r.lines.map((l) => ({
      key: `iml-${l.id ?? l.lineNo}`,
      itemId: l.itemId,
      itemLabel: l.item?.name,
      quantity: l.quantity,
      unitId: l.unitId,
      unitLabel: l.unit?.name,
      unitCost: l.unitCost ?? undefined,
      sourceWarehouseId: l.sourceWarehouseId ?? undefined,
      sourceWarehouseLabel: l.sourceWarehouse?.name,
      destinationWarehouseId: l.destinationWarehouseId ?? undefined,
      destinationWarehouseLabel: l.destinationWarehouse?.name,
      costCenterId: l.costCenterId ?? undefined,
      notes: l.notes ?? undefined,
    })),
  };
}

export function toInvStockMovementPayload(d: InvStockMovementFormData): CreateInvStockMovementPayload {
  return {
    auto: d.auto,
    docNumber: d.auto ? undefined : d.docNumber || undefined,
    movementType: d.movementType,
    movementDate: d.movementDate,
    neededDate: d.neededDate || undefined,
    branchId: d.branchId,
    locationId: d.locationId || undefined,
    sourceWarehouseId: d.sourceWarehouseId || undefined,
    transitWarehouseId: d.transitWarehouseId || undefined,
    destinationWarehouseId: d.destinationWarehouseId || undefined,
    requestedPartnerId: d.requestedPartnerId || undefined,
    requestedTo: d.requestedTo || undefined,
    referenceNo: d.referenceNo || undefined,
    referenceDate: d.referenceDate || undefined,
    description: d.description || undefined,
    notes: d.notes || undefined,
    lines: d.lines
      .filter((l) => l.itemId && Number(l.quantity) > 0)
      .map((l, i) => ({
        itemId: l.itemId,
        quantity: l.quantity,
        unitId: l.unitId,
        unitCost: l.unitCost || undefined,
        sourceWarehouseId: l.sourceWarehouseId || undefined,
        destinationWarehouseId: l.destinationWarehouseId || undefined,
        costCenterId: l.costCenterId || undefined,
        notes: l.notes || undefined,
        lineNo: i + 1,
      })),
  };
}
