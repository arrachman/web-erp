'use client';

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { fmtIDR, type Translator } from '@/lib/mock';
import { notify } from '@/lib/feedback';

interface GlAccount {
  code: string;
  name: string;
  open: number;
}

interface LedgerEntry {
  id: number;
  ref: string;
  tgl: string;
  uraian: string;
  debit: number;
  credit: number;
  bal: number;
}

const GL_ACCOUNTS: GlAccount[] = [
  { code: '110101.101', name: 'Cash (IDR)', open: 250000000 },
  { code: '110102.001', name: 'Bank BCA 4520-xxx', open: 812400000 },
  { code: '110102.002', name: 'Bank Mandiri 1390-xxx', open: 318900000 },
  { code: '110201.101', name: 'Piutang Usaha', open: 876500000 },
  { code: '410101.101', name: 'Penjualan Barang Jadi', open: 0 },
  { code: '510101.101', name: 'HPP', open: 0 },
  { code: '610101.101', name: 'Beban Gaji', open: 0 },
];

const URAIAN_KAS: string[] = [
  'Pelunasan piutang',
  'Pembayaran ke supplier',
  'Pembayaran gaji',
  'Setoran tunai',
  'Penarikan bank',
  'Penyesuaian akhir bulan',
  'Biaya operasional',
  'Penjualan tunai',
];

/** Seeded LCG so the ledger is deterministic per account. */
function rng(seed: number): () => number {
  let s = seed >>> 0;
  return () => {
    s = (s * 1664525 + 1013904223) >>> 0;
    return s / 4294967296;
  };
}

interface BukuBesarProps {
  t: Translator;
}

