'use client';

/**
 * Toolbar for TreeDndMasterPage — header, tree filter selects, search,
 * reload + create buttons. Atomic tier: organism (co-located sibling of
 * tree-dnd-master-page.tsx). Pure presentational; parent owns all state
 * + option derivation.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { tGlobal } from '@/lib/mock';
import type { TreeRow, TreeFilterConfig } from './tree-dnd-master-page.types';

export interface TreeDndMasterToolbarProps<T extends TreeRow> {
  title: string;
  treeFilters?: TreeFilterConfig[];
  filterSel: string[];
  filterOptions: T[][];
  search: string;
  searchRef: React.RefObject<HTMLInputElement | null>;
  onFilterAt: (i: number, value: string) => void;
  onSearchChange: (value: string) => void;
  onReload: () => void;
  onAdd: () => void;
}

export function TreeDndMasterToolbar<T extends TreeRow>({
  title,
  treeFilters,
  filterSel,
  filterOptions,
  search,
  searchRef,
  onFilterAt,
  onSearchChange,
  onReload,
  onAdd,
}: TreeDndMasterToolbarProps<T>) {
  return (
    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 12, padding: '12px 12px 0' }}>
      <h2 style={{ fontSize: 'calc(15px * var(--font-scale, 1))', fontWeight: 600 }}>{tGlobal(title)}</h2>
      <div style={{ display: 'flex', gap: 8 }}>
        {treeFilters?.map((cfg, i) => {
          const label = cfg.label ?? cfg.type;
          return (
            <Select
              key={cfg.type + i}
              value={filterSel[i] || '__ALL__'}
              onValueChange={(v) => onFilterAt(i, v === '__ALL__' ? '' : v)}
            >
              <SelectTrigger aria-label={tGlobal(label)} style={{ width: 180 }}>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="__ALL__">
                  {tGlobal('Semua')} {tGlobal(label)}
                </SelectItem>
                {filterOptions[i]?.map((n) => (
                  <SelectItem key={n.id} value={n.id}>
                    {n.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          );
        })}
        <Input ref={searchRef} value={search} onChange={(e) => onSearchChange(e.target.value)} placeholder={tGlobal('Cari…')} style={{ width: 220 }} />
        <button className="btn ghost" onClick={onReload} title={tGlobal('Muat ulang')}><Icon name="refresh" /></button>
        <button className="btn primary" onClick={onAdd}><Icon name="plus" /> {tGlobal('Tambah')}</button>
      </div>
    </div>
  );
}