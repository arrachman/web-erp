'use client';

// Presentational helpers + constants for the Setting → Tampilan page.
// Split out of `appearance.tsx` to keep each file ≤400 lines.
import * as React from 'react';
import { Icon, type IconName } from '@/components/ui/icons';
import { type Translator } from '@/lib/mock';

export type Lang = 'id' | 'en' | 'ja';
export type FontScale = 'sm' | 'base' | 'lg' | 'xl';
export type Density = 'compact' | 'comfortable';
export type SidebarMode = 'icon' | 'label' | 'horizontal';
export type SidebarMenuMode = 'flyout' | 'accordion';
/** Layout of the horizontal top menu bar (icon-only or icon+label). */
export type MenubarTemplate = 'icon' | 'label';

export const STORAGE_KEY = 'erp-appearance';

export interface Tweaks {
  primary: string;
  density: Density;
  fontScale: FontScale;
  sidebar: SidebarMode;
  sidebarMenu: SidebarMenuMode;
  /** Horizontal menubar: icon-only or icon+label. Only consumed when sidebar==='horizontal'. */
  menubarTemplate: MenubarTemplate;
  lang: Lang;
  urlRouting: boolean;
}

export const DEFAULTS: Tweaks = {
  primary: 'blue',
  density: 'compact',
  fontScale: 'base',
  sidebar: 'icon',
  sidebarMenu: 'flyout',
  menubarTemplate: 'label',
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

/** Sidebar mode SetCard — extracted to keep appearance.tsx ≤400 lines.
 * Orientation (vertical/horizontal) is the primary knob; the icon-vs-label
 * template and flyout-vs-accordion menu mode apply to BOTH orientations
 * (vertical = sidebar template / flyout panel; horizontal = menubar layout /
 * dropdown style). */
export function SidebarModeCard({
  sidebar,
  sidebarMenu,
  menubarTemplate,
  onChange,
  onMenuMode,
  onMenubarTemplate,
  t,
}: {
  sidebar: SidebarMode;
  sidebarMenu: SidebarMenuMode;
  menubarTemplate: MenubarTemplate;
  onChange: (v: SidebarMode) => void;
  onMenuMode: (v: SidebarMenuMode) => void;
  onMenubarTemplate: (v: MenubarTemplate) => void;
  t: Translator;
}) {
  const isHorizontal = sidebar === 'horizontal';
  // Template value: vertical → sidebar (icon/label); horizontal → menubarTemplate.
  const templateValue = isHorizontal ? menubarTemplate : (sidebar || 'icon');
  const onTemplate = (v: string) => {
    if (isHorizontal) onMenubarTemplate(v as MenubarTemplate);
    else onChange(v as SidebarMode);
  };
  return (
    <SetCard icon="database" title={t('Menu Sidebar')} sub={t('Template navigasi samping')}>
      <SetRow label={t('Posisi Menu')} hint={t('Vertical: sidebar kiri · Horizontal: menu bar di atas')}>
        <Seg
          value={isHorizontal ? 'horizontal' : 'vertical'}
          onChange={(v) => onChange(v === 'horizontal' ? 'horizontal' : (isHorizontal ? 'icon' : sidebar))}
          options={[
            { v: 'vertical', label: t('Vertical'), icon: 'boxes' },
            { v: 'horizontal', label: t('Horizontal'), icon: 'layers' },
          ]}
        />
      </SetRow>
      <SetRow
        label={t('Template')}
        hint={isHorizontal ? t('Ikon saja atau ikon + label di menu bar atas') : t('Ikon saja atau dengan label teks')}
      >
        <Seg
          value={templateValue}
          onChange={onTemplate}
          options={[
            { v: 'icon', label: t('Ikon'), icon: 'boxes' },
            { v: 'label', label: t('Ikon + Label'), icon: 'database' },
          ]}
        />
      </SetRow>
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
      <SetRow label={t('Pratinjau')}>
        <div style={{ display: 'inline-flex', flexDirection: sidebar === 'horizontal' ? 'row' : 'column', gap: 3, border: '1px solid var(--border)', borderRadius: 8, padding: 8, background: 'var(--panel-2)', minWidth: sidebar === 'horizontal' ? 'auto' : sidebar === 'label' ? 170 : 'auto', ...(sidebar === 'horizontal' ? { alignItems: 'center', gap: 4 } : {}) }}>
          {PREVIEW_ITEMS.map(({ ic, lb }, i) => {
            const showLabel = sidebar !== 'horizontal' || menubarTemplate === 'label';
            const isAccordionH = sidebar === 'horizontal' && sidebarMenu === 'accordion';
            return (
              <React.Fragment key={ic}>
                <span style={{ display: 'flex', alignItems: 'center', gap: 8, padding: '5px 8px', borderRadius: 6, fontSize: 'calc(12px * var(--font-scale, 1))', background: i === 0 ? 'var(--primary-soft)' : 'transparent', color: i === 0 ? 'var(--primary-soft-fg)' : 'var(--fg-muted)' }}>
                  <Icon name={ic} size={14} />
                  {showLabel && <span style={{ marginLeft: 2 }}>{t(lb)}</span>}
                  {sidebar === 'label' && i === 0 && sidebarMenu === 'accordion' && <Icon name="chevdown" size={10} />}
                </span>
                {i === 0 && sidebarMenu === 'accordion' && (sidebar === 'label' || isAccordionH) && (
                  <span style={{ display: 'flex', alignItems: 'center', gap: 6, padding: '3px 8px 3px 28px', fontSize: 'calc(11px * var(--font-scale, 1))', color: 'var(--primary)' }}>
                    <Icon name="dot" size={8} /> {sidebar === 'label' && <span>{t('Sub Menu')}</span>}
                    {isAccordionH && <span>{t('Grup A')}</span>}
                  </span>
                )}
              </React.Fragment>
            );
          })}
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

