// Shared helper: apply server-side user preferences to the DOM and localStorage.
// Called in app-shell.tsx before the shell renders (to eliminate FOUC) and
// after a fresh login. Extracted here so the logic is not duplicated.

import type { Lang } from '@/lib/shell-constants';
import type { ErpUserPreferences } from '@/lib/api/user-preferences';

export function applyServerPrefs(
  prefs: ErpUserPreferences,
  setLang: (l: Lang) => void,
): void {
  const meta = (prefs.metadata ?? {}) as Record<string, string>;
  const el = document.documentElement;

  // Apply appearance tokens to <html> data-attributes (consumed by CSS).
  if (meta.density === 'compact' || meta.density === 'comfortable')
    el.setAttribute('data-density', meta.density);
  if (meta.fontScale) el.setAttribute('data-fontscale', meta.fontScale);
  if (meta.sidebar) el.setAttribute('data-sidebar', meta.sidebar);
  if (meta.sidebarMenu) el.setAttribute('data-sidebar-menu', meta.sidebarMenu);
  if (meta.primary) el.setAttribute('data-primary', meta.primary);

  // Apply language to React state (drives the translator in the shell).
  if (prefs.language) {
    const next = prefs.language as Lang;
    if (next === 'id' || next === 'en' || next === 'ja') setLang(next);
  }

  // Sync ALL appearance prefs to localStorage so the blocking script in
  // layout.tsx reads correct values on next page load, eliminating FOUC.
  try {
    const raw = window.localStorage.getItem('erp-appearance') ?? '{}';
    const stored = JSON.parse(raw) as Record<string, unknown>;
    if (meta.density) stored.density = meta.density;
    if (meta.fontScale) stored.fontScale = meta.fontScale;
    if (meta.sidebar) stored.sidebar = meta.sidebar;
    if (meta.sidebarMenu) stored.sidebarMenu = meta.sidebarMenu;
    if (meta.primary) stored.primary = meta.primary;
    if (prefs.language) stored.lang = prefs.language;
    if ('urlRouting' in meta)
      stored.urlRouting = !!(meta as unknown as { urlRouting?: boolean }).urlRouting;
    window.localStorage.setItem('erp-appearance', JSON.stringify(stored));
  } catch { /* localStorage unavailable */ }

  // Notify useUrlRouting hook of any server-side urlRouting change.
  if ('urlRouting' in meta) {
    window.dispatchEvent(new CustomEvent('erp-hydrate-url-routing', {
      detail: { enabled: !!(meta as unknown as { urlRouting?: boolean }).urlRouting },
    }));
  }
}
