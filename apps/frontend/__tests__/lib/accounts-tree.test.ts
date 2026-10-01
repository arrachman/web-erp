import { describe, expect, it } from 'vitest';
import {
  ancestorIds,
  childrenOf,
  defaultExpandedIds,
  filterAccountTreeIds,
  flattenAccountTree,
} from '@/lib/accounts-tree';

type N = { id: string; code: string; name: string; parentId: string | null };

const sample: N[] = [
  { id: '1', code: '1000', name: 'Asset', parentId: null },
  { id: '2', code: '1100', name: 'Current', parentId: '1' },
  { id: '3', code: '1110', name: 'Cash', parentId: '2' },
  { id: '4', code: '2000', name: 'Liability', parentId: null },
  { id: '5', code: '2100', name: 'Payable', parentId: '4' },
];

describe('accounts-tree', () => {
  it('lists children sorted by code', () => {
    expect(childrenOf(sample, null).map((r) => r.code)).toEqual(['1000', '2000']);
    expect(childrenOf(sample, '1').map((r) => r.code)).toEqual(['1100']);
  });

  it('flattens full tree with depth and hasChildren', () => {
    const out: { row: N; depth: number; hasChildren: boolean }[] = [];
    flattenAccountTree(sample, null, 0, out);
    expect(out.map((f) => [f.row.code, f.depth, f.hasChildren])).toEqual([
      ['1000', 0, true],
      ['1100', 1, true],
      ['1110', 2, false],
      ['2000', 0, true],
      ['2100', 1, false],
    ]);
  });

  it('hides children of collapsed parents', () => {
    const out: { row: N; depth: number; hasChildren: boolean }[] = [];
    // only root 1000 expanded — 2000 collapsed, 1100 not expanded
    flattenAccountTree(sample, null, 0, out, new Set(['1']));
    expect(out.map((f) => f.row.code)).toEqual(['1000', '1100', '2000']);
  });

  it('defaultExpandedIds is every parent that has children', () => {
    expect([...defaultExpandedIds(sample)].sort()).toEqual(['1', '2', '4']);
  });

  it('filter keeps matches + ancestors', () => {
    const keep = filterAccountTreeIds(sample, 'cash');
    expect([...keep].sort()).toEqual(['1', '2', '3']);
  });

  it('ancestorIds walks parent chain', () => {
    expect(ancestorIds(sample, '3')).toEqual(['2', '1']);
    expect(ancestorIds(sample, '1')).toEqual([]);
  });
});
