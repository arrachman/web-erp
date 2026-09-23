'use client';

/**
 * Cash/bank binding of the generic grid navigation engine. The spreadsheet
 * state machine now lives in `use-grid-nav` (model-agnostic); this wrapper just
 * injects the cash/bank `GridModel`. Kept as a named hook so the cash/bank
 * organism keeps importing `useCashGridNav` unchanged.
 */

import { useGridNav, type GridNav, type CellSel } from './use-grid-nav';
import { cashBankGridModel, type CashLineRow } from './cash-bank-line-model';
import type { GridCol } from './grid-line-core';

export type { CellSel };
export type CashGridNav = GridNav<CashLineRow>;

export function useCashGridNav({
  lines,
  onChange,
  cols,
  readOnly,
  onAppendBlocked,
}: {
  lines: CashLineRow[];
  onChange: (lines: CashLineRow[]) => void;
  cols: GridCol[];
  readOnly: boolean;
  onAppendBlocked?: (missing: string[]) => void;
}): CashGridNav {
  return useGridNav<CashLineRow>({ lines, onChange, cols, readOnly, model: cashBankGridModel, onAppendBlocked });
}
