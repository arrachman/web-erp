import type * as React from 'react';

/**
 * How a grid column's cell behaves in display and edit mode.
 *
 * display context  → how the raw value is formatted when not editing
 * edit context     → what input widget appears when the cell is activated
 */
export type GridFieldType =
  | 'text'      // free-text input; plain string display
  | 'int'       // integer input (step=1); locale-formatted number display
  | 'currency'  // numeric input; IDR-formatted display (Geist Mono)
  | 'dropdown'  // <select> from options[]; selected label display
  | 'checkbox'  // click-to-toggle; ✓ / — display (no separate edit mode)
  | 'radio'     // inline radio group from options[]; selected label display
  | 'date'      // date input; DD/MM/YYYY string display
  | 'status'    // StatusBadge display — always read-only, never edited
  | 'custom';   // caller-supplied render prop; no generic editor

/**
 * Developer-facing column definition passed to useGridColumns().
 * Represents the *intent* for a column before user overrides are applied.
 */
export interface GridColumnDef<T = Record<string, unknown>> {
  /** Must match a key on the row data type. Used as stable identifier. */
  key: string;
  header: string;
  width?: number;
  /** Controls edit widget + display format. Default: 'text'. */
  fieldType?: GridFieldType;
  /** Show this column by default. Default: true. */
  defaultVisible?: boolean;
  /** Tab key can enter this cell. Default: true. */
  focusable?: boolean;
  sortable?: boolean;
  /**
   * Right-align + monospaced font.
   * Auto-derived as true for 'currency' and 'int' if not provided.
   */
  numeric?: boolean;
  /** Option list for 'dropdown' and 'radio' field types. */
  options?: string[];
  /** Required when fieldType === 'custom'. Receives (value, row). */
  render?: (value: unknown, row: T) => React.ReactNode;
}

/** User-settable overrides for one column, stored in localStorage. */
export interface GridColumnOverride {
  visible?: boolean;
  fieldType?: GridFieldType;
  focusable?: boolean;
  width?: number;
}

/** Map from column key → override. Serialized to localStorage as JSON. */
export type GridColumnOverrides = Record<string, GridColumnOverride>;

/**
 * Effective (merged) column descriptor after applying user overrides to the
 * developer def. This is what the grid and cell editor operate on at runtime.
 */
export interface GridColumnState {
  key: string;
  header: string;
  width?: number;
  fieldType: GridFieldType;
  visible: boolean;
  focusable: boolean;
  sortable: boolean;
  numeric: boolean;
  options: string[];
  render?: (value: unknown) => React.ReactNode;
}
