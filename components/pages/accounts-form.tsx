'use client';

/**
 * Account create/edit form — used inside a Modal.
 * Parent-first CoA input: child account scope follows the selected parent.
 * Kode diisi otomatis: increment sibling di bawah parent (atau mulai 1).
 * Atomic tier: Molecule/Organism sub-part.
 */

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { BooleanRadio } from '@/components/ui/radio-group';
import { Badge } from '@/components/ui/badge';
import { SearchSelect, type SearchSelectOption } from '@/components/molecules/search-select';
import { loadCurrencyOptions } from './partners-lookups';
import {
  loadBankOptions,
  loadBranchOptions,
  loadDivisionOptions,
  loadLocationOptions,
} from './items-form-lookups';
import { MultiLookupField } from './items-form-parts';
import {
  ACCOUNT_TYPES,
  CASH_FLOW_CATEGORIES,
  listAccounts,
  getAccountCodeFormat,
} from '@/lib/api/accounts';
import type {
  ErpAccount,
  ErpAccountType,
  ErpAccountKind,
  ErpNormalBalance,
  ErpCashFlowCategory,
  CreateAccountPayload,
  AccountCodeFormat,
} from '@/lib/api/accounts';
import { generateNextAccountCode } from '@/lib/accounts-code-generator';
import { validateForm, type FormErrors } from '@/lib/form-validation';

const NONE = '__none__';

const TYPE_NORMAL_BALANCE_MAP: Record<ErpAccountType, ErpNormalBalance> = {
  ASSET: 'DEBIT',
  EXPENSE: 'DEBIT',
  LIABILITY: 'CREDIT',
  EQUITY: 'CREDIT',
  REVENUE: 'CREDIT',
};

function normalBalanceForAccountType(type: ErpAccountType): ErpNormalBalance {
  return TYPE_NORMAL_BALANCE_MAP[type];
}

function splitAccountCode(code: string, format: AccountCodeFormat): string[] {
  if (!format.separator) {
    let offset = 0;
    return format.segments.map((length) => {
      const part = code.slice(offset, offset + length);
      offset += length;
      return part;
    });
  }
  return code.split(format.separator);
}

function isLeafAccountCode(code: string, format: AccountCodeFormat | null): boolean {
  if (!format || !code) return false;
  const parts = splitAccountCode(code, format);
  const last = parts[parts.length - 1] ?? '';
  return /[1-9]/.test(last);
}

function parentCodeFromLabel(label: string): string | null {
  const code = label.split('—')[0]?.trim() ?? '';
  return code || null;
}

// ─── Account code format cache (sys_settings group "account-code") ────────────

let accountCodeFormatCache: AccountCodeFormat | null = null;
let inflightFormatRequest: Promise<AccountCodeFormat> | null = null;
type FormatListener = (f: AccountCodeFormat | null) => void;
const formatListeners = new Set<FormatListener>();

function buildAccountCodePattern(segments: number[], separator: string): RegExp {
  const escapedSep = separator.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  const parts = segments.map((n) => `\\d{${n}}`);
  return new RegExp(`^${parts.join(escapedSep)}$`);
}

async function fetchAccountCodeFormat(): Promise<AccountCodeFormat> {
  if (inflightFormatRequest) return inflightFormatRequest;
  inflightFormatRequest = getAccountCodeFormat()
    .then((f) => {
      accountCodeFormatCache = f;
      formatListeners.forEach((cb) => cb(f));
      return f;
    })
    .finally(() => {
      inflightFormatRequest = null;
    });
  return inflightFormatRequest;
}

export function invalidateAccountCodeFormatCache(): void {
  accountCodeFormatCache = null;
  inflightFormatRequest = null;
  formatListeners.forEach((cb) => cb(null));
  void fetchAccountCodeFormat().catch(() => {
    /* form will retry on next mount */
  });
}

export function useAccountCodeFormat(): AccountCodeFormat | null {
  const [format, setFormat] = React.useState<AccountCodeFormat | null>(accountCodeFormatCache);
  React.useEffect(() => {
    const listener: FormatListener = (f) => setFormat(f);
    formatListeners.add(listener);
    if (!accountCodeFormatCache) {
      void fetchAccountCodeFormat()
        .then((f) => setFormat(f))
        .catch(() => {
          /* keep null — server validates */
        });
    }
    return () => {
      formatListeners.delete(listener);
    };
  }, []);
  return format;
}

