'use client';

import * as React from 'react';
import {
  DndContext, PointerSensor, KeyboardSensor,
  useSensor, useSensors, closestCenter, type DragEndEvent,
} from '@dnd-kit/core';
import {
  SortableContext, horizontalListSortingStrategy, useSortable,
  sortableKeyboardCoordinates, arrayMove,
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { Input } from '@/components/ui/input';
import { Icon } from '@/components/ui/icons';
import { cn } from '@/lib/utils';
import type { ErpTransactionGrid } from '@/lib/api/transaction-grids';

function uniqueKey(base: string, taken: Set<string>) {
  let key = base;
  let n = 2;
  while (taken.has(key)) { key = `${base}-${n}`; n += 1; }
  return key;
}

function SortableTabItem({
  g, total, activeKey, editingKey,
  onSelect, onSetEditingKey, onRename, onSetPrimary, onRemove,
}: {
  g: ErpTransactionGrid;
  total: number;
  activeKey?: string;
  editingKey: string | null;
  onSelect: (key: string) => void;
  onSetEditingKey: (key: string | null) => void;
  onRename: (key: string, label: string) => void;
  onSetPrimary: (key: string) => void;
  onRemove: (key: string) => void;
}) {
  const {
    attributes, listeners, setNodeRef, transform, transition, isDragging,
  } = useSortable({ id: g.key });

  const active = g.key === activeKey;

  const style: React.CSSProperties = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : undefined,
  };

  return (
    <div
      ref={setNodeRef}
      style={style}
      className={cn(
        'group flex items-center gap-1 rounded-t px-2 py-1.5 text-[12.5px]',
        active ? 'bg-secondary text-foreground' : 'text-muted-foreground hover:text-foreground',
        isDragging && 'z-10',
      )}
      {...attributes}
    >
      <button
        type="button"
        className="iconbtn text-muted-foreground"
        style={{ cursor: isDragging ? 'grabbing' : 'grab', touchAction: 'none' }}
        title="Seret untuk menata ulang"
        {...listeners}
      >
        <Icon name="grip-vertical" size={12} />
      </button>
      {editingKey === g.key ? (
        <Input
          autoFocus
          value={g.label}
          onChange={(e) => onRename(g.key, e.target.value)}
          onBlur={() => onSetEditingKey(null)}
          onKeyDown={(e) => { if (e.key === 'Enter') onSetEditingKey(null); }}
          className="h-6 w-28 px-1 text-[12.5px]"
          onPointerDown={(e) => e.stopPropagation()}
        />
      ) : (
        <button
          type="button"
          className="cursor-pointer whitespace-nowrap font-medium"
          onClick={() => onSelect(g.key)}
          onDoubleClick={() => onSetEditingKey(g.key)}
          onPointerDown={(e) => e.stopPropagation()}
          title="Klik untuk pilih · klik-ganda untuk ubah nama · tarik untuk menata ulang"
        >
          {g.label}
        </button>
      )}
      <button
        type="button"
        className={cn('iconbtn', g.isPrimary ? 'text-primary' : 'text-muted-foreground')}
        title={g.isPrimary ? 'Tab utama (dibaca grid entry)' : 'Jadikan tab utama'}
        onClick={() => onSetPrimary(g.key)}
        onPointerDown={(e) => e.stopPropagation()}
      >
        <Icon name={g.isPrimary ? 'check' : 'dot'} size={12} />
      </button>
      {active && (
        <button
          type="button"
          className="iconbtn danger"
          title="Hapus tab"
          onClick={() => onRemove(g.key)}
          onPointerDown={(e) => e.stopPropagation()}
          disabled={total <= 1}
        >
          <Icon name="trash" size={12} />
        </button>
      )}
    </div>
  );
}

export function GridCustomizationTabs({
  grids, activeKey, onSelect, onGridsChange,
}: {
  grids: ErpTransactionGrid[];
  activeKey?: string;
  onSelect: (key: string) => void;
  onGridsChange: (grids: ErpTransactionGrid[]) => void;
}) {
  const [editingKey, setEditingKey] = React.useState<string | null>(null);

  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 5 } }),
    useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates }),
  );

  const handleDragEnd = (e: DragEndEvent) => {
    const { active, over } = e;
    if (!over || active.id === over.id) return;
    const from = grids.findIndex((g) => g.key === String(active.id));
    const to = grids.findIndex((g) => g.key === String(over.id));
    if (from !== -1 && to !== -1) onGridsChange(arrayMove(grids, from, to));
  };

  const addTab = () => {
    const taken = new Set(grids.map((g) => g.key));
    const key = uniqueKey('tab', taken);
    const next: ErpTransactionGrid = {
      key, label: `Tab ${grids.length + 1}`, sortOrder: grids.length,
      lineTable: null, isPrimary: grids.length === 0, columns: [],
    };
    onGridsChange([...grids, next]);
    onSelect(key);
    setEditingKey(key);
  };

  const rename = (key: string, label: string) =>
    onGridsChange(grids.map((g) => (g.key === key ? { ...g, label } : g)));

  const setPrimary = (key: string) =>
    onGridsChange(grids.map((g) => ({ ...g, isPrimary: g.key === key })));

  const remove = (key: string) => {
    if (grids.length <= 1) return;
    const next = grids.filter((g) => g.key !== key);
    if (!next.some((g) => g.isPrimary)) next[0] = { ...next[0], isPrimary: true };
    onGridsChange(next);
    if (activeKey === key) onSelect(next[0].key);
  };

  const gridKeys = React.useMemo(() => grids.map((g) => g.key), [grids]);

  return (
    <div className="flex items-center gap-1 border-b border-border bg-card px-2">
      <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={handleDragEnd}>
        <SortableContext items={gridKeys} strategy={horizontalListSortingStrategy}>
          <div className="flex min-w-0 flex-1 items-center gap-0.5 overflow-x-auto">
            {grids.map((g) => (
              <SortableTabItem
                key={g.key}
                g={g}
                total={grids.length}
                activeKey={activeKey}
                editingKey={editingKey}
                onSelect={onSelect}
                onSetEditingKey={setEditingKey}
                onRename={rename}
                onSetPrimary={setPrimary}
                onRemove={remove}
              />
            ))}
          </div>
        </SortableContext>
      </DndContext>
      <button type="button" className="btn shrink-0" onClick={addTab}>
        <Icon name="plus" size={12} /> Tab
      </button>
    </div>
  );
}
