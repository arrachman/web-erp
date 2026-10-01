// Form-data model for the inventory daily-check transaction form. Header +
// check lines. Mirrors the stock-adjustment form model pattern.

import {
  newInvDailyCheckLine,
  type InvDailyCheckLineRow,
} from '@/components/organisms/inv-daily-check-line-model';
import { TODAY_DEFAULT, type ErpFormField } from '@/lib/api/form-fields';
import type { FormFieldsConfig } from '@/lib/use-form-fields';
import type {
  CreateInvDailyCheckPayload,
  ErpDocumentStatus,
  ErpInvDailyCheck,
} from '@/lib/api/inv-daily-checks';

export interface InvDailyCheckFormData {
  id?: string;
  docNumber: string;
  auto: boolean;
  checkDate: string;
  fiscalPeriodId: string;
  branchId: string;
  branchLabel?: string;
  locationId: string;
  locationLabel?: string;
  machineRef: string;
  operatorRef: string;
  description: string;
  notes: string;
  status: ErpDocumentStatus;
  postedAt?: string | null;
  lines: InvDailyCheckLineRow[];
  customFields: Record<string, string | number | null>;
}

const todayIso = () => new Date().toISOString().slice(0, 10);

export function defaultInvDailyCheckForm(): InvDailyCheckFormData {
  return {
    docNumber: '',
    auto: true,
    checkDate: '',
    fiscalPeriodId: '',
    branchId: '',
    locationId: '',
    machineRef: '',
    operatorRef: '',
    description: '',
    notes: '',
    status: 'DRAFT',
    lines: [newInvDailyCheckLine()],
    customFields: {},
  };
}

/**
 * Fallback header layout until Form Builder config loads.
 * LEFT: description, machineRef, operatorRef.
 * CENTER: branchId (req), locationId.
 * RIGHT: checkDate (req, today), docNumber.
 */
export const DEFAULT_INV_DC_FORM_FIELDS: ErpFormField[] = [
  { fieldKey: 'description', kind: 'STRUCTURAL', label: 'Uraian', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 0, columnSlot: 'LEFT' },
  { fieldKey: 'machineRef', kind: 'STRUCTURAL', label: 'Referensi Mesin', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'LEFT' },
  { fieldKey: 'operatorRef', kind: 'STRUCTURAL', label: 'Referensi Operator', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 2, columnSlot: 'LEFT' },
  { fieldKey: 'branchId', kind: 'STRUCTURAL', label: 'Cabang', fieldType: 'BRANCH', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'CENTER' },
  { fieldKey: 'locationId', kind: 'STRUCTURAL', label: 'Lokasi', fieldType: 'LOCATION', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'CENTER' },
  { fieldKey: 'checkDate', kind: 'STRUCTURAL', label: 'Tanggal Cek', fieldType: 'DATE', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'RIGHT', defaultValue: TODAY_DEFAULT },
  { fieldKey: 'docNumber', kind: 'STRUCTURAL', label: 'No Transaksi', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'RIGHT' },
];

/** Structural keys whose default value maps onto InvDailyCheckFormData. */
const STRUCTURAL_DEFAULT_KEYS = [
  'branchId', 'locationId', 'checkDate', 'docNumber', 'description',
  'machineRef', 'operatorRef',
] as const;

const STRUCTURAL_LABEL_KEYS: Record<string, keyof InvDailyCheckFormData> = {
  branchId: 'branchLabel',
  locationId: 'locationLabel',
};

/** Patch of default values for a NEW form, derived from Form Builder config (fill-empty only). */
export function formDefaultsPatch(
  data: InvDailyCheckFormData,
  config: FormFieldsConfig,
): Partial<InvDailyCheckFormData> {
  const patch: Partial<InvDailyCheckFormData> = {};
  const customPatch: Record<string, string | number | null> = {};
  const isEmpty = (v: unknown) => v == null || v === '';

  for (const key of Object.keys(config.byKey)) {
    const f = config.byKey[key];
    if (isEmpty(f.defaultValue)) continue;
    const val =
      f.fieldType === 'DATE' && f.defaultValue === TODAY_DEFAULT
        ? todayIso()
        : f.defaultValue!;
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

export function fromInvDailyCheck(r: ErpInvDailyCheck): InvDailyCheckFormData {
  return {
    id: r.id,
    docNumber: r.docNumber,
    auto: !!r.autoNumber,
    checkDate: r.checkDate.slice(0, 10),
    fiscalPeriodId: r.fiscalPeriodId ?? '',
    branchId: r.branchId,
    branchLabel: r.branch?.name,
    locationId: r.locationId ?? '',
    locationLabel: r.location?.name,
    machineRef: r.machineRef ?? '',
    operatorRef: r.operatorRef ?? '',
    description: r.description ?? '',
    notes: r.notes ?? '',
    status: r.status,
    postedAt: r.postedAt,
    customFields: {},
    lines: r.lines.map((l) => ({
      key: `dcl-${l.id ?? l.lineNo}`,
      itemId: l.itemId,
      itemLabel: l.item?.name,
      quantity: l.quantity,
      unitId: l.unitId,
      unitLabel: l.unit?.name,
      warehouseId: l.warehouseId ?? undefined,
      warehouseLabel: l.warehouse?.name,
      costCenterId: l.costCenterId ?? undefined,
      machineRef: l.machineRef ?? undefined,
      workHours: l.workHours ?? undefined,
      notes: l.notes ?? undefined,
    })),
  };
}

export function toInvDailyCheckPayload(d: InvDailyCheckFormData): CreateInvDailyCheckPayload {
  return {
    auto: d.auto,
    docNumber: d.auto ? undefined : d.docNumber || undefined,
    checkDate: d.checkDate,
    fiscalPeriodId: d.fiscalPeriodId || undefined,
    branchId: d.branchId,
    locationId: d.locationId || undefined,
    machineRef: d.machineRef || undefined,
    operatorRef: d.operatorRef || undefined,
    description: d.description || undefined,
    notes: d.notes || undefined,
    lines: d.lines
      .filter((l) => l.itemId && Number(l.quantity) > 0)
      .map((l, i) => ({
        itemId: l.itemId,
        quantity: l.quantity,
        unitId: l.unitId,
        warehouseId: l.warehouseId || undefined,
        costCenterId: l.costCenterId || undefined,
        machineRef: l.machineRef || undefined,
        workHours: l.workHours || undefined,
        notes: l.notes || undefined,
        lineNo: i + 1,
      })),
  };
}