function dimFromRows<T>(
  rows: T[] | undefined | null,
  getId: (r: T) => string,
  getRef: (r: T) => { name: string } | null | undefined,
): { ids: string[]; labels: Record<string, string> } {
  const ids: string[] = [];
  const labels: Record<string, string> = {};
  (rows ?? []).forEach((r) => {
    const id = getId(r);
    ids.push(id);
    const ref = getRef(r);
    if (ref) labels[id] = ref.name;
  });
  return { ids, labels };
}

export interface AccountFormData {
  code: string;
  name: string;
  alias: string;
  accountType: ErpAccountType;
  accountKind: ErpAccountKind;
  normalBalance: ErpNormalBalance;
  cashFlowCategory: string;
  parentId: string;
  parentLabel: string;
  currencyId: string;
  currencyLabel: string;
  bankId: string;
  bankLabel: string;
  bankAccountNo: string;
  branchIds: string[];
  branchLabels: Record<string, string>;
  locationIds: string[];
  locationLabels: Record<string, string>;
  divisionIds: string[];
  divisionLabels: Record<string, string>;
  notes: string;
  isActive: boolean;
}

function emptyPostingDetails(): Pick<
  AccountFormData,
  | 'currencyId'
  | 'currencyLabel'
  | 'bankId'
  | 'bankLabel'
  | 'bankAccountNo'
  | 'branchIds'
  | 'branchLabels'
  | 'locationIds'
  | 'locationLabels'
  | 'divisionIds'
  | 'divisionLabels'
> {
  return {
    currencyId: '',
    currencyLabel: '',
    bankId: '',
    bankLabel: '',
    bankAccountNo: '',
    branchIds: [],
    branchLabels: {},
    locationIds: [],
    locationLabels: {},
    divisionIds: [],
    divisionLabels: {},
  };
}

export const defaultAccountForm = (): AccountFormData => ({
  code: '',
  name: '',
  alias: '',
  accountType: 'ASSET',
  // Empty code is non-leaf until auto-suggest fills a leaf/header code.
  accountKind: 'HEADER',
  normalBalance: 'DEBIT',
  cashFlowCategory: '',
  parentId: '',
  parentLabel: '',
  ...emptyPostingDetails(),
  notes: '',
  isActive: true,
});

export function fromAccount(a: ErpAccount): AccountFormData {
  const b = dimFromRows(a.dimBranches, (r) => r.branchId, (r) => r.branch);
  const l = dimFromRows(a.dimLocations, (r) => r.locationId, (r) => r.location);
  const d = dimFromRows(a.dimDivisions, (r) => r.divisionId, (r) => r.division);
  return {
    code: a.code,
    name: a.name,
    alias: a.alias ?? '',
    accountType: a.type,
    accountKind: a.kind,
    normalBalance: normalBalanceForAccountType(a.type),
    cashFlowCategory: a.cashFlowCategory ?? '',
    parentId: a.parentId ?? '',
    parentLabel: a.parent ? `${a.parent.code} — ${a.parent.name}` : '',
    currencyId: a.currencyId ?? '',
    currencyLabel: a.currency ? `${a.currency.code} — ${a.currency.name}` : '',
    bankId: a.bankId ?? '',
    bankLabel: a.bank
      ? `${a.bank.code} — ${a.bank.name}`
      : a.bankName
        ? a.bankName
        : '',
    bankAccountNo: a.bankAccountNo ?? '',
    branchIds: b.ids,
    branchLabels: b.labels,
    locationIds: l.ids,
    locationLabels: l.labels,
    divisionIds: d.ids,
    divisionLabels: d.labels,
    notes: a.notes ?? '',
    isActive: a.isActive,
  };
}

function accountKindFromCode(
  code: string,
  format: AccountCodeFormat | null,
): ErpAccountKind {
  if (!format || !code) return 'HEADER';
  return isLeafAccountCode(code, format) ? 'POSTABLE' : 'HEADER';
}

