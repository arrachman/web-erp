'use client';

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Kbd } from '@/components/ui/kbd';
import { fmtCompact } from '@/lib/mock';
import {
  avatarHue,
  CAT_PILL,
  type Contact,
  initials,
  MONO_FONT,
  Stat,
} from './contact-picker-data';

/**
 * Right preview pane in the contact picker. Sub-organism extracted from
 * `ContactPicker` to keep the parent file under 400 LOC.
 */

export interface ContactPickerPreviewProps {
  active: Contact | undefined;
  onClose: () => void;
  onChoose: (c: Contact) => void;
}

export function ContactPickerPreview({
  active,
  onClose,
  onChoose,
}: ContactPickerPreviewProps): React.ReactElement {
  if (!active) {
    return (
      <aside className="cm-preview">
        <div className="cm-empty" style={{ height: '100%' }}>
          <Icon name="user" size={28} />
          <div className="muted" style={{ marginTop: 8 }}>
            Pilih kontak untuk melihat detail
          </div>
        </div>
      </aside>
    );
  }

  return (
    <aside className="cm-preview">
      <div className="cm-preview-hero">
        <div
          className="cm-av cm-av-lg"
          style={{
            background: `hsl(${avatarHue(active)} 70% 92%)`,
            color: `hsl(${avatarHue(active)} 70% 32%)`,
          }}
        >
          {initials(active.name)}
        </div>
        <div style={{ minWidth: 0 }}>
          <div className="cm-preview-name">{active.name}</div>
          <div
            style={{
              display: 'flex',
              gap: 6,
              alignItems: 'center',
              marginTop: 4,
            }}
          >
            <span
              className="mono"
              style={{
                ...MONO_FONT,
                fontSize: 'calc(11px * var(--font-scale, 1))',
                color: 'var(--fg-muted)',
              }}
            >
              {active.code}
            </span>
            <span className={`pill ${CAT_PILL[active.cat]}`}>{active.cat}</span>
          </div>
        </div>
      </div>

      <div className="cm-stats">
        <Stat
          label="Saldo"
          value={`Rp ${fmtCompact(active.balance)}`}
          accent={active.balance < 0 ? 'danger' : undefined}
        />
        <Stat label="Transaksi" value={`${active.trxCount}`} />
        <Stat label="Trx terakhir" value={active.lastTrx} mono />
      </div>

      <div className="cm-fields">
        <div className="cm-field">
          <span className="muted">Kota</span>
          <span>{active.city}</span>
        </div>
        <div className="cm-field">
          <span className="muted">Telepon</span>
          <span className="mono" style={MONO_FONT}>
            {active.phone}
          </span>
        </div>
        <div className="cm-field">
          <span className="muted">NPWP</span>
          <span className="mono" style={{ ...MONO_FONT, fontSize: 'calc(11.5px * var(--font-scale, 1))' }}>
            {active.npwp}
          </span>
        </div>
      </div>

      <div className="cm-preview-foot">
        <button
          type="button"
          className="btn"
          style={{ flex: 1 }}
          onClick={onClose}
        >
          Batal
        </button>
        <button
          type="button"
          className="btn primary"
          style={{ flex: 1 }}
          onClick={() => onChoose(active)}
        >
          Pilih <Kbd>↵</Kbd>
        </button>
      </div>
    </aside>
  );
}
