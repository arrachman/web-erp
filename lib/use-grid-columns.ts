'use client';

import * as React from 'react';
import type {
  GridColumnDef,
  GridColumnState,
  GridColumnOverride,
  GridColumnOverrides,
  GridFieldType,
} from './grid-types';

// ---------------------------------------------------------------------------
// localStorage helpers
// ---------------------------------------------------------------------------

const STORAGE_PREFIX = 'grid-col:';

function loadOverrides(gridId: string): GridColumnOverrides {
  if (typeof window === 'undefined') return {};
  try {
    const raw = localStorage.getItem(STORAGE_PREFIX + gridId);
    return raw ? (JSON.parse(raw) as GridColumnOverrides) : {};
  } catch {
    return {};
  }
}

function saveOverrides(gridId: string, ov: GridColumnOverrides): void {
  try {
    localStorage.setItem(STORAGE_PREFIX + gridId, JSON.stringify(ov));
  } catch {
    // storage quota exceeded — ignore silently
  }
}

// ---------------------------------------------------------------------------
// Merge logic: def + user overrides → effective GridColumnState
// ---------------------------------------------------------------------------

function autoNumeric(ft: GridFieldType, explicit?: boolean): boolean {
  if (explicit !== undefined) return explicit;
  return ft === 'currency' || ft === 'int';
}

function mergeColumns(
  defs: GridColumnDef[],
  overrides: GridColumnOverrides,
): GridColumnState[] {
  return defs.map((d) => {
    const ov = overrides[d.key] ?? {};
    const ft: GridFieldType = ov.fieldType ?? d.fieldType ?? 'text';
    return {
      key: d.key,
      header: d.header,
      width: ov.width ?? d.width,
      fieldType: ft,
      visible: ov.visible ?? d.defaultVisible ?? true,
      focusable: ov.focusable ?? d.focusable ?? true,
      sortable: d.sortable ?? false,
      numeric: autoNumeric(ft, d.numeric),
      options: d.options ?? [],
      render: d.render as GridColumnState['render'] | undefined,
    };
  });
}

// ---------------------------------------------------------------------------
// Hook
// ---------------------------------------------------------------------------

export interface UseGridColumnsReturn {
  /** All columns (visible and hidden). */
  columns: GridColumnState[];
  /** Only visible columns, in definition order. */
  visibleColumns: GridColumnState[];
  /** Patch one column's user override and persist. */
  updateOverride: (key: string, patch: GridColumnOverride) => void;
  /** Clear all user overrides for this grid, reverting to defaults. */
  resetOverrides: () => void;
}

/**
 * Manages column state for a configurable data grid.
 *
 * @param gridId  Stable string ID for this grid instance — used as
 *                localStorage key suffix. Keep it unique per grid.
 * @param defs    Static column definitions from the developer.
 *
 * User overrides (visibility, field type, focusable, width) are merged
 * on top of the defs and persisted to localStorage automatically.
 */
export function useGridColumns(
  gridId: string,
  defs: GridColumnDef[],
): UseGridColumnsReturn {
  const [overrides, setOverrides] = React.useState<GridColumnOverrides>(() =>
    loadOverrides(gridId),
  );

  const columns = React.useMemo(
    () => mergeColumns(defs, overrides),
    [defs, overrides],
  );

  const visibleColumns = React.useMemo(
    () => columns.filter((c) => c.visible),
    [columns],
  );

  const updateOverride = React.useCallback(
    (key: string, patch: GridColumnOverride) => {
      setOverrides((prev) => {
        const next = { ...prev, [key]: { ...prev[key], ...patch } };
        saveOverrides(gridId, next);
        return next;
      });
    },
    [gridId],
  );

  const resetOverrides = React.useCallback(() => {
    try {
      localStorage.removeItem(STORAGE_PREFIX + gridId);
    } catch {
      // ignore
    }
    setOverrides({});
  }, [gridId]);

  return { columns, visibleColumns, updateOverride, resetOverrides };
}