export function toAccountPayload(f: AccountFormData): CreateAccountPayload {
  const kind = accountCodeFormatCache
    ? accountKindFromCode(f.code, accountCodeFormatCache)
    : f.accountKind;
  const shouldSendLeafDetails = kind === 'POSTABLE';
  return {
    code: f.code,
    name: f.name,
    alias: f.alias || undefined,
    accountType: f.accountType,
    accountKind: kind,
    normalBalance: normalBalanceForAccountType(f.accountType),
    cashFlowCategory: (f.cashFlowCategory as ErpCashFlowCategory) || undefined,
    parentId: f.parentId || null,
    currencyId: shouldSendLeafDetails ? f.currencyId || null : null,
    bankId: shouldSendLeafDetails ? f.bankId || null : null,
    bankAccountNo: shouldSendLeafDetails ? f.bankAccountNo || undefined : undefined,
    branchIds: shouldSendLeafDetails ? f.branchIds : [],
    locationIds: shouldSendLeafDetails ? f.locationIds : [],
    divisionIds: shouldSendLeafDetails ? f.divisionIds : [],
    notes: f.notes || undefined,
    isActive: f.isActive,
  };
}

async function loadParentOptions(
  search: string,
  page: number,
  limit: number,
): Promise<{ data: SearchSelectOption[]; total: number }> {
  const res = await listAccounts({
    search: search || undefined,
    page,
    limit,
    isActive: true,
    accountKind: 'HEADER',
  });
  return {
    data: res.data.map((a) => ({
      value: a.id,
      label: `${a.code} — ${a.name}`,
      code: a.code,
      meta: a.type,
    })),
    total: res.meta?.total ?? res.data.length,
  };
}

export const validateAccount = (form: AccountFormData) => {
  const fmt = accountCodeFormatCache;
  const isLeaf = isLeafAccountCode(form.code, fmt);
  return validateForm(form, [
    {
      field: 'code',
      label: 'Kode',
      required: true,
      validate: (value) => {
        if (typeof value !== 'string') return undefined;
        if (!fmt) return undefined;
        const re = buildAccountCodePattern(fmt.segments, fmt.separator);
        return re.test(value)
          ? undefined
          : `Format wajib ${fmt.patternSource} (contoh: ${fmt.example})`;
      },
    },
    { field: 'name', label: 'Nama', required: true },
    {
      field: 'currencyId',
      label: 'Mata Uang',
      validate: () => {
        const hasDetails = Boolean(form.currencyId || form.bankId || form.bankAccountNo);
        return fmt && hasDetails && !isLeaf
          ? 'Mata uang/bank hanya boleh untuk kode segmen terakhir'
          : undefined;
      },
    },
    {
      field: 'bankId',
      label: 'Bank',
      validate: () => {
        if (form.bankAccountNo && !form.bankId) {
          return 'Bank wajib diisi bila No. Rekening diisi';
        }
        if (form.bankId && !form.bankAccountNo) {
          return 'No. Rekening wajib diisi bila Bank diisi';
        }
        return undefined;
      },
    },
  ]);
};

