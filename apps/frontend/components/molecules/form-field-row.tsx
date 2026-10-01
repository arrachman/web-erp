'use client';

/**
 * Shared header form-field row: left label (fixed width) + control. Used by the
 * dynamic transaction-form renderer for both structural and custom fields so the
 * label column stays aligned regardless of field kind.
 */

import * as React from 'react';

export function FormFieldRow({
  label,
  required,
  fieldKey,
  children,
}: {
  label: string;
  required?: boolean;
  /** Optional field key — rendered as data-field-key on the wrapper so
   *  the parent form can scroll-to / focus-first-error by key. */
  fieldKey?: string;
  children: React.ReactNode;
}) {
  return (
    <label className="flex items-center gap-2" data-field-key={fieldKey ?? undefined}>
      <span className="text-xs text-muted-foreground w-24 shrink-0 text-left">
        {label}
        {required && <span className="text-danger">&nbsp;*</span>}
      </span>
      <div className="flex-1 min-w-0">{children}</div>
    </label>
  );
}
