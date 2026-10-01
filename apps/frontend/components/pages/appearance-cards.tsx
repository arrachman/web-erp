'use client';

// Presentational card components for Setting → Tampilan.
// Extracted from appearance.tsx to keep each file ≤400 lines.
import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Sparkline } from '@/components/ui/sparkline';
import { KPI_SERIES, type Translator } from '@/lib/mock';
import {
  FONT_PX,
  PALETTE_PACKS,
  Seg,
  SetCard,
  SetRow,
  SWATCHES,
  type Density,
  type FontScale,
  type Lang,
  type Tweaks,
} from './appearance-parts';

// Re-export Translator so appearance.tsx doesn't need to import directly from @/lib/mock.
export type { Translator };

interface ApplyFn {
  <K extends keyof Tweaks>(key: K, val: Tweaks[K]): void;
}

/** Tema + Bahasa card */
export function ThemeLanguageCard({
  theme,
  lang,
  setTheme,
  applyTweak,
  t,
}: {
  theme: string | undefined;
  lang: Lang;
  setTheme: (v: string) => void;
  applyTweak: ApplyFn;
  t: Translator;
}) {
  return (
    <SetCard icon="moon" title={t('Tema')} sub={t('Mode terang atau gelap')}>
      <SetRow label={t('Mode Tema')} hint={t('Berlaku untuk seluruh aplikasi')}>
        <Seg
          value={theme}
          onChange={(v) => setTheme(v)}
          options={[
            { v: 'light', label: t('Terang'), icon: 'sun' },
            { v: 'dark', label: t('Gelap'), icon: 'moon' },
          ]}
        />
      </SetRow>
      <SetRow label={t('Bahasa')} hint={t('Antarmuka')}>
        <Seg
          value={lang}
          onChange={(v) => applyTweak('lang', v as Lang)}
          options={[
            { v: 'id', label: t('Indonesia') },
            { v: 'en', label: t('English') },
            { v: 'ja', label: t('Japanese') },
          ]}
        />
      </SetRow>
    </SetCard>
  );
}

/** Warna Aksen card — palette packs + swatch dots */
export function AccentColorCard({
  primary,
  applyTweak,
  t,
}: {
  primary: string;
  applyTweak: ApplyFn;
  t: Translator;
}) {
  return (
    <SetCard icon="layers" title={t('Warna Aksen')} sub={t('Paket & warna primer')}>
      <SetRow label={t('Paket Warna')} hint={t('Set warna siap pakai')}>
        <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
          {PALETTE_PACKS.map((p) => (
            <button
              key={p.v}
              onClick={() => applyTweak('primary', p.v)}
              style={{
                display: 'flex', alignItems: 'center', gap: 8,
                padding: '7px 10px', borderRadius: 8, cursor: 'pointer',
                font: 'inherit', textAlign: 'left',
                background: primary === p.v ? 'var(--primary-soft)' : 'var(--panel)',
                border: primary === p.v ? '1px solid var(--primary)' : '1px solid var(--border)',
              }}
            >
              <span style={{ display: 'flex' }}>
                {p.colors.map((c, i) => (
                  <span key={i} style={{
                    width: 13, height: 13, borderRadius: '50%', background: c,
                    marginLeft: i ? -5 : 0, boxShadow: '0 0 0 1.5px var(--panel)',
                  }} />
                ))}
              </span>
              <span style={{ lineHeight: 1.2 }}>
                <span style={{
                  fontSize: 'calc(12px * var(--font-scale, 1))', fontWeight: 600,
                  display: 'block', color: primary === p.v ? 'var(--primary-soft-fg)' : 'var(--fg)',
                }}>{t(p.label)}</span>
                <span className="muted" style={{ fontSize: 'calc(10.5px * var(--font-scale, 1))' }}>
                  {t(p.sub)}
                </span>
              </span>
            </button>
          ))}
        </div>
      </SetRow>
      <SetRow label={t('Warna Spesifik')}>
        <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap' }}>
          {SWATCHES.map((s) => (
            <button
              key={s.v} title={s.label} onClick={() => applyTweak('primary', s.v)}
              style={{
                width: 30, height: 30, borderRadius: '50%', background: s.c,
                cursor: 'pointer',
                border: primary === s.v ? '2px solid var(--fg)' : '2px solid transparent',
                boxShadow: '0 0 0 1px var(--border)',
                display: 'inline-flex', alignItems: 'center', justifyContent: 'center',
                color: '#fff',
              }}
            >
              {primary === s.v && <Icon name="check" size={13} />}
            </button>
          ))}
        </div>
      </SetRow>
      <SetRow label={t('Aksen Aktif')}>
        <span className="pill primary">
          <span className="dot" />
          {t(SWATCHES.find((s) => s.v === primary)?.label || primary)}
        </span>
      </SetRow>
    </SetCard>
  );
}

