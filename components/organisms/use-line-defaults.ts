'use client';

/**
 * One-time seeding of column default values (Kustomisasi Grid) onto the initial
 * pristine line(s) of a config-driven grid. The append path (`useGridNav`) already
 * defaults rows the user adds; this covers the blank row a form starts with.
 *
 * Fires once `ready` flips true (i.e. real grid config has loaded — never the
 * built-in fallback columns, which carry no defaults), applies defaults to every
 * still-pristine row, then never again — so clearing a defaulted cell won't
 * re-fill it. Skipped in read-only (edit/view) mode.
 */

import * as React from 'react';
import {
  applyColumnDefaults, colsHaveDefaults, isRowPristine,
  type GridCol, type GridModel, type GridRowBase,
} from './grid-line-core';

export function useSeedLineDefaults<Row extends GridRowBase>({
  ready,
  lines,
  cols,
  model,
  readOnly,
  onChange,
}: {
  /** True once the authoritative grid config is loaded (not the fallback columns). */
  ready: boolean;
  lines: Row[];
  cols: GridCol[];
  model: GridModel<Row>;
  readOnly: boolean;
  onChange: (lines: Row[]) => void;
}) {
  const seeded = React.useRef(false);
  // Latest values via refs so the effect fires on `ready`, not on every keystroke.
  const linesRef = React.useRef(lines); linesRef.current = lines;
  const colsRef = React.useRef(cols); colsRef.current = cols;

  React.useEffect(() => {
    if (!ready || readOnly || seeded.current) return;
    seeded.current = true;
    const c = colsRef.current;
    if (!colsHaveDefaults(c)) return;
    const cur = linesRef.current;
    const next = cur.map((l) => (isRowPristine(model, l, c) ? applyColumnDefaults(model, l, c) : l));
    if (next.some((l, i) => l !== cur[i])) onChange(next);
  }, [ready, readOnly, model, onChange]);
}
