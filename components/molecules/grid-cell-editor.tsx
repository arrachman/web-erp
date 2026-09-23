'use client';

/**
 * GridCellEditor — molecule that renders the correct display or edit widget
 * for one grid cell, driven by `fieldType`.
 *
 * Renders a `.cell` div (compatible with `.lines td .cell` CSS rules).
 * The *parent* <td> must carry `className="editing"` when this cell is
 * being edited — that triggers the blue-highlight rule `td.editing .cell`.
 *
 * Atomic tier: Molecule (combines display + edit behaviour into one unit).
 */

import * as React from 'react';
import { StatusBadge } from '@/components/ui/badge';
import { fmtIDR } from '@/lib/mock';
import type { GridFieldType } from '@/lib/grid-types';

// ---------------------------------------------------------------------------
// Display formatter
// ---------------------------------------------------------------------------

function formatValue(fieldType: GridFieldType, value: unknown): React.ReactNode {
  if (value === null || value === undefined || value === '') {
    return <span style={{ color: 'var(--fg-faint)' }}>—</span>;
  }
  switch (fieldType) {
    case 'currency':
      return fmtIDR(Number(value));
    case 'int':
      return Number(value).toLocaleString('id-ID');
    case 'date':
      return String(value);
    case 'status':
      return <StatusBadge status={String(value)} />;
    case 'checkbox':
      return (
        <span style={{ color: value ? 'var(--primary)' : 'var(--fg-faint)', fontWeight: 600 }}>
          {value ? '✓' : '—'}
        </span>
      );
    default:
      return String(value);
  }
}

// ---------------------------------------------------------------------------
// Props
// ---------------------------------------------------------------------------

export interface GridCellEditorProps {
  fieldType: GridFieldType;
  value: unknown;
  /** Options list for 'dropdown' and 'radio'. */
  options?: string[];
  /** Whether this cell is currently in edit mode. */
  editing: boolean;
  /** Whether Tab can land on this cell. Drives tabIndex. */
  focusable: boolean;
  /** Right-align + mono font class. */
  numeric: boolean;
  /** Called whenever the user changes the value (during editing). */
  onChange: (v: unknown) => void;
  /** Exit edit mode and commit (blur, Enter, Escape, Tab). */
  onCommit: () => void;
  /** Switch this cell into edit mode (click / Enter / F2). */
  onActivate: () => void;
  /** Required when fieldType === 'custom'. */
  render?: (value: unknown) => React.ReactNode;
}

// Keys that should exit edit mode
const COMMIT_KEYS = new Set(['Enter', 'Escape', 'Tab']);

// ---------------------------------------------------------------------------
// Component
// ---------------------------------------------------------------------------

export function GridCellEditor({
  fieldType,
  value,
  options = [],
  editing,
  focusable,
  numeric,
  onChange,
  onCommit,
  onActivate,
  render,
}: GridCellEditorProps): React.ReactElement {
  const cellCls = `cell${numeric ? ' num' : ''}`;

  // ------------------------------------------------------------------
  // custom — caller supplies render fn (no generic editor)
  // ------------------------------------------------------------------
  if (fieldType === 'custom' && render) {
    return <div className={cellCls}>{render(value)}</div>;
  }

  // ------------------------------------------------------------------
  // status — always read-only
  // ------------------------------------------------------------------
  if (fieldType === 'status') {
    return (
      <div className={cellCls}>
        <StatusBadge status={String(value ?? '')} />
      </div>
    );
  }

  // ------------------------------------------------------------------
  // checkbox — click-to-toggle; no separate edit mode
  // ------------------------------------------------------------------
  if (fieldType === 'checkbox') {
    return (
      <div
        className={cellCls}
        role="checkbox"
        aria-checked={!!value}
        tabIndex={focusable ? 0 : -1}
        style={{ cursor: 'pointer', userSelect: 'none' }}
        onClick={() => { onChange(!value); onCommit(); }}
        onKeyDown={(e) => {
          if (e.key === ' ' || e.key === 'Enter') {
            e.preventDefault();
            onChange(!value);
            onCommit();
          }
        }}
      >
        <span style={{ color: value ? 'var(--primary)' : 'var(--fg-faint)', fontWeight: 600 }}>
          {value ? '✓' : '—'}
        </span>
      </div>
    );
  }

  // ------------------------------------------------------------------
  // display mode (not editing)
  // ------------------------------------------------------------------
  if (!editing) {
    return (
      <div
        className={cellCls}
        tabIndex={focusable ? 0 : -1}
        style={{ cursor: 'text', outline: 'none' }}
        onClick={onActivate}
        onKeyDown={(e) => {
          if (e.key === 'Enter' || e.key === 'F2') {
            e.preventDefault();
            onActivate();
          }
        }}
      >
        {formatValue(fieldType, value)}
      </div>
    );
  }

  // ------------------------------------------------------------------
  // edit mode — dropdown
  // ------------------------------------------------------------------
  if (fieldType === 'dropdown') {
    return (
      <div className={cellCls}>
        <select
          autoFocus
          value={String(value ?? '')}
          onChange={(e) => onChange(e.target.value)}
          onBlur={onCommit}
          onKeyDown={(e) => { if (COMMIT_KEYS.has(e.key)) onCommit(); }}
          style={{
            width: '100%',
            height: '100%',
            background: 'transparent',
            border: 0,
            outline: 0,
            font: 'inherit',
            fontSize: 'calc(12px * var(--font-scale, 1))',
            color: 'var(--fg)',
          }}
        >
          <option value="">—</option>
          {options.map((o) => <option key={o} value={o}>{o}</option>)}
        </select>
      </div>
    );
  }

  // ------------------------------------------------------------------
  // edit mode — radio (compact inline)
  // ------------------------------------------------------------------
  if (fieldType === 'radio') {
    return (
      <div className={cellCls} style={{ flexWrap: 'wrap', gap: 8 }}>
        {options.map((o) => (
          <label
            key={o}
            style={{ display: 'flex', alignItems: 'center', gap: 3, cursor: 'pointer', fontSize: 'calc(11px * var(--font-scale, 1))' }}
          >
            <input
              type="radio"
              value={o}
              checked={value === o}
              onChange={() => { onChange(o); onCommit(); }}
            />
            {o}
          </label>
        ))}
      </div>
    );
  }

  // ------------------------------------------------------------------
  // edit mode — text / int / currency / date
  // ------------------------------------------------------------------
  const inputType =
    fieldType === 'date' ? 'date'
    : (fieldType === 'int' || fieldType === 'currency') ? 'number'
    : 'text';

  return (
    <div className={cellCls}>
      <input
        autoFocus
        type={inputType}
        step={fieldType === 'int' ? '1' : undefined}
        defaultValue={String(value ?? '')}
        onChange={(e) => {
          const raw = e.target.value;
          onChange(
            (fieldType === 'int' || fieldType === 'currency')
              ? (parseFloat(raw.replace(/[^0-9.-]/g, '')) || 0)
              : raw,
          );
        }}
        onBlur={onCommit}
        onKeyDown={(e) => {
          if (COMMIT_KEYS.has(e.key)) {
            e.preventDefault();
            onCommit();
          }
        }}
      />
    </div>
  );
}
