'use client';

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Sparkline } from '@/components/ui/sparkline';
import { BarChart } from '@/components/ui/bar-chart';
import { DonutChart, type DonutSlice } from '@/components/ui/donut-chart';
import { fmtCompact, type Translator } from '@/lib/mock';

interface StatistikProps {
  t: Translator;
  onNavigate: (route: string) => void;
}

interface Kpi {
  k: string;
  v: string;
  d: number;
  series: number[];
  color: string;
}

/** Analytics dashboard page — ported from prototype `pages/statistik.jsx`. */
export function Statistik({ t, onNavigate }: StatistikProps) {
  const months = ['Jun', 'Jul', 'Agu', 'Sep', 'Okt', 'Nov', 'Des', 'Jan', 'Feb', 'Mar', 'Apr', 'Mei'];
  const revenue = [382, 401, 428, 396, 451, 478, 512, 489, 534, 561, 548, 603];
  const expense = [261, 274, 288, 271, 299, 312, 331, 318, 342, 358, 351, 372];
  const margin = revenue.map((v, i) => +(((v - expense[i]) / v) * 100).toFixed(1));

  const kpis: Kpi[] = [
    { k: 'Pendapatan YTD', v: 'Rp 5,30 M', d: 15.5, series: revenue.slice(-8), color: 'var(--primary)' },
    { k: 'Laba Bersih YTD', v: 'Rp 602 jt', d: 63.8, series: margin.slice(-8), color: 'var(--success)' },
    { k: 'Rata-rata Margin', v: '34,8%', d: 4.2, series: margin.slice(-8), color: 'var(--info)' },
    { k: 'Order Bulan Ini', v: '1.284', d: -2.1, series: [42, 48, 51, 39, 58, 62, 71, 65], color: 'var(--warn)' },
  ];

  const byBranch: DonutSlice[] = [
    { v: 44, color: 'var(--primary)', label: 'PCI' },
    { v: 27, color: 'var(--success)', label: 'JKT' },
    { v: 18, color: 'var(--warn)', label: 'BDG' },
    { v: 11, color: 'var(--info)', label: 'SBY' },
  ];

  const topCust = [
    { n: 'PT Sumber Rejeki', v: 842500000, p: 100 },
    { n: 'PT Karya Mandiri', v: 671200000, p: 80 },
    { n: 'PT Mitra Sentosa', v: 538900000, p: 64 },
    { n: 'PT Global Niaga', v: 412600000, p: 49 },
    { n: 'Toko Berkah Jaya', v: 318400000, p: 38 },
  ];

  const topItem = [
    { n: 'Plat Baja 2mm', q: 4820, u: 'KG' },
    { n: 'Pipa PVC 4"', q: 3140, u: 'M' },
    { n: 'Motor 3 Phase 5HP', q: 286, u: 'UNIT' },
    { n: 'Bearing 6204', q: 2150, u: 'PCS' },
    { n: 'Kabel NYM 3x2.5', q: 1890, u: 'ROLL' },
  ];

  const prodEff: DonutSlice[] = [
    { v: 82, color: 'var(--success)', label: 'Tercapai' },
    { v: 18, color: 'var(--panel-2)', label: 'Sisa' },
  ];

  return (
    <div className="page scrollbar" style={{ overflow: 'auto' }}>
      <div className="page-header">
        <h1 className="page-title">
          {t('Statistik')}
          <span className="code-tag">12 bln · Konsolidasi</span>
        </h1>
        <div className="page-actions">
          <button className="btn">
            <Icon name="calendar" size={12} /> 12 Bulan Terakhir
          </button>
          <button className="btn">
            <Icon name="refresh" size={12} /> Refresh
          </button>
          <button className="btn">
            <Icon name="download" size={12} /> {t('Export')}
          </button>
        </div>
      </div>

      <div className="dash-grid">
        {kpis.map((k) => (
          <div key={k.k} className="card" style={{ gridColumn: 'span 3' }}>
            <div className="kpi">
              <div className="label">{k.k}</div>
              <div className="value">{k.v}</div>
              <div className={`delta ${k.d >= 0 ? 'up' : 'down'}`}>
                <Icon name={k.d >= 0 ? 'arrow-tr' : 'arrow-br'} size={11} />
                {k.d > 0 ? '+' : ''}
                {k.d.toFixed(1)}%{' '}
                <span className="muted" style={{ marginLeft: 2 }}>
                  {t('vs bulan lalu')}
                </span>
              </div>
              <div className="spark">
                <Sparkline data={k.series} color={k.color} />
              </div>
            </div>
          </div>
        ))}

        <div className="card" style={{ gridColumn: 'span 8' }}>
          <div className="card-h">
            <div>
              <div className="title">Pendapatan vs Beban</div>
              <div className="sub" style={{ marginTop: 1 }}>
                12 bulan terakhir (juta IDR)
              </div>
            </div>
            <div className="more" style={{ display: 'flex', gap: 12, fontSize: 'calc(11.5px * var(--font-scale, 1))' }}>
              <span style={{ display: 'flex', alignItems: 'center', gap: 5 }}>
                <span style={{ width: 8, height: 8, borderRadius: 2, background: 'var(--primary)' }} /> Pendapatan
              </span>
              <span style={{ display: 'flex', alignItems: 'center', gap: 5 }}>
                <span style={{ width: 8, height: 8, borderRadius: 2, background: 'var(--danger)' }} /> Beban
              </span>
            </div>
          </div>
          <div className="card-b">
            <div style={{ position: 'relative' }}>
              <div style={{ color: 'var(--primary)' }}>
                <BarChart data={revenue} color="var(--primary)" height={150} />
              </div>
              <div style={{ position: 'absolute', inset: 0, color: 'var(--danger)', opacity: 0.55 }}>
                <BarChart data={expense} color="var(--danger)" height={150} />
              </div>
            </div>
            <div
              style={{
                display: 'flex',
                justifyContent: 'space-between',
                fontSize: 'calc(10.5px * var(--font-scale, 1))',
                color: 'var(--fg-subtle)',
                marginTop: 6,
                fontFamily: 'Geist Mono, monospace',
              }}
            >
              {months.map((m) => (
                <span key={m}>{m}</span>
              ))}
            </div>
          </div>
        </div>

        <div className="card" style={{ gridColumn: 'span 4' }}>
          <div className="card-h">
            <div className="title">Penjualan per Cabang</div>
            <div className="sub" style={{ marginLeft: 6 }}>
              YTD
            </div>
          </div>
          <div className="card-b" style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
            <DonutChart slices={byBranch} size={108} stroke={16} />
            <div style={{ flex: 1 }}>
              {byBranch.map((s) => (
                <div
                  key={s.label}
                  style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 'calc(12px * var(--font-scale, 1))', padding: '3px 0' }}
                >
                  <span style={{ width: 8, height: 8, borderRadius: 2, background: s.color }} />
                  <span style={{ flex: 1 }}>{s.label}</span>
                  <span className="mono muted" style={{ fontFamily: 'Geist Mono, monospace', fontSize: 'calc(11.5px * var(--font-scale, 1))' }}>
                    {s.v}%
                  </span>
                </div>
              ))}
            </div>
          </div>
        </div>

        <div className="card" style={{ gridColumn: 'span 5' }}>
          <div className="card-h">
            <div className="title">Top Customer</div>
            <button
              className="btn ghost sm"
              style={{ marginLeft: 'auto' }}
              onClick={() => onNavigate('m-customer')}
            >
              {t('Lihat semua')}
            </button>
          </div>
          <div style={{ padding: '6px 12px 12px' }}>
            {topCust.map((c, i) => (
              <div key={c.n} style={{ padding: '7px 0', borderTop: i ? '1px solid var(--border)' : 0 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 'calc(12px * var(--font-scale, 1))', marginBottom: 4 }}>
                  <span>{c.n}</span>
                  <span className="mono muted" style={{ fontFamily: 'Geist Mono, monospace' }}>
                    Rp {fmtCompact(c.v)}
                  </span>
                </div>
                <div style={{ height: 5, background: 'var(--panel-2)', borderRadius: 3, overflow: 'hidden' }}>
                  <div style={{ width: `${c.p}%`, height: '100%', background: 'var(--primary)' }} />
                </div>
              </div>
            ))}
          </div>
        </div>

        <div className="card" style={{ gridColumn: 'span 4' }}>
          <div className="card-h">
            <div className="title">Item Terlaris</div>
            <div className="sub" style={{ marginLeft: 6 }}>
              Qty MTD
            </div>
          </div>
          <div>
            {topItem.map((it, i) => (
              <div
                key={it.n}
                style={{
                  display: 'grid',
                  gridTemplateColumns: 'auto 1fr auto',
                  gap: 10,
                  padding: '8px 12px',
                  borderTop: i ? '1px solid var(--border)' : 0,
                  alignItems: 'center',
                  fontSize: 'calc(12px * var(--font-scale, 1))',
                }}
              >
                <span className="pill info" style={{ fontFamily: 'Geist Mono, monospace', fontSize: 'calc(10px * var(--font-scale, 1))' }}>
                  {String(i + 1).padStart(2, '0')}
                </span>
                <span style={{ overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                  {it.n}
                </span>
                <span className="mono" style={{ fontFamily: 'Geist Mono, monospace', fontWeight: 500 }}>
                  {it.q.toLocaleString('id-ID')} <span className="muted">{it.u}</span>
                </span>
              </div>
            ))}
          </div>
        </div>

        <div className="card" style={{ gridColumn: 'span 3' }}>
          <div className="card-h">
            <div className="title">Efisiensi Produksi</div>
          </div>
          <div
            className="card-b"
            style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 8 }}
          >
            <div style={{ position: 'relative', width: 120, height: 120 }}>
              <DonutChart slices={prodEff} size={120} stroke={14} />
              <div
                style={{
                  position: 'absolute',
                  inset: 0,
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  justifyContent: 'center',
                }}
              >
                <div style={{ fontSize: 'calc(22px * var(--font-scale, 1))', fontWeight: 700, fontFamily: 'Geist Mono, monospace' }}>82%</div>
                <div className="muted" style={{ fontSize: 'calc(10.5px * var(--font-scale, 1))' }}>
                  OEE
                </div>
              </div>
            </div>
            <div className="muted" style={{ fontSize: 'calc(11.5px * var(--font-scale, 1))', textAlign: 'center' }}>
              Target 85% · selisih -3 pts
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
