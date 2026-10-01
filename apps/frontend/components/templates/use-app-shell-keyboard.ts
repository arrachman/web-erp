'use client';

import * as React from 'react';
import type { Lang } from '@/lib/shell-constants';

interface UseAppShellKeyboardOptions {
  activeId: string;
  tabs: { id: string }[];
  urlRoutingEnabled: boolean;
  confirmClose: (id: string) => void;
  setActiveId: (id: string) => void;
  setPaletteOpen: (open: boolean) => void;
  setShortcutsOpen: (open: boolean) => void;
  setLang: React.Dispatch<React.SetStateAction<Lang>>;
  setSidebarMenuMode: React.Dispatch<React.SetStateAction<'flyout' | 'accordion'>>;
  setSidebarMode: React.Dispatch<React.SetStateAction<'icon' | 'label' | 'horizontal'>>;
}

/** Registers all window-level keyboard and custom-event listeners for AppShell. */
export function useAppShellKeyboard({
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
}: UseAppShellKeyboardOptions): void {
  // Global keyboard shortcuts
  React.useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      const tag = (e.target as HTMLElement)?.tagName;
      const inEditor = ['INPUT', 'TEXTAREA', 'SELECT'].includes(tag);
      if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === 'k') {
        e.preventDefault();
        setPaletteOpen(true);
        return;
      }
      if (e.metaKey && e.code === 'KeyE') {
        e.preventDefault();
        confirmClose(activeId);
        return;
      }
      if (!urlRoutingEnabled && (e.metaKey || e.ctrlKey) && /^[1-9]$/.test(e.key)) {
        e.preventDefault();
        const n = parseInt(e.key, 10);
        const target = n === 9 ? tabs[tabs.length - 1] : tabs[n - 1];
        if (target) setActiveId(target.id);
        return;
      }
      if (inEditor) return;
      if (e.key === '?' || (e.shiftKey && e.key === '/')) {
        e.preventDefault();
        setShortcutsOpen(true);
        return;
      }
      if (e.key.toLowerCase() === 'l') {
        setLang((l) => (l === 'id' ? 'en' : l === 'en' ? 'ja' : 'id'));
      }
    };
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, [activeId, tabs, confirmClose, setActiveId, urlRoutingEnabled, setPaletteOpen, setShortcutsOpen, setLang]);

  // Open shortcuts overlay via custom event (e.g. dispatched by AppearancePage)
  React.useEffect(() => {
    const sc = () => setShortcutsOpen(true);
    window.addEventListener('open-shortcuts', sc);
    return () => window.removeEventListener('open-shortcuts', sc);
  }, [setShortcutsOpen]);

  // Listen for sidebar menu mode changes dispatched by AppearancePage
  React.useEffect(() => {
    const onSetMode = (e: Event) => {
      const detail = (e as CustomEvent<{ mode: string }>).detail;
      if (detail?.mode === 'accordion' || detail?.mode === 'flyout') {
        setSidebarMenuMode(detail.mode as 'flyout' | 'accordion');
      }
    };
    window.addEventListener('erp-set-sidebar-menu', onSetMode as EventListener);
    return () => window.removeEventListener('erp-set-sidebar-menu', onSetMode as EventListener);
  }, [setSidebarMenuMode]);

  // Listen for sidebar mode changes dispatched by AppearancePage
  React.useEffect(() => {
    const onSetMode = (e: Event) => {
      const detail = (e as CustomEvent<{ mode: string }>).detail;
      if (['icon', 'label', 'horizontal'].includes(detail?.mode)) {
        setSidebarMode(detail.mode as 'icon' | 'label' | 'horizontal');
      }
    };
    window.addEventListener('erp-set-sidebar', onSetMode as EventListener);
    return () => window.removeEventListener('erp-set-sidebar', onSetMode as EventListener);
  }, [setSidebarMode]);

  // Listen for lang changes dispatched by AppearancePage
  React.useEffect(() => {
    const onSetLang = (e: Event) => {
      const detail = (e as CustomEvent<{ lang: Lang }>).detail;
      if (!detail) return;
      const next = detail.lang;
      if (next === 'id' || next === 'en' || next === 'ja') setLang(next);
    };
    window.addEventListener('erp-set-lang', onSetLang as EventListener);
    return () => window.removeEventListener('erp-set-lang', onSetLang as EventListener);
  }, [setLang]);
}
