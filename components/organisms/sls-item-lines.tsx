'use client';

/**
 * Editable item-line grid for sales transactions (Sales Order/Invoice/DO…).
 * Atomic tier: Organism. Reuses the generic config-driven grid engine
 * (`useGridNav` + `LineCell`) with the sales item-line `GridModel`.
 *
 * Columns are config-driven (Kustomisasi Grid): pass `columns` (visible
 * GridCol[]) or `transactionCode` (e.g. "SLS.SO") to self-fetch; falls back to
 * `defaultSlsItemCols()`. Spreadsheet-style: cells default to SELECTED, edit on
 * demand (click+type / F2 / dbl-click). Footer shows the subtotal (Σ line total).
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
import { formatNumber } from '@/lib/format';
import { notify } from '@/lib/feedback';
import { getGridColumns, type ErpGridColumn } from '@/lib/api/transaction-grids';
import { getItemForAutoFill } from '@/lib/api/sls-orders';
import { LineCell } from './cash-bank-line-cell';
import { useGridNav } from './use-grid-nav';
import { useSeedLineDefaults } from './use-line-defaults';
import { linesRequiredMissing, type GridCol } from './grid-line-core';
import {
  computeLineTotal,
  defaultSlsItemCols,
  getCellRaw,
  slsItemGridModel,
  type SlsItemLineRow,
} from './sls-item-line-model';

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

export type { SlsItemLineRow } from './sls-item-line-model';
export { newSlsItemLine, computeLineTotal } from './sls-item-line-model';

/** Imperative handle so a parent form can move focus into the grid (selects cell 0,0). */
export interface SlsItemLinesHandle {
  focus: () => void;
}

/** Resolved display label for a lookup cell (picked label, else stored row label). */
const lookupLabel = (row: SlsItemLineRow, col: GridCol): string | undefined => {
  if (row.labels?.[col.dataField]) return row.labels[col.dataField];
  if (col.dataField === 'itemId') return row.itemLabel;
  if (col.dataField === 'unitId') return row.unitLabel;
  if (col.dataField === 'tax1Id') return row.tax1Label;
  if (col.dataField === 'warehouseId') return row.warehouseLabel;
  return undefined;
};

export const SlsItemLinesEditor = React.forwardRef<SlsItemLinesHandle, {
  lines: SlsItemLineRow[];
  onChange: (lines: SlsItemLineRow[]) => void;
  readOnly?: boolean;
  /** Explicit column config (wins over self-fetch). */
  columns?: GridCol[];
  /** When set (e.g. "SLS.SO"), the editor fetches its Kustomisasi Grid config. */
  transactionCode?: string;
  /** Reports the required ("Wajib") columns still left empty across all rows. */
  onValidityChange?: (missing: string[]) => void;
  /**
   * Tingkat harga/diskon jual (1..10) dari kategori pelanggan terpilih. Saat item
   * dipilih, harga & diskon baris di-default dari Harga Jual/Diskon Jual tingkat ini;
   * fallback ke Harga Jual 1 (salePrice) bila tier tak di-set / tak ada.
   */
  salesTier?: number | null;
}>(function SlsItemLinesEditor({
  lines,
  onChange,
  readOnly = false,
  columns,
  transactionCode,
  onValidityChange,
  salesTier,
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
    return defaultSlsItemCols();
  }, [columns, fetched]);

  // Seed configured default values onto the initial blank line once real config loads.
  useSeedLineDefaults({
    ready: columns ? true : fetched !== undefined,
    lines, cols, model: slsItemGridModel, readOnly, onChange,
  });

  const {
    rootRef, sel, editing, seed, selectOnFocus, openModal,
    onRootKeyDown, selectCell, editCell, endEdit, patch,
  } = useGridNav<SlsItemLineRow>({
    lines,
    onChange,
    cols,
    readOnly,
    model: slsItemGridModel,
    onAppendBlocked: (missing) =>
      notify(`Lengkapi kolom wajib dulu: ${missing.join(', ')}`, 'warn'),
  });

  const missingRequired = React.useMemo(
    () => linesRequiredMissing(slsItemGridModel, lines, cols),
    [lines, cols],
  );
  React.useEffect(() => { onValidityChange?.(missingRequired); }, [missingRequired, onValidityChange]);

  React.useImperativeHandle(ref, () => ({ focus: () => rootRef.current?.focus() }), [rootRef]);

  const handleRootFocus = () => {
    if (!readOnly && !sel && !editing) selectCell(0, 0);
  };

  const subtotal = lines.reduce((s, l) => s + computeLineTotal(l), 0);
  const colSpan = cols.length;
  const fixedWidth = cols.reduce((s, c) => s + (c.width || 0), 0);
  const hasElastic = cols.some((c) => !c.width);

  return (
    <div
      className="sls-item-lines outline-none overflow-x-auto"
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
                      onSet={(value, label) => {
                        patch(l.key, slsItemGridModel.buildCellPatch(l, c, value, label));
                        if (c.dataField === 'itemId' && value) {
                          getItemForAutoFill(value).then((item) => {
                            if (!item) return;
                            const autoPatch: Partial<SlsItemLineRow> = {};
                            // Tingkat harga/diskon jual = salesTier kategori pelanggan.
                            // Fallback ke Harga Jual 1 (salePrice) bila tier tak ada.
                            const tier = salesTier
                              ? item.prices?.find((p) => p.level === salesTier)
                              : undefined;
                            const tierPrice = tier?.price ?? item.salePrice;
                            if (tierPrice && !l.unitPrice) autoPatch.unitPrice = tierPrice;
                            if (tier?.discountPercent && Number(tier.discountPercent) > 0 && !l.discountPercent) {
                              autoPatch.discountPercent = tier.discountPercent;
                            }
                            // Satuan default penjualan: pakai satuan jual (fieldUnit,
                            // mis. kwintal) bila di-set di master item, else satuan dasar.
                            const defUnitId = item.fieldUnitId || item.unitId;
                            const defUnit = item.fieldUnitId ? item.fieldUnit : item.unit;
                            if (defUnitId && !l.unitId) {
                              autoPatch.unitId = defUnitId;
                              autoPatch.unitLabel = defUnit?.name;
                            }
                            if (item.saleTaxId && !l.tax1Id) {
                              autoPatch.tax1Id = item.saleTaxId;
                              autoPatch.tax1Label = item.saleTax?.name;
                            }
                            if (item.saleTax2Id && !l.tax2Id) {
                              autoPatch.tax2Id = item.saleTax2Id;
                              autoPatch.tax2Label = item.saleTax2?.name;
                            }
                            if (Object.keys(autoPatch).length) patch(l.key, autoPatch);
                          }).catch(() => null);
                        }
                      }}
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

      <div className="flex justify-end gap-6 pt-3 pr-2 text-sm">
        <span>
          Subtotal <strong className="tabular-nums ml-2">{formatNumber(subtotal, 2)}</strong>
        </span>
      </div>
    </div>
  );
});
