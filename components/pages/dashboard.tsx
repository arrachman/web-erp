'use client';

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Kbd } from '@/components/ui/kbd';
import { Card } from '@/components/ui/card';
import { Sparkline } from '@/components/ui/sparkline';
import { BarChart } from '@/components/ui/bar-chart';
import { DonutChart } from '@/components/ui/donut-chart';
import { ACTIVITY, KPI_SERIES, fmtCompact, fmtIDR, type Translator } from '@/lib/mock';
import { QuickAction } from './quick-action';

interface DashboardProps {
  t: Translator;
  onNavigate: (route: string) => void;
}

/** Representative dashboard page — ported from prototype `pages/dashboard.jsx`. */
export function Dashboard({ t, onNavigate }: DashboardProps) {
  const kpis = [
    {
      key: 'Pendapatan bulan ini',
      value: 487520000,
      delta: 12.4,
      deltaInverse: false,
      series: KPI_SERIES.kasMasuk,
      color: 'var(--primary)',
    },
    {
      key: 'Pengeluaran bulan ini',
      value: 261840000,
      delta: -3.1,
      deltaInverse: true,
      series: KPI_SERIES.kasKeluar,
      color: 'var(--warn)',
    },
    {
      key: 'Saldo Bank',
      value: 1284910000,
      delta: 8.7,
      deltaInverse: false,
      series: KPI_SERIES.bankMasuk,
      color: 'var(--success)',
    },
    {
      key: 'Giro Outstanding',
      value: 92450000,
      delta: 2.2,
      deltaInverse: false,
      series: KPI_SERIES.giro,
      color: 'var(--info)',
    },
  ];

  const flow = KPI_SERIES.kasMasuk.map(
    (v, i) => (v - KPI_SERIES.kasKeluar[i]) * 100000,
  );

  const topTrx = [
    { no: 'CR-2605-2398', desc: 'PT Sumber Rejeki', total: 4250000, kind: 'in' },
    { no: 'RM-2605-0871', desc: 'PT Karya Mandiri', total: 12500000, kind: 'in' },
    { no: 'SM-2605-1182', desc: 'PT Cahaya Listrik', total: -1850000, kind: 'out' },
    { no: 'CD-2605-1640', desc: 'Operasional cabang', total: -3200000, kind: 'out' },
    { no: 'CR-2605-2397', desc: 'Toko Berkah Jaya', total: 8750000, kind: 'in' },
    { no: 'GJ-2605-0412', desc: 'Penyesuaian akhir bulan', total: 0, kind: 'jnl' },
  ];

  const dist = [
    { v: 42, color: 'var(--primary)', label: t('Sales') },
    { v: 22, color: 'var(--success)', label: t('Pembelian') },
    { v: 18, color: 'var(--warn)', label: t('Persediaan') },
    { v: 10, color: 'var(--info)', label: t('Produksi') },
    { v: 8, color: 'var(--fg-faint)', label: 'Lainnya' },
  ];

  return (
    <div className="page scrollbar" style={{ overflow: 'auto' }}>
      <div className="page-header">
        <h1 className="page-title">
          {t('Dashboard')}
          <span className="code-tag">12 Mei 2026 · PCI</span>
        </h1>
        <div className="page-actions">
          <button className="btn">
            <Icon name="calendar" size={12} /> Bulan ini
          </button>
          <button className="btn">
            <Icon name="refresh" size={12} /> Refresh
          </button>
          <button className="btn primary">
            <Icon name="plus" size={12} /> {t('Tambah')} <Kbd>N</Kbd>
          </button>
        </div>
      </div>

      <div className="dash-grid">
        {kpis.map((k) => {
          const eff = k.deltaInverse ? -k.delta : k.delta;
          return (
            <Card key={k.key} style={{ gridColumn: 'span 3' }}>
              <div className="kpi">
                <div className="label">{t(k.key)}</div>
                <div className="value">Rp {fmtCompact(k.value)}</div>
                <div className={`delta ${eff >= 0 ? 'up' : 'down'}`}>
                  <Icon
                    name={eff >= 0 ? 'arrow-tr' : 'arrow-br'}
                    size={11}
                  />
                  {k.delta > 0 ? '+' : ''}
                  {k.delta.toFixed(1)}%{' '}
                  <span className="muted" style={{ marginLeft: 2 }}>
                    {t('vs bulan lalu')}
                  </span>
                </div>
                <div className="spark">
                  <Sparkline data={[...k.series]} color={k.color} />
                </div>
              </div>
            </Card>
          );
        })}

        <Card style={{ gridColumn: 'span 8' }}>
          <div className="card-h">
            <div>
              <div className="title">{t('Cash Flow 14 Hari')}</div>
              <div className="sub" style={{ marginTop: 1 }}>
                Net = Masuk − Keluar (juta IDR)
              </div>
            </div>
            <div className="more" style={{ display: 'flex', gap: 6 }}>
              <div className="chip">14D</div>
              <div className="chip active">
                <span className="val">30D</span>
              </div>
              <div className="chip">90D</div>
            </div>
          </div>
          <div className="card-b" style={{ color: 'var(--primary)' }}>
            <BarChart
              data={flow.map((v) => v / 1e6)}
              color="var(--primary)"
              negColor="var(--danger)"
              height={140}
            />
            <div
              style={{
                display: 'flex',
                justifyContent: 'space-between',
                fontSize: 'calc(10.5px * var(--font-scale, 1))',
                color: 'var(--fg-subtle)',
                marginTop: 4,
                fontFamily: 'Geist Mono, monospace',
              }}
            >
              {['29/04', '01', '03', '05', '07', '09', '12'].map((d) => (
                <span key={d}>{d}</span>
              ))}
            </div>
          </div>
        </Card>

        <Card style={{ gridColumn: 'span 4' }}>
          <div className="card-h">
            <div className="title">Komposisi Transaksi</div>
            <div className="sub" style={{ marginLeft: 6 }}>
              MTD
            </div>
          </div>
          <div
            className="card-b"
            style={{ display: 'flex', alignItems: 'center', gap: 16 }}
          >
            <DonutChart slices={dist} size={108} stroke={16} />
            <div style={{ flex: 1 }}>
              {dist.map((s) => (
                <div
                  key={s.label}
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: 8,
                    fontSize: 'calc(12px * var(--font-scale, 1))',
                    padding: '3px 0',
                  }}
                >
                  <span
                    style={{
                      width: 8,
                      height: 8,
                      borderRadius: 2,
                      background: s.color,
                    }}
                  />
                  <span style={{ flex: 1 }}>{s.label}</span>
                  <span
                    className="mono muted"
                    style={{
                      fontFamily: 'Geist Mono, monospace',
                      fontSize: 'calc(11.5px * var(--font-scale, 1))',
                    }}
                  >
                    {s.v}%
                  </span>
                </div>
              ))}
            </div>
          </div>
        </Card>

        <Card style={{ gridColumn: 'span 4' }}>
          <div className="card-h">
            <div className="title">{t('Aksi Cepat')}</div>
            <div className="sub" style={{ marginLeft: 'auto' }}>
              {t('Pintasan')}
            </div>
          </div>
          <div
            className="card-b"
            style={{
              display: 'grid',
              gridTemplateColumns: '1fr 1fr',
              gap: 8,
            }}
          >
            <QuickAction
              icon="wallet"
              label={t('Buat Kas Masuk')}
              hint="N C"
              onClick={() => onNavigate('kas-masuk')}
            />
            <QuickAction
              icon="wallet"
              label={t('Buat Kas Keluar')}
              hint="N D"
              onClick={() => onNavigate('kas-keluar')}
            />
            <QuickAction
              icon="book"
              label={t('Posting Jurnal')}
              hint="N J"
              onClick={() => onNavigate('jurnal-umum')}
            />
            <QuickAction
              icon="book"
              label={t('Lihat Buku Besar')}
              hint="G L"
              onClick={() => onNavigate('buku-besar')}
            />
            <QuickAction
              icon="bank"
              label={t('Bank Masuk')}
              hint="N B"
              onClick={() => onNavigate('bank-masuk')}
            />
            <QuickAction
              icon="receipt"
              label={t('Giro Masuk')}
              hint="N G"
              onClick={() => onNavigate('giro-masuk')}
            />
          </div>
        </Card>

        <Card style={{ gridColumn: 'span 4' }}>
          <div className="card-h">
            <div className="title">{t('Top Transaksi Hari Ini')}</div>
            <button className="btn ghost sm" style={{ marginLeft: 'auto' }}>
              {t('Lihat semua')}
            </button>
          </div>
          <div>
            {topTrx.map((trx, i) => (
              <div
                key={trx.no}
                style={{
                  display: 'grid',
                  gridTemplateColumns: 'auto 1fr auto',
                  gap: 10,
                  padding: '8px 12px',
                  borderTop: i === 0 ? 0 : '1px solid var(--border)',
                  alignItems: 'center',
                  fontSize: 'calc(12px * var(--font-scale, 1))',
                }}
              >
                <span
                  className={`pill ${
                    trx.kind === 'in'
                      ? 'success'
                      : trx.kind === 'out'
                        ? 'danger'
                        : 'info'
                  }`}
                  style={{
                    fontFamily: 'Geist Mono, monospace',
                    fontSize: 'calc(10px * var(--font-scale, 1))',
                  }}
                >
                  {trx.no.split('-')[0]}
                </span>
                <div style={{ minWidth: 0 }}>
                  <div
                    style={{
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                      whiteSpace: 'nowrap',
                    }}
                  >
                    {trx.desc}
                  </div>
                  <div
                    style={{
                      fontSize: 'calc(11px * var(--font-scale, 1))',
                      color: 'var(--fg-subtle)',
                      fontFamily: 'Geist Mono, monospace',
                    }}
                  >
                    {trx.no}
                  </div>
                </div>
                <div
                  className="mono"
                  style={{
                    fontFamily: 'Geist Mono, monospace',
                    fontVariantNumeric: 'tabular-nums',
                    fontWeight: 500,
                    color:
                      trx.total > 0
                        ? 'var(--success)'
                        : trx.total < 0
                          ? 'var(--danger)'
                          : 'var(--fg-muted)',
                  }}
                >
                  {trx.total === 0
                    ? '—'
                    : (trx.total > 0 ? '+' : '') + fmtIDR(trx.total)}
                </div>
              </div>
            ))}
          </div>
        </Card>

        <Card style={{ gridColumn: 'span 4' }}>
          <div className="card-h">
            <div className="title">{t('Aktivitas Terbaru')}</div>
            <button className="btn ghost sm" style={{ marginLeft: 'auto' }}>
              <Icon name="filter" size={11} /> Filter
            </button>
          </div>
          <div className="activity-list">
            {ACTIVITY.slice(0, 7).map((a) => (
              <div
                key={`${a.target}-${a.ts}`}
                className={`activity-row ${a.type}`}
              >
                <span className="dot" />
                <div>
                  <span className="who">{a.who}</span>{' '}
                  <span className="meta">{a.what}</span>{' '}
                  <span
                    style={{
                      fontFamily: 'Geist Mono, monospace',
                      fontSize: 'calc(11px * var(--font-scale, 1))',
                    }}
                  >
                    {a.target}
                  </span>
                  {a.amount != null && (
                    <span
                      className="mono"
                      style={{
                        marginLeft: 6,
                        fontFamily: 'Geist Mono, monospace',
                        fontVariantNumeric: 'tabular-nums',
                        color:
                          a.amount > 0
                            ? 'var(--success)'
                            : 'var(--danger)',
                      }}
                    >
                      {a.amount > 0 ? '+' : ''}
                      {fmtIDR(a.amount)}
                    </span>
                  )}
                </div>
                <span className="ts">{a.ts}</span>
              </div>
            ))}
          </div>
        </Card>
      </div>
    </div>
  );
}
