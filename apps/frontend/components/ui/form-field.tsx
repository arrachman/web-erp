'use client';

import * as React from 'react';
import { cn } from '@/lib/utils';
import { Label } from './label';

/**
 * Dense ERP field row. Mirrors prototype `.field` — a 110px label
 * column + control + optional help/error text (`.field .help`,
 * `.field.err`). Wrap an Input/Select as `children`.
 */
export interface FormFieldProps extends React.HTMLAttributes<HTMLDivElement> {
  label?: React.ReactNode;
  htmlFor?: string;
  required?: boolean;
  error?: React.ReactNode;
  help?: React.ReactNode;
  /** Element rendered into the control column (e.g. the lookup button). */
  controlAddon?: React.ReactNode;
}

export const FormField = React.forwardRef<HTMLDivElement, FormFieldProps>(
  (
    {
      className,
      label,
      htmlFor,
      required,
      error,
      help,
      controlAddon,
      children,
      ...props
    },
    ref,
  ) => (
    <div
      ref={ref}
      className={cn(
        'grid grid-cols-[110px_1fr] items-center gap-x-3 py-[5px]',
        className,
      )}
      {...props}
    >
      {label != null && (
        <Label htmlFor={htmlFor} required={required}>
          {label}
        </Label>
      )}
      <div className="relative flex items-center">
        {children}
        {controlAddon}
      </div>
      {(error || help) && (
        <p
          className={cn(
            'col-start-2 pt-0.5 text-[11px]',
            error ? 'text-danger' : 'text-[var(--fg-subtle)]',
          )}
        >
          {error ?? help}
        </p>
      )}
    </div>
  ),
);
FormField.displayName = 'FormField';
