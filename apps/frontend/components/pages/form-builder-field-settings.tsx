'use client';

/**
 * Per-field settings dialog for the Form Builder, shown for EVERY field type.
 * Holds the generic knobs (placeholder, default value, read-only) plus, for
 * lookup-style fields, the lookup source/sort/filter config (LookupConfigSection).
 */

import * as React from 'react';
import { Input } from '@/components/ui/input';
import { Icon } from '@/components/ui/icons';
import { NumInput } from '@/components/molecules/num-input';
import { DateInput } from '@/components/ui/date-input';
import { BooleanRadio, RadioGroup, type RadioOption } from '@/components/ui/radio-group';
import { SearchSelect } from '@/components/molecules/search-select';
import {
  Dialog, DialogTrigger, DialogContent, DialogHeader, DialogTitle, DialogBody, DialogClose,
} from '@/components/ui/dialog';
import {
  loadPartnerOptions, loadAccountOptionsCoded, loadBranchOptions,
  loadLocationOptions, loadCurrencyOptions,
} from '@/components/pages/items-form-lookups';
import { buildLookupLoader, BUILTIN_SOURCE } from '@/lib/lookup-source-registry';
import { TODAY_DEFAULT, type ErpFormField, type FormFieldType } from '@/lib/api/form-fields';
import type { SearchSelectProps } from '@/components/molecules/search-select-types';
import { LookupConfigSection, hasLookupConfig } from './form-builder-lookup-config';

type LoaderFn = SearchSelectProps['loadOptions'];

const LOOKUP_TYPES = ['PARTNER', 'ACCOUNT', 'BRANCH', 'LOCATION', 'CURRENCY', 'LOOKUP'] as const;
const isLookupType = (t: FormFieldType) => (LOOKUP_TYPES as readonly string[]).includes(t);

const BUILTIN_LOADERS: Partial<Record<FormFieldType, LoaderFn>> = {
  PARTNER: loadPartnerOptions,
  ACCOUNT: loadAccountOptionsCoded,
  BRANCH: loadBranchOptions,
  LOCATION: loadLocationOptions,
  CURRENCY: loadCurrencyOptions,
};

type DateMode = 'empty' | 'today' | 'fixed';
const DATE_MODE_OPTIONS: ReadonlyArray<RadioOption<DateMode>> = [
  { value: 'empty', label: 'Kosong' },
  { value: 'today', label: 'Hari ini' },
  { value: 'fixed', label: 'Tanggal tetap' },
];
const isoToday = () => new Date().toISOString().slice(0, 10);

/**
 * Default-value editor for DATE fields: choose "Kosong" (no default),
 * "Hari ini" (dynamic — stores the {@link TODAY_DEFAULT} sentinel, resolved to
 * the current date when a record opens), or "Tanggal tetap" (a fixed ISO date).
 */
function DateDefaultEditor({
  value,
  onChange,
}: {
  value: string;
  onChange: (v: string | null) => void;
}) {
  const mode: DateMode = !value ? 'empty' : value === TODAY_DEFAULT ? 'today' : 'fixed';
  const setMode = (m: DateMode) => {
    if (m === 'empty') onChange(null);
    else if (m === 'today') onChange(TODAY_DEFAULT);
    else onChange(value && value !== TODAY_DEFAULT ? value : isoToday());
  };
  return (
    <div className="flex flex-col gap-2">
      <RadioGroup
        value={mode}
        onValueChange={setMode}
        options={DATE_MODE_OPTIONS}
        aria-label="Mode nilai default tanggal"
      />
      {mode === 'today' && (
        <span className="text-[11px] text-muted-foreground">
          Otomatis terisi tanggal hari ini setiap menambah data baru.
        </span>
      )}
      {mode === 'fixed' && (
        <DateInput
          value={value === TODAY_DEFAULT ? '' : value}
          onChange={(v) => onChange(v || null)}
        />
      )}
    </div>
  );
}

