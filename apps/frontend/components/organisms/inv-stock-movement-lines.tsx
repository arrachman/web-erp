'use client';

/**
 * Editable movement-line grid for inventory stock movements (Material Request /
 * Stock Transfer / Transfer Receipt / Fuel Refill). Atomic tier: Organism.
 * Reuses the generic config-driven grid engine (`useGridNav` + `LineCell`) with
 * the inventory line `GridModel`.
 *
 * Columns are config-driven (Kustomisasi Grid): pass `columns` (visible
 * GridCol[]) or `transactionCode` (e.g. "INV.MR") to self-fetch; falls back to
 * `defaultInvStockMovementCols()`. Spreadsheet-style: cells default to SELECTED,
 * edit on demand (click+type / F2 / dbl-click). No money subtotal — inventory
 * movements track quantity, not value; footer just shows the line count.
 */

import * as React from 'react';
import { Kbd } from '@/components/ui/kbd';
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableCell,
} from '@/components/organisms/table';
import { notify } from '@/lib/feedback';
import { getGridColumns, type ErpGridColumn } from '@/lib/api/transaction-grids';
import { LineCell } from './cash-bank-line-cell';
import { useGridNav } from './use-grid-nav';
import { useSeedLineDefaults } from './use-line-defaults';
import { linesRequiredMissing, type GridCol } from './grid-line-core';
import {
  defaultInvStockMovementCols,
  getCellRaw,
  invStockMovementGridModel,
  type InvStockMovementLineRow,
} from './inv-stock-movement-line-model';

const toGridCols = (cols: ErpGridColumn[]): GridCol[] =>
  cols
    .filter((c) => c.isVisible)
    .map((c) => ({
      dataField: c.dataField,
      headerText: c.headerText,
      width: c.width,
      dataType: c.dataType,
      lookupSource: c.lookupSource,
      lookupDefaultFilter: c.lookupDefaultFilter,
      lookupDefaultSort: c.lookupDefaultSort,
      kind: c.kind,
      // ROWNUM = auto sequence from row position → always read-only.
      isEditable: c.cellEditor === 'ROWNUM' ? false : c.isEditable,
      isRequired: c.isRequired,
      isSkippable: c.isSkippable,
      cellEditor: c.cellEditor ?? null,
      placeholder: c.placeholder,
      defaultValue: c.defaultValue,
      defaultValueLabel: c.defaultValueLabel,
    }));

export type { InvStockMovementLineRow } from './inv-stock-movement-line-model';
export { newInvStockMovementLine } from './inv-stock-movement-line-model';

/** Imperative handle so a parent form can move focus into the grid (selects cell 0,0). */
export interface InvStockMovementLinesHandle {
  focus: () => void;
}

/** Resolved display label for a lookup cell (picked label, else stored row label). */
const lookupLabel = (row: InvStockMovementLineRow, col: GridCol): string | undefined => {
  if (row.labels?.[col.dataField]) return row.labels[col.dataField];
  if (col.dataField === 'itemId') return row.itemLabel;
  if (col.dataField === 'unitId') return row.unitLabel;
  if (col.dataField === 'sourceWarehouseId') return row.sourceWarehouseLabel;
  if (col.dataField === 'destinationWarehouseId') return row.destinationWarehouseLabel;
  return undefined;
};

