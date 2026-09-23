'use client';

/**
 * Spreadsheet-style cell navigation + edit state machine for config-driven
 * transaction line grids (cash/bank contra lines, sales item lines, …). Generic
 * over the row type via a `GridModel<Row>` adapter — the engine never knows the
 * concrete row shape (§3 no duplication; one nav implementation for every grid).
 *
 * Default state of every cell = SELECTED (highlighted), NOT an active input.
 * Interaction model (confirmed with user, §Kas Masuk / § Kustomisasi Grid):
 * - Click selects a cell; double-click / Enter / F2 / typing → edit mode.
 * - Typing a printable char enters edit & seeds the value (Excel-style).
 * - Arrow keys move the selected cell (when not editing).
 * - Enter (not editing) = advance to the next non-skip cell (data-entry flow);
 *   F2 / double-click / typing open edit mode instead. Landing only selects (never
 *   auto-opens, incl. on a freshly appended row). Pressing Enter while sitting on
 *   an EMPTY required cell opens it (LOOKUP → search window); a filled one advances.
 * - While editing: Enter = commit & advance, Tab = commit & move,
 *   Esc = cancel (revert to snapshot). Caret arrows stay inside the input.
 * - Tab/arrows can land on Skip cells (view-only); only Enter jumps over them.
 * - Tab at the last cell of the last row, or ArrowDown on the last row,
 *   appends a new row. Ctrl/Cmd+Delete removes the active row (keeps ≥1).
 */

import * as React from 'react';
import {
  GridCol, GridModel, GridRowBase, applyColumnDefaults, isCellFilled, isLookupCol, rowRequiredMissing,
} from './grid-line-core';

export interface CellSel { r: number; c: number }

export interface GridNav<Row extends GridRowBase> {
  rootRef: React.RefObject<HTMLDivElement | null>;
  sel: CellSel | null;
  editing: boolean;
  seed?: string;
  selectOnFocus: boolean;
  /** True when the active edit should auto-open the lookup search window. */
  openModal: boolean;
  onRootKeyDown: (e: React.KeyboardEvent<HTMLDivElement>) => void;
  selectCell: (r: number, c: number) => void;
  editCell: (r: number, c: number) => void;
  endEdit: (focusRoot?: boolean) => void;
  patch: (key: string, p: Partial<Row>) => void;
}

const isPrintable = (e: React.KeyboardEvent) =>
  e.key.length === 1 && !e.ctrlKey && !e.metaKey && !e.altKey;