/** Buku Besar — running-balance ledger; ported from prototype `pages/buku-besar.jsx`. */
export function BukuBesar({ t }: BukuBesarProps) {
  const [active, setActive] = React.useState(GL_ACCOUNTS[0].code);
  const [q, setQ] = React.useState('');

  const acct = GL_ACCOUNTS.find((a) => a.code === active) || GL_ACCOUNTS[0];
  const accounts = GL_ACCOUNTS.filter(
    (a) =>
      !q ||
      a.code.includes(q) ||
      a.name.toLowerCase().includes(q.toLowerCase()),
  );

  const entries = React.useMemo<LedgerEntry[]>(() => {
    const seed = active
      .split('')
      .reduce((s, c) => s + c.charCodeAt(0), 7);
    const r = rng(seed);
    const raw = Array.from({ length: 32 }, (_, i) => {
      const debit = r() > 0.5 ? Math.round((r() * 5000000) / 1000) * 1000 : 0;
      const credit = !debit ? Math.round((r() * 4500000) / 1000) * 1000 : 0;
      return { i, debit, credit };
    });
    return raw.map(({ i, debit, credit }, idx) => {
      const bal =
        acct.open +
        raw
          .slice(0, idx + 1)
          .reduce((sum, x) => sum + x.debit - x.credit, 0);
      const day = (i % 12) + 1;
      return {
        id: i,
        ref: `${['CR', 'RM', 'GJ', 'SM', 'CD'][i % 5]}-2605-${String(
          2400 - i,
        ).padStart(4, '0')}`,
        tgl: `${String(day).padStart(2, '0')}/05/2026`,
        uraian: URAIAN_KAS[i % URAIAN_KAS.length],
        debit,
        credit,
        bal,
      };
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [active]);

  const sumD = entries.reduce((s, e) => s + e.debit, 0);
  const sumK = entries.reduce((s, e) => s + e.credit, 0);
  const endBal = entries.length
    ? entries[entries.length - 1].bal
    : acct.open;

  const gridStyle: React.CSSProperties = {
    display: 'grid',
    gridTemplateColumns: '280px 1fr',
    height: '100%',
    minHeight: 0,
    flex: 1,
  };
  const asideStyle: React.CSSProperties = {
    background: 'var(--panel)',
    borderRight: '1px solid var(--border)',
    overflow: 'auto',
  };
  const summaryStyle: React.CSSProperties = {
    display: 'flex',
    gap: 16,
    padding: '10px 16px',
    borderBottom: '1px solid var(--border)',
    background: 'var(--panel-2)',
    fontSize: 'calc(12.5px * var(--font-scale, 1))',
    flexWrap: 'wrap',
  };
  const monoStyle: React.CSSProperties = { fontFamily: 'Geist Mono, monospace' };

  return (
    <div className="page">
      <div className="page-header">
        <h1 className="page-title">
          {t('Buku Besar')}
          <span className="code-tag">GL</span>
        </h1>
        <div className="page-actions">
          <button className="btn">
            <Icon name="calendar" size={12} /> Mei 2026
          </button>
          <button
            className="btn"
            onClick={() =>
              notify('Buku besar diekspor (.xlsx)', 'success')
            }
          >
            <Icon name="download" size={12} /> {t('Export')}
          </button>
        </div>
      </div>
      <div style={gridStyle}>
        <aside style={asideStyle} className="scrollbar">
          <div
            style={{
              padding: 10,
              borderBottom: '1px solid var(--border)',
            }}
          >
            <div className="search-input" style={{ width: '100%' }}>
              <Icon name="search" size={12} />
              <input
                placeholder="Cari akun..."
                value={q}
                onChange={(e) => setQ(e.target.value)}
              />
            </div>
          </div>
          {accounts.map((a) => (
            <div
              key={a.code}
              className={`flyout-item ${a.code === active ? 'active' : ''}`}
              style={{ margin: 4, padding: '8px 10px' }}
              onClick={() => setActive(a.code)}
            >
              <span
                className="code"
                style={{ marginLeft: 0, marginRight: 8 }}
              >
                {a.code}
              </span>
              <span>{a.name}</span>
            </div>
          ))}
          {accounts.length === 0 && (
            <div
              className="muted"
              style={{ padding: 16, fontSize: 'calc(12px * var(--font-scale, 1))' }}
            >
              Akun tidak ditemukan
            </div>
          )}
        </aside>
        <div className="tbl-wrap scrollbar">
          <div style={summaryStyle}>
            <div>
              <span className="muted">Akun:</span>{' '}
              <strong style={monoStyle}>{acct.code}</strong> {acct.name}
            </div>
            <div>
              <span className="muted">Saldo Awal:</span>{' '}
              <span style={monoStyle}>{fmtIDR(acct.open)}</span>
            </div>
            <div>
              <span className="muted">Debit:</span>{' '}
              <span
                style={{ ...monoStyle, color: 'var(--success)' }}
              >
                {fmtIDR(sumD)}
              </span>
            </div>
            <div>
              <span className="muted">Kredit:</span>{' '}
              <span style={{ ...monoStyle, color: 'var(--danger)' }}>
                {fmtIDR(sumK)}
              </span>
            </div>
            <div style={{ marginLeft: 'auto' }}>
              <span className="muted">Saldo Akhir:</span>{' '}
              <strong style={monoStyle}>{fmtIDR(endBal)}</strong>
            </div>
          </div>
          <table className="tbl">
            <thead>
              <tr>
                <th>Tanggal</th>
                <th>Referensi</th>
                <th>Uraian</th>
                <th className="col-num">Debit</th>
                <th className="col-num">Kredit</th>
                <th className="col-num">Saldo</th>
              </tr>
            </thead>
            <tbody>
              {entries.map((e) => (
                <tr key={e.id}>
                  <td className="mono muted">{e.tgl}</td>
                  <td className="mono">
                    <span style={{ color: 'var(--primary-soft-fg)' }}>
                      {e.ref}
                    </span>
                  </td>
                  <td className="muted">{e.uraian}</td>
                  <td
                    className="num"
                    style={{
                      color: e.debit
                        ? 'var(--success)'
                        : 'var(--fg-faint)',
                    }}
                  >
                    {e.debit ? fmtIDR(e.debit) : '—'}
                  </td>
                  <td
                    className="num"
                    style={{
                      color: e.credit
                        ? 'var(--danger)'
                        : 'var(--fg-faint)',
                    }}
                  >
                    {e.credit ? fmtIDR(e.credit) : '—'}
                  </td>
                  <td className="num">
                    <strong>{fmtIDR(e.bal)}</strong>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
