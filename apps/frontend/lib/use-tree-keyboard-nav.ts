'use client';

/**
 * Keyboard-first row navigation for the menu tree list (CLAUDE.md §2.7 F).
 *
 *   J / ↓   move focus down        K / ↑   move focus up
 *   Enter   open focused row       N       add new        /   focus search
 *
 * No X/select binding: the tree has no row selection (§2.22 — the drag handle
 * replaces the checkbox column). Focus is visual only; it never selects.
 *
 * The window listener subscribes once; live values (row count, focused index,
 * callbacks) are read through a ref so handlers stay current without
 * re-subscribing on every render.
 */

import * as React from 'react';

interface TreeKeyboardNavOptions {
  /** Number of currently visible rows (post-filter). */
  rowCount: number;
  /** Focus resets to none whenever this value changes (e.g. the search term). */
  resetKey: unknown;
  searchRef: React.RefObject<HTMLInputElement | null>;
  onAdd: () => void;
  /** Open the focused row (e.g. edit). Receives the visible-row index. */
  onOpenFocused: (index: number) => void;
}

export function useTreeKeyboardNav({
  rowCount,
  resetKey,
  searchRef,
  onAdd,
  onOpenFocused,
}: TreeKeyboardNavOptions) {
  const [focusedIndex, setFocusedIndex] = React.useState(-1);

  const live = React.useRef({ rowCount, focusedIndex, onAdd, onOpenFocused });
  React.useEffect(() => {
    live.current = { rowCount, focusedIndex, onAdd, onOpenFocused };
  });

  React.useEffect(() => { setFocusedIndex(-1); }, [resetKey]);

  React.useEffect(() => {
    if (focusedIndex < 0) return;
    document
      .querySelector('[data-focused="true"]')
      ?.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
  }, [focusedIndex]);

  React.useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      const t = e.target as HTMLElement;
      const inField =
        t.tagName === 'INPUT' ||
        t.tagName === 'TEXTAREA' ||
        t.tagName === 'SELECT' ||
        t.tagName === 'BUTTON' ||
        t.isContentEditable ||
        !!t.closest('[role="dialog"]');
      if (inField || e.metaKey || e.ctrlKey || e.altKey) return;

      const { rowCount: count, focusedIndex: idx, onAdd: add, onOpenFocused: open } = live.current;

      if (e.key === '/') {
        e.preventDefault();
        searchRef.current?.focus();
        return;
      }
      if (e.key === 'n' || e.key === 'N') {
        e.preventDefault();
        add();
        return;
      }
      if (count === 0) return;
      if (e.key === 'j' || e.key === 'J' || e.key === 'ArrowDown') {
        e.preventDefault();
        setFocusedIndex((i) => Math.min(Math.max(i, -1) + 1, count - 1));
      } else if (e.key === 'k' || e.key === 'K' || e.key === 'ArrowUp') {
        e.preventDefault();
        setFocusedIndex((i) => Math.max(i <= 0 ? 0 : i - 1, 0));
      } else if (e.key === 'Enter' && idx >= 0) {
        e.preventDefault();
        open(idx);
      }
    };
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, [searchRef]);

  return { focusedIndex, setFocusedIndex };
}
