export function toAuditUserId(actorId?: string | number | null): number | null {
  if (actorId === undefined || actorId === null) {
    return null;
  }
  const parsed = Number(String(actorId).trim());
  if (!Number.isInteger(parsed) || parsed <= 0) {
    return null;
  }
  return parsed;
}
