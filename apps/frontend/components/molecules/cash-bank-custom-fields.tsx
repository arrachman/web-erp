'use client';

/**
 * Renders a single CUSTOM header field (added via Form Builder) for the cash/bank
 * transaction form. Value lives in CashBankFormData.customFields keyed by fieldKey
 * (no fixed DB column). The transaction form renders structural + custom fields in
 * one config-ordered loop, dispatching per field kind.
 */

import * as React from 'react';
import { Input } from '@/components/ui/input';
import { NumInput } from '@/components/molecules/num-input';
import { DateInput } from '@/components/ui/date-input';
import { SearchSelect } from '@/components/molecules/search-select';
import { FormFieldRow } from '@/components/molecules/form-field-row';
import {
  loadPartnerOptions,
  loadAccountOptionsCoded,
  loadBranchOptions,
  loadLocationOptions,
  loadCurrencyOptions,
} from '@/components/pages/items-form-lookups';
import { buildLookupLoader, BUILTIN_SOURCE } from '@/lib/lookup-source-registry';
import type { SearchSelectProps } from '@/components/molecules/search-select-types';
import type { ErpFormField, FormFieldType } from '@/lib/api/form-fields';

type LoaderFn = SearchSelectProps['loadOptions'];

// Fallback loaders for built-in types when no custom config is set.
const BUILTIN_LOADERS: Partial<Record<FormFieldType, LoaderFn>> = {
  PARTNER:  loadPartnerOptions,
  ACCOUNT:  loadAccountOptionsCoded,
  BRANCH:   loadBranchOptions,
  LOCATION: loadLocationOptions,
  CURRENCY: loadCurrencyOptions,
};

function CustomFieldControl({
  field,
  value,
  onChange,
  disabled,
}: {
  field: ErpFormField;
  value: string | number | null;
  onChange: (v: string | number | null) => void;
  disabled?: boolean;
}) {
  const hasLookupConfig = !!field.lookupDefaultFilter || !!field.lookupDefaultSort || !!field.lookupSource;
  const isLookupType = (['PARTNER','ACCOUNT','BRANCH','LOCATION','CURRENCY','LOOKUP'] as string[]).includes(field.fieldType);
  const ro = disabled || field.isReadonly === true;

  if (isLookupType) {
    // Build a configured loader when custom filter/sort/source is set; fall back to built-in loaders.
    const builtinSource = BUILTIN_SOURCE[field.fieldType as FormFieldType];
    const configuredLoader = hasLookupConfig
      ? buildLookupLoader(
          field.fieldType === 'LOOKUP' ? field.lookupSource : builtinSource,
          field.lookupDefaultFilter,
          field.lookupDefaultSort,
        )
      : null;
    const effectiveLoader = configuredLoader ?? BUILTIN_LOADERS[field.fieldType as FormFieldType] ?? BUILTIN_LOADERS.PARTNER!;
    return (
      <SearchSelect
        placeholder={field.placeholder || `Pilih ${field.label.toLowerCase()}…`}
        value={value ? String(value) : ''}
        disabled={ro}
        onValueChange={(v) => onChange(v)}
        loadOptions={effectiveLoader}
      />
    );
  }

  if (field.fieldType === 'DATE') {
    return (
      <DateInput
        value={value ? String(value) : ''}
        placeholder={field.placeholder || undefined}
        disabled={ro}
        onChange={(v) => onChange(v)}
      />
    );
  }

  if (field.fieldType === 'NUMBER') {
    return (
      <NumInput
        value={value != null ? String(value) : ''}
        placeholder={field.placeholder || undefined}
        disabled={ro}
        onChange={(v) => onChange(v)}
      />
    );
  }

  return (
    <Input
      value={value ? String(value) : ''}
      placeholder={field.placeholder || undefined}
      disabled={ro}
      onChange={(e) => onChange(e.target.value)}
    />
  );
}

/** One custom header field row (label + control). */
export function CashBankCustomField({
  field,
  value,
  onChange,
  disabled,
}: {
  field: ErpFormField;
  value: string | number | null;
  onChange: (v: string | number | null) => void;
  disabled?: boolean;
}) {
  return (
    <FormFieldRow label={field.label} required={field.isRequired}>
      <CustomFieldControl field={field} value={value} onChange={onChange} disabled={disabled} />
    </FormFieldRow>
  );
}
