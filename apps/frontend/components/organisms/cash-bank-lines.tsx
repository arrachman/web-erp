'use client';

/**
 * Editable contra-account grid for cash/bank transactions (Kas Masuk/Keluar).
 * Atomic tier: Organism. Reusable across CR/CD/BD forms.
 *
 * Columns are config-driven (Kustomisasi Grid): pass `columns` (visible GridCol[])
 * to control which fields render, their order/width/header/editability + custom
 * columns. Falls back to `defaultGridCols(showFx)` when no config is supplied.
 *
 * Spreadsheet-style: cells default to SELECTED (not active inputs). Navigation +
 * row add/remove live in `useCashGridNav`; per-cell display/edit in `LineCell`.
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
import {
  buildCellPatch,
  CashLineRow,
  defaultGridCols,
  getCellRaw,
  linesRequiredMissing,
  type GridCol,
} from './cash-bank-line-model';
import { LineCell } from './cash-bank-line-cell';
import { useCashGridNav } from './use-cash-grid-nav';
import { cashBankGridModel } from './cash-bank-line-model';
import { useSeedLineDefaults } from './use-line-defaults';

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

export type { CashLineRow } from './cash-bank-line-model';
export { newCashLine } from './cash-bank-line-model';

/** Imperative handle so a parent form can move focus into the grid (selects cell 0,0). */
export interface CashBankLinesHandle {
  focus: () => void;
}

const lookupLabel = (row: CashLineRow, col: GridCol): string | undefined => {
  if (row.labels?.[col.dataField]) return row.labels[col.dataField];
  if (col.dataField === 'accountId') return row.accountLabel;
  if (col.dataField === 'costCenterId') return row.costCenterLabel;
  return undefined;
};

export const CashBankLinesEditor = React.forwardRef<CashBankLinesHandle, {
  lines: CashLineRow[];
  onChange: (lines: CashLineRow[]) => void;
  readOnly?: boolean;
  showFx?: boolean;
  /** Explicit column config (wins over self-fetch). */
  columns?: GridCol[];
  /** When set (e.g. "FIN.CR"), the editor fetches its Kustomisasi Grid config. */
  transactionCode?: string;
  /** Reports the required ("Wajib") columns still left empty across all rows. */
  onValidityChange?: (missing: string[]) => void;
}>(function CashBankLinesEditor({
  lines,
  onChange,
  readOnly = false,
  showFx = false,
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
    return defaultGridCols(showFx);
  }, [columns, fetched, showFx]);

  useSeedLineDefaults({
    ready: columns ? true : fetched !== undefined,
    lines, cols, model: cashBankGridModel, readOnly, onChange,
  });

  const {
    rootRef, sel, editing, seed, selectOnFocus, openModal,
    onRootKeyDown, selectCell, editCell, endEdit, patch,
  } = useCashGridNav({
    lines,
    onChange,
    cols,
    readOnly,
    onAppendBlocked: (missing) =>
      notify(`Lengkapi kolom wajib dulu: ${missing.join(', ')}`, 'warn'),
  });

  // Surface required-column completeness so the form can gate saving.
  const missingRequired = React.useMemo(() => linesRequiredMissing(lines, cols), [lines, cols]);
  React.useEffect(() => { onValidityChange?.(missingRequired); }, [missingRequired, onValidityChange]);

  // Let a parent form move focus into the grid (onFocus then selects cell 0,0).
  React.useImperativeHandle(ref, () => ({ focus: () => rootRef.current?.focus() }), [rootRef]);

  const handleRootFocus = () => {
    if (!readOnly && !sel && !editing) selectCell(0, 0);
  };

  const total = lines.reduce((s, l) => s + Number(l.amount || 0), 0);
  const totalFx = lines.reduce((s, l) => s + Number(l.amountFx || 0), 0);
  const hasFx = cols.some((c) => c.dataField === 'amountFx');
  const colSpan = cols.length;
  // Fixed layout honours each column's px width exactly. Columns with width 0
  // (falsy) get NO width style → they stay elastic and absorb the leftover space.
  // - With ≥1 elastic column: table is w-full so the elastic column(s) fill the
  //   slack; minWidth = Σ fixed widths keeps the fixed columns intact (elastic
  //   shrinks freely when the container is narrow).
  // - With no elastic column: table width = Σ widths, so a wide container leaves
  //   plain background to the right and a narrow one scrolls instead of squashing.
  const fixedWidth = cols.reduce((s, c) => s + (c.width || 0), 0);
  const hasElastic = cols.some((c) => !c.width);

  return (
    <div
      className="cashbank-lines outline-none overflow-x-auto"
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
                Belum ada baris akun.
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
                      onSet={(value, label) => patch(l.key, buildCellPatch(l, c, value, label))}
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
        {hasFx && (
          <span className="text-muted-foreground">
            Total Valas <strong className="tabular-nums ml-2">{formatNumber(totalFx, 2)}</strong>
          </span>
        )}
        <span>
          Total <strong className="tabular-nums ml-2">{formatNumber(total, 2)}</strong>
        </span>
      </div>
    </div>
  );
});
