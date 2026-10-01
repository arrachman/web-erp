/**
 * Pure helpers for tree-dnd-master-page organism. No React imports.
 * Keeps the organism file under the 400-line cap (CLAUDE.md §3).
 */

import type { TreeRow, TreeNodeType } from './tree-dnd-master-page.types';

export function flattenTree<T extends TreeRow>(
  all: T[],
  parentId: string | null,
  depth: number,
  out: { row: T; depth: number }[],
): void {
  const siblings = all
    .filter((r) => (r.parentId ?? null) === parentId)
    .sort((a, b) => a.sortOrder - b.sortOrder || a.code.localeCompare(b.code));
  for (const s of siblings) {
    out.push({ row: s, depth });
    flattenTree(all, s.id, depth + 1, out);
  }
}

/**
 * Cross-parent drop rule: the dragged item's new parent is derived from
 * the row immediately above it in the post-arrayMove flat order.
 * - MODULE always becomes root (null).
 * - GROUP nests under nearest MODULE walking upwards.
 * - ITEM becomes child of nearest MODULE/GROUP container, or sibling
 *   of nearest ITEM (inheriting that item's parent).
 */
export function inferNewParent<T extends TreeRow>(
  ordered: T[],
  movedIdx: number,
): string | null {
  const moved = ordered[movedIdx];
  if (moved.type === 'MODULE') return null;
  for (let i = movedIdx - 1; i >= 0; i--) {
    const prev = ordered[i];
    if (moved.type === 'GROUP') {
      if (prev.type === 'MODULE') return prev.id;
      if (prev.type === 'GROUP') return prev.parentId;
      continue;
    }
    if (prev.type === 'MODULE' || prev.type === 'GROUP') return prev.id;
    return prev.parentId;
  }
  return null;
}

export interface ReorderChange {
  id: string;
  parentId: string | null;
  sortOrder: number;
}

/**
 * Compute the minimal set of changes (parentId, sortOrder) needed to
 * transform `original` into the post-move flat order. Returns only rows
 * that actually changed parent or sortOrder.
 */
export function computeReorderChanges<T extends TreeRow>(
  original: T[],
  reorderedFlat: T[],
  movedId: string,
  newParentId: string | null,
): ReorderChange[] {
  const projected = reorderedFlat.map((r) =>
    r.id === movedId ? ({ ...r, parentId: newParentId } as T) : r,
  );

  const byParent = new Map<string | null, T[]>();
  for (const r of projected) {
    const pid = r.parentId ?? null;
    const arr = byParent.get(pid) ?? [];
    arr.push(r);
    byParent.set(pid, arr);
  }

  const changes: ReorderChange[] = [];
  for (const [pid, siblings] of byParent) {
    siblings.forEach((s, i) => {
      const orig = original.find((r) => r.id === s.id);
      if (!orig) return;
      const samePid = (orig.parentId ?? null) === pid;
      if (!samePid || orig.sortOrder !== i) {
        changes.push({ id: s.id, parentId: pid, sortOrder: i });
      }
    });
  }
  return changes;
}

/**
 * Validate the prospective drop against MODULE/GROUP/ITEM hierarchy.
 * Returns an error message or null when valid.
 */
export function validateDrop<T extends TreeRow>(
  reorderedFlat: T[],
  movedId: string,
  newParentId: string | null,
): string | null {
  const moved = reorderedFlat.find((r) => r.id === movedId);
  if (!moved) return 'Item tidak ditemukan';
  if (moved.type === 'MODULE' && newParentId !== null) {
    return 'MODULE harus berada di level tertinggi';
  }
  if (moved.type === 'GROUP') {
    const parent = reorderedFlat.find((r) => r.id === newParentId);
    if (!parent || parent.type !== 'MODULE') {
      return 'GROUP hanya bisa berada di bawah MODULE';
    }
  }
  if (moved.type === 'ITEM') {
    if (!newParentId) return 'ITEM harus berada di bawah MODULE atau GROUP';
    const parent = reorderedFlat.find((r) => r.id === newParentId);
    if (!parent || (parent.type !== 'MODULE' && parent.type !== 'GROUP')) {
      return 'ITEM hanya bisa berada di bawah MODULE atau GROUP';
    }
  }
  return null;
}

export type { TreeRow, TreeNodeType };
