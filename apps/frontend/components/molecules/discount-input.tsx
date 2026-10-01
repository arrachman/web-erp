'use client';

import * as React from 'react';
import { NumInput } from '@/components/molecules/num-input';
import { cn } from '@/lib/utils';

export interface DiscountInputProps
  extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'value' | 'onChange'> {
  value: string;
  onChange: (raw: string) => void;
}

/** Percent input clamped 0–100. Displays a trailing '%' suffix. */
export const DiscountInput = React.forwardRef<HTMLInputElement, DiscountInputProps>(
  ({ value, onChange, onBlur, className, ...rest }, ref) => {
    const clamp = (raw: string): string => {
      const n = parseFloat(raw);
      if (Number.isNaN(n)) return raw;
      if (n > 100) return '100';
      if (n < 0) return '0';
      return raw;
    };

    const handleChange = (raw: string) => onChange(clamp(raw));

    const handleBlur = (e: React.FocusEvent<HTMLInputElement>) => {
      onChange(clamp(value));
      (onBlur as React.FocusEventHandler<HTMLInputElement> | undefined)?.(e);
    };

    return (
      <div className="relative flex items-center">
        <NumInput
          ref={ref as React.Ref<HTMLInputElement>}
          value={value}
          onChange={handleChange}
          decimals={2}
          onBlur={handleBlur}
          className={cn('pr-6', className)}
          {...rest}
        />
        <span
          aria-hidden
          style={{
            position: 'absolute',
            right: 8,
            color: 'var(--fg-subtle)',
            fontSize: 'calc(11px * var(--font-scale, 1))',
            pointerEvents: 'none',
            userSelect: 'none',
          }}
        >
          %
        </span>
      </div>
    );
  },
);
DiscountInput.displayName = 'DiscountInput';
