// Shared form-data model for giro transactions (Register/Clear × Incoming/
// Outgoing). One model discriminated by kind×type: REGISTER carries free-typed
// instruments; CLEAR carries picked clearing rows + a settlement bankAccountId.
// One default/from/to mapping reused by every giro page (§3 — no duplication).

import { newGiroInstrument, type GiroInstrumentRow } from '@/components/organisms/giro-instruments';
import type { GiroClearingRow } from '@/components/organisms/giro-clearing-lines';
import { TODAY_DEFAULT } from '@/lib/api/form-fields';
import type { FormFieldsConfig } from '@/lib/use-form-fields';
import type { ErpDocumentStatus } from '@/lib/api/fin-journal-entries';
import type {
  CreateGiroEntryPayload,
  ErpGiroEntry,
  GiroKind,
  GiroType,
  GiroRow,
} from '@/lib/api/fin-giro-entries';

export interface GiroFormData {
  id?: string;
  docNumber: string;
  auto: boolean;
  kind: GiroKind;
  type: GiroType;
  entryDate: string;
  partnerId: string;
  partnerLabel?: string;
  branchId: string;
  branchLabel?: string;
  bankAccountId: string;
  bankAccountLabel?: string;
  giroAccountId: string;
  giroAccountLabel?: string;
  currencyId: string;
  exchangeRate: string;
  description: string;
  notes: string;
  status: ErpDocumentStatus;
  postedAt?: string | null;
  /** REGISTER detail. */
  instruments: GiroInstrumentRow[];
  /** CLEAR detail. */
  clearings: GiroClearingRow[];
  /** Values for custom header fields added via Form Builder, keyed by fieldKey. */
  customFields: Record<string, string | number | null>;
}

const todayIso = () => new Date().toISOString().slice(0, 10);

export function defaultGiroForm(kind: GiroKind, type: GiroType): GiroFormData {
  return {
    docNumber: '',
    auto: true,
    kind,
    type,
    entryDate: '',
    partnerId: '',
    branchId: '',
    bankAccountId: '',
    giroAccountId: '',
    currencyId: '',
    exchangeRate: '1',
    description: '',
    notes: '',
    status: 'DRAFT',
    instruments: kind === 'REGISTER' ? [newGiroInstrument()] : [],
    clearings: [],
    customFields: {},
  };
}

const STRUCTURAL_DEFAULT_KEYS = [
  'partnerId', 'branchId', 'bankAccountId', 'giroAccountId',
  'description', 'notes', 'entryDate', 'docNumber', 'currencyId',
] as const;

const STRUCTURAL_LABEL_KEYS: Record<string, keyof GiroFormData> = {
  partnerId: 'partnerLabel',
  branchId: 'branchLabel',
  bankAccountId: 'bankAccountLabel',
  giroAccountId: 'giroAccountLabel',
};

/** Patch of default values for a NEW form from Form Builder config (fill-empty only). */
export function formDefaultsPatch(
  data: GiroFormData,
  config: FormFieldsConfig,
): Partial<GiroFormData> {
  const patch: Partial<GiroFormData> = {};
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

export function fromGiroEntry(r: ErpGiroEntry): GiroFormData {
  return {
    id: r.id,
    docNumber: r.docNumber,
    auto: !!r.autoNumber,
    kind: r.kind,
    type: r.type,
    entryDate: r.entryDate.slice(0, 10),
    partnerId: r.partnerId ?? '',
    branchId: r.branchId,
    bankAccountId: r.bankAccountId ?? '',
    giroAccountId: '',
    currencyId: r.currencyId,
    exchangeRate: r.exchangeRate,
    description: r.description ?? '',
    notes: r.notes ?? '',
    status: r.status,
    postedAt: r.postedAt,
    customFields: (r as unknown as { customFields?: Record<string, string | number | null> }).customFields ?? {},
    instruments: r.kind === 'REGISTER'
      ? r.registeredGiros.map((g) => ({
          key: `gi-${g.id ?? g.lineNo}`,
          giroNumber: g.giroNumber,
          bankName: g.bankName ?? undefined,
          dueDate: g.dueDate.slice(0, 10),
          amount: g.amount ?? '',
          notes: g.notes ?? undefined,
          giroAccountId: g.giroAccountId ?? undefined,
        }))
      : [],
    clearings: r.kind === 'CLEAR'
      ? r.clearedGiros.map((g) => ({
          giroId: g.id,
          giroNumber: g.giroNumber,
          bankName: g.bankName ?? null,
          dueDate: g.dueDate.slice(0, 10),
          amount: g.amount ?? '',
          clearedDate: (g.clearedDate ?? '').slice(0, 10),
        }))
      : [],
  };
}

export function toGiroPayload(d: GiroFormData): CreateGiroEntryPayload {
  const rows: GiroRow[] = d.kind === 'REGISTER'
    ? d.instruments
        .filter((l) => l.giroNumber.trim() && Number(l.amount) > 0)
        .map((l) => ({
          giroNumber: l.giroNumber,
          bankName: l.bankName || undefined,
          dueDate: l.dueDate,
          amount: l.amount || '0',
          notes: l.notes || undefined,
          giroAccountId: l.giroAccountId || undefined,
        }))
    : d.clearings
        .filter((c) => c.giroId && c.clearedDate)
        .map((c) => ({ giroId: c.giroId, clearedDate: c.clearedDate }));

  return {
    auto: d.auto,
    docNumber: d.auto ? undefined : d.docNumber || undefined,
    kind: d.kind,
    type: d.type,
    branchId: d.branchId,
    partnerId: d.partnerId || undefined,
    entryDate: d.entryDate,
    bankAccountId: d.bankAccountId || undefined,
    giroAccountId: d.giroAccountId || undefined,
    currencyId: d.currencyId,
    exchangeRate: d.exchangeRate || '1',
    description: d.description || undefined,
    notes: d.notes || undefined,
    rows,
  };
}
