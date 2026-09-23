'use client';

import * as React from 'react';
import { useTheme } from 'next-themes';
import { makeTranslator } from '@/lib/mock';
import { notify } from '@/lib/feedback';
import {
  getMyPreferences,
  updateMyPreferences,
  ErpApiError,
} from '@/lib/api';
import {
  DEFAULTS,
  STORAGE_KEY,
  type Density,
  type FontScale,
  type Lang,
  type SidebarMode,
  type SidebarMenuMode,
  type Tweaks,
} from './appearance-parts';

export interface UseAppearanceResult {
  tw: Tweaks;
  t: ReturnType<typeof makeTranslator>;
  theme: string | undefined;
  setTheme: (theme: string) => void;
  applyTweak: <K extends keyof Tweaks>(key: K, val: Tweaks[K]) => void;
  resetAll: () => void;
  fontScale: FontScale;
}

export function useAppearance(): UseAppearanceResult {
  const { theme, setTheme } = useTheme();
  const [tw, setTw] = React.useState<Tweaks>(DEFAULTS);
  const t = React.useMemo(() => makeTranslator(tw.lang), [tw.lang]);
  const hydratedRef = React.useRef(false);
  const saveTimerRef = React.useRef<ReturnType<typeof setTimeout> | null>(null);

  const applyToDom = React.useCallback((next: Tweaks) => {
    const el = document.documentElement;
    el.setAttribute('data-primary', next.primary);
    el.setAttribute('data-density', next.density);
    el.setAttribute('data-fontscale', next.fontScale);
    el.setAttribute('data-sidebar', next.sidebar);
    el.setAttribute('data-sidebar-menu', next.sidebarMenu || 'flyout');
  }, []);

  // Sync local state from the DOM / localStorage / API after mount.
  // Order: API (server SSOT) > localStorage > DOM attr > DEFAULTS.
  React.useEffect(() => {
    const el = document.documentElement;
    let stored: Partial<Tweaks> = {};
    try {
      const raw = window.localStorage.getItem(STORAGE_KEY);
      if (raw) stored = JSON.parse(raw) as Partial<Tweaks>;
    } catch {
      stored = {};
    }
    const baseline: Tweaks = {
      primary:
        stored.primary ?? el.getAttribute('data-primary') ?? DEFAULTS.primary,
      density:
        (stored.density as Density) ??
        (el.getAttribute('data-density') as Density) ??
        DEFAULTS.density,
      fontScale:
        (stored.fontScale as FontScale) ??
        (el.getAttribute('data-fontscale') as FontScale) ??
        DEFAULTS.fontScale,
      sidebar:
        (stored.sidebar as SidebarMode) ??
        (el.getAttribute('data-sidebar') as SidebarMode) ??
        DEFAULTS.sidebar,
      sidebarMenu:
        (stored.sidebarMenu as SidebarMenuMode) ??
        (el.getAttribute('data-sidebar-menu') as SidebarMenuMode) ??
        DEFAULTS.sidebarMenu,
      lang: (stored.lang as Lang) ?? DEFAULTS.lang,
      urlRouting: stored.urlRouting ?? DEFAULTS.urlRouting,
    };
    setTw(baseline);

    let cancelled = false;
    getMyPreferences()
      .then((prefs) => {
        if (cancelled || !prefs) {
          hydratedRef.current = true;
          return;
        }
        const meta = (prefs.metadata ?? {}) as Partial<Tweaks>;
        const merged: Tweaks = {
          primary: meta.primary ?? baseline.primary,
          density: (meta.density as Density) ?? baseline.density,
          fontScale: (meta.fontScale as FontScale) ?? baseline.fontScale,
          sidebar: (meta.sidebar as SidebarMode) ?? baseline.sidebar,
          sidebarMenu: (meta.sidebarMenu as SidebarMenuMode) ?? baseline.sidebarMenu,
          lang: (prefs.language as Lang) ?? baseline.lang,
          urlRouting: meta.urlRouting ?? baseline.urlRouting,
        };
        setTw(merged);
        applyToDom(merged);
        if (prefs.theme) setTheme(prefs.theme);
        // Sync server SSOT → localStorage so readUrlRoutingEnabled() is correct
        // cross-device / after localStorage cleared.
        try {
          window.localStorage.setItem(STORAGE_KEY, JSON.stringify(merged));
        } catch { /* localStorage unavailable */ }
        hydratedRef.current = true;
      })
      .catch(() => {
        hydratedRef.current = true;
      });
    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const twRef = React.useRef(tw);
  React.useEffect(() => {
    twRef.current = tw;
  }, [tw]);

  const applyTweak = React.useCallback(
    <K extends keyof Tweaks>(key: K, val: Tweaks[K]) => {
      const next = { ...twRef.current, [key]: val };
      twRef.current = next;
      const el = document.documentElement;
      el.setAttribute('data-primary', next.primary);
      el.setAttribute('data-density', next.density);
      el.setAttribute('data-fontscale', next.fontScale);
      el.setAttribute('data-sidebar', next.sidebar);
      el.setAttribute('data-sidebar-menu', next.sidebarMenu || 'flyout');
      try {
        window.localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
      } catch {
        /* localStorage unavailable — ignore */
      }
      if (key === 'lang')
        window.dispatchEvent(new CustomEvent('erp-set-lang', { detail: { lang: next.lang } }));
      if (key === 'urlRouting')
        window.dispatchEvent(new CustomEvent('erp-set-url-routing', { detail: { enabled: next.urlRouting } }));
      if (key === 'sidebarMenu')
        window.dispatchEvent(new CustomEvent('erp-set-sidebar-menu', { detail: { mode: next.sidebarMenu } }));
      if (key === 'sidebar')
        window.dispatchEvent(new CustomEvent('erp-set-sidebar', { detail: { mode: next.sidebar } }));
      setTw(next);
    },
    [],
  );

  // Debounce-save to API whenever theme or tw changes (post-hydration).
  React.useEffect(() => {
    if (!hydratedRef.current) return;
    if (saveTimerRef.current) clearTimeout(saveTimerRef.current);
    saveTimerRef.current = setTimeout(() => {
      updateMyPreferences({
        theme: theme ?? 'light',
        language: tw.lang,
        metadata: {
          primary: tw.primary,
          density: tw.density,
          fontScale: tw.fontScale,
          sidebar: tw.sidebar,
          sidebarMenu: tw.sidebarMenu,
          urlRouting: tw.urlRouting,
        },
      }).catch((err) => {
        const msg =
          err instanceof ErpApiError
            ? err.message
            : t('Gagal menyimpan preferensi tampilan');
        notify(msg, 'danger');
      });
    }, 500);
    return () => {
      if (saveTimerRef.current) clearTimeout(saveTimerRef.current);
    };
  }, [theme, tw]);

  const resetAll = React.useCallback(() => {
    setTheme('light');
    setTw(DEFAULTS);
    const el = document.documentElement;
    el.setAttribute('data-primary', DEFAULTS.primary);
    el.setAttribute('data-density', DEFAULTS.density);
    el.setAttribute('data-fontscale', DEFAULTS.fontScale);
    el.setAttribute('data-sidebar', DEFAULTS.sidebar);
    el.setAttribute('data-sidebar-menu', DEFAULTS.sidebarMenu);
    window.dispatchEvent(new CustomEvent('erp-set-sidebar-menu', { detail: { mode: DEFAULTS.sidebarMenu } }));
    window.dispatchEvent(new CustomEvent('erp-set-sidebar', { detail: { mode: DEFAULTS.sidebar } }));
    try {
      window.localStorage.setItem(STORAGE_KEY, JSON.stringify(DEFAULTS));
    } catch {
      // ignore
    }
    notify(t('Tampilan dikembalikan ke bawaan'), 'info');
  }, [setTheme, t]);

  const fontScale: FontScale = tw.fontScale || 'base';

  return { tw, t, theme, setTheme, applyTweak, resetAll, fontScale };
}
