'use client';

import * as React from 'react';
import {
  DndContext, PointerSensor,
  useSensor, useSensors, closestCenter, type DragEndEvent,
} from '@dnd-kit/core';
import {
  SortableContext, verticalListSortingStrategy, useSortable,
  arrayMove,
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { Icon } from '@/components/ui/icons';
import {
  Table, TableHeader, TableBody, TableRow, TableHead, TableCell,
} from '@/components/organisms/table';
import {
  COLUMN_TYPE_PRESETS, inferColumnType,
  type ErpGridColumn, type ColumnType,
} from '@/lib/api/transaction-grids';
import { GridBulkToolbar, type BooleanColumnFlag } from '@/components/molecules/grid-bulk-toolbar';
import {
  EditableTextCell, EditableNumCell, EditableSelectCell, FlagCell,
  GridLookupSourceCell, cellSelectedStyle, type EditableCellHandle,
} from '@/components/molecules/grid-editable-cells';
import { GridColumnSettings } from '@/components/pages/grid-column-settings';

// ── constants ─────────────────────────────────────────────────────────────────

const POINTER_ACTIVATION = { activationConstraint: { distance: 5 } };

const BOOL_FLAGS: BooleanColumnFlag[] = ['isVisible', 'isRequired', 'isEditable', 'isSkippable'];
const ALL_SELECTABLE_COLS = [...BOOL_FLAGS, 'headerText', 'dataField', 'width', 'columnType'] as const;
const DEFAULT_FLAGS: Record<BooleanColumnFlag, boolean> = {
  isVisible: true, isRequired: false, isEditable: true, isSkippable: false,
};
const NAV_COLS = [
  'headerText', 'dataField', 'width',
  'isVisible', 'isRequired', 'isEditable', 'isSkippable', 'columnType',
] as const;
type NavColId = typeof NAV_COLS[number];

// ── helpers ───────────────────────────────────────────────────────────────────

function isColChanged(a: ErpGridColumn, b: ErpGridColumn | undefined): boolean {
  if (!b) return true;
  return (
    a.headerText !== b.headerText || a.dataField !== b.dataField ||
    a.width !== b.width || a.isVisible !== b.isVisible ||
    a.isRequired !== b.isRequired || a.isEditable !== b.isEditable ||
    a.isSkippable !== b.isSkippable || (a.columnType ?? null) !== (b.columnType ?? null) ||
    (a.lookupSource ?? null) !== (b.lookupSource ?? null) ||
    (a.lookupDefaultSort ?? null) !== (b.lookupDefaultSort ?? null) ||
    JSON.stringify(a.lookupDefaultFilter ?? null) !== JSON.stringify(b.lookupDefaultFilter ?? null) ||
    (a.placeholder ?? null) !== (b.placeholder ?? null) ||
    (a.defaultValue ?? null) !== (b.defaultValue ?? null) ||
    (a.defaultValueLabel ?? null) !== (b.defaultValueLabel ?? null)
  );
}

function setsEqual(a: Set<string>, b: Set<string>): boolean {
  if (a.size !== b.size) return false;
  for (const v of a) if (!b.has(v)) return false;
  return true;
}

// ── sortable row ──────────────────────────────────────────────────────────────

export type ColumnRowHandle = { startEdit: (colId: string, char?: string) => void };

interface SortableColumnRowProps {
  col: ErpGridColumn; index: number; saved: ErpGridColumn | undefined;
  selectedCells: Set<string>;
  onCellSelect: (rowIdx: number, colId: string) => void;
  onPatch: (rowIdx: number, p: Partial<ErpGridColumn>) => void;
  onRemove: (rowIdx: number) => void;
  onEditDone: () => void;
}

const SortableColumnRow = React.memo(
  React.forwardRef<ColumnRowHandle, SortableColumnRowProps>(
    ({ col, index, saved, selectedCells, onCellSelect, onPatch, onRemove, onEditDone }, ref) => {
      const headerTextRef = React.useRef<EditableCellHandle>(null);
      const dataFieldRef = React.useRef<EditableCellHandle>(null);
      const widthRef = React.useRef<EditableCellHandle>(null);

      React.useImperativeHandle(ref, () => ({
        startEdit: (colId: string, char?: string) => {
          if (colId === 'headerText') headerTextRef.current?.startEdit(char);
          else if (colId === 'dataField') dataFieldRef.current?.startEdit(char);
          else if (colId === 'width') widthRef.current?.startEdit(char);
        },
      }), []);

      const { attributes, listeners, setNodeRef, transform, transition, isDragging } =
        useSortable({ id: String(index) });

      const style: React.CSSProperties = {
        transform: CSS.Transform.toString(transform),
        transition,
        opacity: isDragging ? 0.5 : undefined,
        backgroundColor: isDragging ? 'var(--bg-hover)' : undefined,
      };

      const currentType: ColumnType = col.columnType
        ?? inferColumnType(col.labelFormatter, col.cellRenderer, col.cellEditor, col.dataType);

      const handleCellClick = (colId: string) => (e: React.MouseEvent) => {
        const t = e.target as HTMLElement;
        if (t.tagName === 'INPUT' || t.tagName === 'TEXTAREA') return;
        onCellSelect(index, colId);
      };

      const handleTypeChange = (type: ColumnType) => {
        const preset = COLUMN_TYPE_PRESETS[type];
        const patch: Partial<ErpGridColumn> = {
          columnType: type,
          labelFormatter: preset.labelFormatter, headerRenderer: preset.headerRenderer,
          cellRenderer: preset.cellRenderer, cellEditor: preset.cellEditor,
        };
        if (type === 'lookup') patch.dataType = 'LOOKUP';
        onPatch(index, patch);
      };

      return (
        <TableRow ref={setNodeRef as React.Ref<HTMLTableRowElement>} style={style}>
          <TableCell truncate={false} className="!px-0 text-center" style={{ width: 36 }}>
            <button type="button"
              className="iconbtn text-muted-foreground hover:text-foreground"
              title="Seret untuk urutkan kolom"
              aria-label="Seret untuk urutkan kolom"
              style={{ cursor: isDragging ? 'grabbing' : 'grab', touchAction: 'none' }}
              {...attributes} {...listeners}>
              <Icon name="grip-vertical" size={14} />
            </button>
          </TableCell>
          <TableCell style={{ width: 48 }}>
            <div className="flex items-center justify-center gap-1 text-muted-foreground">
              {index + 1}
              {isColChanged(col, saved) && <span title="Belum disimpan" className="inline-block h-1.5 w-1.5 shrink-0 rounded-full bg-warning" />}
            </div>
          </TableCell>
          <TableCell style={cellSelectedStyle(selectedCells.has('headerText'))} onClick={handleCellClick('headerText')}>
            <EditableTextCell ref={headerTextRef} value={col.headerText} onChange={(v) => onPatch(index, { headerText: v })} onEditDone={onEditDone} />
          </TableCell>
          <TableCell style={cellSelectedStyle(selectedCells.has('dataField'))} onClick={handleCellClick('dataField')}>
            <EditableTextCell ref={dataFieldRef} value={col.dataField} onChange={(v) => onPatch(index, { dataField: v })} onEditDone={onEditDone} />
          </TableCell>
          <TableCell style={cellSelectedStyle(selectedCells.has('width'))} onClick={handleCellClick('width')}>
            <EditableNumCell ref={widthRef} value={String(col.width)} onChange={(raw) => onPatch(index, { width: Number(raw || 0) })} onEditDone={onEditDone} />
          </TableCell>
          <FlagCell checked={col.isVisible} selected={selectedCells.has('isVisible')} onChange={(v) => onPatch(index, { isVisible: v })} onSelect={() => onCellSelect(index, 'isVisible')} />
          <FlagCell checked={col.isRequired} selected={selectedCells.has('isRequired')} onChange={(v) => onPatch(index, { isRequired: v })} onSelect={() => onCellSelect(index, 'isRequired')} />
          <FlagCell checked={col.isEditable} selected={selectedCells.has('isEditable')} onChange={(v) => onPatch(index, { isEditable: v })} onSelect={() => onCellSelect(index, 'isEditable')} />
          <FlagCell checked={col.isSkippable} selected={selectedCells.has('isSkippable')} onChange={(v) => onPatch(index, { isSkippable: v })} onSelect={() => onCellSelect(index, 'isSkippable')} />
          <TableCell style={cellSelectedStyle(selectedCells.has('columnType'))} onClick={() => onCellSelect(index, 'columnType')}>
            <div className="flex items-center gap-1">
              <div className="min-w-0 flex-1">
                <EditableSelectCell value={currentType} onChange={handleTypeChange} />
              </div>
              <span onClick={(e) => e.stopPropagation()}>
                <GridColumnSettings col={col} onPatch={(p) => onPatch(index, p)} />
              </span>
            </div>
            {currentType === 'lookup' && (
              <div className="mt-1 flex items-center gap-1" onClick={(e) => e.stopPropagation()}>
                <div className="min-w-0 flex-1">
                  <GridLookupSourceCell value={col.lookupSource} onChange={(v) => onPatch(index, { lookupSource: v })} />
                </div>
              </div>
            )}
          </TableCell>
          <TableCell truncate={false} className="text-center" style={{ width: 48 }}>
            <button type="button" className="iconbtn danger" title="Hapus kolom" onClick={() => onRemove(index)}>
              <Icon name="trash" size={13} />
            </button>
          </TableCell>
        </TableRow>
      );
    },
  ),
  (prev, next) => (
    prev.col === next.col &&
    prev.index === next.index &&
    prev.saved === next.saved &&
    setsEqual(prev.selectedCells, next.selectedCells) &&
    prev.onCellSelect === next.onCellSelect &&
    prev.onPatch === next.onPatch &&
    prev.onRemove === next.onRemove &&
    prev.onEditDone === next.onEditDone
  ),
);
(SortableColumnRow as { displayName?: string }).displayName = 'SortableColumnRow';

// ── main export ───────────────────────────────────────────────────────────────

export function GridCustomizationColumns({ columns, savedColumns, onColumnsChange }: {
  columns: ErpGridColumn[];
  savedColumns: ErpGridColumn[];
  onColumnsChange: (cols: ErpGridColumn[]) => void;
}) {
  const [selected, setSelected] = React.useState<Set<string>>(new Set());
  const containerRef = React.useRef<HTMLDivElement>(null);
  const rowRefs = React.useRef<(ColumnRowHandle | null)[]>([]);

  // Stable refs so callbacks below have zero deps (never recreated)
  const columnsRef = React.useRef(columns);
  columnsRef.current = columns;
  const onColumnsChangeRef = React.useRef(onColumnsChange);
  onColumnsChangeRef.current = onColumnsChange;
  const selectedRef = React.useRef(selected);
  selectedRef.current = selected;

  React.useEffect(() => { setSelected(new Set()); }, [columns.length]);

  const sensors = useSensors(useSensor(PointerSensor, POINTER_ACTIVATION));

  const handleDragEnd = React.useCallback((e: DragEndEvent) => {
    const { active, over } = e;
    if (!over || active.id === over.id) return;
    onColumnsChangeRef.current(arrayMove(columnsRef.current, Number(active.id), Number(over.id)));
  }, []);

  // Stable callbacks — no deps, use refs inside
  const handleCellSelect = React.useCallback((rowIdx: number, colId: string) =>
    setSelected((prev) => {
      const key = `${rowIdx}:${colId}`;
      return prev.size === 1 && prev.has(key) ? new Set<string>() : new Set([key]);
    }), []);

  const handlePatch = React.useCallback((rowIdx: number, p: Partial<ErpGridColumn>) =>
    onColumnsChangeRef.current(
      columnsRef.current.map((col, i) => i === rowIdx ? { ...col, ...p } : col)
    ), []);

  const handleRemove = React.useCallback((rowIdx: number) =>
    onColumnsChangeRef.current(columnsRef.current.filter((_, i) => i !== rowIdx)), []);

  const handleSelectAll = React.useCallback((checked: boolean) => {
    if (checked) {
      const all = new Set<string>();
      columnsRef.current.forEach((_, i) => ALL_SELECTABLE_COLS.forEach((c) => all.add(`${i}:${c}`)));
      setSelected(all);
    } else {
      setSelected(new Set());
    }
  }, []);

  const handleSelectColumn = React.useCallback((flag: BooleanColumnFlag) =>
    setSelected((prev) => {
      const keys = columnsRef.current.map((_, i) => `${i}:${flag}`);
      const allSel = keys.every((k) => prev.has(k));
      const next = new Set(prev);
      if (allSel) keys.forEach((k) => next.delete(k)); else keys.forEach((k) => next.add(k));
      return next;
    }), []);

  const handleBulkToggle = React.useCallback((flag: BooleanColumnFlag, value: boolean) =>
    onColumnsChangeRef.current(
      columnsRef.current.map((c, i) => selectedRef.current.has(`${i}:${flag}`) ? { ...c, [flag]: value } : c)
    ), []);

  const handleBulkReset = React.useCallback(() => {
    onColumnsChangeRef.current(columnsRef.current.map((c, i) =>
      BOOL_FLAGS.some((f) => selectedRef.current.has(`${i}:${f}`)) ? { ...c, ...DEFAULT_FLAGS } : c,
    ));
    setSelected(new Set());
  }, []);

  const returnFocus = React.useCallback(() => { containerRef.current?.focus(); }, []);

  const handleKeyDown = React.useCallback((e: React.KeyboardEvent<HTMLDivElement>) => {
    const t = e.target as HTMLElement;
    if (t.tagName === 'INPUT' || t.tagName === 'TEXTAREA' || t.tagName === 'SELECT') return;
    const sel = selectedRef.current;
    const cols = columnsRef.current;
    if (sel.size !== 1) return;
    const [key] = sel;
    const sep = key.indexOf(':');
    const rowIdx = parseInt(key.slice(0, sep), 10);
    const colId = key.slice(sep + 1) as NavColId;
    const colIdx = NAV_COLS.indexOf(colId);
    if (colIdx === -1) return;

    const moveRow = (dir: 1 | -1) => {
      e.preventDefault();
      setSelected(new Set([`${Math.max(0, Math.min(rowIdx + dir, cols.length - 1))}:${colId}`]));
    };
    const moveCol = (dir: 1 | -1) => {
      e.preventDefault();
      const ni = colIdx + dir;
      if (ni >= 0 && ni < NAV_COLS.length) setSelected(new Set([`${rowIdx}:${NAV_COLS[ni]}`]));
      else if (dir > 0 && rowIdx < cols.length - 1) setSelected(new Set([`${rowIdx + 1}:${NAV_COLS[0]}`]));
      else if (dir < 0 && rowIdx > 0) setSelected(new Set([`${rowIdx - 1}:${NAV_COLS[NAV_COLS.length - 1]}`]));
    };

    if (e.key === 'ArrowDown') { moveRow(1); return; }
    if (e.key === 'ArrowUp') { moveRow(-1); return; }
    if (e.key === 'ArrowRight' || (e.key === 'Tab' && !e.shiftKey)) { moveCol(1); return; }
    if (e.key === 'ArrowLeft' || (e.key === 'Tab' && e.shiftKey)) { moveCol(-1); return; }
    if (e.key === 'Escape') { e.preventDefault(); setSelected(new Set()); return; }
    if (e.key === ' ' && BOOL_FLAGS.includes(colId as BooleanColumnFlag)) {
      e.preventDefault();
      onColumnsChangeRef.current(cols.map((c, i) => i === rowIdx ? { ...c, [colId]: !c[colId as keyof ErpGridColumn] } : c));
      return;
    }
    if (e.key === 'Enter') { e.preventDefault(); rowRefs.current[rowIdx]?.startEdit(colId); return; }
    if (e.key.length === 1 && !e.ctrlKey && !e.metaKey && !e.altKey) {
      if (colId === 'headerText' || colId === 'dataField') { e.preventDefault(); rowRefs.current[rowIdx]?.startEdit(colId, e.key); }
      else if (colId === 'width' && /^\d$/.test(e.key)) { e.preventDefault(); rowRefs.current[rowIdx]?.startEdit(colId, e.key); }
    }
  }, []); // stable — reads state via refs, never recreated

  const colIds = React.useMemo(() => columns.map((_, i) => String(i)), [columns.length]);
  const rowSelectedCells = (rowIdx: number): Set<string> =>
    new Set(ALL_SELECTABLE_COLS.filter((c) => selected.has(`${rowIdx}:${c}`)));

  const thFlag = (flag: BooleanColumnFlag, label: string) => (
    <TableHead
      style={{ width: 56, textAlign: 'center', cursor: 'pointer', userSelect: 'none' }}
      onClick={() => handleSelectColumn(flag)}
      title={`Klik untuk pilih/batal semua sel ${label}`}
    >
      {label}
    </TableHead>
  );

  return (
    <div>
      <GridBulkToolbar
        total={columns.length}
        totalSelectableCols={ALL_SELECTABLE_COLS.length}
        selectedCells={selected}
        columns={columns}
        onSelectAll={handleSelectAll}
        onBulkToggle={handleBulkToggle}
        onBulkReset={handleBulkReset}
      />
      <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={handleDragEnd}>
        <div ref={containerRef} tabIndex={0} className="overflow-x-auto outline-none" onKeyDown={handleKeyDown}>
          <Table className="table-fixed min-w-[900px]">
            <TableHeader>
              <TableRow>
                <TableHead style={{ width: 36 }} />
                <TableHead style={{ width: 48, textAlign: 'center' }}>No</TableHead>
                <TableHead style={{ width: 260 }}>Header Text</TableHead>
                <TableHead style={{ width: 200 }}>Data Field</TableHead>
                <TableHead style={{ width: 90, textAlign: 'right' }}>Lebar</TableHead>
                {thFlag('isVisible', 'Tampil')}
                {thFlag('isRequired', 'Wajib')}
                {thFlag('isEditable', 'Edit')}
                {thFlag('isSkippable', 'Skip')}
                <TableHead style={{ width: 180 }}>Tipe Kolom</TableHead>
                <TableHead style={{ width: 48 }} />
              </TableRow>
            </TableHeader>
            <SortableContext items={colIds} strategy={verticalListSortingStrategy}>
              <TableBody>
                {columns.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={11} className="py-4 text-center text-muted-foreground">
                      Belum ada kolom. Klik &ldquo;Tambah Kolom&rdquo;.
                    </TableCell>
                  </TableRow>
                ) : (
                  columns.map((c, i) => (
                    <SortableColumnRow
                      key={String(i)}
                      ref={(el) => { rowRefs.current[i] = el; }}
                      col={c}
                      index={i}
                      saved={savedColumns[i]}
                      selectedCells={rowSelectedCells(i)}
                      onCellSelect={handleCellSelect}
                      onPatch={handlePatch}
                      onRemove={handleRemove}
                      onEditDone={returnFocus}
                    />
                  ))
                )}
              </TableBody>
            </SortableContext>
          </Table>
        </div>
      </DndContext>
    </div>
  );
}
