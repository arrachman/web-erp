'use client';

import * as React from 'react';
import { cn } from '@/lib/utils';

export type RadioOption<T extends string = string> = {
  value: T;
  label: React.ReactNode;
};

type RadioGroupProps<T extends string = string> = {
  id?: string;
  name?: string;
  value: T;
  onValueChange: (value: T) => void;
  options: ReadonlyArray<RadioOption<T>>;
  disabled?: boolean;
  className?: string;
  'aria-label'?: string;
};

/**
 * Segmented radio group untuk pilihan biner / sangat sedikit opsi
 * (lihat apps/web-erp/CLAUDE.md §2.6). Pakai ini menggantikan `Select`
 * saat opsi hanya 2.
 */
export function RadioGroup<T extends string = string>({
  id,
  name,
  value,
  onValueChange,
  options,
  disabled,
  className,
  ...aria
}: RadioGroupProps<T>) {
  const groupName = React.useId();
  const fieldName = name ?? groupName;
  return (
    <div
      id={id}
      role="radiogroup"
      aria-label={aria['aria-label']}
      className={cn(
        'inline-flex h-[26px] rounded-md border border-border bg-card p-0.5 text-[12.5px]',
        disabled && 'cursor-not-allowed opacity-45',
        className,
      )}
    >
      {options.map((opt) => {
        const selected = opt.value === value;
        return (
          <label
            key={opt.value}
            className={cn(
              'relative flex flex-1 cursor-pointer items-center justify-center gap-1.5 rounded-[4px] px-2.5 transition-colors',
              'focus-within:shadow-[0_0_0_2px_color-mix(in_oklab,var(--primary)_22%,transparent)]',
              selected
                ? 'bg-primary text-primary-foreground'
                : 'text-foreground hover:bg-muted',
              disabled && 'pointer-events-none',
            )}
          >
            <input
              type="radio"
              name={fieldName}
              value={opt.value}
              checked={selected}
              onChange={() => onValueChange(opt.value)}
              disabled={disabled}
              className="sr-only"
            />
            <span>{opt.label}</span>
          </label>
        );
      })}
    </div>
  );
}

/**
 * Helper khusus boolean field (Aktif/Nonaktif, Ya/Tidak, dll).
 */
type BooleanRadioProps = {
  id?: string;
  value: boolean;
  onValueChange: (next: boolean) => void;
  trueLabel?: React.ReactNode;
  falseLabel?: React.ReactNode;
  disabled?: boolean;
  className?: string;
  'aria-label'?: string;
};

export function BooleanRadio({
  id,
  value,
  onValueChange,
  trueLabel = 'Aktif',
  falseLabel = 'Nonaktif',
  disabled,
  className,
  ...aria
}: BooleanRadioProps) {
  return (
    <RadioGroup<'true' | 'false'>
      id={id}
      value={value ? 'true' : 'false'}
      onValueChange={(v) => onValueChange(v === 'true')}
      options={[
        { value: 'true', label: trueLabel },
        { value: 'false', label: falseLabel },
      ]}
      disabled={disabled}
      className={className}
      aria-label={aria['aria-label']}
    />
  );
}
