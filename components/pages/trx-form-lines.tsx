'use client';

/**
 * Detail table for the generic <TrxForm>. Inline-editable grid of TrxLine
 * rows driven by GridColumnDef — field type, visibility, and Tab focus are
 * all user-configurable via the GridColumnConfig panel (⊞ Kolom button).
 *
 * Column state persists to localStorage keyed by grid variant:
 *   'trx-lines-journal'  → journal/memorial (debit + kredit)
 *   'trx-lines-std'      → non-journal (total + proyek)
 *
 * Atomic tier: Pages (sub-page composite, tightly coupled to <TrxForm>).
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Kbd } from '@/components/ui/kbd';
import { fmtIDR, type Translator } from '@/lib/mock';
import { useGridColumns } from '@/lib/use-grid-columns';
import type { GridColumnDef, GridColumnState } from '@/lib/grid-types';
import { GridCellEditor } from '@/components/molecules/grid-cell-editor';
import { GridColumnConfig } from '@/components/organisms/grid-column-config';
import type { TrxConfig, TrxLine } from './trx-form-config';

// ---------------------------------------------------------------------------
// Column definitions (static — user overrides applied by useGridColumns)
// ---------------------------------------------------------------------------

const JOURNAL_COLS: GridColumnDef<TrxLine>[] = [
  { key: 'no',     header: 'No Akun',     width: 130, fieldType: 'text' },
  { key: 'nama',   header: 'Nama Akun',               fieldType: 'text' },
  { key: 'debit',  header: 'Debit',       width: 130, fieldType: 'currency' },
  { key: 'kredit', header: 'Kredit',      width: 130, fieldType: 'currency' },
  { key: 'catatan',header: 'Catatan',     width: 170, fieldType: 'text' },
  { key: 'cc',     header: 'Cost Center', width: 110, fieldType: 'text' },
];

const STD_COLS: GridColumnDef<TrxLine>[] = [
  { key: 'no',     header: 'No Akun',     width: 130, fieldType: 'text' },
  { key: 'nama',   header: 'Nama Akun',               fieldType: 'text' },
  { key: 'total',  header: 'Total',       width: 140, fieldType: 'currency' },
  { key: 'catatan',header: 'Catatan',     width: 180, fieldType: 'text' },
  { key: 'cc',     header: 'Cost Center', width: 120, fieldType: 'text' },
  { key: 'proyek', header: 'Proyek',      width: 100, fieldType: 'text' },
];

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

export type TrxEditing = { rowId: number; col: string } | null;

export interface TrxLinesProps {
  cfg: TrxConfig;
  t: Translator;
  lines: TrxLine[];
  setLines: React.Dispatch<React.SetStateAction<TrxLine[]>>;
  editing: TrxEditing;
  setEditing: React.Dispatch<React.SetStateAction<TrxEditing>>;
  total: number;
  totalKredit: number;
  head: { uang: string };
  addLine: () => void;
}

// ---------------------------------------------------------------------------
// Row component
// ---------------------------------------------------------------------------

interface RowProps {
  idx: number;
  line: TrxLine;
  cols: GridColumnState[];
  editing: TrxEditing;
  setEditing: React.Dispatch<React.SetStateAction<TrxEditing>>;
  update: (id: number, col: string, value: unknown) => void;
  remove: (id: number) => void;
}

function TrxLineRow({
  idx,
  line,
  cols,
  editing,
  setEditing,
  update,
  remove,
}: RowProps): React.ReactElement {
  const isEd = (col: string): boolean =>
    editing != null && editing.rowId === line.id && editing.col === col;

  const activate = (col: string): void =>
    setEditing({ rowId: line.id, col });

  const commit = (): void => setEditing(null);

  return (
    <tr>
      <td>
        <div className="cell muted" style={{ fontFamily: 'var(--font-mono)', fontSize: 'calc(11px * var(--font-scale, 1))' }}>
          {idx}
        </div>
      </td>

      {cols.map((c) => (
        <td
          key={c.key}
          className={isEd(c.key) ? 'editing' : ''}
          onClick={(e) => { e.stopPropagation(); if (c.fieldType !== 'status') activate(c.key); }}
          style={{ width: c.width }}
        >
          <GridCellEditor
            fieldType={c.fieldType}
            value={(line as unknown as Record<string, unknown>)[c.key]}
            options={c.options}
            editing={isEd(c.key)}
            focusable={c.focusable}
            numeric={c.numeric}
            onChange={(v) => update(line.id, c.key, v)}
            onCommit={commit}
            onActivate={() => activate(c.key)}
            render={c.render ? (v) => c.render!(v) : undefined}
          />
        </td>
      ))}

      <td onClick={(e) => e.stopPropagation()}>
        <button
          type="button"
          className="iconbtn"
          style={{ width: 26, height: 24, color: 'var(--fg-faint)' }}
          onClick={() => remove(line.id)}
          aria-label="Hapus baris"
        >
          <Icon name="x" size={11} />
        </button>
      </td>
    </tr>
  );
}

// ---------------------------------------------------------------------------
// TrxLines
// ---------------------------------------------------------------------------

export function TrxLines({
  cfg,
  t,
  lines,
  setLines,
  editing,
  setEditing,
  total,
  totalKredit,
  head,
  addLine,
}: TrxLinesProps): React.ReactElement {
  const gridId = cfg.journal ? 'trx-lines-journal' : 'trx-lines-std';
  const defs = (cfg.journal ? JOURNAL_COLS : STD_COLS) as unknown as import('@/lib/grid-types').GridColumnDef[];

  const { columns, visibleColumns, updateOverride, resetOverrides } =
    useGridColumns(gridId, defs);

  const [configOpen, setConfigOpen] = React.useState(false);

  const update = React.useCallback(
    (id: number, col: string, value: unknown): void => {
      setLines((arr) =>
        arr.map((l) => (l.id === id ? { ...l, [col]: value } : l)),
      );
    },
    [setLines],
  );

  const remove = React.useCallback(
    (id: number): void => {
      setLines((arr) => arr.filter((l) => l.id !== id));
    },
    [setLines],
  );

  // Close config panel when clicking outside
  const configRef = React.useRef<HTMLDivElement>(null);
  React.useEffect(() => {
    if (!configOpen) return;
    const handler = (e: MouseEvent): void => {
      if (configRef.current && !configRef.current.contains(e.target as Node)) {
        setConfigOpen(false);
      }
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, [configOpen]);

  return (
    <div className="lines scrollbar">
      {/* ---- column config trigger ---- */}
      <div style={{ position: 'relative', display: 'flex', justifyContent: 'flex-end', padding: '4px 6px 2px' }} ref={configRef}>
        <button
          type="button"
          className="btn ghost sm"
          style={{ fontSize: 'calc(11px * var(--font-scale, 1))', gap: 4 }}
          onClick={() => setConfigOpen((o) => !o)}
          aria-expanded={configOpen}
          aria-haspopup="dialog"
        >
          <Icon name="gear" size={11} />
          {t('Kolom')}
          {columns.some((c) => !c.visible) && (
            <span style={{
              display: 'inline-flex',
              alignItems: 'center',
              justifyContent: 'center',
              width: 14,
              height: 14,
              borderRadius: '50%',
              background: 'var(--primary)',
              color: 'var(--primary-fg)',
              fontSize: 'calc(9px * var(--font-scale, 1))',
              fontWeight: 700,
            }}>
              {columns.filter((c) => !c.visible).length}
            </span>
          )}
        </button>

        {configOpen && (
          <GridColumnConfig
            columns={columns}
            updateOverride={updateOverride}
            resetOverrides={resetOverrides}
            onClose={() => setConfigOpen(false)}
          />
        )}
      </div>

      {/* ---- grid ---- */}
      <table>
        <thead>
          <tr>
            <th style={{ width: 30 }}>#</th>
            {visibleColumns.map((c) => (
              <th
                key={c.key}
                className={c.numeric ? 'num' : ''}
                style={{ width: c.width }}
              >
                {t(c.header)}
              </th>
            ))}
            <th style={{ width: 32 }} />
          </tr>
        </thead>

        <tbody>
          {lines.map((l, i) => (
            <TrxLineRow
              key={l.id}
              idx={i + 1}
              line={l}
              cols={visibleColumns}
              editing={editing}
              setEditing={setEditing}
              update={update}
              remove={remove}
            />
          ))}
          <tr
            className="adder"
            onClick={(e) => { e.stopPropagation(); addLine(); }}
          >
            <td>
              <div className="cell">{lines.length + 1}</div>
            </td>
            <td colSpan={visibleColumns.length + 1}>
              <div className="cell" style={{ cursor: 'pointer' }}>
                <Icon name="plus" size={11} />
                &nbsp; Klik di sini atau tekan <Kbd>+</Kbd> untuk menambah baris…
              </div>
            </td>
          </tr>
        </tbody>

        <tfoot>
          <tr>
            <td colSpan={3} style={{ textAlign: 'right', paddingRight: 12 }}>
              {t('Total')} {head.uang}
            </td>
            {cfg.journal ? (
              <>
                <td className="num">{fmtIDR(total)}</td>
                <td className="num">{fmtIDR(totalKredit)}</td>
                <td colSpan={3} />
              </>
            ) : (
              <>
                <td className="num">{fmtIDR(total)}</td>
                <td colSpan={4} />
              </>
            )}
          </tr>
        </tfoot>
      </table>
    </div>
  );
}
