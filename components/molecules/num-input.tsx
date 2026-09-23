'use client';

/**
 * Numeric input dgn live thousand-separator masking. Format dinamis dari
 * sys_settings group `number-format` (lihat apps/web-erp/CLAUDE.md §2.31).
 *
 * - `value` = raw canonical string ("12345" / "12345.5" / "" / "-12.5").
 * - `onChange(raw)` emit raw canonical (decimal sep = ".") ke parent.
 * - Display = `value` di-format dgn `format.thousandsSep` + `format.decimalSep`.
 * - Caret restore via digit-index (tahan saat user ketik di tengah angka).
 *
 * Atomic tier: Molecule.
 */

import * as React from 'react';
import { Input } from '@/components/ui/input';
import { cn } from '@/lib/utils';
import {
  countDigits,
  formatRawForDisplay,
  offsetAfterNthDigit,
  parseDisplayToRaw,
  useNumberFormat,
} from '@/lib/format';

export interface NumInputProps
  extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'value' | 'onChange'> {
  value: string;
  onChange: (raw: string) => void;
  /** Override jumlah desimal (default = setting). 0 = integer, 2 = currency. */
  decimals?: number;
}

export const NumInput = React.forwardRef<HTMLInputElement, NumInputProps>(
  ({ value, onChange, decimals, className, ...rest }, forwardedRef) => {
    const fmt = useNumberFormat();
    const innerRef = React.useRef<HTMLInputElement>(null);
    const setRef = (node: HTMLInputElement | null) => {
      innerRef.current = node;
      if (typeof forwardedRef === 'function') forwardedRef(node);
      else if (forwardedRef) (forwardedRef as React.MutableRefObject<HTMLInputElement | null>).current = node;
    };
    const pendingCaret = React.useRef<number | null>(null);

    const display = formatRawForDisplay(value ?? '', fmt, decimals);

    React.useLayoutEffect(() => {
      if (pendingCaret.current != null && innerRef.current) {
        try {
          innerRef.current.setSelectionRange(pendingCaret.current, pendingCaret.current);
        } catch {
          /* some input types reject selection — ignore */
        }
        pendingCaret.current = null;
      }
    });

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
      const target = e.target;
      const rawDisplay = target.value;
      const caret = target.selectionStart ?? rawDisplay.length;
      const digitsBeforeCaret = countDigits(rawDisplay, caret);

      const newRaw = parseDisplayToRaw(rawDisplay, fmt);
      const finalDisplay = formatRawForDisplay(newRaw, fmt, decimals);
      pendingCaret.current = offsetAfterNthDigit(finalDisplay, digitsBeforeCaret);
      onChange(newRaw);
    };

    return (
      <Input
        ref={setRef}
        inputMode="decimal"
        value={display}
        onChange={handleChange}
        className={cn('text-right tabular-nums', className)}
        {...rest}
      />
    );
  },
);
NumInput.displayName = 'NumInput';
