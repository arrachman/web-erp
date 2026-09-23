'use client';

import * as React from 'react';
import { cn } from '@/lib/utils';

export interface DescriptiveRadioCardOption<T extends string> {
  value: T;
  label: React.ReactNode;
  description: React.ReactNode;
  example?: React.ReactNode;
}

interface DescriptiveRadioCardsProps<T extends string> {
  value: T;
  onValueChange: (value: T) => void;
  options: ReadonlyArray<DescriptiveRadioCardOption<T>>;
  name?: string;
  disabled?: boolean;
  className?: string;
  'aria-label': string;
}

export function DescriptiveRadioCards<T extends string>({
  value,
  onValueChange,
  options,
  name,
  disabled = false,
  className,
  'aria-label': ariaLabel,
}: DescriptiveRadioCardsProps<T>) {
  const generatedName = React.useId();
  const groupName = name ?? generatedName;

  return (
    <div
      role="radiogroup"
      aria-label={ariaLabel}
      className={cn('grid grid-cols-1 gap-2 sm:grid-cols-3', className)}
    >
      {options.map((option) => {
        const isSelected = option.value === value;

        return (
          <label
            key={option.value}
            className={cn(
              'relative flex min-h-28 cursor-pointer flex-col rounded-[var(--radius)] border bg-card p-3 transition-colors',
              'focus-within:outline focus-within:outline-2 focus-within:outline-offset-2 focus-within:outline-[var(--accent)]',
              isSelected
                ? 'border-primary bg-[var(--primary-soft)]'
                : 'border-border hover:border-[var(--fg-muted)] hover:bg-[var(--panel-hover)]',
              disabled && 'cursor-not-allowed opacity-45',
            )}
          >
            <input
              type="radio"
              name={groupName}
              value={option.value}
              checked={isSelected}
              onChange={() => onValueChange(option.value)}
              disabled={disabled}
              className="sr-only"
            />
            <span className="flex items-center gap-2">
              <span
                aria-hidden="true"
                className={cn(
                  'flex h-4 w-4 shrink-0 items-center justify-center rounded-full border',
                  isSelected ? 'border-primary' : 'border-[var(--fg-muted)]',
                )}
              >
                {isSelected && <span className="h-2 w-2 rounded-full bg-primary" />}
              </span>
              <span className="text-xs font-semibold text-foreground">{option.label}</span>
            </span>
            <span className="mt-2 text-[11px] leading-relaxed text-[var(--fg-muted)]">
              {option.description}
            </span>
            {option.example && (
              <span className="mt-auto pt-2 text-[10px] leading-relaxed text-[var(--fg-subtle)]">
                {option.example}
              </span>
            )}
          </label>
        );
      })}
    </div>
  );
}
