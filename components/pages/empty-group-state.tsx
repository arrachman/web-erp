'use client';

import * as React from 'react';
import { Icon } from '@/components/ui/icons';

interface EmptyGroupProps {
  group: string;
}

export function EmptyGroup({ group }: EmptyGroupProps) {
  return (
    <div
      style={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        gap: 16,
        padding: '48px 24px',
        textAlign: 'center',
      }}
    >
      <div
        style={{
          width: 48,
          height: 48,
          borderRadius: 'var(--radius-lg, 10px)',
          background: 'var(--primary-soft)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          color: 'var(--primary)',
          flexShrink: 0,
        }}
      >
        <Icon name="gear" size={22} />
      </div>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
        <div
          style={{
            fontSize: 'calc(13.5px * var(--font-scale, 1))',
            fontWeight: 600,
            color: 'var(--fg)',
          }}
        >
          Belum ada pengaturan
        </div>
        <div
          style={{
            fontSize: 'calc(12px * var(--font-scale, 1))',
            color: 'var(--fg-muted)',
            lineHeight: 1.55,
            maxWidth: 320,
          }}
        >
          Grup{' '}
          <code
            style={{
              fontFamily: 'var(--font-mono)',
              background: 'var(--panel-2)',
              padding: '1px 5px',
              borderRadius: 4,
            }}
          >
            {group}
          </code>{' '}
          belum memiliki entri pengaturan. Pengaturan ditambahkan oleh administrator sistem.
        </div>
      </div>
    </div>
  );
}