export const InvStockMovementLinesEditor = React.forwardRef<InvStockMovementLinesHandle, {
  lines: InvStockMovementLineRow[];
  onChange: (lines: InvStockMovementLineRow[]) => void;
  readOnly?: boolean;
  /** Explicit column config (wins over self-fetch). */
  columns?: GridCol[];
  /** When set (e.g. "INV.MR"), the editor fetches its Kustomisasi Grid config. */
  transactionCode?: string;
  /** Reports the required ("Wajib") columns still left empty across all rows. */
  onValidityChange?: (missing: string[]) => void;
}>(function InvStockMovementLinesEditor({
  lines,
  onChange,
  readOnly = false,
  columns,
  transactionCode,
  onValidityChange,
}, ref) {
  const [fetched, setFetched] = React.useState<GridCol[] | undefined>();

  React.useEffect(() => {
    if (columns || !transactionCode) return;
    let alive = true;
    getGridColumns(transactionCode)
      .then((r) => { if (alive) setFetched(toGridCols(r.columns)); })
      .catch(() => { /* fall back to default columns */ });
    return () => { alive = false; };
  }, [columns, transactionCode]);

  const cols = React.useMemo(() => {
    if (columns && columns.length) return columns;
    if (fetched && fetched.length) return fetched;
    return defaultInvStockMovementCols();
  }, [columns, fetched]);

  useSeedLineDefaults({
    ready: columns ? true : fetched !== undefined,
    lines, cols, model: invStockMovementGridModel, readOnly, onChange,
  });

  const {
    rootRef, sel, editing, seed, selectOnFocus, openModal,
    onRootKeyDown, selectCell, editCell, endEdit, patch,
  } = useGridNav<InvStockMovementLineRow>({
    lines,
    onChange,
    cols,
    readOnly,
    model: invStockMovementGridModel,
    onAppendBlocked: (missing) =>
      notify(`Lengkapi kolom wajib dulu: ${missing.join(', ')}`, 'warn'),
  });

  const missingRequired = React.useMemo(
    () => linesRequiredMissing(invStockMovementGridModel, lines, cols),
    [lines, cols],
  );
  React.useEffect(() => { onValidityChange?.(missingRequired); }, [missingRequired, onValidityChange]);

  React.useImperativeHandle(ref, () => ({ focus: () => rootRef.current?.focus() }), [rootRef]);

  const handleRootFocus = () => {
    if (!readOnly && !sel && !editing) selectCell(0, 0);
  };

  const colSpan = cols.length;
  const fixedWidth = cols.reduce((s, c) => s + (c.width || 0), 0);
  const hasElastic = cols.some((c) => !c.width);

  return (
    <div
      className="inv-stock-movement-lines outline-none overflow-x-auto"
      ref={rootRef}
      tabIndex={readOnly ? -1 : 0}
      onKeyDown={onRootKeyDown}
      onFocus={handleRootFocus}
    >
      {!readOnly && (
        <div className="mb-2 flex flex-wrap items-center justify-end gap-1 text-[11px] text-muted-foreground">
          Klik cell lalu ketik / <Kbd>F2</Kbd> untuk edit · <Kbd>Enter</Kbd> maju ke kolom berikutnya ·{' '}
          <Kbd>↑↓←→</Kbd> pindah · <Kbd>Tab</Kbd>/<Kbd>↓</Kbd> di akhir = baris baru ·{' '}
          <Kbd>Ctrl</Kbd>+<Kbd>Del</Kbd> hapus baris
        </div>
      )}

      <Table
        className={hasElastic ? 'table-fixed w-full' : 'table-fixed !w-auto'}
        style={hasElastic ? { minWidth: fixedWidth } : { width: fixedWidth, minWidth: fixedWidth }}
      >
        <TableHeader>
          <TableRow>
            {cols.map((c) => (
              <TableHead
                key={c.dataField}
                style={{ width: c.width || undefined, textAlign: c.cellEditor === 'ROWNUM' ? 'center' : c.dataType === 'NUMBER' ? 'right' : 'left' }}
              >
                {c.headerText}
              </TableHead>
            ))}
          </TableRow>
        </TableHeader>
        <TableBody>
          {lines.length === 0 ? (
            <TableRow>
              <TableCell colSpan={colSpan} className="text-center text-muted-foreground py-4">
                Belum ada baris item.
              </TableCell>
            </TableRow>
          ) : (
            lines.map((l, i) => (
              <TableRow key={l.key} data-row={i}>
                {cols.map((c, ci) => {
                  const isSel = sel?.r === i && sel?.c === ci;
                  const isEdit = editing && isSel;
                  return (
                    <LineCell
                      key={c.dataField}
                      col={c}
                      rowIndex={i}
                      value={getCellRaw(l, c)}
                      label={lookupLabel(l, c)}
                      selected={!readOnly && !!isSel}
                      editing={isEdit}
                      seed={isEdit ? seed : undefined}
                      selectOnFocus={selectOnFocus}
                      autoOpenModal={isEdit ? openModal : false}
                      onSet={(value, label) => patch(l.key, invStockMovementGridModel.buildCellPatch(l, c, value, label))}
                      onSelect={() => { if (!readOnly) selectCell(i, ci); }}
                      onEdit={() => { if (!readOnly) editCell(i, ci); }}
                      onEndEdit={(focus) => endEdit(focus)}
                    />
                  );
                })}
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>

      <div className="flex justify-end gap-6 pt-3 pr-2 text-sm text-muted-foreground">
        <span>
          Jumlah baris <strong className="tabular-nums ml-2 text-foreground">{lines.length}</strong>
        </span>
      </div>
    </div>
  );
});
