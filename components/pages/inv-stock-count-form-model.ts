// Form-data model for the inventory stock-count transaction form (opname).
// Header + count lines. Quantity-based: no currency / price / discount / tax /
// total. Mirrors the stock-movement form model, slimmer header (branch +
// warehouse + date), and lines carry system/physical/variance qty.

import {
  newInvStockCountLine,
  type InvStockCountLineRow,
} from '@/components/organisms/inv-stock-count-lines';
import { TODAY_DEFAULT, type ErpFormField } from '@/lib/api/form-fields';
import type { FormFieldsConfig } from '@/lib/use-form-fields';
import type {
  CreateInvStockCountPayload,
  ErpDocumentStatus,
  ErpInvCountType,
  ErpInvStockCount,
} from '@/lib/api/inv-stock-counts';

export interface InvStockCountFormData {
  id?: string;
  docNumber: string;
  auto: boolean;
  countDate: string;
  countType: ErpInvCountType;
  branchId: string;
  branchLabel?: string;
  warehouseId: string;
  warehouseLabel?: string;
  description: string;
  notes: string;
  status: ErpDocumentStatus;
  lines: InvStockCountLineRow[];
  /** Values for custom header fields added via Form Builder, keyed by fieldKey. */
  customFields: Record<string, string | number | null>;
}

const todayIso = () => new Date().toISOString().slice(0, 10);

export function defaultInvStockCountForm(): InvStockCountFormData {
  return {
    docNumber: '',
    auto: true,
    // Date driven by Form Builder default (Kosong / Hari ini / Tanggal tetap).
    countDate: '',
    countType: 'FULL',
    branchId: '',
    warehouseId: '',
    description: '',
    notes: '',
    status: 'DRAFT',
    lines: [newInvStockCountLine()],
    customFields: {},
  };
}

/**
 * Built-in fallback header layout used until Form Builder config loads — prevents
 * an empty-header flash. The authoritative layout always comes from the API
 * (seeded per transaction code).
 */
export const DEFAULT_INV_COUNT_FORM_FIELDS: ErpFormField[] = [
  { fieldKey: 'description', kind: 'STRUCTURAL', label: 'Uraian', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 0, columnSlot: 'LEFT' },
  { fieldKey: 'branchId', kind: 'STRUCTURAL', label: 'Cabang', fieldType: 'BRANCH', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'CENTER' },
  { fieldKey: 'warehouseId', kind: 'STRUCTURAL', label: 'Gudang', fieldType: 'LOOKUP', lookupSource: 'warehouses', isRequired: true, isVisible: true, sortOrder: 1, columnSlot: 'CENTER' },
  { fieldKey: 'countDate', kind: 'STRUCTURAL', label: 'Tanggal', fieldType: 'DATE', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'RIGHT', defaultValue: TODAY_DEFAULT },
  { fieldKey: 'docNumber', kind: 'STRUCTURAL', label: 'No Transaksi', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'RIGHT' },
];

/** Structural keys whose default value maps straight onto InvStockCountFormData. */
const STRUCTURAL_DEFAULT_KEYS = [
  'branchId', 'warehouseId', 'countDate', 'docNumber', 'description',
] as const;

const STRUCTURAL_LABEL_KEYS: Record<string, keyof InvStockCountFormData> = {
  branchId: 'branchLabel',
  warehouseId: 'warehouseLabel',
};

/** Patch of default values for a NEW form, derived from Form Builder config (fill-empty only). */
export function formDefaultsPatch(
  data: InvStockCountFormData,
  config: FormFieldsConfig,
): Partial<InvStockCountFormData> {
  const patch: Partial<InvStockCountFormData> = {};
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

export function fromInvStockCount(r: ErpInvStockCount): InvStockCountFormData {
  return {
    id: r.id,
    docNumber: r.docNumber,
    auto: !!r.autoNumber,
    countDate: r.countDate.slice(0, 10),
    countType: r.countType ?? 'FULL',
    branchId: r.branchId,
    branchLabel: r.branch?.name,
    warehouseId: r.warehouseId,
    warehouseLabel: r.warehouse?.name,
    description: r.description ?? '',
    notes: r.notes ?? '',
    status: r.status,
    customFields: {},
    lines: r.lines.map((l) => ({
      key: `icl-${l.id ?? l.lineNo}`,
      itemId: l.itemId,
      itemLabel: l.item?.name,
      unitId: l.unitId,
      unitLabel: l.unit?.name,
      warehouseId: l.warehouseId ?? undefined,
      warehouseLabel: l.warehouse?.name,
      systemQty: l.systemQty ?? undefined,
      physicalQty: l.physicalQty,
      goodQty: l.goodQty ?? undefined,
      damagedQty: l.damagedQty ?? undefined,
      varianceQty: l.varianceQty ?? undefined,
      costCenterId: l.costCenterId ?? undefined,
      notes: l.notes ?? undefined,
    })),
  };
}

export function toInvStockCountPayload(d: InvStockCountFormData): CreateInvStockCountPayload {
  return {
    auto: d.auto,
    docNumber: d.auto ? undefined : d.docNumber || undefined,
    countDate: d.countDate,
    countType: d.countType || undefined,
    branchId: d.branchId,
    warehouseId: d.warehouseId,
    description: d.description || undefined,
    notes: d.notes || undefined,
    lines: d.lines
      .filter((l) => l.itemId && Number(l.physicalQty) >= 0 && l.physicalQty !== '')
      .map((l, i) => ({
        itemId: l.itemId,
        unitId: l.unitId,
        warehouseId: l.warehouseId || undefined,
        systemQty: l.systemQty || undefined,
        physicalQty: l.physicalQty,
        goodQty: l.goodQty || undefined,
        damagedQty: l.damagedQty || undefined,
        costCenterId: l.costCenterId || undefined,
        notes: l.notes || undefined,
        lineNo: i + 1,
      })),
  };
}
