'use client';

// URL-routing concern for AppShell: owns the Per-page URL toggle, keeps the
// browser URL in sync with the active route, and exposes navigate() which
// replaces the current page in place when the tab strip is hidden.

import * as React from 'react';
import { GLOBAL_BASE_PATH } from '@/lib/shell-constants';
import type { ShellTab } from '@/components/organisms/tab-bar';

const APPEARANCE_STORAGE_KEY = 'erp-appearance';

/** Reads the persisted Per-page URL routing preference from localStorage. */
export function readUrlRoutingEnabled(): boolean {
  try {
    const raw =
      typeof window !== 'undefined' && window.localStorage.getItem(APPEARANCE_STORAGE_KEY);
    if (!raw) return false;
    return !!(JSON.parse(raw) as { urlRouting?: boolean }).urlRouting;
  } catch {
    return false;
  }
}

interface UseUrlRoutingArgs {
  workspaceId?: string;
  activeRoute: string;
  activeId: string;
  setTabs: React.Dispatch<React.SetStateAction<ShellTab[]>>;
  setActiveId: React.Dispatch<React.SetStateAction<string>>;
  navigateInTab: (route: string) => void;
  openTab: (route: string) => void;
}

export interface UseUrlRoutingApi {
  /** True when Per-page URL mode is active (tab strip hidden, single page). */
  urlRoutingEnabled: boolean;
  /**
   * Navigates to a route: replaces the current page in Per-page URL mode,
   * opens/focuses a tab in Internal mode.
   */
  navigate: (route: string) => void;
}

/**
 * Drives the AppShell URL-routing mode. Switching to Per-page URL collapses
 * the workspace to a single fresh tab; the shell then hides the tab strip and
 * routes every navigation through navigate() (replace-in-place).
 */
export function useUrlRouting({
  workspaceId,
  activeRoute,
  activeId,
  setTabs,
  setActiveId,
  navigateInTab,
  openTab,
}: UseUrlRoutingArgs): UseUrlRoutingApi {
  const [urlRoutingEnabled, setUrlRoutingEnabled] = React.useState(false);

  // Refs so event handlers always read live values without re-registering.
  const activeRouteRef = React.useRef(activeRoute);
  const activeIdRef = React.useRef(activeId);
  React.useEffect(() => {
    activeRouteRef.current = activeRoute;
    activeIdRef.current = activeId;
  }, [activeRoute, activeId]);

  React.useEffect(() => {
    setUrlRoutingEnabled(readUrlRoutingEnabled());
    const onStorage = (e: StorageEvent) => {
      if (e.key === APPEARANCE_STORAGE_KEY) setUrlRoutingEnabled(readUrlRoutingEnabled());
    };
    const onCustom = (e: Event) => {
      setUrlRoutingEnabled(!!(e as CustomEvent<{ enabled: boolean }>).detail?.enabled);
      // Reuse the current tab ID so the page component is NOT remounted — its
      // React state (including the updated urlRouting tweak) is preserved.
      setTabs([{ id: activeIdRef.current, route: activeRouteRef.current || 'home' }]);
      setActiveId(activeIdRef.current);
    };
    // Server-side hydration: only update the flag, never reset workspace tabs.
    const onHydrate = (e: Event) => {
      setUrlRoutingEnabled(!!(e as CustomEvent<{ enabled: boolean }>).detail?.enabled);
    };
    window.addEventListener('storage', onStorage);
    window.addEventListener('erp-set-url-routing', onCustom as EventListener);
    window.addEventListener('erp-hydrate-url-routing', onHydrate as EventListener);
    return () => {
      window.removeEventListener('storage', onStorage);
      window.removeEventListener('erp-set-url-routing', onCustom as EventListener);
      window.removeEventListener('erp-hydrate-url-routing', onHydrate as EventListener);
    };
  }, [setTabs, setActiveId]);

  React.useEffect(() => {
    // Global mode: routes map to /app/<route> (e.g. /app/master/provinces).
    // Workspace mode: workspaceRoot = /ws1 so routes map to /ws1/<route>.
    const workspaceRoot = workspaceId ? `/${workspaceId}` : '';
    const homeUrl = workspaceId ? workspaceRoot : GLOBAL_BASE_PATH;
    if (urlRoutingEnabled) {
      if (!activeRoute || activeRoute === 'home') return;
      // Global: prepend /app prefix so URL = /app/master/provinces.
      // Workspace: prepend workspace prefix.
      const target = workspaceId
        ? `${workspaceRoot}${activeRoute}`
        : `${GLOBAL_BASE_PATH}${activeRoute}`;
      if (window.location.pathname !== target) {
        window.history.replaceState(null, '', target);
      }
    } else {
      // Internal mode: URL shows only the root (workspace or /org).
      if (window.location.pathname !== homeUrl) {
        window.history.replaceState(null, '', homeUrl);
      }
    }
  }, [urlRoutingEnabled, activeRoute, workspaceId]);

  // In Per-page URL mode the tab strip is hidden, so navigation must replace
  // the current page in place instead of spawning hidden tabs.
  const navigate = React.useCallback(
    (route: string) => {
      if (urlRoutingEnabled) navigateInTab(route);
      else openTab(route);
    },
    [urlRoutingEnabled, navigateInTab, openTab],
  );

  return { urlRoutingEnabled, navigate };
}
