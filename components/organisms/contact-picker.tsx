'use client';

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Kbd } from '@/components/ui/kbd';
import { type Translator } from '@/lib/mock';
import { cn } from '@/lib/utils';
import {
  avatarHue,
  CAT_ORDER,
  type CategoryFilter,
  type Contact,
  CONTACT_DATA,
  initials,
  RECENT_IDS,
} from './contact-picker-data';
import { ContactPickerPreview } from './contact-picker-preview';
import { ContactPickerRow } from './contact-picker-row';

export type { Contact } from './contact-picker-data';

/**
 * Contact picker (Organism) — "Cari Kontak" overlay. Port 1:1 dari
 * prototype `contact-picker.jsx`. Dataset deterministik + sub-komponen
 * (row, preview, helpers) dipecah ke modul tetangga supaya semua file
 * tetap < 400 baris (clean-code rule §3). Tidak ada `window.*` global.
 */

export interface ContactPickerProps {
  open: boolean;
  onClose: () => void;
  onSelect: (c: Contact) => void;
  t: Translator;
}

export function ContactPicker({
  open,
  onClose,
  onSelect,
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  t: _t,
}: ContactPickerProps): React.ReactElement | null {
  const [q, setQ] = React.useState('');
  const [cat, setCat] = React.useState<CategoryFilter>('Semua');
  const [focused, setFocused] = React.useState(0);
  const inputRef = React.useRef<HTMLInputElement>(null);
  const rowsRef = React.useRef<HTMLDivElement>(null);

  React.useEffect(() => {
    if (!open) return;
    setQ('');
    setFocused(0);
    setCat('Semua');
    const tm = window.setTimeout(() => inputRef.current?.focus(), 30);
    return () => window.clearTimeout(tm);
  }, [open]);

  const filtered = React.useMemo<Contact[]>(() => {
    const ql = q.toLowerCase();
    return CONTACT_DATA.filter((c) => {
      if (cat !== 'Semua' && c.cat !== cat) return false;
      if (!q) return true;
      return (
        c.name.toLowerCase().includes(ql)
        || c.code.toLowerCase().includes(ql)
        || c.city.toLowerCase().includes(ql)
      );
    });
  }, [q, cat]);

  React.useEffect(() => {
    setFocused(0);
  }, [q, cat]);

  const active = filtered[focused];

  const counts = React.useMemo<Record<string, number>>(() => {
    const m: Record<string, number> = { Semua: CONTACT_DATA.length };
    CONTACT_DATA.forEach((c) => {
      m[c.cat] = (m[c.cat] ?? 0) + 1;
    });
    return m;
  }, []);

  const recentItems = React.useMemo<Contact[]>(
    () =>
      RECENT_IDS.map((id) => CONTACT_DATA.find((c) => c.id === id + 1)).filter(
        (c): c is Contact => Boolean(c),
      ),
    [],
  );

  const choose = React.useCallback(
    (c: Contact): void => {
      onSelect(c);
      onClose();
    },
    [onSelect, onClose],
  );

  const onKey = (e: React.KeyboardEvent<HTMLDivElement>): void => {
    if (e.key === 'Escape') {
      onClose();
    } else if (e.key === 'ArrowDown' || (e.ctrlKey && e.key === 'n')) {
      e.preventDefault();
      setFocused((f) => Math.min(filtered.length - 1, f + 1));
    } else if (e.key === 'ArrowUp' || (e.ctrlKey && e.key === 'p')) {
      e.preventDefault();
      setFocused((f) => Math.max(0, f - 1));
    } else if (e.key === 'PageDown') {
      e.preventDefault();
      setFocused((f) => Math.min(filtered.length - 1, f + 8));
    } else if (e.key === 'PageUp') {
      e.preventDefault();
      setFocused((f) => Math.max(0, f - 8));
    } else if (e.key === 'Enter') {
      e.preventDefault();
      if (active) choose(active);
    } else if (e.key === 'Tab' && !e.shiftKey) {
      const i = CAT_ORDER.indexOf(cat);
      if (i !== -1) {
        e.preventDefault();
        setCat(CAT_ORDER[(i + 1) % CAT_ORDER.length]);
      }
    }
  };

  React.useEffect(() => {
    const list = rowsRef.current;
    if (!list) return;
    const row = list.querySelector(`[data-row="${focused}"]`);
    if (!(row instanceof HTMLElement)) return;
    const rb = row.getBoundingClientRect();
    const cb = list.getBoundingClientRect();
    if (rb.top < cb.top + 30) list.scrollTop -= cb.top + 30 - rb.top;
    if (rb.bottom > cb.bottom - 6) list.scrollTop += rb.bottom - cb.bottom + 6;
  }, [focused]);

  if (!open) return null;

  return (
    <div
      className="cp-backdrop"
      onMouseDown={(e) => {
        if (e.target === e.currentTarget) onClose();
      }}
      role="presentation"
    >
      <div
        className="contact-modal fade-in"
        onKeyDown={onKey}
        role="dialog"
        aria-modal="true"
        aria-label="Cari Kontak"
      >
        <div className="cm-head">
          <div className="cm-title">
            <Icon name="user" size={14} />
            <span>Cari Kontak</span>
            <span
              className="muted"
              style={{ fontWeight: 400, fontSize: 'calc(11.5px * var(--font-scale, 1))', marginLeft: 4 }}
            >
              · {filtered.length} dari {CONTACT_DATA.length}
            </span>
          </div>
          <div className="cm-search">
            <Icon name="search" size={13} />
            <input
              ref={inputRef}
              placeholder="Cari nama, kode, kota..."
              value={q}
              onChange={(e) => setQ(e.target.value)}
            />
            {q && (
              <button
                type="button"
                className="iconbtn"
                style={{ width: 22, height: 22 }}
                onClick={() => setQ('')}
                aria-label="Bersihkan pencarian"
              >
                <Icon name="x" size={10} />
              </button>
            )}
          </div>
          <button type="button" className="btn ghost sm" onClick={onClose}>
            Tutup <Kbd>ESC</Kbd>
          </button>
        </div>

        <div className="cm-body">
          <aside className="cm-rail">
            <div className="cm-rail-grp">
              <div className="cm-rail-label">Kategori</div>
              {CAT_ORDER.map((c) => (
                <button
                  type="button"
                  key={c}
                  className={cn('cm-rail-item', cat === c && 'active')}
                  onClick={() => setCat(c)}
                >
                  <span className={`cm-dot cm-dot-${c}`} />
                  <span>{c}</span>
                  <span className="cm-count">{counts[c] ?? 0}</span>
                </button>
              ))}
            </div>
            {recentItems.length > 0 && (
              <div className="cm-rail-grp">
                <div className="cm-rail-label">
                  <Icon name="history" size={10} /> Terakhir digunakan
                </div>
                {recentItems.map((rc) => (
                  <button
                    type="button"
                    key={rc.id}
                    className="cm-rail-item"
                    onClick={() => choose(rc)}
                  >
                    <span
                      className="cm-mini-av"
                      style={{
                        background: `hsl(${avatarHue(rc)} 60% 92%)`,
                        color: `hsl(${avatarHue(rc)} 70% 35%)`,
                      }}
                    >
                      {initials(rc.name)}
                    </span>
                    <span
                      style={{
                        overflow: 'hidden',
                        textOverflow: 'ellipsis',
                        whiteSpace: 'nowrap',
                      }}
                    >
                      {rc.name}
                    </span>
                  </button>
                ))}
              </div>
            )}
            <div className="cm-rail-grp" style={{ marginTop: 'auto' }}>
              <button type="button" className="cm-rail-item primary-action">
                <Icon name="plus" size={11} />
                <span>Buat kontak baru</span>
                <span className="cm-count">
                  <Kbd>+</Kbd>
                </span>
              </button>
            </div>
          </aside>

          <div className="cm-list scrollbar" ref={rowsRef}>
            {filtered.length === 0 ? (
              <div className="cm-empty">
                <div className="cm-empty-art">
                  <Icon name="search" size={28} />
                </div>
                <div style={{ fontWeight: 500, marginTop: 8 }}>Tidak ditemukan</div>
                <div className="muted" style={{ marginTop: 2, fontSize: 'calc(12px * var(--font-scale, 1))' }}>
                  Coba ubah kata kunci atau ganti kategori.
                </div>
              </div>
            ) : (
              filtered.map((c, i) => (
                <ContactPickerRow
                  key={c.id}
                  contact={c}
                  index={i}
                  focused={i === focused}
                  query={q}
                  onFocus={setFocused}
                  onChoose={choose}
                />
              ))
            )}
          </div>

          <ContactPickerPreview
            active={active}
            onClose={onClose}
            onChoose={choose}
          />
        </div>

        <div className="cm-foot">
          <span>
            <Kbd>↑</Kbd>
            <Kbd>↓</Kbd> navigasi
          </span>
          <span>
            <Kbd>Tab</Kbd> ganti kategori
          </span>
          <span>
            <Kbd>↵</Kbd> pilih
          </span>
          <span>
            <Kbd>ESC</Kbd> tutup
          </span>
          <span style={{ marginLeft: 'auto' }} className="muted">
            Sentient ERP · Kontak
          </span>
        </div>
      </div>
    </div>
  );
}
