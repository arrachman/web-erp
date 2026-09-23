'use client';

import { Icon } from '@/components/ui/icons';
import { confirmAction } from '@/lib/feedback';
import {
  LivePreviewCard,
  SidebarModeCard,
  UrlRoutingCard,
  SWATCHES,
} from './appearance-parts';
import {
  AccentColorCard,
  DensityCard,
  FontScaleCard,
  ThemeLanguageCard,
  type Translator,
} from './appearance-cards';
import { useAppearance } from './use-appearance';

interface AppearancePageProps {
  // Accepted for shell-route-renderer parity; this page derives its own
  // translator from tw.lang so language switch updates UI instantly.
  t?: Translator;
}

/** Setting → Tampilan — ported from prototype `pages/appearance.jsx`. */
export function AppearancePage(_props: AppearancePageProps) {
  const { tw, t, theme, setTheme, applyTweak, resetAll, fontScale } = useAppearance();

  return (
    <div className="page">
      <div className="page-header">
        <h1 className="page-title">
          {t('Tampilan')}<span className="code-tag">UI</span>
        </h1>
        <div className="page-actions">
          <button className="btn ghost" onClick={resetAll}>
            <Icon name="refresh" size={12} /> {t('Reset')}
          </button>
        </div>
      </div>

      <div
        className="dash-grid scrollbar"
        style={{ overflow: 'auto', flex: 1, alignContent: 'start' }}
      >
        <ThemeLanguageCard
          theme={theme}
          lang={tw.lang}
          setTheme={setTheme}
          applyTweak={applyTweak}
          t={t}
        />

        <AccentColorCard
          primary={tw.primary}
          applyTweak={applyTweak}
          t={t}
        />

        <FontScaleCard
          fontScale={fontScale}
          applyTweak={applyTweak}
          t={t}
        />

        <DensityCard
          density={tw.density}
          applyTweak={applyTweak}
          t={t}
        />

        <SidebarModeCard
          sidebar={tw.sidebar}
          sidebarMenu={tw.sidebarMenu}
          onChange={(v) => applyTweak('sidebar', v)}
          onMenuMode={(v) => applyTweak('sidebarMenu', v)}
          t={t}
        />

        <UrlRoutingCard
          urlRouting={tw.urlRouting ?? false}
          onChange={(v) => {
            confirmAction({
              title: v
                ? t('Aktifkan Mode Per-halaman URL?')
                : t('Kembali ke Mode Internal?'),
              message: v
                ? t('Tab navigator akan dihapus — Anda hanya dapat membuka satu halaman dalam satu waktu. Semua tab lain akan ditutup; halaman ini tetap aktif.')
                : t('Tab navigator akan ditampilkan kembali sehingga Anda bisa membuka banyak halaman. Semua tab lain akan ditutup; halaman ini tetap aktif.'),
              variant: 'warn',
              icon: 'layers',
              confirmLabel: v ? t('Aktifkan Per-halaman URL') : t('Kembali ke Internal'),
              confirmIcon: 'layers',
              cancelLabel: t('Batal'),
              onConfirm: () => applyTweak('urlRouting', v),
            });
          }}
          t={t}
        />

        <LivePreviewCard t={t} />
      </div>

      <div className="pager">
        <span className="muted">{t('Setting · Tampilan')}</span>
        <div className="spacer" />
        <span className="muted">
          {t('Tema')} {theme} ·{' '}
          {t(SWATCHES.find((s) => s.v === tw.primary)?.label || tw.primary)} ·{' '}
          {t('Ukuran')} {fontScale} · {t(tw.density === 'compact' ? 'Compact' : 'Comfortable')} ·{' '}
          {t('Menu Sidebar')} {t((tw.sidebar || 'icon') === 'icon' ? 'Ikon' : (tw.sidebar || 'icon') === 'horizontal' ? 'Horizontal' : 'Ikon + Label')} · {t((tw.sidebarMenu || 'flyout') === 'flyout' ? 'Flyout' : 'Accordion')} ·{' '}
          {t('URL')} {tw.urlRouting ? t('Per-halaman URL') : t('Internal')}
        </span>
      </div>
    </div>
  );
}
