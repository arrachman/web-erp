// Shared form-data model for journal transactions (General/Adjustment/Memorial/
// Opening-Balance/Revaluation). Header + double-entry lines are identical across
// journal types; only `journalType` (and a few UI labels) differ. One default/
// from/to mapping reused by every journal page (§3 — no duplication).

import { newJournalLine, type JournalLineRow } from '@/components/organisms/journal-lines';
import { TODAY_DEFAULT } from '@/lib/api/form-fields';
import type { FormFieldsConfig } from '@/lib/use-form-fields';
import type {
  CreateJournalEntryPayload,
  ErpDocumentStatus,
  ErpJournalEntry,
  ErpJournalType,
} from '@/lib/api/fin-journal-entries';

export interface JournalFormData {
  id?: string;
  docNumber: string;
  auto: boolean;
  journalType: ErpJournalType;
  entryDate: string;
  partnerId: string;
  partnerLabel?: string;
  branchId: string;
  branchLabel?: string;
  locationId: string;
  locationLabel?: string;
  currencyId: string;
  exchangeRate: string;
  description: string;
  notes: string;
  status: ErpDocumentStatus;
  postedAt?: string | null;
  lines: JournalLineRow[];
  /** Values for custom header fields added via Form Builder, keyed by fieldKey. */
  customFields: Record<string, string | number | null>;
}

const todayIso = () => new Date().toISOString().slice(0, 10);

export function defaultJournalForm(journalType: ErpJournalType = 'GENERAL'): JournalFormData {
  return {
    docNumber: '',
    auto: true,
    journalType,
    entryDate: '',
    partnerId: '',
    branchId: '',
    locationId: '',
    currencyId: '',
    exchangeRate: '1',
    description: '',
    notes: '',
    status: 'DRAFT',
    lines: [newJournalLine()],
    customFields: {},
  };
}

const STRUCTURAL_DEFAULT_KEYS = [
  'partnerId', 'branchId', 'locationId',
  'description', 'notes', 'entryDate', 'docNumber', 'currencyId',
] as const;

const STRUCTURAL_LABEL_KEYS: Record<string, keyof JournalFormData> = {
  partnerId: 'partnerLabel',
  branchId: 'branchLabel',
  locationId: 'locationLabel',
};

/** Patch of default values for a NEW form from Form Builder config (fill-empty only). */
export function formDefaultsPatch(
  data: JournalFormData,
  config: FormFieldsConfig,
): Partial<JournalFormData> {
  const patch: Partial<JournalFormData> = {};
  const customPatch: Record<string, string | number | null> = {};
  const isEmpty = (v: unknown) => v == null || v === '';

  for (const key of Object.keys(config.byKey)) {
    const f = config.byKey[key];
    if (isEmpty(f.defaultValue)) continue;
    const val = f.fieldType === 'DATE' && f.defaultValue === TODAY_DEFAULT
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

export function fromJournalEntry(r: ErpJournalEntry): JournalFormData {
  return {
    id: r.id,
    docNumber: r.docNumber,
    auto: !!r.autoNumber,
    journalType: r.journalType,
    entryDate: r.entryDate.slice(0, 10),
    partnerId: r.partnerId ?? '',
    branchId: r.branchId,
    locationId: r.locationId ?? '',
    currencyId: r.currencyId,
    exchangeRate: r.exchangeRate,
    description: r.description,
    notes: r.notes ?? '',
    status: r.status,
    postedAt: r.postedAt,
    customFields: (r as unknown as { customFields?: Record<string, string | number | null> }).customFields ?? {},
    lines: r.lines.map((l) => ({
      key: `jl-${l.id ?? l.lineNo}`,
      accountId: l.accountId,
      debit: l.debit ?? '',
      credit: l.credit ?? '',
      notes: l.notes ?? undefined,
      costCenterId: l.costCenterId ?? undefined,
      divisionId: l.divisionId ?? undefined,
      subdivisionId: l.subdivisionId ?? undefined,
      projectId: l.projectId ?? undefined,
    })),
  };
}

export function toJournalPayload(d: JournalFormData): CreateJournalEntryPayload {
  return {
    auto: d.auto,
    docNumber: d.auto ? undefined : d.docNumber || undefined,
    journalType: d.journalType,
    branchId: d.branchId,
    locationId: d.locationId || undefined,
    entryDate: d.entryDate,
    partnerId: d.partnerId || undefined,
    description: d.description,
    notes: d.notes || undefined,
    currencyId: d.currencyId,
    exchangeRate: d.exchangeRate || '1',
    lines: d.lines
      .filter((l) => l.accountId && (Number(l.debit) > 0 || Number(l.credit) > 0))
      .map((l, i) => ({
        accountId: l.accountId,
        debit: l.debit || '0',
        credit: l.credit || '0',
        notes: l.notes || undefined,
        costCenterId: l.costCenterId || undefined,
        divisionId: l.divisionId || undefined,
        subdivisionId: l.subdivisionId || undefined,
        projectId: l.projectId || undefined,
        lineNo: i + 1,
      })),
  };
}
