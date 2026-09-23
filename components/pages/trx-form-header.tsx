'use client';

/**
 * Page-header strip for <TrxForm>. Renders back button + title +
 * draft pill + action cluster (Batal, Dokumen, Reset, Simpan,
 * Simpan & Baru). Sub-page composite — Pages tier, tightly coupled
 * to `<TrxForm>` so it lives in `pages/`.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Kbd } from '@/components/ui/kbd';
import { type Translator } from '@/lib/mock';
import type { TrxConfig } from './trx-form-config';

export interface TrxFormHeaderProps {
  cfg: TrxConfig;
  t: Translator;
  onBack: () => void;
  onReset: () => void;
  onSave: (andNew: boolean) => void;
}

export function TrxFormHeader({
  cfg,
  t,
  onBack,
  onReset,
  onSave,
}: TrxFormHeaderProps): React.ReactElement {
  return (
    <div className="page-header">
      <button
        type="button"
        className="iconbtn"
        onClick={onBack}
        aria-label="Kembali"
      >
        <Icon name="arrowleft" size={14} />
      </button>
      <h1 className="page-title">
        {t(cfg.label)}
        <span className="code-tag">{cfg.code} · Baru</span>
      </h1>
      <span className="pill warn" style={{ marginLeft: 8 }}>
        <span className="dot" />
        Draft baru
      </span>
      <div className="page-actions">
        <button type="button" className="btn ghost" onClick={onBack}>
          Batal <Kbd>ESC</Kbd>
        </button>
        <button type="button" className="btn">
          <Icon name="file" size={12} /> {t('Dokumen')}
        </button>
        <button type="button" className="btn" onClick={onReset}>
          <Icon name="refresh" size={12} /> {t('Reset')}
        </button>
        <button
          type="button"
          className="btn primary"
          onClick={() => onSave(false)}
        >
          <Icon name="save" size={12} /> {t('Simpan')} <Kbd>⌘S</Kbd>
        </button>
        <div className="btn-split">
          <button type="button" className="btn" onClick={() => onSave(true)}>
            <Icon name="plus" size={12} /> {t('Simpan & Baru')}
          </button>
          <button type="button" className="btn" aria-label="Lainnya">
            <Icon name="chevdown" size={12} />
          </button>
        </div>
      </div>
    </div>
  );
}
