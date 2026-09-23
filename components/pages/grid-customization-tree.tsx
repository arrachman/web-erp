'use client';

/**
 * Left panel of Kustomisasi Grid: module → group → transaction tree.
 * Built from the flat transaction-type list (grouped client-side). Selectable.
 */

import * as React from 'react';
import { cn } from '@/lib/utils';
import type { ErpTransactionType } from '@/lib/api/transaction-grids';

interface ModuleNode {
  key: string;
  label: string;
  groups: { label: string; items: ErpTransactionType[] }[];
}

function buildTree(types: ErpTransactionType[]): ModuleNode[] {
  const modules = new Map<string, ModuleNode>();
  for (const t of types) {
    let mod = modules.get(t.moduleKey);
    if (!mod) {
      mod = { key: t.moduleKey, label: t.moduleLabel, groups: [] };
      modules.set(t.moduleKey, mod);
    }
    const groupLabel = t.groupLabel || 'Transaction';
    let grp = mod.groups.find((g) => g.label === groupLabel);
    if (!grp) { grp = { label: groupLabel, items: [] }; mod.groups.push(grp); }
    grp.items.push(t);
  }
  return [...modules.values()];
}

export function GridCustomizationTree({
  types,
  selectedCode,
  onSelect,
}: {
  types: ErpTransactionType[];
  selectedCode?: string;
  onSelect: (code: string) => void;
}) {
  const tree = React.useMemo(() => buildTree(types), [types]);
  const [collapsed, setCollapsed] = React.useState<Set<string>>(new Set());
  const toggle = (key: string) =>
    setCollapsed((prev) => {
      const next = new Set(prev);
      if (next.has(key)) next.delete(key); else next.add(key);
      return next;
    });

  return (
    <div className="flex h-full flex-col overflow-y-auto text-sm">
      {tree.map((mod) => {
        const isCollapsed = collapsed.has(mod.key);
        return (
          <div key={mod.key} className="border-b border-border">
            <button
              type="button"
              onClick={() => toggle(mod.key)}
              className="flex w-full items-center gap-1.5 bg-secondary px-3 py-1.5 text-left text-xs font-semibold text-foreground"
            >
              <span className="inline-block w-3 text-[10px] leading-none text-muted-foreground">
                {isCollapsed ? '▸' : '▾'}
              </span>
              {mod.label}
            </button>
            {!isCollapsed &&
              mod.groups.map((grp) => (
                <div key={grp.label}>
                  <div className="px-3 py-1 text-[11px] uppercase tracking-wide text-muted-foreground">
                    {grp.label}
                  </div>
                  {grp.items.map((t) => (
                    <button
                      key={t.code}
                      type="button"
                      onClick={() => onSelect(t.code)}
                      className={cn(
                        'flex w-full items-center gap-2 px-3 py-1.5 pl-7 text-left',
                        t.code === selectedCode
                          ? 'bg-[var(--primary-soft)] font-medium text-foreground'
                          : 'text-muted-foreground hover:bg-[var(--panel-hover)]',
                      )}
                    >
                      {t.name}
                    </button>
                  ))}
                </div>
              ))}
          </div>
        );
      })}
    </div>
  );
}
