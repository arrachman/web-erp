'use client';

// Presentational helpers + constants for the Setting → Tampilan page.
// Split out of `appearance.tsx` to keep each file ≤400 lines.
import * as React from 'react';
import { Icon, type IconName } from '@/components/ui/icons';
import { Sparkline } from '@/components/ui/sparkline';
import { KPI_SERIES, type Translator } from '@/lib/mock';

export type Lang = 'id' | 'en' | 'ja';
export type FontScale = 'sm' | 'base' | 'lg' | 'xl';
export type Density = 'compact' | 'comfortable';
export type SidebarMode = 'icon' | 'label' | 'horizontal';
export type SidebarMenuMode = 'flyout' | 'accordion';

export const STORAGE_KEY = 'erp-appearance';

export interface Tweaks {
  primary: string;
  density: Density;
  fontScale: FontScale;
  sidebar: SidebarMode;
  sidebarMenu: SidebarMenuMode;
  lang: Lang;
  urlRouting: boolean;
}

export const DEFAULTS: Tweaks = {
  primary: 'blue',
  density: 'compact',
  fontScale: 'base',
  sidebar: 'icon',
  sidebarMenu: 'flyout',
  lang: 'id',
  urlRouting: false,
};

export interface SegOption {
  v: string;
  label: string;
  icon?: IconName;
}

export function Seg({
  value,
  options,
  onChange,
}: {
  value: string | undefined;
  options: SegOption[];
  onChange: (v: string) => void;
}) {
  return (
    <div
      style={{
        display: 'inline-flex',
        border: '1px solid var(--border)',
        borderRadius: 7,
        overflow: 'hidden',
      }}
    >
      {options.map((o, i) => (
        <button
          key={o.v}
          onClick={() => onChange(o.v)}
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: 6,
            padding: '6px 12px',
            border: 0,
            borderLeft: i ? '1px solid var(--border)' : 0,
            background: value === o.v ? 'var(--primary)' : 'var(--panel)',
            color: value === o.v ? 'var(--primary-fg)' : 'var(--fg-muted)',
            font: 'inherit',
            fontSize: 'calc(12px * var(--font-scale, 1))',
            cursor: 'pointer',
          }}
        >
          {o.icon && <Icon name={o.icon} size={12} />}
          {o.label}
        </button>
      ))}
    </div>
  );
}

export function SetRow({
  label,
  hint,
  children,
}: {
  label: string;
  hint?: string;
  children: React.ReactNode;
}) {
  return (
    <div
      style={{
        display: 'grid',
        gridTemplateColumns: '200px 1fr',
        gap: 16,
        alignItems: 'center',
        padding: '10px 0',
        borderTop: '1px solid var(--border)',
      }}
    >
      <div>
        <div style={{ fontSize: 'calc(12.5px * var(--font-scale, 1))' }}>{label}</div>
        {hint && (
          <div className="muted" style={{ fontSize: 'calc(11px * var(--font-scale, 1))', marginTop: 2 }}>
            {hint}
          </div>
        )}
      </div>
      <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
        {children}
      </div>
    </div>
  );
}

export function SetCard({
  icon,
  title,
  sub,
  children,
}: {
  icon: IconName;
  title: string;
  sub?: string;
  children: React.ReactNode;
}) {
  return (
    <div className="card" style={{ gridColumn: 'span 6' }}>
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
          <Icon name={icon} size={13} />
        </span>
        <div>
          <div className="title">{title}</div>
          {sub && (
            <div className="sub" style={{ marginTop: 1 }}>
              {sub}
            </div>
          )}
        </div>
      </div>
      <div className="card-b" style={{ paddingTop: 2 }}>
        {children}
      </div>
    </div>
  );
}

export const SWATCHES = [
  { v: 'blue', c: '#2563eb', label: 'Biru' },
  { v: 'indigo', c: '#4f46e5', label: 'Indigo' },
  { v: 'violet', c: '#7c3aed', label: 'Violet' },
  { v: 'fuchsia', c: '#c026d3', label: 'Fuchsia' },
  { v: 'rose', c: '#e11d48', label: 'Rose' },
  { v: 'amber', c: '#d97706', label: 'Amber' },
  { v: 'emerald', c: '#059669', label: 'Emerald' },
  { v: 'teal', c: '#0d9488', label: 'Teal' },
  { v: 'cyan', c: '#0891b2', label: 'Cyan' },
];

export const PALETTE_PACKS = [
  { v: 'blue', label: 'Korporat', sub: 'Biru profesional', colors: ['#2563eb', '#0891b2', '#0d9488'] },
  { v: 'violet', label: 'Kreatif', sub: 'Violet & fuchsia', colors: ['#7c3aed', '#c026d3', '#e11d48'] },
  { v: 'emerald', label: 'Natural', sub: 'Hijau segar', colors: ['#059669', '#0d9488', '#65a30d'] },
  { v: 'amber', label: 'Hangat', sub: 'Amber & rose', colors: ['#d97706', '#e11d48', '#c026d3'] },
];

