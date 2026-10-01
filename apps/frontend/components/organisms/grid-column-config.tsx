'use client';

/**
 * GridColumnConfig — floating panel to configure grid columns per-user.
 *
 * Features per column row:
 *   • Visibility toggle (show / hide)
 *   • Field type selector (text, int, currency, dropdown, checkbox, radio,
 *                          date, status, custom)
 *   • Tab focus toggle (FOKUS = Tab stops here; SKIP = Tab skips this column)
 *
 * Persisted automatically via useGridColumns → localStorage.
 * Positioned absolutely below the trigger button (caller sets position:relative
 * on the container).
 *
 * Atomic tier: Organism (complex UI section with multiple interactive parts).
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import type {
  GridColumnState,
  GridFieldType,
  GridColumnOverride,
} from '@/lib/grid-types';

// ---------------------------------------------------------------------------
// Field type metadata
// ---------------------------------------------------------------------------

const FIELD_TYPES: GridFieldType[] = [
  'text', 'int', 'currency', 'dropdown', 'checkbox', 'radio', 'date', 'status', 'custom',
];

const FIELD_TYPE_LABEL: Record<GridFieldType, string> = {
  text:     'Teks',
  int:      'Angka',
  currency: 'Mata Uang',
  dropdown: 'Pilihan',
  checkbox: 'Centang',
  radio:    'Radio',
  date:     'Tanggal',
  status:   'Status',
  custom:   'Custom',
};

// ---------------------------------------------------------------------------
// Single column row
// ---------------------------------------------------------------------------

interface ColRowProps {
  col: GridColumnState;
  updateOverride: (key: string, patch: GridColumnOverride) => void;
}

function ColRow({ col, updateOverride }: ColRowProps): React.ReactElement {
  return (
    <div
      style={{
        display: 'grid',
        gridTemplateColumns: '18px 1fr 108px 66px',
        gap: 8,
        alignItems: 'center',
        padding: '5px 0',
        borderBottom: '1px solid var(--border)',
        opacity: col.visible ? 1 : 0.45,
        transition: 'opacity 0.15s',
      }}
    >
      {/* drag handle placeholder — DnD integration deferred */}
      <span
        aria-hidden
        style={{ color: 'var(--fg-faint)', cursor: 'grab', fontSize: 'calc(14px * var(--font-scale, 1))', lineHeight: 1, textAlign: 'center' }}
      >
        ⠿
      </span>

      {/* visibility toggle + label */}
      <label style={{ display: 'flex', alignItems: 'center', gap: 6, cursor: 'pointer', fontSize: 'calc(12px * var(--font-scale, 1))', overflow: 'hidden' }}>
        <input
          type="checkbox"
          className="checkbox"
          checked={col.visible}
          onChange={(e) => updateOverride(col.key, { visible: e.target.checked })}
        />
        <span style={{ fontWeight: 500, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
          {col.header}
        </span>
      </label>

      {/* field type selector */}
      <select
        value={col.fieldType}
        onChange={(e) => updateOverride(col.key, { fieldType: e.target.value as GridFieldType })}
        style={{
          fontSize: 'calc(11px * var(--font-scale, 1))',
          padding: '2px 4px',
          border: '1px solid var(--border)',
          borderRadius: 'var(--radius-sm)',
          background: 'var(--panel)',
          color: 'var(--fg)',
          width: '100%',
        }}
      >
        {FIELD_TYPES.map((ft) => (
          <option key={ft} value={ft}>{FIELD_TYPE_LABEL[ft]}</option>
        ))}
      </select>

      {/* tab focus toggle */}
      <button
        type="button"
        onClick={() => updateOverride(col.key, { focusable: !col.focusable })}
        style={{
          fontSize: 'calc(10px * var(--font-scale, 1))',
          fontWeight: 700,
          letterSpacing: 0.3,
          padding: '2px 0',
          width: '100%',
          border: '1px solid var(--border)',
          borderRadius: 'var(--radius-sm)',
          background: col.focusable ? 'var(--primary)' : 'var(--panel-2)',
          color: col.focusable ? 'var(--primary-fg)' : 'var(--fg-faint)',
          cursor: 'pointer',
          transition: 'background 0.12s, color 0.12s',
        }}
        title={col.focusable ? 'Tab berhenti di kolom ini — klik untuk SKIP' : 'Tab melewati kolom ini — klik untuk FOKUS'}
      >
        {col.focusable ? 'FOKUS' : 'SKIP'}
      </button>
    </div>
  );
}

