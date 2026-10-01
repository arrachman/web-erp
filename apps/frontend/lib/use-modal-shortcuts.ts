'use client';

/**
 * Keyboard shortcuts untuk modal form create/edit di SimpleMasterPage
 * (dan modal serupa). Ctrl/Cmd+S = simpan (tutup); Ctrl/Cmd+Enter =
 * simpan & tambah baru (saat create) ATAU simpan (saat edit). Esc default Radix Dialog.
 */

import * as React from 'react';

export interface ModalShortcutOptions {
  open: boolean;
  editing: boolean;
  onSave: () => void;
  onSaveAndNew: () => void;
}

export function useModalShortcuts({ open, editing, onSave, onSaveAndNew }: ModalShortcutOptions) {
  const onSaveRef = React.useRef(onSave);
  const onSaveAndNewRef = React.useRef(onSaveAndNew);
  const editingRef = React.useRef(editing);
  onSaveRef.current = onSave;
  onSaveAndNewRef.current = onSaveAndNew;
  editingRef.current = editing;

  React.useEffect(() => {
    if (!open) return;
    const handler = (e: KeyboardEvent) => {
      const mod = e.metaKey || e.ctrlKey;
      if (!mod) return;
      const tag = (e.target as HTMLElement)?.tagName;
      if (tag === 'TEXTAREA') return;
      if (e.key === 's' || e.key === 'S') { e.preventDefault(); onSaveRef.current(); }
      else if (e.key === 'Enter') { e.preventDefault(); editingRef.current ? onSaveRef.current() : onSaveAndNewRef.current(); }
    };
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, [open]);
}
