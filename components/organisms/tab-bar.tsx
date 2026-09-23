'use client';

import * as React from 'react';
import {
  DndContext,
  PointerSensor,
  KeyboardSensor,
  useSensor,
  useSensors,
  closestCenter,
  type DragEndEvent,
} from '@dnd-kit/core';
import {
  SortableContext,
  horizontalListSortingStrategy,
  useSortable,
  sortableKeyboardCoordinates,
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { cn } from '@/lib/utils';
import { Icon } from '@/components/ui/icons';
import { pageMeta } from '@/lib/nav';

export interface ShellTab {
  id: string;
  route: string;
  /** Bumped to force-remount the tab's view (reload action). */
  nonce?: number;
}

interface TabBarProps {
  tabs: ShellTab[];
  activeId: string;
  onActivate: (id: string) => void;
  onClose: (id: string) => void;
  onReload: (id: string) => void;
  onCloseOthers: (id: string) => void;
  onCloseRight: (id: string) => void;
  onDuplicate: (id: string) => void;
  onNew: () => void;
  /** Called when user drag-drops a tab onto another tab's slot. */
  onReorder: (fromId: string, toId: string) => void;
  t: (key: string) => string;
}

interface SortableTabChipProps {
  tab: ShellTab;
  active: boolean;
  t: (key: string) => string;
  onActivate: (id: string) => void;
  onClose: (id: string) => void;
  onContextMenu: (e: React.MouseEvent, id: string) => void;
}

function SortableTabChip({
  tab,
  active,
  t,
  onActivate,
  onClose,
  onContextMenu,
}: SortableTabChipProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: tab.id });
  const meta = pageMeta(tab.route, t);
  const style: React.CSSProperties = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : undefined,
    zIndex: isDragging ? 10 : undefined,
  };
  return (
    <div
      ref={setNodeRef}
      style={style}
      data-tab={tab.id}
      className={cn('tab-chip', active && 'active', isDragging && 'dragging')}
      title={meta.crumbs.map((c) => c.label).join(' / ')}
      onClick={() => onActivate(tab.id)}
      onContextMenu={(e) => onContextMenu(e, tab.id)}
      onAuxClick={(e) => {
        if (e.button === 1) {
          e.preventDefault();
          onClose(tab.id);
        }
      }}
      {...attributes}
      {...listeners}
    >
      <Icon name={meta.icon} size={13} className="tab-ico" />
      <span className="tab-label">{meta.title}</span>
      {meta.code && <span className="tab-code">{meta.code}</span>}
      <span
        className="tab-x"
        title={active ? `${t('Tutup tab')} (⌘E)` : t('Tutup tab')}
        onPointerDown={(e) => e.stopPropagation()}
        onClick={(e) => {
          e.stopPropagation();
          onClose(tab.id);
        }}
        onAuxClick={(e) => e.stopPropagation()}
      >
        <Icon name="x" size={10} />
      </span>
    </div>
  );
}

/** Browser-style tab strip — ported from `tabs.jsx`. */
export function TabBar({
  tabs,
  activeId,
  onActivate,
  onClose,
  onReload,
  onCloseOthers,
  onCloseRight,
  onDuplicate,
  onNew,
  onReorder,
  t,
}: TabBarProps) {
  const stripRef = React.useRef<HTMLDivElement>(null);
  const [menu, setMenu] = React.useState<{
    id: string;
    x: number;
    y: number;
  } | null>(null);

  React.useEffect(() => {
    const el = stripRef.current?.querySelector(`[data-tab="${activeId}"]`);
    if (el) el.scrollIntoView({ inline: 'nearest', block: 'nearest' });
  }, [activeId, tabs.length]);

  React.useEffect(() => {
    if (!menu) return;
    const close = () => setMenu(null);
    window.addEventListener('click', close);
    window.addEventListener('resize', close);
    window.addEventListener('keydown', close);
    return () => {
      window.removeEventListener('click', close);
      window.removeEventListener('resize', close);
      window.removeEventListener('keydown', close);
    };
  }, [menu]);

  const openMenu = (e: React.MouseEvent, id: string) => {
    e.preventDefault();
    e.stopPropagation();
    setMenu({ id, x: e.clientX, y: e.clientY });
  };

  const run = (fn: () => void) => (e: React.MouseEvent) => {
    e.stopPropagation();
    fn();
    setMenu(null);
  };

  const menuIdx = menu ? tabs.findIndex((tb) => tb.id === menu.id) : -1;
  const hasOthers = tabs.length > 1;
  const hasRight = menuIdx > -1 && menuIdx < tabs.length - 1;

  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 5 } }),
    useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates }),
  );

  const handleDragEnd = (e: DragEndEvent) => {
    const { active, over } = e;
    if (!over || active.id === over.id) return;
    onReorder(String(active.id), String(over.id));
  };

  const tabIds = React.useMemo(() => tabs.map((tb) => tb.id), [tabs]);

  return (
    <div className="tabstrip" ref={stripRef}>
      <DndContext
        sensors={sensors}
        collisionDetection={closestCenter}
        onDragEnd={handleDragEnd}
      >
        <SortableContext items={tabIds} strategy={horizontalListSortingStrategy}>
          {tabs.map((tab) => (
            <SortableTabChip
              key={tab.id}
              tab={tab}
              active={tab.id === activeId}
              t={t}
              onActivate={onActivate}
              onClose={onClose}
              onContextMenu={openMenu}
            />
          ))}
        </SortableContext>
      </DndContext>
      <button
        className="tab-new"
        title={`${t('Tab baru')} (⌘K)`}
        onClick={onNew}
      >
        <Icon name="plus" size={13} />
      </button>
      <div style={{ flex: 1 }} />
      {tabs.length > 0 && (
        <button
          className="tab-new"
          title={t('Duplikat tab')}
          onClick={() => onDuplicate(activeId)}
        >
          <Icon name="boxes" size={12} />
        </button>
      )}
      <span className="tab-count">{tabs.length} tab</span>

      {menu && (
        <div
          className="tab-ctx"
          style={{ left: menu.x, top: menu.y }}
          onClick={(e) => e.stopPropagation()}
          onContextMenu={(e) => e.preventDefault()}
        >
          <button
            className="tab-ctx-item"
            onClick={run(() => onReload(menu.id))}
          >
            <Icon name="refresh" size={13} />
            <span>{t('Muat ulang')}</span>
          </button>
          <div className="tab-ctx-sep" />
          <button
            className="tab-ctx-item"
            onClick={run(() => onClose(menu.id))}
          >
            <Icon name="x" size={13} />
            <span>{t('Tutup')}</span>
          </button>
          <button
            className="tab-ctx-item"
            disabled={!hasOthers}
            onClick={hasOthers ? run(() => onCloseOthers(menu.id)) : undefined}
          >
            <Icon name="trash" size={13} />
            <span>{t('Tutup tab lain')}</span>
          </button>
          <button
            className="tab-ctx-item"
            disabled={!hasRight}
            onClick={hasRight ? run(() => onCloseRight(menu.id)) : undefined}
          >
            <Icon name="chevright" size={13} />
            <span>{t('Tutup tab di kanan')}</span>
          </button>
        </div>
      )}
    </div>
  );
}
