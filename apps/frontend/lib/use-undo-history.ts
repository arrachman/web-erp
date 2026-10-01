import { useCallback, useState } from 'react';

interface HistoryState<T> {
  past: T[];
  present: T;
  future: T[];
}

/**
 * Generic undo/redo stack. push() records a new state; undo/redo navigate.
 * reset() replaces the present without adding to history (use after API load/save).
 */
export function useUndoHistory<T>(initial: T) {
  const [hist, setHist] = useState<HistoryState<T>>({
    past: [],
    present: initial,
    future: [],
  });

  const push = useCallback((next: T) => {
    setHist((s) => ({
      past: [...s.past, s.present],
      present: next,
      future: [],
    }));
  }, []);

  const undo = useCallback(() => {
    setHist((s) => {
      if (s.past.length === 0) return s;
      const previous = s.past[s.past.length - 1];
      return {
        past: s.past.slice(0, -1),
        present: previous,
        future: [s.present, ...s.future],
      };
    });
  }, []);

  const redo = useCallback(() => {
    setHist((s) => {
      if (s.future.length === 0) return s;
      const [next, ...rest] = s.future;
      return {
        past: [...s.past, s.present],
        present: next,
        future: rest,
      };
    });
  }, []);

  const reset = useCallback((value: T) => {
    setHist({ past: [], present: value, future: [] });
  }, []);

  return {
    value: hist.present,
    push,
    undo,
    redo,
    canUndo: hist.past.length > 0,
    canRedo: hist.future.length > 0,
    reset,
  };
}