/** Ukuran Font card */
export function FontScaleCard({
  fontScale,
  applyTweak,
  t,
}: {
  fontScale: FontScale;
  applyTweak: ApplyFn;
  t: Translator;
}) {
  return (
    <SetCard icon="info" title={t('Ukuran Font')} sub={t('Skala teks antarmuka')}>
      <SetRow label={t('Ukuran')} hint={t('Kecil · Normal · Besar · Ekstra Besar')}>
        <Seg
          value={fontScale}
          onChange={(v) => applyTweak('fontScale', v as FontScale)}
          options={[
            { v: 'sm', label: t('Kecil') },
            { v: 'base', label: t('Normal') },
            { v: 'lg', label: t('Besar') },
            { v: 'xl', label: t('Ekstra Besar') },
          ]}
        />
      </SetRow>
      <SetRow label={t('Pratinjau')}>
        <span style={{ fontSize: FONT_PX[fontScale] || 13 }}>
          {t('Contoh teks tabel & form')} — {fontScale}
        </span>
      </SetRow>
    </SetCard>
  );
}

/** Layout / Kepadatan card */
export function DensityCard({
  density,
  applyTweak,
  t,
}: {
  density: Density;
  applyTweak: ApplyFn;
  t: Translator;
}) {
  return (
    <SetCard icon="boxes" title={t('Layout')} sub={t('Kepadatan tampilan tabel & list')}>
      <SetRow label={t('Kepadatan')} hint={t('Compact memuat lebih banyak baris')}>
        <Seg
          value={density}
          onChange={(v) => applyTweak('density', v as Density)}
          options={[
            { v: 'compact', label: t('Compact') },
            { v: 'comfortable', label: t('Comfortable') },
          ]}
        />
      </SetRow>
    </SetCard>
  );
}

/** Static "Pratinjau Langsung" card — reflects live tweaks via CSS vars. */
export function LivePreviewCard({ t }: { t: Translator }) {
  return (
    <div className="card" style={{ gridColumn: 'span 12' }}>
      <div className="card-h">
        <span
          style={{
            display: 'inline-flex',
            width: 24,
            height: 24,
            alignItems: 'center',
            justifyContent: 'center',
            background: 'var(--primary-soft)',
            color: 'var(--primary-soft-fg)',
            borderRadius: 5,
          }}
        >
          <Icon name="eye" size={13} />
        </span>
        <div>
          <div className="title">{t('Pratinjau Langsung')}</div>
          <div className="sub" style={{ marginTop: 1 }}>
            {t('Perubahan diterapkan seketika')}
          </div>
        </div>
      </div>
      <div
        className="card-b"
        style={{
          display: 'flex',
          gap: 14,
          flexWrap: 'wrap',
          alignItems: 'flex-start',
        }}
      >
        <div
          style={{
            flex: '1 1 220px',
            border: '1px solid var(--border)',
            borderRadius: 8,
            padding: 14,
          }}
        >
          <div className="kpi" style={{ padding: 0 }}>
            <div className="label">{t('Pendapatan bulan ini')}</div>
            <div className="value">Rp 487,5jt</div>
            <div className="delta up">
              <Icon name="arrow-tr" size={11} /> +12,4%
            </div>
            <div className="spark">
              <Sparkline
                data={[...KPI_SERIES.kasMasuk]}
                color="var(--primary)"
              />
            </div>
          </div>
        </div>
        <div
          style={{
            flex: '1 1 240px',
            display: 'flex',
            flexDirection: 'column',
            gap: 10,
          }}
        >
          <div style={{ display: 'flex', gap: 8 }}>
            <button className="btn primary">
              <Icon name="plus" size={12} /> {t('Tambah')}
            </button>
            <button className="btn">
              <Icon name="download" size={12} /> {t('Export')}
            </button>
            <button className="btn ghost">{t('Batal')}</button>
          </div>
          <div style={{ display: 'flex', gap: 8 }}>
            <span className="pill success">
              <span className="dot" />
              Approved
            </span>
            <span className="pill warn">
              <span className="dot" />
              Need Approve
            </span>
            <span className="pill primary">
              <span className="dot" />
              Posted
            </span>
          </div>
          <table
            className="tbl"
            style={{ border: '1px solid var(--border)', borderRadius: 8 }}
          >
            <thead>
              <tr>
                <th>{t('No')}</th>
                <th>{t('Nama')}</th>
                <th className="col-num">{t('Total')}</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td className="mono">CR-2605-2400</td>
                <td>PT Sumber Rejeki</td>
                <td className="num">4.250.000,00</td>
              </tr>
              <tr className="selected">
                <td className="mono">CR-2605-2399</td>
                <td>CV Cahaya Abadi</td>
                <td className="num">1.875.000,00</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
