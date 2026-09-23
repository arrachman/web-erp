/**
 * Client-side auto-code generator untuk item master.
 *
 * Bukan transactional — ambil item terakhir dengan prefix matching dari list
 * API lalu increment. Race condition kalau dua user generate bersamaan;
 * backend tetap validasi unique constraint. Untuk transactional sequence
 * pakai `sys_document_numberings` (belum dibikin endpoint-nya).
 */

import { listItems } from '@/lib/api/items';
import type { ErpItemType } from '@/lib/api/items';

const PREFIX_BY_TYPE: Record<ErpItemType, string> = {
  INVENTORY: 'ITM',
  CONSUMABLE: 'CNS',
  ASSET: 'AST',
  SERVICE: 'SVC',
  NON_INVENTORY: 'NIN',
};

const PAD = 4;

export function codePrefix(itemType: ErpItemType): string {
  return PREFIX_BY_TYPE[itemType] ?? 'ITM';
}

export function nextCodePreview(itemType: ErpItemType): string {
  return `${codePrefix(itemType)}-${'0'.repeat(PAD - 1)}1`;
}

/**
 * Fetch existing items with matching prefix, find max sequence, return next.
 * Returns prefix-only placeholder on error so caller can still display
 * something meaningful.
 */
export async function generateNextItemCode(itemType: ErpItemType): Promise<string> {
  const prefix = codePrefix(itemType);
  try {
    const res = await listItems({
      page: 1,
      limit: 100,
      search: prefix,
      sortBy: 'code',
      sortDir: 'desc',
    });
    let maxSeq = 0;
    const re = new RegExp(`^${prefix}-(\\d+)$`);
    for (const item of res.data) {
      const match = re.exec(item.code);
      if (match) {
        const n = parseInt(match[1], 10);
        if (Number.isFinite(n) && n > maxSeq) maxSeq = n;
      }
    }
    const nextSeq = maxSeq + 1;
    return `${prefix}-${String(nextSeq).padStart(PAD, '0')}`;
  } catch {
    return nextCodePreview(itemType);
  }
}
