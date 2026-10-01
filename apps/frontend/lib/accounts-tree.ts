/**
 * Pure helpers for Chart of Accounts hierarchical list (expand/collapse).
 * Flat parentId tree → depth-ordered visible rows. No React.
 */

export interface AccountTreeNode {
  id: string;
  code: string;
  name: string;
  /** Optional on API payloads; treat missing as root. */
  parentId?: string | null;
}

export interface FlatAccountRow<T extends AccountTreeNode> {
  row: T;
  depth: number;
  hasChildren: boolean;
}

/** Direct children of `parentId`, sorted by code. */
export function childrenOf<T extends AccountTreeNode>(
  all: T[],
  parentId: string | null,
): T[] {
  return all
    .filter((r) => (r.parentId ?? null) === parentId)
    .sort((a, b) => a.code.localeCompare(b.code, undefined, { numeric: true }));
}

/**
 * Depth-first flatten. When `expanded` is provided, only include children of
 * expanded parents (roots always shown). Omit `expanded` to show full tree.
 */
export function flattenAccountTree<T extends AccountTreeNode>(
  all: T[],
  parentId: string | null,
  depth: number,
  out: FlatAccountRow<T>[],
  expanded?: Set<string>,
): void {
  const kids = childrenOf(all, parentId);
  for (const row of kids) {
    const hasChildren = all.some((r) => (r.parentId ?? null) === row.id);
    out.push({ row, depth, hasChildren });
    if (hasChildren && (!expanded || expanded.has(row.id))) {
      flattenAccountTree(all, row.id, depth + 1, out, expanded);
    }
  }
}

/** All ancestor ids of `id` (not including self), walking parentId. */
export function ancestorIds<T extends AccountTreeNode>(
  all: T[],
  id: string,
): string[] {
  const byId = new Map(all.map((r) => [r.id, r]));
  const out: string[] = [];
  let cur = byId.get(id);
  const seen = new Set<string>();
  while (cur?.parentId && !seen.has(cur.parentId)) {
    seen.add(cur.parentId);
    out.push(cur.parentId);
    cur = byId.get(cur.parentId);
  }
  return out;
}

/**
 * Filter tree by code/name query. Matching nodes + all ancestors are kept so
 * the hierarchy stays readable; expanded should include those ancestors.
 */
export function filterAccountTreeIds<T extends AccountTreeNode>(
  all: T[],
  query: string,
): Set<string> {
  const q = query.trim().toLowerCase();
  const keep = new Set<string>();
  if (!q) {
    all.forEach((r) => keep.add(r.id));
    return keep;
  }
  for (const r of all) {
    if (
      r.code.toLowerCase().includes(q) ||
      r.name.toLowerCase().includes(q)
    ) {
      keep.add(r.id);
      for (const a of ancestorIds(all, r.id)) keep.add(a);
    }
  }
  return keep;
}

/** Expand every node that has children (initial open tree). */
export function defaultExpandedIds<T extends AccountTreeNode>(all: T[]): Set<string> {
  const withKids = new Set<string>();
  for (const r of all) {
    if (r.parentId) withKids.add(r.parentId);
  }
  return withKids;
}
