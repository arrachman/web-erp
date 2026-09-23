'use client';

import * as React from 'react';
import { Input } from '@/components/ui/input';
import type { ErpSetting } from '@/lib/api/settings';

interface SettingRowProps {
  setting: ErpSetting;
  value: string;
  dirty: boolean;
  onChange: (v: string) => void;
}

export function SettingRow({ setting, value, dirty, onChange }: SettingRowProps) {
  return (
    <div
      style={{
        display: 'grid',
        gridTemplateColumns: '260px 1fr',
        gap: 16,
        alignItems: 'start',
        padding: '10px 0',
        borderTop: '1px solid var(--border)',
      }}
    >
      <div>
        <label
          htmlFor={`sg-${setting.key}`}
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: 6,
            fontSize: 'calc(12.5px * var(--font-scale, 1))',
            color: 'var(--fg)',
            cursor: 'pointer',
          }}
        >
          {setting.description ?? setting.key}
          {dirty && (
            <span
              style={{
                display: 'inline-block',
                width: 6,
                height: 6,
                borderRadius: '50%',
                background: 'var(--warning, #f59e0b)',
                flexShrink: 0,
              }}
              title="Belum disimpan"
            />
          )}
        </label>
        <div
          style={{
            fontFamily: 'var(--font-mono)',
            fontSize: 'calc(10.5px * var(--font-scale, 1))',
            color: 'var(--fg-faint, var(--fg-muted))',
            marginTop: 2,
          }}
        >
          {setting.key}
        </div>
      </div>
      <Input
        id={`sg-${setting.key}`}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={setting.value ?? ''}
      />
    </div>
  );
}
