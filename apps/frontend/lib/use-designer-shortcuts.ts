'use client';

import * as React from 'react';
import { cloneComponents, resolveTargetBand } from './report-component-factory';
import type { DesignerAction, DesignerState, RptComponent } from './report-types';

interface Opts {
  active: boolean;
  state: DesignerState;
  dispatch: React.Dispatch<DesignerAction>;
  onSave: () => void;
}

/**
 * Keyboard report designer: undo/redo (Ctrl+Z/Shift+Z/Y), simpan (Ctrl+S),
 * copy/paste/duplicate (Ctrl+C/V/D), hapus (Del/Backspace). Shortcut komponen
 * di-skip saat fokus di input/textarea agar tak ganggu pengetikan.
 */
export function useDesignerShortcuts({ active, state, dispatch, onSave }: Opts) {
  const stateRef = React.useRef(state);
  const onSaveRef = React.useRef(onSave);
  const clipboard = React.useRef<RptComponent[]>([]);

  React.useEffect(() => { stateRef.current = state; });
  React.useEffect(() => { onSaveRef.current = onSave; });

  React.useEffect(() => {
    if (!active) return;

    function selected(): { bandId?: string; comps: RptComponent[] } {
      const s = stateRef.current.selection;
      if (s.type !== 'component' || !s.bandId) return { comps: [] };
      const band = stateRef.current.template.bands.find(b => b.id === s.bandId);
      if (!band) return { comps: [] };
      const ids = new Set(s.componentIds ?? (s.componentId ? [s.componentId] : []));
      return { bandId: s.bandId, comps: band.components.filter(c => ids.has(c.id)) };
    }

    function paste(comps: RptComponent[]) {
      if (!comps.length) return;
      const bands = stateRef.current.template.bands;
      const bandId = selected().bandId ?? resolveTargetBand(bands)?.id;
      if (!bandId) return;
      const clones = cloneComponents(comps);
      dispatch({ type: 'ADD_COMPONENTS', bandId, components: clones });
      dispatch({ type: 'SELECT_COMPONENTS', bandId, componentIds: clones.map(c => c.id) });
    }

    function onKey(e: KeyboardEvent) {
      const tgt = e.target as HTMLElement | null;
      const typing = !!tgt && (['INPUT', 'TEXTAREA', 'SELECT'].includes(tgt.tagName) || tgt.isContentEditable);
      const mod = e.ctrlKey || e.metaKey;
      const k = e.key.toLowerCase();

      if (mod && k === 'z') { e.preventDefault(); dispatch({ type: e.shiftKey ? 'REDO' : 'UNDO' }); return; }
      if (mod && k === 'y') { e.preventDefault(); dispatch({ type: 'REDO' }); return; }
      if (mod && k === 's') { e.preventDefault(); onSaveRef.current(); return; }
      if (typing) return;

      if (mod && k === 'c') { const { comps } = selected(); if (comps.length) clipboard.current = comps.map(c => structuredClone(c)); return; }
      if (mod && k === 'v') { e.preventDefault(); paste(clipboard.current); return; }
      if (mod && k === 'd') { e.preventDefault(); paste(selected().comps); return; }
      if (!mod && (k === 'delete' || k === 'backspace')) { e.preventDefault(); dispatch({ type: 'REMOVE_SELECTED' }); return; }
    }

    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [active, dispatch]);
}
