/**
 * Normalisasi nomor telepon Indonesia untuk WhatsApp gateway.
 *
 * - Nomor Indonesia (08xx / 628xx / +628xx / 8xx) → format `62xxx` (tanpa +).
 * - Nomor internasional eksplisit lain (+1..., +44...) → dipertahankan `+xxx`.
 * Return null bila tidak bisa dinormalisasi.
 */
export function normalizePhoneId(raw?: string | null): string | null {
  if (!raw) return null;
  const trimmed = raw.trim();
  if (!trimmed) return null;
  const hasPlus = trimmed.startsWith('+');
  const digits = trimmed.replace(/\D/g, '');
  if (!digits) return null;
  if (hasPlus && !digits.startsWith('62')) return `+${digits}`;
  if (digits.startsWith('62')) return digits;
  if (digits.startsWith('0')) return `62${digits.slice(1)}`;
  if (digits.startsWith('8')) return `62${digits}`;
  return hasPlus ? `+${digits}` : digits;
}

/**
 * Varian bentuk nomor untuk lookup webhook (gateway kadang melaporkan sender
 * dengan/tanpa '+', atau format lokal 0xxx).
 */
export function phoneLookupVariants(raw?: string | null): string[] {
  const normalized = normalizePhoneId(raw);
  if (!normalized) return raw ? [raw] : [];
  const digits = normalized.replace(/\D/g, '');
  const variants = new Set<string>([normalized, digits, `+${digits}`]);
  if (digits.startsWith('62')) variants.add(`0${digits.slice(2)}`);
  return [...variants];
}