export function AccountFormFields({
  data,
  onChange,
  errors = {},
}: {
  data: AccountFormData;
  onChange: (d: AccountFormData) => void;
  errors?: FormErrors<AccountFormData>;
}) {
  const format = useAccountCodeFormat();
  const codePlaceholder = format?.example ?? '1101.01.001';
  const codeMaxLength = format?.maxLength ?? 30;
  const isChild = Boolean(data.parentId);
  const derivedKind = accountKindFromCode(data.code, format);
  const isLeaf = derivedKind === 'POSTABLE';
  const canShowPostingDetails = isLeaf;
  const [suggestingCode, setSuggestingCode] = React.useState(false);
  const dataRef = React.useRef(data);
  dataRef.current = data;
  const onChangeRef = React.useRef(onChange);
  onChangeRef.current = onChange;
  const suggestedForParentRef = React.useRef<string | null>(null);

  const applyCode = React.useCallback((code: string, base?: AccountFormData) => {
    const current = base ?? dataRef.current;
    const nextKind = accountKindFromCode(code, accountCodeFormatCache);
    const patch: AccountFormData = { ...current, code, accountKind: nextKind };
    if (nextKind === 'HEADER') {
      Object.assign(patch, emptyPostingDetails());
    }
    onChangeRef.current(patch);
  }, []);

  const fillNextCode = React.useCallback(
    async (parentId: string, parentCode: string | null, base?: AccountFormData) => {
      const fmt = accountCodeFormatCache;
      if (!fmt) return;
      setSuggestingCode(true);
      try {
        const next = await generateNextAccountCode(parentId || null, parentCode, fmt);
        applyCode(next, base);
        suggestedForParentRef.current = parentId || '__root__';
      } finally {
        setSuggestingCode(false);
      }
    },
    [applyCode],
  );

  // Create form (empty code): auto-suggest next sibling under parent / root.
  React.useEffect(() => {
    if (!format || data.code) return;
    const key = data.parentId || '__root__';
    if (suggestedForParentRef.current === key) return;
    void fillNextCode(data.parentId, parentCodeFromLabel(data.parentLabel), data);
  }, [format, data.code, data.parentId, data.parentLabel, data, fillNextCode]);

  // When format loads after mount (or cache invalidates), re-derive kind from code.
  React.useEffect(() => {
    if (!data.code) return;
    const nextKind = accountKindFromCode(data.code, format);
    if (nextKind === data.accountKind) return;
    const patch: AccountFormData = { ...data, accountKind: nextKind };
    if (nextKind === 'HEADER') {
      Object.assign(patch, emptyPostingDetails());
    }
    onChange(patch);
    // eslint-disable-next-line react-hooks/exhaustive-deps -- format-driven rederive only
  }, [format]);

  const set = (k: keyof AccountFormData, v: string | boolean) => {
    if (k === 'code' && typeof v === 'string') {
      applyCode(v);
      return;
    }
    onChange({ ...data, [k]: v });
  };

  const setAccountType = (type: ErpAccountType) => {
    onChange({ ...data, accountType: type, normalBalance: normalBalanceForAccountType(type) });
  };

  const handleParentPick = (opt: SearchSelectOption) => {
    const parentType = opt.meta as ErpAccountType | undefined;
    if (!parentType) return;
    const parentCode =
      typeof opt.code === 'string' ? opt.code : parentCodeFromLabel(String(opt.label));
    const next: AccountFormData = {
      ...data,
      parentId: opt.value,
      parentLabel: String(opt.label),
      accountType: parentType,
      normalBalance: normalBalanceForAccountType(parentType),
    };
    onChange(next);
    suggestedForParentRef.current = null;
    void fillNextCode(opt.value, parentCode, next);
  };

  const clearParent = (value: string) => {
    if (value) {
      set('parentId', value);
      return;
    }
    const next: AccountFormData = { ...data, parentId: '', parentLabel: '' };
    onChange(next);
    suggestedForParentRef.current = null;
    void fillNextCode('', null, next);
  };

  return (
    <div className="p-4">
      <FormField label="Parent" htmlFor="ac-parent">
        <SearchSelect
          id="ac-parent"
          placeholder="— Root —"
          value={data.parentId}
          initialLabel={data.parentLabel}
          onValueChange={clearParent}
          onPick={handleParentPick}
          loadOptions={loadParentOptions}
        />
      </FormField>
      <FormField label="Kode" htmlFor="ac-code" required error={errors.code}>
        <Input
          id="ac-code"
          value={data.code}
          onChange={(e) => set('code', e.target.value)}
          placeholder={codePlaceholder}
          aria-invalid={!!errors.code}
          maxLength={codeMaxLength}
        />
      </FormField>
      <FormField label="Nama" htmlFor="ac-name" required error={errors.name}>
        <Input
          id="ac-name"
          value={data.name}
          onChange={(e) => set('name', e.target.value)}
          placeholder="Kas Kecil"
          aria-invalid={!!errors.name}
        />
      </FormField>
      <FormField label="Alias" htmlFor="ac-alias">
        <Input
          id="ac-alias"
          value={data.alias}
          onChange={(e) => set('alias', e.target.value)}
          placeholder="Petty Cash"
        />
      </FormField>
      <FormField label="Tipe Akun" htmlFor="ac-type" required>
        <Select
          value={data.accountType}
          onValueChange={(v) => setAccountType(v as ErpAccountType)}
          disabled={isChild}
        >
          <SelectTrigger id="ac-type">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {ACCOUNT_TYPES.map((t) => (
              <SelectItem key={t} value={t}>
                {t}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        {isChild ? (
          <p className="mt-1 text-xs text-muted-foreground italic">Mengikuti tipe parent.</p>
        ) : null}
      </FormField>
      <FormField label="Jenis" htmlFor="ac-kind">
        <div className="flex items-center h-10">
          <Badge variant={derivedKind === 'POSTABLE' ? 'success' : 'default'}>{derivedKind}</Badge>
          <span className="text-xs text-muted-foreground ml-3 italic">
            * Otomatis dari kode — leaf (segmen terakhir non-nol) = POSTABLE, else HEADER
          </span>
        </div>
      </FormField>
      <FormField label="Saldo Normal" htmlFor="ac-nb">
        <div className="flex items-center h-10">
          <Badge variant={data.normalBalance === 'DEBIT' ? 'success' : 'info'}>
            {data.normalBalance}
          </Badge>
          <span className="text-xs text-muted-foreground ml-3 italic">
            * Ditentukan otomatis dari Tipe Akun
          </span>
        </div>
      </FormField>
      <FormField label="Kategori Arus Kas" htmlFor="ac-cf">
        <Select
          value={data.cashFlowCategory || NONE}
          onValueChange={(v) => set('cashFlowCategory', v === NONE ? '' : v)}
        >
          <SelectTrigger id="ac-cf">
            <SelectValue placeholder="— Tidak ada —" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={NONE}>— Tidak ada —</SelectItem>
            {CASH_FLOW_CATEGORIES.map((c) => (
              <SelectItem key={c} value={c}>
                {c}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </FormField>

      {canShowPostingDetails ? (
        <div className="my-3 rounded-md border border-border bg-muted/20 p-3">
          <p className="mb-3 text-sm font-medium text-foreground">Detail Akun Posting</p>
          <FormField label="Mata Uang" htmlFor="ac-currency" error={errors.currencyId}>
            <SearchSelect
              id="ac-currency"
              placeholder="Pilih mata uang"
              value={data.currencyId}
              initialLabel={data.currencyLabel}
              onValueChange={(v) => {
                if (!v) {
                  onChange({ ...data, currencyId: '', currencyLabel: '' });
                  return;
                }
                set('currencyId', v);
              }}
              onPick={(opt) =>
                onChange({
                  ...data,
                  currencyId: opt.value,
                  currencyLabel: String(opt.label),
                })
              }
              loadOptions={loadCurrencyOptions}
              error={!!errors.currencyId}
            />
          </FormField>
          <FormField label="Bank (Cek/Giro)" htmlFor="ac-bank" error={errors.bankId}>
            <SearchSelect
              id="ac-bank"
              placeholder="Pilih bank"
              value={data.bankId}
              initialLabel={data.bankLabel}
              onValueChange={(v) => {
                if (!v) {
                  onChange({ ...data, bankId: '', bankLabel: '' });
                  return;
                }
                set('bankId', v);
              }}
              onPick={(opt) =>
                onChange({
                  ...data,
                  bankId: opt.value,
                  bankLabel: opt.code ? `${opt.code} — ${opt.label}` : String(opt.label),
                })
              }
              loadOptions={loadBankOptions}
              error={!!errors.bankId}
            />
          </FormField>
          <FormField label="No. Rekening Bank" htmlFor="ac-bank-no">
            <Input
              id="ac-bank-no"
              value={data.bankAccountNo}
              onChange={(e) => set('bankAccountNo', e.target.value)}
              placeholder="123-456-7890"
            />
          </FormField>
          <MultiLookupField
            id="ac-branch"
            label="Cabang"
            values={data.branchIds}
            labels={data.branchLabels}
            onChange={(ids, labels) =>
              onChange({ ...data, branchIds: ids, branchLabels: labels })
            }
            loader={loadBranchOptions}
            placeholder="Pilih cabang…"
          />
          <MultiLookupField
            id="ac-location"
            label="Lokasi"
            values={data.locationIds}
            labels={data.locationLabels}
            onChange={(ids, labels) =>
              onChange({ ...data, locationIds: ids, locationLabels: labels })
            }
            loader={loadLocationOptions}
            placeholder="Pilih lokasi…"
          />
          <MultiLookupField
            id="ac-division"
            label="Divisi"
            values={data.divisionIds}
            labels={data.divisionLabels}
            onChange={(ids, labels) =>
              onChange({ ...data, divisionIds: ids, divisionLabels: labels })
            }
            loader={loadDivisionOptions}
            placeholder="Pilih divisi…"
          />
        </div>
      ) : (
        <p className="mb-3 text-xs text-muted-foreground italic">
          Mata uang, bank, no. rekening, cabang, lokasi, dan divisi hanya tersedia untuk akun
          POSTABLE pada segmen kode terakhir.
        </p>
      )}

      <FormField label="Catatan" htmlFor="ac-notes">
        <Input id="ac-notes" value={data.notes} onChange={(e) => set('notes', e.target.value)} />
      </FormField>
      <FormField label="Status" htmlFor="ac-active">
        <BooleanRadio id="ac-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}