/** Type-aware editor for a field's default value. Stores lookup id / raw string. */
function DefaultValueEditor({
  field,
  onChange,
}: {
  field: ErpFormField;
  onChange: (v: string | null) => void;
}) {
  const value = field.defaultValue ?? '';

  if (isLookupType(field.fieldType)) {
    const hasCfg = !!field.lookupDefaultFilter || !!field.lookupDefaultSort || !!field.lookupSource;
    const builtinSource = BUILTIN_SOURCE[field.fieldType];
    const loader = (hasCfg
      ? buildLookupLoader(
          field.fieldType === 'LOOKUP' ? field.lookupSource : builtinSource,
          field.lookupDefaultFilter,
          field.lookupDefaultSort,
        )
      : BUILTIN_LOADERS[field.fieldType] ?? BUILTIN_LOADERS.PARTNER) as LoaderFn;
    return (
      <SearchSelect
        placeholder="Pilih nilai default…"
        value={value}
        initialLabel={field.defaultValueLabel ?? undefined}
        onValueChange={(v) => onChange(v || null)}
        loadOptions={loader}
      />
    );
  }

  if (field.fieldType === 'DATE') {
    return <DateDefaultEditor value={value} onChange={onChange} />;
  }

  if (field.fieldType === 'NUMBER') {
    return <NumInput value={value} onChange={(v) => onChange(v || null)} />;
  }

  return (
    <Input
      className="h-8 text-xs"
      placeholder="Nilai default…"
      value={value}
      onChange={(e) => onChange(e.target.value || null)}
    />
  );
}

export function FieldSettingsPopover({
  field,
  onUpdate,
  onSave,
  saving,
}: {
  field: ErpFormField;
  onUpdate: (patch: Partial<ErpFormField>) => void;
  /** Persist the whole form config to the DB (page-level save). */
  onSave?: () => void | Promise<void>;
  saving?: boolean;
}) {
  const lookup = isLookupType(field.fieldType);
  const configured =
    !!field.placeholder || !!field.defaultValue || field.isReadonly === true ||
    (lookup && hasLookupConfig(field));

  return (
    <Dialog>
      <DialogTrigger asChild>
        <button
          type="button"
          className={`iconbtn ${configured ? 'text-primary' : 'text-muted-foreground'}`}
          title="Konfigurasi field"
        >
          <Icon name="gear" size={12} />
        </button>
      </DialogTrigger>
      <DialogContent className="w-[520px] max-w-[95vw] max-h-[90vh] flex flex-col">
        <DialogHeader>
          <DialogTitle>Konfigurasi Field — {field.label || field.fieldKey}</DialogTitle>
        </DialogHeader>
        <DialogBody className="flex flex-col gap-5 flex-1 max-h-[calc(90vh-64px)]">

          {/* Placeholder */}
          <div className="flex flex-col gap-1">
            <span className="text-xs text-muted-foreground">Placeholder</span>
            <Input
              className="h-8 text-xs"
              placeholder="Teks petunjuk saat kosong…"
              value={field.placeholder ?? ''}
              onChange={(e) => onUpdate({ placeholder: e.target.value || null })}
            />
          </div>

          {/* Default value */}
          <div className="flex flex-col gap-1">
            <span className="text-xs text-muted-foreground">Nilai default (saat tambah baru)</span>
            <DefaultValueEditor field={field} onChange={(v) => onUpdate({ defaultValue: v })} />
          </div>

          {/* Read-only */}
          <div className="flex items-center justify-between gap-2">
            <span className="text-xs text-muted-foreground">Kunci (read-only)</span>
            <BooleanRadio
              value={field.isReadonly === true}
              onValueChange={(v) => onUpdate({ isReadonly: v })}
              trueLabel="Ya"
              falseLabel="Tidak"
            />
          </div>

          {/* Lookup-specific config */}
          {lookup && (
            <div className="flex flex-col gap-5 border-t border-border pt-4">
              <LookupConfigSection field={field} onUpdate={onUpdate} />
            </div>
          )}

        </DialogBody>

        <div className="flex items-center justify-end gap-2 border-t border-border px-5 py-3">
          <DialogClose asChild>
            <button type="button" className="btn shrink-0">Tutup</button>
          </DialogClose>
          {onSave && (
            <DialogClose asChild>
              <button
                type="button"
                className="btn primary shrink-0"
                disabled={saving}
                onClick={() => { void onSave(); }}
              >
                <Icon name="save" size={13} /> {saving ? 'Menyimpan…' : 'Simpan'}
              </button>
            </DialogClose>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}
