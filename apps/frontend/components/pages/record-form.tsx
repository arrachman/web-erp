'use client';

/**
 * RecordForm — full-page create form for any REGISTRY module
 * (master data / dokumen sederhana). Ported from prototype
 * `pages/record-form.jsx`; opened in its own tab via route
 * `<moduleId>-new`, never as popup.
 *
 * Atomic tier: Pages. Strict TS; no `window.*` globals — pulls
 * REGISTRY/STATUSES/Translator from real modules and uses
 * `useTabKey` so keyboard shortcuts auto-detach when the host tab
 * is inactive (prototype `useKey` did not).
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Kbd } from '@/components/ui/kbd';
import { FormField } from '@/components/ui/form-field';
import { REGISTRY, type RegistryCol } from '@/lib/registry';
import { STATUSES, type Translator } from '@/lib/mock';
import { useTabKey } from '@/lib/tab-context';
import { notify } from '@/lib/feedback';

export interface RecordFormProps {
  moduleId: string;
  t: Translator;
  onNavigate: (route: string) => void;
}

type FormState = Record<string, string>;

const MONO: React.CSSProperties = { fontFamily: 'Geist Mono, monospace' };
const NUM_TYPES: ReadonlyArray<RegistryCol['t']> = [
  'num',
  'qty',
  'qtyS',
  'money',
  'moneyS',
  'pct',
];

function initialForm(cols: RegistryCol[], codePrefix: string): FormState {
  const state: FormState = {};
  cols.forEach((c) => {
    if (c.t === 'status') state[c.k] = 'Draft';
    else if (c.t === 'code') state[c.k] = `${codePrefix}-2605-2401`;
    else state[c.k] = '';
  });
  return state;
}

function pickNameCol(cols: RegistryCol[]): RegistryCol | undefined {
  return (
    cols.find((c) => /nama|name/i.test(c.k)) ??
    cols.find((c) => c.t === 'text')
  );
}

interface FieldControlProps {
  col: RegistryCol;
  value: string;
  onChange: (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>,
  ) => void;
}

function FieldControl({ col, value, onChange }: FieldControlProps) {
  if (col.t === 'status') {
    const opts = ['Draft', ...STATUSES.filter((s) => s !== 'Draft')];
    return (
      <select value={value} onChange={onChange}>
        {opts.map((s) => (
          <option key={s}>{s}</option>
        ))}
      </select>
    );
  }
  if (col.t === 'code') {
    return <input value={value} disabled style={MONO} />;
  }
  if (col.t === 'date') {
    return (
      <input
        value={value}
        onChange={onChange}
        placeholder="dd/mm/yyyy"
      />
    );
  }
  if (NUM_TYPES.includes(col.t)) {
    return (
      <input
        type="number"
        value={value}
        onChange={onChange}
        placeholder="0"
        style={{ ...MONO, textAlign: 'right' }}
      />
    );
  }
  return (
    <input
      value={value}
      onChange={onChange}
      placeholder={`Masukkan ${col.h.toLowerCase()}…`}
    />
  );
}

export function RecordForm({
  moduleId,
  t,
  onNavigate,
}: RecordFormProps): React.ReactElement {
  const mod = REGISTRY[moduleId];

  // All hooks must be called unconditionally — derive safe fallbacks
  // when the module is missing so the early-return path stays valid.
  const cols = React.useMemo<RegistryCol[]>(
    () => (mod ? mod.cols.filter((c) => c.k !== 'id') : []),
    [mod],
  );
  const codeCol = React.useMemo(
    () => cols.find((c) => c.t === 'code'),
    [cols],
  );
  const nameCol = React.useMemo(() => pickNameCol(cols), [cols]);
  const codePrefix = mod?.code ?? '';

  const [form, setForm] = React.useState<FormState>(() =>
    initialForm(cols, codePrefix),
  );

  const setField =
    (key: string) =>
    (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>): void =>
      setForm((f) => ({ ...f, [key]: e.target.value }));

  const reset = React.useCallback((): void => {
    setForm(initialForm(cols, codePrefix));
    notify('Form direset', 'info');
  }, [cols, codePrefix]);

  const save = React.useCallback(
    (andNew: boolean): void => {
      if (!mod) return;
      if (nameCol && !form[nameCol.k]?.trim()) {
        notify(`${nameCol.h} wajib diisi.`, 'danger');
        return;
      }
      notify(`${t(mod.label)} baru tersimpan`, 'success');
      if (andNew) setForm(initialForm(cols, codePrefix));
      else onNavigate(moduleId);
    },
    [mod, nameCol, form, t, cols, codePrefix, onNavigate, moduleId],
  );

  const onKey = React.useCallback(
    (e: KeyboardEvent): void => {
      if (!mod) return;
      const target = e.target as HTMLElement | null;
      const tag = target?.tagName ?? '';
      if (
        e.key === 'Escape' &&
        !['INPUT', 'TEXTAREA', 'SELECT'].includes(tag)
      ) {
        onNavigate(moduleId);
        return;
      }
      if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === 's') {
        e.preventDefault();
        save(e.shiftKey);
      }
    },
    [mod, onNavigate, moduleId, save],
  );
  useTabKey(onKey);

  if (!mod) {
    return (
      <div className="page" style={{ padding: 24 }}>
        Modul tidak ditemukan: {moduleId}
      </div>
    );
  }

  const third = Math.ceil(cols.length / 3);
  const groups: RegistryCol[][] = [
    cols.slice(0, third),
    cols.slice(third, third * 2),
    cols.slice(third * 2),
  ];

  return (
    <div className="page">
      <div className="page-header">
        <button
          className="iconbtn"
          onClick={() => onNavigate(moduleId)}
          aria-label="Kembali"
        >
          <Icon name="arrowleft" size={14} />
        </button>
        <h1 className="page-title">
          {t(mod.label)}
          <span className="code-tag">{mod.code} · Baru</span>
        </h1>
        <span className="pill warn" style={{ marginLeft: 8 }}>
          <span className="dot" />
          Draft baru
        </span>
        <div className="page-actions">
          <button className="btn ghost" onClick={() => onNavigate(moduleId)}>
            Batal <Kbd>ESC</Kbd>
          </button>
          <button className="btn" onClick={reset}>
            <Icon name="refresh" size={12} /> {t('Reset')}
          </button>
          <button className="btn primary" onClick={() => save(false)}>
            <Icon name="save" size={12} /> {t('Simpan')} <Kbd>⌘S</Kbd>
          </button>
          <div className="btn-split">
            <button className="btn" onClick={() => save(true)}>
              <Icon name="plus" size={12} /> {t('Simpan & Baru')}
            </button>
            <button className="btn" aria-label="Lainnya">
              <Icon name="chevdown" size={12} />
            </button>
          </div>
        </div>
      </div>

      <div className="form-section" style={{ alignItems: 'start' }}>
        {groups.map((group, gi) => (
          <div key={gi}>
            {group.map((c) => (
              <FormField
                key={c.k}
                label={c.h}
                required={c === nameCol || c.t === 'status'}
                help={c.t === 'code' ? 'Otomatis saat disimpan' : undefined}
              >
                <FieldControl
                  col={c}
                  value={form[c.k] ?? ''}
                  onChange={setField(c.k)}
                />
              </FormField>
            ))}
          </div>
        ))}
      </div>

      <div
        style={{
          flex: 1,
          background: 'var(--panel)',
          padding: 16,
          fontSize: 'calc(12.5px * var(--font-scale, 1))',
          color: 'var(--fg-muted)',
          display: 'flex',
          alignItems: 'center',
          gap: 8,
        }}
      >
        <Icon name="info" size={13} /> Lengkapi data{' '}
        <strong style={{ color: 'var(--fg)' }}>{t(mod.label)}</strong> lalu
        simpan. Form ini terbuka sebagai tab tersendiri — gunakan{' '}
        <Kbd>⌘S</Kbd> untuk simpan, <Kbd>ESC</Kbd> untuk batal.
      </div>

      <div className="pager">
        <span className="muted">
          {mod.group} · {t(mod.label)}
        </span>
        <div style={{ flex: 1 }} />
        <span className="muted">Belum disimpan</span>
      </div>
    </div>
  );
}
