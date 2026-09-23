'use client';

import * as React from 'react';
import { Icon, type IconName } from '@/components/ui/icons';

/** Shared toolbar filter chips — ported from prototype `pages/filters.jsx`. */

function useOutsideClose(
  ref: React.RefObject<HTMLElement | null>,
  onClose: () => void,
): void {
  React.useEffect(() => {
    const fn = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) onClose();
    };
    document.addEventListener('mousedown', fn);
    return () => document.removeEventListener('mousedown', fn);
  }, [ref, onClose]);
}

interface FilterChipProps {
  label: string;
  val: string;
  options: readonly string[];
  onChange: (v: string) => void;
  onRemove?: () => void;
  icon?: IconName;
}

/** Single-select dropdown chip. "Semua" = no filter (chip stays neutral). */
export function FilterChip({
  label,
  val,
  options,
  onChange,
  onRemove,
  icon,
}: FilterChipProps) {
  const [open, setOpen] = React.useState(false);
  const ref = React.useRef<HTMLDivElement>(null);
  useOutsideClose(ref, () => setOpen(false));
  const set = val && val !== 'Semua';
  return (
    <div ref={ref} style={{ position: 'relative' }}>
      <div
        className={`chip ${set ? 'active' : ''}`}
        onClick={() => setOpen((o) => !o)}
      >
        {icon && <Icon name={icon} size={11} />}
        <span className="label">{label}</span>
        <span className="val">{val}</span>
        <Icon name="chevdown" size={10} />
        {set && onRemove && (
          <span
            className="x"
            onClick={(e) => {
              e.stopPropagation();
              onRemove();
            }}
          >
            <Icon name="x" size={10} />
          </span>
        )}
      </div>
      {open && (
        <div
          className="flyout fade-in scrollbar"
          style={{
            position: 'absolute',
            left: 0,
            top: '100%',
            marginTop: 4,
            minWidth: 180,
            maxHeight: 280,
            overflow: 'auto',
          }}
        >
          {options.map((o) => (
            <div
              key={o}
              className={`flyout-item ${o === val ? 'active' : ''}`}
              onClick={() => {
                onChange(o);
                setOpen(false);
              }}
            >
              <Icon name={o === val ? 'check' : 'dot'} size={o === val ? 12 : 8} />
              <span>{o}</span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

const DATE_PRESETS = [
  { id: 'today', label: 'Hari ini', from: '12/05/2026', to: '12/05/2026' },
  { id: '7d', label: '7 hari terakhir', from: '06/05/2026', to: '12/05/2026' },
  { id: 'mtd', label: 'Bulan ini', from: '01/05/2026', to: '12/05/2026' },
  { id: 'last', label: 'Bulan lalu', from: '01/04/2026', to: '30/04/2026' },
  { id: 'ytd', label: 'Tahun berjalan', from: '01/01/2026', to: '12/05/2026' },
];

const DR_INPUT: React.CSSProperties = {
  width: '50%',
  height: 26,
  padding: '0 6px',
  background: 'var(--panel)',
  border: '1px solid var(--border)',
  borderRadius: 5,
  color: 'var(--fg)',
  font: 'inherit',
  fontSize: 'calc(11.5px * var(--font-scale, 1))',
};

interface DateRangeChipProps {
  from: string;
  to: string;
  onChange: (from: string, to: string) => void;
  onRemove?: () => void;
}

/** Date-range chip with quick presets + free-text from/to. */
export function DateRangeChip({
  from,
  to,
  onChange,
  onRemove,
}: DateRangeChipProps) {
  const [open, setOpen] = React.useState(false);
  const ref = React.useRef<HTMLDivElement>(null);
  useOutsideClose(ref, () => setOpen(false));
  const [f, setF] = React.useState(from);
  const [tt, setTt] = React.useState(to);
  React.useEffect(() => {
    setF(from);
    setTt(to);
  }, [from, to]);
  return (
    <div ref={ref} style={{ position: 'relative' }}>
      <div className="chip active" onClick={() => setOpen((o) => !o)}>
        <Icon name="calendar" size={11} />
        <span className="label">Tanggal</span>
        <span className="val">
          {from} – {to}
        </span>
        <Icon name="chevdown" size={10} />
        {onRemove && (
          <span
            className="x"
            onClick={(e) => {
              e.stopPropagation();
              onRemove();
            }}
          >
            <Icon name="x" size={10} />
          </span>
        )}
      </div>
      {open && (
        <div
          className="flyout fade-in"
          style={{
            position: 'absolute',
            left: 0,
            top: '100%',
            marginTop: 4,
            minWidth: 230,
            padding: 8,
          }}
        >
          {DATE_PRESETS.map((p) => (
            <div
              key={p.id}
              className="flyout-item"
              onClick={() => {
                onChange(p.from, p.to);
                setOpen(false);
              }}
            >
              <Icon name="dot" size={8} />
              <span>{p.label}</span>
            </div>
          ))}
          <div
            style={{
              borderTop: '1px solid var(--border)',
              margin: '6px 0',
            }}
          />
          <div style={{ display: 'flex', gap: 6, padding: '2px 6px' }}>
            <input
              value={f}
              onChange={(e) => setF(e.target.value)}
              placeholder="dd/mm/yyyy"
              style={DR_INPUT}
            />
            <input
              value={tt}
              onChange={(e) => setTt(e.target.value)}
              placeholder="dd/mm/yyyy"
              style={DR_INPUT}
            />
          </div>
          <button
            className="btn primary sm"
            style={{ width: '100%', marginTop: 6, justifyContent: 'center' }}
            onClick={() => {
              onChange(f, tt);
              setOpen(false);
            }}
          >
            Terapkan
          </button>
        </div>
      )}
    </div>
  );
}

interface AddFilterChipProps {
  available: { id: string; label?: string }[];
  onAdd: (id: string) => void;
  t?: (k: string) => string;
}

/** "Tambah Filter" chip — picks an inactive filter to surface. */
export function AddFilterChip({ available, onAdd, t }: AddFilterChipProps) {
  const [open, setOpen] = React.useState(false);
  const ref = React.useRef<HTMLDivElement>(null);
  useOutsideClose(ref, () => setOpen(false));
  if (!available || available.length === 0) return null;
  const tr = t ?? ((x: string) => x);
  return (
    <div ref={ref} style={{ position: 'relative' }}>
      <div className="chip add" onClick={() => setOpen((o) => !o)}>
        <Icon name="plus" size={10} />
        <span>{tr('Tambah Filter')}</span>
      </div>
      {open && (
        <div
          className="flyout fade-in"
          style={{
            position: 'absolute',
            left: 0,
            top: '100%',
            marginTop: 4,
            minWidth: 170,
          }}
        >
          {available.map((fl) => (
            <div
              key={fl.id}
              className="flyout-item"
              onClick={() => {
                onAdd(fl.id);
                setOpen(false);
              }}
            >
              <Icon name="plus" size={10} />
              <span>{fl.label ?? fl.id}</span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
