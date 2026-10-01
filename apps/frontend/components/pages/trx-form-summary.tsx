'use client';

/**
 * Right column of the <TrxForm> header section: Status select +
 * "Ringkasan" card (total debit/sub-total, kredit/pajak, selisih/
 * total, balance pill for journal mode). Sub-page composite for
 * <TrxForm>.
 */

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { fmtIDR, STATUSES, type Translator } from '@/lib/mock';
import type { TrxConfig } from './trx-form-config';
import type { TrxHeadState } from './trx-form-fields';

const MONO: React.CSSProperties = { fontFamily: 'Geist Mono, monospace' };

export interface TrxFormSummaryProps {
  cfg: TrxConfig;
  t: Translator;
  head: TrxHeadState;
  setHead: React.Dispatch<React.SetStateAction<TrxHeadState>>;
  total: number;
  totalKredit: number;
  balanced: boolean;
}

export function TrxFormSummary({
  cfg,
  t,
  head,
  setHead,
  total,
  totalKredit,
  balanced,
}: TrxFormSummaryProps): React.ReactElement {
  return (
    <div>
      <FormField label={t('Status')} required>
        <select
          value={head.status}
          onChange={(e) =>
            setHead((h) => ({ ...h, status: e.target.value }))
          }
        >
          {STATUSES.map((s) => (
            <option key={s}>{s}</option>
          ))}
        </select>
      </FormField>
      <div
        style={{
          marginTop: 8,
          padding: 10,
          background: 'var(--panel-2)',
          borderRadius: 6,
          border: '1px solid var(--border)',
        }}
      >
        <div
          style={{
            fontSize: 'calc(11px * var(--font-scale, 1))',
            color: 'var(--fg-muted)',
            textTransform: 'uppercase',
            letterSpacing: '0.04em',
            marginBottom: 4,
          }}
        >
          Ringkasan
        </div>
        <div
          style={{
            display: 'flex',
            justifyContent: 'space-between',
            fontSize: 'calc(12px * var(--font-scale, 1))',
            padding: '2px 0',
          }}
        >
          <span className="muted">
            {cfg.journal ? 'Total Debit' : 'Sub-total'}
          </span>
          <span style={{ ...MONO, fontVariantNumeric: 'tabular-nums' }}>
            {fmtIDR(total)}
          </span>
        </div>
        <div
          style={{
            display: 'flex',
            justifyContent: 'space-between',
            fontSize: 'calc(12px * var(--font-scale, 1))',
            padding: '2px 0',
          }}
        >
          <span className="muted">
            {cfg.journal ? 'Total Kredit' : 'Pajak (otomatis)'}
          </span>
          <span className="muted" style={MONO}>
            {cfg.journal ? fmtIDR(totalKredit) : '0,00'}
          </span>
        </div>
        <div
          style={{
            borderTop: '1px solid var(--border)',
            marginTop: 4,
            paddingTop: 4,
            display: 'flex',
            justifyContent: 'space-between',
            fontSize: 'calc(13px * var(--font-scale, 1))',
            fontWeight: 600,
          }}
        >
          <span>{cfg.journal ? 'Selisih' : `Total ${head.uang}`}</span>
          <span
            style={{
              ...MONO,
              fontVariantNumeric: 'tabular-nums',
              color: balanced ? 'var(--primary-soft-fg)' : 'var(--danger)',
            }}
          >
            {cfg.journal ? fmtIDR(total - totalKredit) : fmtIDR(total)}
          </span>
        </div>
        {cfg.journal && (
          <div style={{ marginTop: 6 }}>
            <span className={`pill ${balanced ? 'success' : 'danger'}`}>
              <span className="dot" />
              {balanced ? 'Balance' : 'Tidak balance'}
            </span>
          </div>
        )}
      </div>
    </div>
  );
}
