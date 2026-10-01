'use client';

// Owns the tab strip state + manipulation callbacks for AppShell.
// Extracted so AppShell stays under 400 lines and so the tab callbacks
// can be expressed with functional setters (no `tabs`/`activeId` in deps),
// which keeps React-Compiler memoization preservation happy.

import * as React from 'react';
import type { ShellTab } from '@/components/organisms/tab-bar';
import { MAX_TABS } from '@/lib/shell-constants';

export interface AppShellTabsApi {
  tabs: ShellTab[];
  activeId: string;
  setTabs: React.Dispatch<React.SetStateAction<ShellTab[]>>;
  setActiveId: React.Dispatch<React.SetStateAction<string>>;
  /** Allocates the next non-colliding tab id (monotonic). */
  nextTabId: () => string;
  /**
   * Re-syncs the internal id counter so freshly minted ids never collide
   * with restored ones. Call after hydrating tabs from persistence.
   */
  syncTabSeq: (tabs: { id: string }[]) => void;
  openTab: (route: string) => void;
  duplicateTab: (id: string) => void;
  navigateInTab: (route: string) => void;
  closeTab: (id: string) => void;
  reloadTab: (id: string) => void;
  closeOtherTabs: (id: string) => void;
  closeTabsToRight: (id: string) => void;
  /** Reorders tab `fromId` to the slot currently held by `toId`. No-op if either id is missing or they match. */
  reorderTabs: (fromId: string, toId: string) => void;
}

const INITIAL_TABS: ShellTab[] = [{ id: 't0', route: 'home' }];

/**
 * Tab-strip state machine. All mutators use functional setters so the
 * returned callbacks have empty (stable) closures — required for
 * react-compiler to preserve manual memoization.
 */
export function useAppShellTabs(): AppShellTabsApi {
  const tabSeq = React.useRef(0);
  const [tabs, setTabs] = React.useState<ShellTab[]>(INITIAL_TABS);
  const [activeId, setActiveId] = React.useState<string>('t0');

  const nextTabId = React.useCallback(() => `t${(tabSeq.current += 1)}`, []);

  const syncTabSeq = React.useCallback((restored: { id: string }[]) => {
    const maxSeq = restored.reduce((max, t) => {
      const n = parseInt(t.id.replace('t', ''), 10);
      return Number.isFinite(n) ? Math.max(max, n) : max;
    }, 0);
    tabSeq.current = maxSeq;
  }, []);

  const openTab = React.useCallback(
    (route: string) => {
      let targetId: string | null = null;
      let allocatedId: string | null = null;
      setTabs((prev) => {
        const existing = prev.find((tb) => tb.route === route);
        if (existing) {
          targetId = existing.id;
          return prev;
        }
        if (prev.length >= MAX_TABS) {
          targetId = prev[prev.length - 1].id;
          return prev;
        }
        // Allocate id only once across StrictMode double-invokes.
        if (!allocatedId) allocatedId = nextTabId();
        targetId = allocatedId;
        return [...prev, { id: allocatedId, route }];
      });
      if (targetId) setActiveId(targetId);
    },
    [nextTabId, setTabs, setActiveId],
  );

  const duplicateTab = React.useCallback(
    (id: string) => {
      let targetId: string | null = null;
      let allocatedId: string | null = null;
      setTabs((prev) => {
        const src = prev.find((tb) => tb.id === id) ?? prev[prev.length - 1];
        if (!src || prev.length >= MAX_TABS) return prev;
        if (!allocatedId) allocatedId = nextTabId();
        targetId = allocatedId;
        return [...prev, { id: allocatedId, route: src.route }];
      });
      if (targetId) setActiveId(targetId);
    },
    [nextTabId, setTabs, setActiveId],
  );

  const navigateInTab = React.useCallback((route: string) => {
    setActiveId((curActive) => {
      setTabs((prev) =>
        prev.map((tb) => (tb.id === curActive ? { ...tb, route } : tb)),
      );
      return curActive;
    });
  }, []);

  const closeTab = React.useCallback(
    (id: string) => {
      let freshId: string | null = null;
      let fallbackId: string | null = null;
      setTabs((prev) => {
        const idx = prev.findIndex((tb) => tb.id === id);
        if (idx === -1) return prev;
        const next = prev.filter((tb) => tb.id !== id);
        if (next.length === 0) {
          if (!freshId) freshId = nextTabId();
          return [{ id: freshId, route: 'home' }];
        }
        fallbackId = next[Math.max(0, idx - 1)].id;
        return next;
      });
      if (freshId) {
        setActiveId(freshId);
      } else if (fallbackId) {
        setActiveId((cur) => (cur === id ? (fallbackId as string) : cur));
      }
    },
    [nextTabId, setTabs, setActiveId],
  );

  const reloadTab = React.useCallback((id: string) => {
    setTabs((prev) =>
      prev.map((tb) => (tb.id === id ? { ...tb, nonce: (tb.nonce ?? 0) + 1 } : tb)),
    );
    setActiveId(id);
  }, [setTabs, setActiveId]);

  const closeOtherTabs = React.useCallback((id: string) => {
    setTabs((prev) => {
      const keep = prev.find((tb) => tb.id === id);
      return keep ? [keep] : prev;
    });
    setActiveId(id);
  }, [setTabs, setActiveId]);

  const closeTabsToRight = React.useCallback((id: string) => {
    let survivors: Set<string> | null = null;
    setTabs((prev) => {
      const idx = prev.findIndex((tb) => tb.id === id);
      if (idx === -1) return prev;
      const next = prev.slice(0, idx + 1);
      if (next.length === prev.length) return prev;
      survivors = new Set(next.map((tb) => tb.id));
      return next;
    });
    if (survivors) {
      setActiveId((cur) => ((survivors as Set<string>).has(cur) ? cur : id));
    }
  }, [setTabs, setActiveId]);

  const reorderTabs = React.useCallback((fromId: string, toId: string) => {
    if (fromId === toId) return;
    setTabs((prev) => {
      const from = prev.findIndex((tb) => tb.id === fromId);
      const to = prev.findIndex((tb) => tb.id === toId);
      if (from === -1 || to === -1) return prev;
      const next = prev.slice();
      const [moved] = next.splice(from, 1);
      next.splice(to, 0, moved);
      return next;
    });
  }, [setTabs]);

  return {
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
  };
}