export const FONT_PX: Record<FontScale, number> = {
  sm: 11,
  base: 13,
  lg: 15,
  xl: 17,
};

const PREVIEW_ITEMS = [
  { ic: 'home', lb: 'Dashboard' },
  { ic: 'coins', lb: 'Keuangan' },
  { ic: 'cart', lb: 'Pembelian' },
] as const;

/** Sidebar mode SetCard — extracted to keep appearance.tsx ≤400 lines. */
export function SidebarModeCard({
  sidebar,
  sidebarMenu,
  onChange,
  onMenuMode,
  t,
}: {
  sidebar: SidebarMode;
  sidebarMenu: SidebarMenuMode;
  onChange: (v: SidebarMode) => void;
  onMenuMode: (v: SidebarMenuMode) => void;
  t: Translator;
}) {
  return (
    <SetCard icon="database" title={t('Menu Sidebar')} sub={t('Template navigasi samping')}>
      <SetRow label={t('Template')} hint={t('Ikon saja, dengan label teks, atau horizontal di atas')}>
        <Seg
          value={sidebar || 'icon'}
          onChange={(v) => onChange(v as SidebarMode)}
          options={[
            { v: 'icon', label: t('Ikon'), icon: 'boxes' },
            { v: 'label', label: t('Ikon + Label'), icon: 'database' },
            { v: 'horizontal', label: t('Horizontal'), icon: 'layers' },
          ]}
        />
      </SetRow>
      {/* Mode Menu only shown when sidebar is vertical (icon/label) */}
      {sidebar !== 'horizontal' && (
        <SetRow label={t('Mode Menu')} hint={t('Flyout: submenu muncul di kanan saat hover · Accordion: submenu expand di bawah modul')}>
          <Seg
            value={sidebarMenu || 'flyout'}
            onChange={(v) => onMenuMode(v as SidebarMenuMode)}
            options={[
              { v: 'flyout', label: t('Flyout'), icon: 'layers' },
              { v: 'accordion', label: t('Accordion'), icon: 'chevdown' },
            ]}
          />
        </SetRow>
      )}
      <SetRow label={t('Pratinjau')}>
        <div style={{ display: 'inline-flex', flexDirection: sidebar === 'horizontal' ? 'row' : 'column', gap: 3, border: '1px solid var(--border)', borderRadius: 8, padding: 8, background: 'var(--panel-2)', minWidth: sidebar === 'horizontal' ? 'auto' : sidebar === 'label' ? 170 : 'auto', ...(sidebar === 'horizontal' ? { alignItems: 'center', gap: 4 } : {}) }}>
          {PREVIEW_ITEMS.map(({ ic, lb }, i) => (
            <React.Fragment key={ic}>
              <span style={{ display: 'flex', alignItems: 'center', gap: 8, padding: '5px 8px', borderRadius: 6, fontSize: 'calc(12px * var(--font-scale, 1))', background: i === 0 ? 'var(--primary-soft)' : 'transparent', color: i === 0 ? 'var(--primary-soft-fg)' : 'var(--fg-muted)' }}>
                <Icon name={ic} size={14} />
                {sidebar === 'horizontal' && <span style={{ marginLeft: 2 }}>{t(lb)}</span>}
                {sidebar === 'label' && <span style={{ flex: 1 }}>{t(lb)}</span>}
                {sidebar === 'label' && i === 0 && sidebarMenu === 'accordion' && <Icon name="chevdown" size={10} />}
              </span>
              {i === 0 && sidebarMenu === 'accordion' && sidebar !== 'horizontal' && (
                <span style={{ display: 'flex', alignItems: 'center', gap: 6, padding: '3px 8px 3px 28px', fontSize: 'calc(11px * var(--font-scale, 1))', color: 'var(--primary)' }}>
                  <Icon name="dot" size={8} /> {sidebar === 'label' && <span>{t('Sub Menu')}</span>}
                </span>
              )}
            </React.Fragment>
          ))}
        </div>
      </SetRow>
    </SetCard>
  );
}

/** URL routing toggle card — sync browser URL to active tab route. */
export function UrlRoutingCard({
  urlRouting,
  onChange,
  t,
}: {
  urlRouting: boolean;
  onChange: (v: boolean) => void;
  t: Translator;
}) {
  return (
    <SetCard icon="layers" title={t('URL Routing')} sub={t('Sinkronisasi URL browser dengan halaman aktif')}>
      <SetRow label={t('Mode')} hint={t('Per-halaman URL: URL browser ikut route aktif; Internal: navigasi tidak mengubah URL')}>
        <Seg
          value={urlRouting ? 'routing' : 'internal'}
          onChange={(v) => onChange(v === 'routing')}
          options={[
            { v: 'internal', label: t('Internal'), icon: 'boxes' },
            { v: 'routing', label: t('Per-halaman URL'), icon: 'layers' },
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
