'use client';

import * as React from 'react';
import { Input } from '@/components/ui/input';

export interface ComboboxInputProps {
  value: string;
  onChange: (v: string) => void;
  onCommit?: () => void;
  options?: string[];
  placeholder?: string;
  autoFocus?: boolean;
}

/**
 * Free-text input with filterable inline dropdown.
 * Options are filtered as the user types; picking an option commits immediately.
 * When `options` is empty the component behaves like a plain Input.
 */
export function ComboboxInput({
  value,
  onChange,
  onCommit,
  options = [],
  placeholder,
  autoFocus,
}: ComboboxInputProps) {
  const [open, setOpen] = React.useState(false);
  const [highlight, setHighlight] = React.useState(-1);

  const filtered = React.useMemo(
    () => options.filter((o) => o.toLowerCase().includes(value.toLowerCase())),
    [options, value],
  );
  const showDrop = open && filtered.length > 0;

  const pick = (opt: string) => {
    onChange(opt);
    setOpen(false);
    onCommit?.();
  };

  return (
    <div style={{ position: 'relative' }}>
      <Input
        autoFocus={autoFocus}
        value={value}
        placeholder={placeholder}
        onChange={(e) => {
          onChange(e.target.value);
          setOpen(true);
          setHighlight(-1);
        }}
        onFocus={() => setOpen(true)}
        onBlur={() => setTimeout(() => setOpen(false), 120)}
        onKeyDown={(e) => {
          if (e.key === 'ArrowDown') {
            e.preventDefault();
            setHighlight((h) => Math.min(h + 1, filtered.length - 1));
          } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            setHighlight((h) => Math.max(h - 1, 0));
          } else if (e.key === 'Enter') {
            e.preventDefault();
            if (highlight >= 0 && filtered[highlight]) {
              pick(filtered[highlight]);
            } else {
              setOpen(false);
              onCommit?.();
            }
          } else if (e.key === 'Escape' || e.key === 'Tab') {
            setOpen(false);
            onCommit?.();
          }
        }}
      />

      {showDrop && (
        <div
          style={{
            position: 'absolute',
            top: '100%',
            left: 0,
            right: 0,
            zIndex: 50,
            background: 'var(--card)',
            border: '1px solid var(--border)',
            borderRadius: 6,
            boxShadow: '0 4px 16px rgba(0,0,0,.12)',
            maxHeight: 160,
            overflowY: 'auto',
            marginTop: 2,
          }}
        >
          {filtered.map((opt, idx) => (
            <div
              key={opt}
              style={{
                padding: '5px 10px',
                fontSize: 'calc(12px * var(--font-scale, 1))',
                cursor: 'pointer',
                background: idx === highlight ? 'var(--bg-hover)' : undefined,
                color: 'var(--fg)',
                userSelect: 'none',
              }}
              onMouseDown={(e) => { e.preventDefault(); pick(opt); }}
              onMouseEnter={() => setHighlight(idx)}
            >
              {opt}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
