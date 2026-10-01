'use client';

import * as React from 'react';

export interface AccountCodePreset {
  label: string;
  segments: number[];
  separator: string;
  hint: string;
}

export const ACCOUNT_CODE_PRESETS: AccountCodePreset[] = [
  { label: 'PSAK 4-2-3 (default)', segments: [4, 2, 3], separator: '.', hint: '11 char  ·  1101.01.001' },
  { label: 'Flat 5 digit', segments: [5], separator: '', hint: '5 char  ·  11101' },
  { label: 'Flat 6 digit', segments: [6], separator: '', hint: '6 char  ·  111010' },
  { label: 'Flat 7 digit', segments: [7], separator: '', hint: '7 char  ·  1110100' },
  { label: 'Grouping 4-3', segments: [4, 3], separator: '.', hint: '8 char  ·  1101.001' },
  { label: 'Grouping 4-3-3', segments: [4, 3, 3], separator: '.', hint: '12 char  ·  1101.001.001' },
  { label: 'Legacy MyERP+ 6-3', segments: [6, 3], separator: '.', hint: '10 char  ·  110101.001' },
];

function arraysEqual(a: number[], b: number[]): boolean {
  return a.length === b.length && a.every((v, i) => v === b[i]);
}

export function AccountCodePresetList({
  segments,
  separator,
  disabled,
  onApply,
}: {
  segments: number[];
  separator: string;
  disabled: boolean;
  onApply: (segs: number[], sep: string) => void;
}) {
  return (
    <div className="card-b" style={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
      {ACCOUNT_CODE_PRESETS.map((p) => {
        const active = arraysEqual(p.segments, segments) && p.separator === separator;
        return (
          <button
            key={p.label}
            type="button"
            onClick={() => onApply(p.segments, p.separator)}
            disabled={disabled}
            style={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              padding: '10px 12px',
              background: active ? 'var(--primary-soft)' : 'transparent',
              border: '1px solid',
              borderColor: active ? 'var(--primary)' : 'var(--border)',
              borderRadius: 'var(--radius)',
              cursor: disabled ? 'not-allowed' : 'pointer',
              opacity: disabled ? 0.6 : 1,
              textAlign: 'left',
            }}
          >
            <span style={{ fontSize: 'calc(12.5px * var(--font-scale, 1))', color: 'var(--fg)' }}>
              {p.label}
            </span>
            <span
              style={{
                fontFamily: 'var(--font-mono)',
                fontSize: 'calc(11.5px * var(--font-scale, 1))',
                color: active ? 'var(--primary)' : 'var(--fg-muted)',
              }}
            >
              {p.hint}
            </span>
          </button>
        );
      })}
    </div>
  );
}
