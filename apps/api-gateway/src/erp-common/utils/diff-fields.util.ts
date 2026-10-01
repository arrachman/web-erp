const SKIP_FIELDS = new Set([
  'updatedAt', 'updatedById', 'createdAt', 'createdById', 'deletedAt',
]);

function serialize(v: unknown): unknown {
  if (v === null || v === undefined) return v;
  if (typeof v === 'bigint') return v.toString();
  if (v instanceof Date) return v.toISOString();
  // Prisma Decimal — has toFixed()
  if (typeof (v as Record<string, unknown>)['toFixed'] === 'function') {
    return String(v);
  }
  return v;
}

/**
 * Computes field-level diff between oldData and newData.
 * Returns only changed fields, serialized for JSON storage.
 * BigInt, Date, and Decimal values are converted to strings.
 */
export function diffFields<T extends Record<string, unknown>>(
  oldData: T,
  newData: T,
  skip: ReadonlySet<string> = SKIP_FIELDS,
): Record<string, { from: unknown; to: unknown }> {
  const changes: Record<string, { from: unknown; to: unknown }> = {};

  const keys = new Set([...Object.keys(oldData), ...Object.keys(newData)]);
  for (const key of keys) {
    if (skip.has(key)) continue;
    const from = serialize(oldData[key]);
    const to = serialize(newData[key]);
    if (from !== to && !(from == null && to == null)) {
      changes[key] = { from, to };
    }
  }

  return changes;
}