export function useGridNav<Row extends GridRowBase>({
  lines,
  onChange,
  cols,
  readOnly,
  model,
  onAppendBlocked,
}: {
  lines: Row[];
  onChange: (lines: Row[]) => void;
  cols: GridCol[];
  readOnly: boolean;
  /** Row adapter (newRow / getCellRaw / buildCellPatch). */
  model: GridModel<Row>;
  /** Called (with the missing required headers) when a row append is refused. */
  onAppendBlocked?: (missing: string[]) => void;
}): GridNav<Row> {
  const rootRef = React.useRef<HTMLDivElement>(null);
  const [sel, setSel] = React.useState<CellSel | null>(null);
  const [editing, setEditing] = React.useState(false);
  const [seed, setSeed] = React.useState<string | undefined>(undefined);
  const [selectOnFocus, setSelectOnFocus] = React.useState(false);
  const [openModal, setOpenModal] = React.useState(false);
  const snapshot = React.useRef<Row | null>(null);
  const wantRoot = React.useRef(false);

  // Refocus the grid container after keyboard nav / exit-edit so keystrokes
  // keep flowing. Skipped when editing (the cell control owns focus).
  React.useLayoutEffect(() => {
    if (wantRoot.current && !editing) {
      wantRoot.current = false;
      rootRef.current?.focus();
    }
  });

  const patch = (key: string, p: Partial<Row>) =>
    onChange(lines.map((l) => (l.key === key ? { ...l, ...p } : l)));

  // The Skip flag is VIEW-ONLY now, not unfocusable: every visible cell can be
  // selected (click / arrows / Tab land on it). Only Enter-navigation jumps over
  // Skip columns, and Skip cells can never enter edit mode.
  const isSelectable = (c: number) => c >= 0 && c < cols.length && !!cols[c];
  const isEditableCol = (c: number) => isSelectable(c) && !!cols[c].isEditable && !cols[c].isSkippable;
  const firstSelectableCol = () => cols.findIndex((_, c) => isSelectable(c));
  // Nearest selectable column from `c` in `dir` (inclusive of `c`); -1 if none.
  const selectableFrom = (c: number, dir: 1 | -1) => {
    for (let i = c; i >= 0 && i < cols.length; i += dir) if (isSelectable(i)) return i;
    return -1;
  };
  // Nearest NON-skip selectable column from `c` in `dir` — Enter jumps over Skip.
  const enterFrom = (c: number, dir: 1 | -1) => {
    for (let i = c; i >= 0 && i < cols.length; i += dir) if (isSelectable(i) && !cols[i].isSkippable) return i;
    return -1;
  };

  const selectCell = (r: number, c: number) => {
    // Snap onto the nearest selectable column for out-of-range targets.
    const target = isSelectable(c) ? c : (selectableFrom(c, 1) >= 0 ? selectableFrom(c, 1) : selectableFrom(c, -1));
    if (target < 0) return;
    setEditing(false);
    setSel({ r, c: target });
    wantRoot.current = true;
  };

  const beginEdit = (r: number, c: number, opts: { seed?: string; selectOnFocus?: boolean; openModal?: boolean }) => {
    snapshot.current = lines[r] ? { ...lines[r] } : null;
    setSel({ r, c });
    setSeed(opts.seed);
    setSelectOnFocus(opts.selectOnFocus ?? false);
    setOpenModal(opts.openModal ?? false);
    setEditing(true);
  };

  const editCell = (r: number, c: number) => {
    if (!isEditableCol(c)) return; // Skip + non-editable columns are view-only
    beginEdit(r, c, { selectOnFocus: true });
  };

  const endEdit = (focusRoot = true) => {
    setEditing(false);
    setSeed(undefined);
    setOpenModal(false);
    if (focusRoot) wantRoot.current = true;
  };

  const cancelEdit = () => {
    const snap = snapshot.current;
    if (snap) onChange(lines.map((l) => (l.key === snap.key ? snap : l)));
    endEdit(true);
  };

  // Append a blank row — refused while any required cell of the last row is
  // empty ("Wajib" flag). Returns false when blocked so callers can stop.
  const appendRow = (focusCol: number): boolean => {
    const last = lines[lines.length - 1];
    const missing = last ? rowRequiredMissing(model, last, cols) : [];
    if (missing.length) { onAppendBlocked?.(missing); return false; }
    const target = isSelectable(focusCol) ? focusCol : Math.max(0, firstSelectableCol());
    setSel({ r: lines.length, c: target });
    setEditing(false);
    wantRoot.current = true;
    onChange([...lines, applyColumnDefaults(model, model.newRow(), cols)]);
    return true;
  };

  const removeRow = (idx: number) => {
    setEditing(false);
    wantRoot.current = true;
    if (lines.length <= 1) {
      setSel({ r: 0, c: 0 });
      onChange([applyColumnDefaults(model, model.newRow(), cols)]);
      return;
    }
    const next = lines.filter((_, i) => i !== idx);
    setSel({ r: Math.min(idx, next.length - 1), c: sel?.c ?? 0 });
    onChange(next);
  };

  const lastRow = lines.length - 1;
  const firstCol = firstSelectableCol();
  const lastCol = selectableFrom(cols.length - 1, -1);
  const firstEnterCol = enterFrom(0, 1);

  // Step to the next selectable column (Tab/arrows land on Skip cells too).
  const stepCol = (c: number, dir: 1 | -1) => selectableFrom(c + dir, dir);
  // Step to the next NON-skip column (Enter-navigation jumps over Skip).
  const stepEnter = (c: number, dir: 1 | -1) => enterFrom(c + dir, dir);

  // Move one cell forward/back, wrapping rows; appends when stepping past the end.
  const moveTab = (r: number, c: number, back: boolean) => {
    if (back) {
      const prev = stepCol(c, -1);
      if (prev >= 0) selectCell(r, prev);
      else if (r > 0) selectCell(r - 1, lastCol);
      return;
    }
    const next = stepCol(c, 1);
    if (next >= 0) selectCell(r, next);
    else if (r < lastRow) selectCell(r + 1, firstCol);
    else appendRow(firstCol);
  };

  // Enter advances forward to the next non-skip cell, wrapping rows / appending.
  // Landing only SELECTS the cell — it never auto-opens an editor (incl. on a new
  // row). Opening a required lookup is a deliberate second Enter (see onRootKeyDown).
  const moveEnter = (r: number, c: number) => {
    const next = stepEnter(c, 1);
    if (next >= 0) { selectCell(r, next); return; }
    if (r < lastRow) { selectCell(r + 1, firstEnterCol >= 0 ? firstEnterCol : firstCol); return; }
    appendRow(firstEnterCol >= 0 ? firstEnterCol : Math.max(0, firstCol));
  };

  // True when Enter on the selected cell should OPEN it instead of advancing:
  // an empty required editable cell (LOOKUP → search window). Filled cells advance.
  const shouldOpenOnEnter = (r: number, c: number) => {
    const col = cols[c];
    return !!col && col.isRequired && isEditableCol(c) && !isCellFilled(model, lines[r], col);
  };

  const startCharEdit = (r: number, c: number, key: string) => {
    const col = cols[c];
    if (!isEditableCol(c)) return false; // Skip + non-editable columns are view-only
    if (col.dataType === 'LOOKUP') { beginEdit(r, c, { seed: key }); return true; }
    if (col.dataType === 'DATE') { beginEdit(r, c, {}); return true; }
    if (col.dataType === 'NUMBER') {
      if (!/[0-9]/.test(key)) return false;
      patch(lines[r].key, model.buildCellPatch(lines[r], col, key));
      beginEdit(r, c, {});
      return true;
    }
    // TEXT
    patch(lines[r].key, model.buildCellPatch(lines[r], col, key));
    beginEdit(r, c, {});
    return true;
  };

  const onRootKeyDown = (e: React.KeyboardEvent<HTMLDivElement>) => {
    if (readOnly || !sel) return;
    const { r, c } = sel;

    if (editing) {
      if (e.key === 'Escape') { e.preventDefault(); cancelEdit(); }
      // SearchSelect preventDefaults its own Enter (resolving a pick); only a
      // plain field's unhandled Enter should commit then advance to the next cell.
      else if (e.key === 'Enter' && !e.defaultPrevented) { e.preventDefault(); endEdit(false); moveEnter(r, c); }
      else if (e.key === 'Tab') { e.preventDefault(); endEdit(false); moveTab(r, c, e.shiftKey); }
      return; // other keys → input handles (typing, caret arrows)
    }

    switch (e.key) {
      case 'ArrowUp': e.preventDefault(); if (r > 0) selectCell(r - 1, c); break;
      case 'ArrowDown':
        e.preventDefault();
        if (r < lastRow) selectCell(r + 1, c); else appendRow(c);
        break;
      case 'ArrowLeft': { e.preventDefault(); const p = stepCol(c, -1); if (p >= 0) selectCell(r, p); break; }
      case 'ArrowRight': { e.preventDefault(); const n = stepCol(c, 1); if (n >= 0) selectCell(r, n); break; }
      case 'Tab': e.preventDefault(); moveTab(r, c, e.shiftKey); break;
      case 'Enter':
        e.preventDefault();
        // Empty required cell → open it (LOOKUP = search window); else advance.
        if (shouldOpenOnEnter(r, c)) beginEdit(r, c, { selectOnFocus: true, openModal: isLookupCol(cols[c]) });
        else moveEnter(r, c);
        break;
      case 'F2': e.preventDefault(); editCell(r, c); break;
      case 'Delete':
      case 'Backspace':
        if (e.ctrlKey || e.metaKey) { e.preventDefault(); removeRow(r); }
        break;
      default:
        if (isPrintable(e) && startCharEdit(r, c, e.key)) e.preventDefault();
    }
  };

  return {
    rootRef, sel, editing, seed, selectOnFocus, openModal,
    onRootKeyDown, selectCell, editCell, endEdit, patch,
  };
}
