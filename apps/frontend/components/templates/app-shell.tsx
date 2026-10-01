'use client';

import * as React from 'react';
import { makeTranslator, setCurrentLang } from '@/lib/mock';
import { Sidebar } from '@/components/organisms/sidebar';
import { Topbar, type ShellUser } from '@/components/organisms/topbar';
import { NAV, type NavItem } from '@/lib/nav';
import { TabBar } from '@/components/organisms/tab-bar';
import { CommandPalette } from '@/components/organisms/command-palette';
import { ConfirmDialogHost } from '@/components/organisms/confirm-dialog';
import { NotificationDrawer } from '@/components/organisms/notification-drawer';
import { ActivityDrawer } from '@/components/organisms/activity-drawer';
import { LoginPage } from '@/components/pages/login';
import { TabActiveContext } from '@/lib/tab-context';
import { pageMeta, type Crumb } from '@/lib/nav';
import { confirmAction } from '@/lib/feedback';
import { USER_STORAGE_KEY, type Lang } from '@/lib/shell-constants';
import { renderRoute } from '@/components/templates/shell-route-renderer';
import { ShortcutsOverlay } from '@/components/templates/shell-shortcuts-overlay';
import { logout as apiLogout } from '@/lib/api/auth';
import { getMyPreferences } from '@/lib/api';
import { useQueryClient } from '@tanstack/react-query';
import { useErpMe, useErpMyMenus, erpQueryKeys } from '@/lib/api/hooks';
import { useAppShellTabs } from '@/lib/use-app-shell-tabs';
import {
  getOrCreateWorkspace,
  saveWorkspace,
  clearAllWorkspaces,
  setLastActive,
} from '@/lib/workspace';
import { useUrlRouting, readUrlRoutingEnabled } from '@/lib/use-url-routing';
import { applyServerPrefs } from '@/lib/apply-server-prefs';
import { useAppShellKeyboard } from '@/components/templates/use-app-shell-keyboard';

interface AppShellProps {
  /** Omit for global (no-workspace) mode — tabs are in-memory only, no localStorage. */
  workspaceId?: string;
  initialRoute?: string;
}

