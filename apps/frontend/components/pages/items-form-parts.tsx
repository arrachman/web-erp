'use client';

/**
 * Helper komponen + visibility rules untuk items-form-fields.
 * Section header, LookupField (SearchSelect wrapper), NumField (decimal
 * input), YesNoField (BooleanRadio). Atomic tier: Molecule.
 */

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { BooleanRadio } from '@/components/ui/radio-group';
import { NumInput } from '@/components/molecules/num-input';
import { SearchSelect } from '@/components/molecules/search-select';
import { Icon, type IconName } from '@/components/ui/icons';
import type { ErpItemType } from '@/lib/api/items';

export const isStockable = (t: ErpItemType) =>
  t === 'INVENTORY' || t === 'CONSUMABLE' || t === 'ASSET';
export const showsWeight = (t: ErpItemType) => t !== 'SERVICE';

export type LookupLoader = (s: string, p: number, l: number) => Promise<{ data: { value: string; label: string; code?: string; [key: string]: unknown }[]; total: number }>;

export function Section({ title, hint, icon, children }: { title: string; hint?: string; icon?: IconName; children: React.ReactNode }) {
  return (
    <section className="border-t border-border first:border-t-0">
      <header className="flex items-center gap-2 bg-[var(--panel-2)] px-5 py-2">
        {icon && (
          <span className="flex h-5 w-5 shrink-0 items-center justify-center rounded-[var(--radius)] bg-[var(--primary-soft)] text-[var(--primary-soft-fg)]">
            <Icon name={icon} size={12} />
          </span>
        )}
        <h4 className="text-[11px] font-semibold uppercase tracking-wide text-foreground">{title}</h4>
        {hint && <span className="text-[11px] text-[var(--fg-subtle)]">— {hint}</span>}
      </header>
      <div className="grid grid-cols-2 gap-x-6 gap-y-0 px-5 py-3">{children}</div>
    </section>
  );
}

export function LookupField(props: {
  id: string; label: string; value: string; onPick: (v: string) => void;
  loader: LookupLoader; placeholder: string; required?: boolean; initialLabel?: string; error?: boolean;
  /** Optional: called with the full option object on pick (for reading extra fields like conversionFactor). */
  onPickOpt?: (opt: { value: string; label: string; [key: string]: unknown }) => void;
}) {
  return (
    <FormField label={props.label} htmlFor={props.id} required={props.required} error={props.error ? `${props.label} wajib diisi` : undefined}>
      <SearchSelect id={props.id} value={props.value} onValueChange={props.onPick} onPick={props.onPickOpt} placeholder={props.placeholder} loadOptions={props.loader} initialLabel={props.initialLabel} title={props.label} error={props.error} />
    </FormField>
  );
}

/**
 * Multi-select lookup (SearchSelect mode="multi") + removable chips.
 * Labels for picked ids are harvested from loader results (the modal pages
 * through the same loader) merged onto the labels map held in form state,
 * so chips survive section remount and edit-mode prefill.
 */
export function MultiLookupField(props: {
  id: string; label: string; values: string[]; labels: Record<string, string>;
  onChange: (values: string[], labels: Record<string, string>) => void;
  loader: LookupLoader; placeholder: string;
}) {
  const { loader, labels, values, onChange } = props;
  const seenRef = React.useRef<Record<string, string>>({});

  const cachingLoader: LookupLoader = React.useCallback(async (s, p, l) => {
    const res = await loader(s, p, l);
    res.data.forEach((o) => { seenRef.current[o.value] = o.label; });
    return res;
  }, [loader]);

  const handleValues = (next: string[]) => {
    const merged = { ...labels };
    next.forEach((v) => { if (!merged[v] && seenRef.current[v]) merged[v] = seenRef.current[v]; });
    onChange(next, merged);
  };

  return (
    <FormField label={props.label} htmlFor={props.id}>
      <div className="flex flex-col gap-1">
        <SearchSelect id={props.id} mode="multi" values={values} onValuesChange={handleValues} placeholder={props.placeholder} loadOptions={cachingLoader} title={props.label} />
        {values.length > 0 && (
          <div className="flex flex-wrap gap-1">
            {values.map((v) => (
              <span key={v} className="inline-flex items-center gap-1 rounded-[var(--radius)] border border-border bg-[var(--panel-2)] px-1.5 py-0.5 text-[11px] text-foreground">
                {labels[v] ?? seenRef.current[v] ?? `#${v}`}
                <button
                  type="button"
                  aria-label={`Hapus ${labels[v] ?? v}`}
                  className="cursor-pointer text-[var(--fg-subtle)] hover:text-foreground"
                  onClick={() => handleValues(values.filter((x) => x !== v))}
                >
                  ×
                </button>
              </span>
            ))}
          </div>
        )}
      </div>
    </FormField>
  );
}

export function NumField(props: { id: string; label: string; value: string; onChange: (v: string) => void; placeholder?: string; decimals?: number; readOnly?: boolean; help?: string }) {
  return (
    <FormField label={props.label} htmlFor={props.id} help={props.help}>
      <NumInput id={props.id} value={props.value} onChange={props.readOnly ? () => {} : props.onChange} placeholder={props.placeholder ?? '0'} decimals={props.decimals} readOnly={props.readOnly} disabled={props.readOnly} />
    </FormField>
  );
}

export function YesNoField(props: { id: string; label: string; value: boolean; onChange: (v: boolean) => void; help?: string }) {
  return (
    <FormField label={props.label} htmlFor={props.id} help={props.help}>
      <BooleanRadio id={props.id} value={props.value} onValueChange={props.onChange} trueLabel="Ya" falseLabel="Tidak" />
    </FormField>
  );
}
