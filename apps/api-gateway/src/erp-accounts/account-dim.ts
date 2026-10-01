/** Build junction rows for one multi-select CoA dimension (md_account_dim_*). */
export function buildAccountDimRows<K extends string>(
  ids: string[] | undefined,
  key: K,
): Record<K, bigint>[] | undefined {
  if (!ids) return undefined;
  const unique = Array.from(new Set(ids.filter((v) => v !== '')));
  return unique.map((v) => ({ [key]: BigInt(v) }) as Record<K, bigint>);
}

export const ACCOUNT_DIM_INCLUDE = {
  dimBranches: {
    select: { branchId: true, branch: { select: { id: true, code: true, name: true } } },
    orderBy: { id: 'asc' as const },
  },
  dimLocations: {
    select: { locationId: true, location: { select: { id: true, code: true, name: true } } },
    orderBy: { id: 'asc' as const },
  },
  dimDivisions: {
    select: { divisionId: true, division: { select: { id: true, code: true, name: true } } },
    orderBy: { id: 'asc' as const },
  },
  bank: { select: { id: true, code: true, name: true } },
  currency: { select: { id: true, code: true, name: true, symbol: true } },
  parent: { select: { id: true, code: true, name: true } },
} as const;
