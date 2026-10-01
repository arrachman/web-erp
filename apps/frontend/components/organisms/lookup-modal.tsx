'use client';

import * as React from 'react';
import { Icon, type IconName } from '@/components/ui/icons';
import { Kbd } from '@/components/ui/kbd';
import { LOKASIS } from '@/lib/mock';
import { cn } from '@/lib/utils';

/**
 * Generic lookup picker (Organism) — single overlay reusable for any
 * "magnifier" field (CoA, Lokasi/Gudang, Cost Center). Ported 1:1 from
 * prototype `lookup-modal.jsx`. Uses legacy CSS (`.cp-backdrop`,
 * `.lookup-modal`, `.cm-head`, `.tbl`, `.cm-foot`) — no new styles.
 */

export type LookupKind = 'coa' | 'lokasi' | 'cc';

export interface LookupRow {
  id: number;
  code: string;
  name: string;
  tipe?: string;
}

export interface LookupModalProps {
  open: boolean;
  kind: LookupKind | null;
  onSelect: (value: string, row: LookupRow) => void;
  onClose: () => void;
}

interface LookupConfig {
  title: string;
  icon: IconName;
  columns: readonly string[];
  fields: readonly (keyof LookupRow)[];
  rows: LookupRow[];
  pick: (row: LookupRow) => string;
}

const COA_RAW: ReadonlyArray<readonly [string, string, string]> = [
  ['110101.101', 'Cash (IDR)', 'Aktiva'],
  ['110101.102', 'Kas Kecil PCI', 'Aktiva'],
  ['110102.001', 'Bank BCA Operasional', 'Aktiva'],
  ['110102.002', 'Bank Mandiri', 'Aktiva'],
  ['110103.001', 'Giro/Cek Diterima', 'Aktiva'],
  ['110201.101', 'Piutang Usaha', 'Aktiva'],
  ['110301.101', 'Persediaan Barang Jadi', 'Aktiva'],
  ['120101.101', 'Mesin & Peralatan', 'Aktiva'],
  ['210101.001', 'Hutang Usaha', 'Kewajiban'],
  ['210103.001', 'Giro/Cek Dikeluarkan', 'Kewajiban'],
  ['210105.001', 'PPN Keluaran', 'Kewajiban'],
  ['310101.101', 'Modal Disetor', 'Modal'],
  ['410101.101', 'Penjualan Barang Jadi', 'Pendapatan'],
  ['410102.101', 'Pendapatan Jasa', 'Pendapatan'],
  ['510101.101', 'HPP', 'Beban'],
  ['610101.101', 'Beban Gaji', 'Beban'],
  ['610102.101', 'Beban Listrik', 'Beban'],
  ['610103.101', 'Beban Penyusutan', 'Beban'],
];

const COA_ROWS: LookupRow[] = COA_RAW.map((r, i) => ({
  id: i + 1,
  code: r[0],
  name: r[1],
  tipe: r[2],
}));

const LOKASI_ROWS: LookupRow[] = LOKASIS.map((name, i) => ({
  id: i + 1,
  code: `LOC-${String(i + 1).padStart(3, '0')}`,
  name,
}));

const CC_NAMES = ['PCI-SALES', 'PCI-PROD', 'JKT-ADM', 'BDG-LOG', 'R&D-ENG', 'UMUM'] as const;
const CC_ROWS: LookupRow[] = CC_NAMES.map((name, i) => ({
  id: i + 1,
  code: `CC-${i + 1}`,
  name,
}));

const LOOKUPS: Record<LookupKind, LookupConfig> = {
  coa: {
    title: 'Pencarian Akun (CoA)',
    icon: 'book',
    columns: ['Kode', 'Nama Akun', 'Tipe'],
    fields: ['code', 'name', 'tipe'],
    rows: COA_ROWS,
    pick: (r) => `${r.code} - ${r.name}`,
  },
  lokasi: {
    title: 'Pilih Lokasi / Gudang',
    icon: 'boxes',
    columns: ['Kode', 'Lokasi'],
    fields: ['code', 'name'],
    rows: LOKASI_ROWS,
    pick: (r) => r.name,
  },
  cc: {
    title: 'Pilih Cost Center',
    icon: 'layers',
    columns: ['Kode', 'Cost Center'],
    fields: ['code', 'name'],
    rows: CC_ROWS,
    pick: (r) => r.name,
  },
};

