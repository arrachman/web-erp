'use client';

import * as React from 'react';
import { NumInput } from '@/components/molecules/num-input';

export interface StepperInputProps
  extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'value' | 'onChange'> {
  value: string;
  onChange: (raw: string) => void;
  step?: number;
  min?: number;
  max?: number;
  decimals?: number;
}

/** Numeric stepper: NumInput flanked by − and + buttons. */
export function StepperInput({
  value,
  onChange,
  step = 1,
  min,
  max,
  decimals = 0,
  ...rest
}: StepperInputProps) {
  const current = parseFloat(value) || 0;

  const clamp = (n: number): number => {
    let v = n;
    if (min !== undefined && v < min) v = min;
    if (max !== undefined && v > max) v = max;
    return v;
  };

  const adjust = (delta: number) => {
    const next = clamp(current + delta);
    onChange(decimals === 0 ? String(next) : next.toFixed(decimals));
  };

  const btnStyle: React.CSSProperties = {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    width: 26,
    height: 26,
    border: '1px solid var(--border)',
    borderRadius: 4,
    background: 'var(--card)',
    color: 'var(--fg)',
    cursor: 'pointer',
    fontSize: 16,
    lineHeight: 1,
    userSelect: 'none',
    flexShrink: 0,
  };

  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 2 }}>
      <button
        type="button"
        tabIndex={-1}
        style={btnStyle}
        onClick={() => adjust(-step)}
        aria-label="Kurang"
      >
        −
      </button>
      <NumInput
        value={value}
        onChange={onChange}
        decimals={decimals}
        className="text-center"
        {...rest}
      />
      <button
        type="button"
        tabIndex={-1}
        style={btnStyle}
        onClick={() => adjust(step)}
        aria-label="Tambah"
      >
        +
      </button>
    </div>
  );
}