// ---------------------------------------------------------------------------
// Panel
// ---------------------------------------------------------------------------

export interface GridColumnConfigProps {
  columns: GridColumnState[];
  updateOverride: (key: string, patch: GridColumnOverride) => void;
  resetOverrides: () => void;
  onClose: () => void;
}

export function GridColumnConfig({
  columns,
  updateOverride,
  resetOverrides,
  onClose,
}: GridColumnConfigProps): React.ReactElement {
  const visibleCount = columns.filter((c) => c.visible).length;

  // Close on Escape
  React.useEffect(() => {
    const handler = (e: KeyboardEvent) => { if (e.key === 'Escape') onClose(); };
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, [onClose]);

  return (
    <div
      role="dialog"
      aria-label="Konfigurasi Kolom"
      style={{
        position: 'absolute',
        top: 32,
        right: 0,
        zIndex: 60,
        width: 360,
        background: 'var(--panel)',
        border: '1px solid var(--border)',
        borderRadius: 'var(--radius-lg)',
        boxShadow: 'var(--shadow-flyout)',
        overflow: 'hidden',
      }}
    >
      {/* ---- header ---- */}
      <div style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: '9px 14px',
        borderBottom: '1px solid var(--border)',
        background: 'var(--panel-2)',
      }}>
        <span style={{ fontWeight: 600, fontSize: 'calc(12px * var(--font-scale, 1))' }}>
          Konfigurasi Kolom
          <span style={{ marginLeft: 6, color: 'var(--fg-faint)', fontWeight: 400, fontSize: 'calc(11px * var(--font-scale, 1))' }}>
            {visibleCount}/{columns.length} tampil
          </span>
        </span>
        <button
          type="button"
          onClick={onClose}
          aria-label="Tutup"
          style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--fg-faint)', padding: 2, lineHeight: 1 }}
        >
          <Icon name="x" size={13} />
        </button>
      </div>

      {/* ---- column header legend ---- */}
      <div style={{
        display: 'grid',
        gridTemplateColumns: '18px 1fr 108px 66px',
        gap: 8,
        padding: '5px 14px 3px',
        fontSize: 'calc(10px * var(--font-scale, 1))',
        fontWeight: 600,
        color: 'var(--fg-faint)',
        textTransform: 'uppercase',
        letterSpacing: 0.5,
      }}>
        <span />
        <span>Kolom</span>
        <span>Tipe Field</span>
        <span style={{ textAlign: 'center' }}>Tab</span>
      </div>

      {/* ---- column rows ---- */}
      <div style={{ padding: '0 14px', maxHeight: 320, overflowY: 'auto' }}>
        {columns.map((col) => (
          <ColRow key={col.key} col={col} updateOverride={updateOverride} />
        ))}
      </div>

      {/* ---- footer ---- */}
      <div style={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        padding: '9px 14px',
        borderTop: '1px solid var(--border)',
        background: 'var(--panel-2)',
      }}>
        <button
          type="button"
          className="btn ghost sm"
          onClick={resetOverrides}
          title="Kembalikan ke pengaturan default"
        >
          <Icon name="refresh" size={11} /> Reset Default
        </button>
        <button
          type="button"
          className="btn primary sm"
          onClick={onClose}
        >
          Terapkan
        </button>
      </div>
    </div>
  );
}
