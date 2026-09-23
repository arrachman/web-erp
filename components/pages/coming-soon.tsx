'use client';

import * as React from 'react';
import { Icon } from '@/components/ui/icons';

interface ComingSoonProps {
  route: string;
}

/** Fallback page for routes not yet ported into this scaffold. */
export function ComingSoon({ route }: ComingSoonProps) {
  const [copied, setCopied] = React.useState(false);

  function handleCopy() {
    navigator.clipboard.writeText(route).catch(() => null);
    setCopied(true);
    setTimeout(() => setCopied(false), 1800);
  }

  return (
    <div
      className="page scrollbar"
      style={{
        overflow: 'auto',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        minHeight: '100%',
        padding: '40px 24px',
      }}
    >
      <div
        style={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          gap: 20,
          maxWidth: 420,
          textAlign: 'center',
        }}
      >
        {/* Icon */}
        <div
          style={{
            width: 56,
            height: 56,
            borderRadius: 'var(--radius-lg)',
            background: 'var(--primary-soft)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: 'var(--primary)',
            flexShrink: 0,
          }}
        >
          <Icon name="layers" size={26} />
        </div>

        {/* Heading */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
          <div
            style={{
              fontSize: 'calc(15px * var(--font-scale, 1))',
              fontWeight: 600,
              color: 'var(--fg)',
              letterSpacing: '-0.01em',
            }}
          >
            Halaman belum tersedia
          </div>
          <div
            style={{
              fontSize: 'calc(13px * var(--font-scale, 1))',
              color: 'var(--fg-muted)',
              lineHeight: 1.55,
            }}
          >
            Modul ini masih dalam pengembangan dan belum di-scaffold di versi ini.
          </div>
        </div>

        {/* Route badge */}
        <button
          type="button"
          onClick={handleCopy}
          title="Salin path"
          style={{
            display: 'inline-flex',
            alignItems: 'center',
            gap: 6,
            padding: '5px 10px',
            borderRadius: 'var(--radius)',
            background: 'var(--panel-2)',
            border: '1px solid var(--border)',
            color: 'var(--fg-muted)',
            fontFamily: 'var(--font-mono)',
            fontSize: 'calc(12px * var(--font-scale, 1))',
            cursor: 'pointer',
            transition: 'background 0.12s, color 0.12s',
          }}
          onMouseEnter={e => {
            (e.currentTarget as HTMLElement).style.background = 'var(--panel-hover)';
          }}
          onMouseLeave={e => {
            (e.currentTarget as HTMLElement).style.background = 'var(--panel-2)';
          }}
        >
          <Icon name={copied ? 'check' : 'file'} size={12} />
          <span>{route}</span>
          <span
            style={{
              color: 'var(--fg-faint)',
              fontSize: 'calc(11px * var(--font-scale, 1))',
              marginLeft: 2,
            }}
          >
            {copied ? 'Tersalin' : 'Salin'}
          </span>
        </button>

        {/* Divider */}
        <div
          style={{
            width: '100%',
            height: 1,
            background: 'var(--border)',
            maxWidth: 260,
          }}
        />

        {/* Footer note */}
        <div
          style={{
            fontSize: 'calc(11px * var(--font-scale, 1))',
            color: 'var(--fg-faint)',
            lineHeight: 1.6,
          }}
        >
          Sementara ini, gunakan menu lain yang sudah tersedia
          atau hubungi tim pengembang jika modul ini mendesak.
        </div>
      </div>
    </div>
  );
}
