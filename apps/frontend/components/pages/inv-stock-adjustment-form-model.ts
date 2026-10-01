// Form-data model for the inventory stock-adjustment transaction form. Header +
// adjustment lines. Mirrors the stock-movement form model, but quantity-based
// with a per-line direction (INCREASE/DECREASE): no currency / price / discount /
// tax / total. `inventoryAccountId`/`contraAccountId` resolve server-side.

import {
  newInvStockAdjustmentLine,
  type InvStockAdjustmentLineRow,
} from '@/components/organisms/inv-stock-adjustment-lines';
import { TODAY_DEFAULT, type ErpFormField } from '@/lib/api/form-fields';
import type { FormFieldsConfig } from '@/lib/use-form-fields';
import type {
  CreateInvStockAdjustmentPayload,
  ErpDocumentStatus,
  ErpInvAdjustmentDirection,
  ErpInvStockAdjustment,
} from '@/lib/api/inv-stock-adjustments';

export interface InvStockAdjustmentFormData {
  id?: string;
  docNumber: string;
  auto: boolean;
  adjustmentDate: string;
  branchId: string;
  branchLabel?: string;
  warehouseId: string;
  warehouseLabel?: string;
  description: string;
  notes: string;
  status: ErpDocumentStatus;
  postedAt?: string | null;
  lines: InvStockAdjustmentLineRow[];
  /** Values for custom header fields added via Form Builder, keyed by fieldKey. */
  customFields: Record<string, string | number | null>;
}

const todayIso = () => new Date().toISOString().slice(0, 10);

export function defaultInvStockAdjustmentForm(): InvStockAdjustmentFormData {
  return {
    docNumber: '',
    auto: true,
    // Date driven by Form Builder default (Kosong / Hari ini / Tanggal tetap).
    adjustmentDate: '',
    branchId: '',
    warehouseId: '',
    description: '',
    notes: '',
    status: 'DRAFT',
    lines: [newInvStockAdjustmentLine()],
    customFields: {},
  };
}

/**
 * Built-in fallback header layout used until Form Builder config loads (or when
 * the adjustment code has no saved config) — prevents an empty-header flash. The
 * authoritative layout always comes from the API (seeded for INV.SA).
 */
export const DEFAULT_INV_ADJUSTMENT_FORM_FIELDS: ErpFormField[] = [
  { fieldKey: 'description', kind: 'STRUCTURAL', label: 'Uraian', fieldType: 'TEXT', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'LEFT' },
  { fieldKey: 'branchId', kind: 'STRUCTURAL', label: 'Cabang', fieldType: 'BRANCH', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'CENTER' },
  { fieldKey: 'warehouseId', kind: 'STRUCTURAL', label: 'Gudang', fieldType: 'LOOKUP', lookupSource: 'warehouses', isRequired: true, isVisible: true, sortOrder: 1, columnSlot: 'CENTER' },
  { fieldKey: 'adjustmentDate', kind: 'STRUCTURAL', label: 'Tanggal', fieldType: 'DATE', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'RIGHT', defaultValue: TODAY_DEFAULT },
  { fieldKey: 'docNumber', kind: 'STRUCTURAL', label: 'No Transaksi', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'RIGHT' },
];

/** Structural keys whose default value maps straight onto InvStockAdjustmentFormData. */
const STRUCTURAL_DEFAULT_KEYS = [
  'branchId', 'warehouseId', 'adjustmentDate', 'docNumber', 'description',
] as const;

const STRUCTURAL_LABEL_KEYS: Record<string, keyof InvStockAdjustmentFormData> = {
  branchId: 'branchLabel',
  warehouseId: 'warehouseLabel',
};

/** Patch of default values for a NEW form, derived from Form Builder config (fill-empty only). */
export function formDefaultsPatch(
  data: InvStockAdjustmentFormData,
  config: FormFieldsConfig,
): Partial<InvStockAdjustmentFormData> {
  const patch: Partial<InvStockAdjustmentFormData> = {};
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

export function fromInvStockAdjustment(r: ErpInvStockAdjustment): InvStockAdjustmentFormData {
  return {
    id: r.id,
    docNumber: r.docNumber,
    auto: !!r.autoNumber,
    adjustmentDate: r.adjustmentDate.slice(0, 10),
    branchId: r.branchId,
    branchLabel: r.branch?.name,
    warehouseId: r.warehouseId,
    warehouseLabel: r.warehouse?.name,
    description: r.description ?? '',
    notes: r.notes ?? '',
    status: r.status,
    postedAt: r.postedAt,
    customFields: {},
    lines: r.lines.map((l) => ({
      key: `ial-${l.id ?? l.lineNo}`,
      itemId: l.itemId,
      itemLabel: l.item?.name,
      direction: l.direction,
      quantity: l.quantity,
      unitId: l.unitId,
      unitLabel: l.unit?.name,
      unitCost: l.unitCost ?? undefined,
      warehouseId: l.warehouseId ?? undefined,
      warehouseLabel: l.warehouse?.name,
      inventoryAccountId: l.inventoryAccountId != null ? String(l.inventoryAccountId) : undefined,
      inventoryAccountLabel: l.inventoryAccount?.name,
      contraAccountId: l.contraAccountId != null ? String(l.contraAccountId) : undefined,
      contraAccountLabel: l.contraAccount?.name,
      costCenterId: l.costCenterId ?? undefined,
      notes: l.notes ?? undefined,
    })),
  };
}

export function toInvStockAdjustmentPayload(d: InvStockAdjustmentFormData): CreateInvStockAdjustmentPayload {
  return {
    auto: d.auto,
    docNumber: d.auto ? undefined : d.docNumber || undefined,
    adjustmentDate: d.adjustmentDate,
    branchId: d.branchId,
    warehouseId: d.warehouseId,
    description: d.description || undefined,
    notes: d.notes || undefined,
    lines: d.lines
      .filter((l) => l.itemId && Number(l.quantity) > 0)
      .map((l, i) => ({
        itemId: l.itemId,
        direction: (l.direction || 'INCREASE') as ErpInvAdjustmentDirection,
        quantity: l.quantity,
        unitId: l.unitId,
        unitCost: l.unitCost || undefined,
        warehouseId: l.warehouseId || undefined,
        inventoryAccountId: l.inventoryAccountId || undefined,
        contraAccountId: l.contraAccountId || undefined,
        costCenterId: l.costCenterId || undefined,
        notes: l.notes || undefined,
        lineNo: i + 1,
      })),
  };
}
