'use client';

/**
 * Type-aware "default value" editor for a Kustomisasi-Grid column. Mirrors the
 * Form Builder's DefaultValueEditor but keys off the grid `ColumnType` instead of
 * the form `fieldType`, and resolves lookup loaders through the grid registry.
 * Stores the picked id in `defaultValue` and its display label in
 * `defaultValueLabel` (so the value reads back nicely on reopen + in the grid).
 */

import * as React from 'react';
import { Input } from '@/components/ui/input';
import { NumInput } from '@/components/molecules/num-input';
import { DateInput } from '@/components/ui/date-input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SearchSelect } from '@/components/molecules/search-select';
import { loadAccountOptionsCoded, loadPartnerOptions } from '@/components/pages/items-form-lookups';
import { gridLookupLoader } from '@/lib/grid-lookup-loaders';
import { inferColumnType, type ColumnType, type ErpGridColumn } from '@/lib/api/transaction-grids';
import type { SearchSelectProps } from '@/components/molecules/search-select-types';

type LoaderFn = SearchSelectProps['loadOptions'];

/** Column types whose default value is a numeric literal. */
const NUMERIC_TYPES: readonly ColumnType[] = ['number', 'currency', 'decimal', 'percent', 'stepper', 'discount'];

export function effectiveColumnType(col: ErpGridColumn): ColumnType {
  return col.columnType ?? inferColumnType(col.labelFormatter, col.cellRenderer, col.cellEditor, col.dataType);
}

/** Default-value editor. `onChange(value, label)` — label only meaningful for lookups. */
export function GridColumnDefaultEditor({
  col,
  onChange,
}: {
  col: ErpGridColumn;
  onChange: (value: string | null, label?: string | null) => void;
}) {
  const type = effectiveColumnType(col);
  const value = col.defaultValue ?? '';

  if (type === 'lookup' || type === 'account_picker' || type === 'partner_picker') {
    const loader = (type === 'account_picker'
      ? loadAccountOptionsCoded
      : type === 'partner_picker'
        ? loadPartnerOptions
        : gridLookupLoader(col.lookupSource, col.lookupDefaultFilter, col.lookupDefaultSort)) as LoaderFn;
    return (
      <SearchSelect
        placeholder="Pilih nilai default…"
        value={value}
        initialLabel={col.defaultValueLabel ?? undefined}
        onValueChange={(v) => { if (!v) onChange(null, null); }}
        onPick={(o) => onChange(o.value || null, o.label ?? null)}
        loadOptions={loader}
      />
    );
  }

  if (NUMERIC_TYPES.includes(type)) {
    return <NumInput value={value} onChange={(v) => onChange(v || null)} />;
  }

  if (type === 'date' || type === 'datetime') {
    return <DateInput value={value} onChange={(v) => onChange(v || null)} />;
  }

  if (type === 'checkbox') {
    return (
      <BooleanRadio
        value={value === 'true' || value === '1'}
        onValueChange={(v) => onChange(v ? 'true' : 'false')}
        trueLabel="Ya"
        falseLabel="Tidak"
      />
    );
  }

  // text / textarea / combobox / badge / link / rownum
  return (
    <Input
      className="h-8 text-xs"
      placeholder="Nilai default…"
      value={value}
      onChange={(e) => onChange(e.target.value || null)}
    />
  );
}
