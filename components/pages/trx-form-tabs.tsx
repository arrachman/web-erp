'use client';

/**
 * Tab bar + Info/Audit Trail panels for <TrxForm>. The Detail panel
 * is rendered by the parent via <TrxLines>; tabs sit above with a
 * CoA quick-pick and a "Tambah Baris" CTA.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Kbd } from '@/components/ui/kbd';
import { ACTIVITY, todayStr, type Translator } from '@/lib/mock';

const MONO: React.CSSProperties = { fontFamily: 'Geist Mono, monospace' };

export const TRX_TABS = ['detail', 'info', 'audit'] as const;
export type TrxTabKey = (typeof TRX_TABS)[number];

export interface TrxFormTabsProps {
  t: Translator;
  tab: TrxTabKey;
  setTab: (k: TrxTabKey) => void;
  linesCount: number;
  coaQuery: string;
  setCoaQuery: (s: string) => void;
  onOpenCoaLookup: () => void;
  onAddLine: () => void;
}

export function TrxFormTabs({
  t,
  tab,
  setTab,
  linesCount,
  coaQuery,
  setCoaQuery,
  onOpenCoaLookup,
  onAddLine,
}: TrxFormTabsProps): React.ReactElement {
  return (
    <div className="tabs">
      <button
        type="button"
        className={`tab ${tab === 'detail' ? 'active' : ''}`}
        onClick={() => setTab('detail')}
      >
        {t('Detail')} <span className="muted">{linesCount}</span>
      </button>
      <button
        type="button"
        className={`tab ${tab === 'info' ? 'active' : ''}`}
        onClick={() => setTab('info')}
      >
        {t('Info')}
      </button>
      <button
        type="button"
        className={`tab ${tab === 'audit' ? 'active' : ''}`}
        onClick={() => setTab('audit')}
      >
        Audit Trail
      </button>
      <div style={{ flex: 1 }} />
      <div
        style={{
          alignSelf: 'center',
          display: 'flex',
          alignItems: 'center',
          gap: 8,
          padding: '0 6px',
        }}
      >
        <span className="muted" style={{ fontSize: 'calc(11.5px * var(--font-scale, 1))' }}>
          {t('Pencarian CoA')}
        </span>
        <div
          className="search-input"
          style={{ width: 240, height: 24, cursor: 'pointer' }}
          onClick={(e) => {
            e.stopPropagation();
            onOpenCoaLookup();
          }}
        >
          <Icon name="search" size={11} />
          <input
            value={coaQuery}
            readOnly
            placeholder="Cari akun untuk tambah baris…"
            style={{ cursor: 'pointer' }}
            onChange={(e) => setCoaQuery(e.target.value)}
          />
        </div>
        <button
          type="button"
          className="btn primary sm"
          onClick={(e) => {
            e.stopPropagation();
            onAddLine();
          }}
        >
          <Icon name="plus" size={11} /> Tambah Baris <Kbd>+</Kbd>
        </button>
      </div>
    </div>
  );
}

export interface TrxInfoPanelProps {
  label: string;
}

export function TrxInfoPanel({ label }: TrxInfoPanelProps): React.ReactElement {
  return (
    <div
      style={{
        padding: 16,
        fontSize: 'calc(12.5px * var(--font-scale, 1))',
        color: 'var(--fg-muted)',
        background: 'var(--panel)',
        flex: 1,
      }}
    >
      Informasi tambahan, lampiran dokumen, dan referensi transaksi
      terkait untuk {label}.
    </div>
  );
}

export function TrxAuditPanel(): React.ReactElement {
  return (
    <div
      style={{
        padding: 16,
        background: 'var(--panel)',
        flex: 1,
        fontSize: 'calc(12.5px * var(--font-scale, 1))',
      }}
    >
      {ACTIVITY.slice(0, 5).map((a, i) => (
        <div
          key={i}
          style={{
            display: 'flex',
            gap: 12,
            padding: '6px 0',
            borderBottom: '1px solid var(--border)',
          }}
        >
          <span className="muted" style={{ ...MONO, minWidth: 80 }}>
            {todayStr} {a.ts}
          </span>
          <span>
            <strong>{a.who}</strong> {a.what}{' '}
            <span style={MONO}>{a.target}</span>
          </span>
        </div>
      ))}
    </div>
  );
}

export function TrxFooterShortcuts(): React.ReactElement {
  return (
    <div className="pager">
      <span className="muted">Pintasan:</span>
      <span>
        <Kbd>⌘S</Kbd> simpan
      </span>
      <span>
        <Kbd>⌘⇧S</Kbd> simpan &amp; baru
      </span>
      <span>
        <Kbd>+</Kbd> tambah baris
      </span>
      <span>
        <Kbd>ESC</Kbd> batal
      </span>
      <div style={{ flex: 1 }} />
      <span className="muted">Terakhir disimpan: belum disimpan</span>
    </div>
  );
}