export function AppShell({ workspaceId, initialRoute }: AppShellProps) {
  const workspaceReady = React.useRef(false);
  const [user, setUser] = React.useState<ShellUser | null>(null);
  const [hydrated, setHydrated] = React.useState(false);
  const [nav, setNav] = React.useState<NavItem[]>(NAV);
  const [workspaceName, setWorkspaceName] = React.useState('');
  const [lang, setLang] = React.useState<Lang>('id');
  const [paletteOpen, setPaletteOpen] = React.useState(false);
  const [shortcutsOpen, setShortcutsOpen] = React.useState(false);
  const [sidebarMode, setSidebarMode] = React.useState<'icon' | 'label' | 'horizontal'>(() => {
    if (typeof window === 'undefined') return 'icon';
    try {
      const stored = JSON.parse(window.localStorage.getItem('erp-appearance') ?? '{}') as Record<string, unknown>;
      if (stored.sidebar === 'horizontal' || stored.sidebar === 'label' || stored.sidebar === 'icon') return stored.sidebar;
    } catch { /* ignore */ }
    return (document.documentElement.getAttribute('data-sidebar') as 'icon' | 'label' | 'horizontal') ?? 'icon';
  });
  const [sidebarMenuMode, setSidebarMenuMode] = React.useState<'flyout' | 'accordion'>(() => {
    if (typeof window === 'undefined') return 'flyout';
    try {
      const stored = JSON.parse(window.localStorage.getItem('erp-appearance') ?? '{}') as Record<string, unknown>;
      if (stored.sidebarMenu === 'accordion') return 'accordion';
    } catch { /* ignore */ }
    return (document.documentElement.getAttribute('data-sidebar-menu') as 'flyout' | 'accordion') ?? 'flyout';
  });

  const {
    tabs,
    activeId,
    setTabs,
    setActiveId,
    nextTabId,
    syncTabSeq,
    openTab,
    duplicateTab,
    navigateInTab,
    closeTab,
    reloadTab,
    closeOtherTabs,
    closeTabsToRight,
    reorderTabs,
  } = useAppShellTabs();

  const activeTab = tabs.find((tb) => tb.id === activeId) ?? tabs[0];
  const activeRoute = activeTab ? activeTab.route : 'home';

  const { urlRoutingEnabled, navigate } = useUrlRouting({
    workspaceId,
    activeRoute,
    activeId,
    setTabs,
    setActiveId,
    navigateInTab,
    openTab,
  });

  const [storedUserRaw] = React.useState<string | null>(() => {
    if (typeof window === 'undefined') return null;
    try { return window.localStorage.getItem(USER_STORAGE_KEY); } catch { return null; }
  });

  // Start prefs fetch eagerly in parallel with the /me auth check so that by
  // the time auth succeeds, the prefs promise may already be resolved.
  const eagerPrefsRef = React.useRef<ReturnType<typeof getMyPreferences>>(
    Promise.resolve(null),
  );
  React.useEffect(() => {
    if (!storedUserRaw) return;
    eagerPrefsRef.current = getMyPreferences().catch(() => null);
  // storedUserRaw is stable (initialized once from localStorage)
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const queryClient = useQueryClient();
  const meQuery = useErpMe({ enabled: !!storedUserRaw, retry: false });
  const myMenusQuery = useErpMyMenus({ enabled: !!user });

  const loadWorkspace = React.useCallback(() => {
    if (!workspaceId) {
      // Global mode: in-memory tabs only — no localStorage reads or writes.
      // Use initialRoute directly so the first visible render already shows
      // the correct page (avoids a Dashboard flash before the initialRoute
      // effect fires one render later).
      const defaultTab = { id: 't1', route: initialRoute ?? 'home' };
      syncTabSeq([defaultTab]);
      setTabs([defaultTab]);
      setActiveId('t1');
      workspaceReady.current = true;
      return;
    }
    const ws = getOrCreateWorkspace(workspaceId);
    setWorkspaceName(ws.meta.name);
    syncTabSeq(ws.tabs);
    setTabs(ws.tabs.map((t) => ({ id: t.id, route: t.route })));
    setActiveId(
      ws.tabs.find((t) => t.id === ws.activeTabId)?.id
        ?? ws.tabs[0]?.id
        ?? nextTabId(),
    );
    workspaceReady.current = true;
    setLastActive(workspaceId);
  }, [workspaceId, initialRoute, nextTabId, syncTabSeq, setTabs, setActiveId]);

  // Ref guard prevents re-running when loadWorkspace ref or meQuery refetches.
  const authInitialized = React.useRef(false);
  React.useEffect(() => {
    if (authInitialized.current) return;
    if (!storedUserRaw) {
      authInitialized.current = true;
      const id = setTimeout(() => setHydrated(true), 0);
      return () => clearTimeout(id);
    }
    if (!meQuery.isSuccess && !meQuery.isError) return; // still pending
    authInitialized.current = true;
    if (meQuery.isSuccess) {
      try {
        setUser(JSON.parse(storedUserRaw) as ShellUser);
        loadWorkspace();
        // Apply server prefs to DOM + localStorage BEFORE revealing the shell
        // so the blocking-script values are never visibly wrong on first render.
        // eagerPrefsRef may already be resolved (parallel fetch started above).
        eagerPrefsRef.current
          .then((prefs) => { if (prefs) applyServerPrefs(prefs, setLang); })
          .catch(() => {})
          .finally(() => setHydrated(true));
        return; // setHydrated called in the prefs chain above
      } catch {
        try { window.localStorage.removeItem(USER_STORAGE_KEY); } catch { /* noop */ }
      }
    } else {
      try { window.localStorage.removeItem(USER_STORAGE_KEY); } catch { /* noop */ }
    }
    setHydrated(true);
  }, [storedUserRaw, meQuery.isSuccess, meQuery.isError, loadWorkspace, setLang]);

  React.useEffect(() => {
    if (myMenusQuery.data && myMenusQuery.data.length > 0) {
      setNav(myMenusQuery.data);
    }
  }, [myMenusQuery.data]);

  const onLogin = React.useCallback((u: ShellUser) => {
    setUser(u);
    loadWorkspace();
    // Force fresh menus for the new session (clears any stale cached result).
    queryClient.invalidateQueries({ queryKey: erpQueryKeys.myMenus });
    try { window.localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(u)); } catch { /* noop */ }
    // Apply prefs immediately after login so appearance is correct from the start.
    getMyPreferences()
      .then((prefs) => { if (prefs) applyServerPrefs(prefs, setLang); })
      .catch(() => {});
  }, [loadWorkspace, queryClient, setLang]);

  const onLogout = React.useCallback(() => {
    workspaceReady.current = false;
    setUser(null);
    setNav(NAV);
    try {
      window.localStorage.removeItem(USER_STORAGE_KEY);
      clearAllWorkspaces();
    } catch { /* noop */ }
    // Clear cached session data so next login fetches fresh.
    queryClient.removeQueries({ queryKey: erpQueryKeys.me });
    queryClient.removeQueries({ queryKey: erpQueryKeys.myMenus });
    apiLogout().catch(() => { /* ignore */ });
  }, [queryClient]);

  // Persist workspace state whenever tabs or active tab change (skip in global mode).
  React.useEffect(() => {
    if (!workspaceReady.current || !user || !workspaceId) return;
    const ws = getOrCreateWorkspace(workspaceId);
    saveWorkspace({
      ...ws,
      tabs: tabs.map((t) => ({ id: t.id, route: t.route })),
      activeTabId: activeId,
    });
  }, [tabs, activeId, workspaceId, user]);

  const t = React.useMemo(() => makeTranslator(lang), [lang]);

  // Mirror the current language into the module-level registry so non-React
  // helpers (confirm dialog, toast bulk-action) can translate without props.
  React.useEffect(() => {
    setCurrentLang(lang);
  }, [lang]);

  const initialRouteHandled = React.useRef(false);
  React.useEffect(() => {
    if (initialRouteHandled.current || !initialRoute || !user) return;
    initialRouteHandled.current = true;
    if (readUrlRoutingEnabled()) navigateInTab(initialRoute);
    else openTab(initialRoute);
  }, [initialRoute, user, openTab, navigateInTab]);

  const confirmClose = React.useCallback(
    (id: string) => {
      // Snapshot the label via functional setState so we don't take a
      // dependency on the changing `tabs` array.
      let label = t('tab ini');
      setTabs((prev) => {
        const tab = prev.find((tb) => tb.id === id);
        if (tab) label = pageMeta(tab.route, t).title;
        return prev;
      });
      confirmAction({
        title: t('Tutup tab?'),
        message: t('"{label}" akan ditutup.').replace('{label}', label),
        variant: 'warn',
        confirmLabel: t('Tutup'),
        confirmIcon: 'x',
        cancelLabel: t('Batal'),
        onConfirm: () => closeTab(id),
      });
    },
    [t, closeTab, setTabs],
  );

  useAppShellKeyboard({
    activeId,
    tabs,
    urlRoutingEnabled,
    confirmClose,
    setActiveId,
    setPaletteOpen,
    setShortcutsOpen,
    setLang,
    setSidebarMenuMode,
    setSidebarMode,
  });

  const onPaletteAction = React.useCallback(
    (id: string) => {
      if (id.startsWith('toggle:')) {
        const what = id.split(':')[1];
        if (what === 'lang') setLang((l) => (l === 'id' ? 'en' : l === 'en' ? 'ja' : 'id'));
        return;
      }
      if (id.startsWith('new:')) { navigate(id.split(':')[1]); return; }
      navigate(id);
    },
    [navigate],
  );

  const meta = pageMeta(activeRoute, t);
  const crumbs: Crumb[] = meta.crumbs;
  const sidebarCurrent = activeRoute;

  React.useEffect(() => {
    document.title = `${meta.title} | Sentient ERP`;
  }, [meta.title]);

  if (!hydrated) return null;
  if (!user) return <LoginPage onLogin={onLogin} />;

  return (
    <>
      <div className="app">
        <Sidebar nav={nav} current={sidebarCurrent} onNavigate={navigate} t={t} workspaceId={workspaceId} sidebarMenuMode={sidebarMenuMode} sidebarMode={sidebarMode} />
        <Topbar
          crumbs={crumbs}
          onOpenPalette={() => setPaletteOpen(true)}
          t={t}
          user={user}
          onNavigate={navigate}
          onLogout={onLogout}
          workspaceId={workspaceId}
          workspaceName={workspaceName}
        />
        <main className="main">
          {!urlRoutingEnabled && (
            <TabBar
              tabs={tabs}
              activeId={activeId}
              onActivate={setActiveId}
              onClose={confirmClose}
              onReload={reloadTab}
              onCloseOthers={closeOtherTabs}
              onCloseRight={closeTabsToRight}
              onDuplicate={duplicateTab}
              onNew={() => setPaletteOpen(true)}
              onReorder={reorderTabs}
              t={t}
            />
          )}
          <div className="tabviews">
            {tabs.map((tab) => {
              const isActive = tab.id === activeId;
              const viewKey = `${tab.id}:${tab.nonce ?? 0}`;
              return (
                <div
                  key={viewKey}
                  className="tabview"
                  style={{ display: isActive ? 'flex' : 'none' }}
                >
                  <TabActiveContext.Provider value={isActive}>
                    {renderRoute(tab.route, navigateInTab, openTab, t, lang)}
                  </TabActiveContext.Provider>
                </div>
              );
            })}
          </div>
        </main>
      </div>

      <CommandPalette
        open={paletteOpen}
        onClose={() => setPaletteOpen(false)}
        onAction={onPaletteAction}
        t={t}
        nav={nav}
        workspaceId={workspaceId}
      />

      <NotificationDrawer onNavigate={navigate} t={t} />
      <ActivityDrawer t={t} />
      <ConfirmDialogHost />

      {shortcutsOpen && (
        <ShortcutsOverlay onClose={() => setShortcutsOpen(false)} urlRoutingEnabled={urlRoutingEnabled} />
      )}
    </>
  );
}