export function LookupModal({
  open,
  kind,
  onSelect,
  onClose,
}: LookupModalProps): React.ReactElement | null {
  const cfg = kind ? LOOKUPS[kind] : null;
  const [q, setQ] = React.useState('');
  const [foc, setFoc] = React.useState(0);
  const inputRef = React.useRef<HTMLInputElement>(null);
  const listRef = React.useRef<HTMLDivElement>(null);

  React.useEffect(() => {
    if (!open) return;
    setQ('');
    setFoc(0);
    const t = window.setTimeout(() => inputRef.current?.focus(), 30);
    return () => window.clearTimeout(t);
  }, [open]);

  const rows = React.useMemo<LookupRow[]>(() => {
    if (!cfg) return [];
    if (!q) return cfg.rows;
    const ql = q.toLowerCase();
    return cfg.rows.filter((r) =>
      cfg.fields.some((f) => String(r[f] ?? '').toLowerCase().includes(ql)),
    );
  }, [q, cfg]);

  React.useEffect(() => {
    setFoc(0);
  }, [q]);

  React.useEffect(() => {
    const el = listRef.current?.querySelector(`[data-r="${foc}"]`);
    if (el) (el as HTMLElement).scrollIntoView({ block: 'nearest' });
  }, [foc]);

  if (!open || !cfg) return null;

  const choose = (r: LookupRow): void => {
    onSelect(cfg.pick(r), r);
    onClose();
  };

  const onKey = (e: React.KeyboardEvent<HTMLDivElement>): void => {
    if (e.key === 'Escape') {
      onClose();
    } else if (e.key === 'ArrowDown') {
      e.preventDefault();
      setFoc((f) => Math.min(rows.length - 1, f + 1));
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      setFoc((f) => Math.max(0, f - 1));
    } else if (e.key === 'Enter') {
      e.preventDefault();
      if (rows[foc]) choose(rows[foc]);
    }
  };

  return (
    <div
      className="cp-backdrop"
      style={{ zIndex: 720 }}
      onMouseDown={(e) => {
        if (e.target === e.currentTarget) onClose();
      }}
      role="presentation"
    >
      <div
        className="lookup-modal fade-in"
        onKeyDown={onKey}
        role="dialog"
        aria-modal="true"
        aria-label={cfg.title}
      >
        <div className="cm-head">
          <div className="cm-title">
            <Icon name={cfg.icon} size={14} />
            <span>{cfg.title}</span>
            <span
              className="muted"
              style={{ fontWeight: 400, fontSize: 'calc(11.5px * var(--font-scale, 1))', marginLeft: 4 }}
            >
              · {rows.length}
            </span>
          </div>
          <div className="cm-search">
            <Icon name="search" size={13} />
            <input
              ref={inputRef}
              placeholder="Ketik kode atau nama..."
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

        <div className="lookup-list scrollbar" ref={listRef}>
          <table className="tbl">
            <thead>
              <tr>
                {cfg.columns.map((c) => (
                  <th key={c}>{c}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {rows.map((r, i) => (
                <tr
                  key={r.id}
                  data-r={i}
                  className={cn(i === foc && 'focused')}
                  onMouseEnter={() => setFoc(i)}
                  onClick={() => choose(r)}
                  style={{ cursor: 'pointer' }}
                >
                  {cfg.fields.map((f, j) => (
                    <td
                      key={f as string}
                      className={cn(j === 0 && 'mono')}
                      style={
                        j === 0 ? { color: 'var(--primary-soft-fg)' } : undefined
                      }
                    >
                      {String(r[f] ?? '')}
                    </td>
                  ))}
                </tr>
              ))}
              {rows.length === 0 && (
                <tr>
                  <td colSpan={cfg.columns.length} className="tbl-empty">
                    Tidak ditemukan
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>

        <div className="cm-foot">
          <span>
            <Kbd>↑</Kbd>
            <Kbd>↓</Kbd> navigasi
          </span>
          <span>
            <Kbd>↵</Kbd> pilih
          </span>
          <span>
            <Kbd>ESC</Kbd> tutup
          </span>
          <button
            type="button"
            className="btn ghost sm"
            style={{ marginLeft: 'auto' }}
            onClick={onClose}
          >
            Batal
          </button>
        </div>
      </div>
    </div>
  );
}
