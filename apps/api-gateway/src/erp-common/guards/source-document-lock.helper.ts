import { BadRequestException } from '@nestjs/common';
import { Prisma } from '@prisma/client';

/**
 * Generic "cannot cancel/reopen a document already used as another
 * document's source" guard — DECISIONS.md § Plan Fase 0, "Aturan
 * pembatalan": "Dokumen yang sudah dipakai sebagai sumber dokumen lain
 * tidak bisa dibatalkan sebelum dokumen turunannya dibatalkan."
 *
 * Call this at the top of a REOPEN/VOID/CANCELLED transition, before any
 * reversal happens. Each entry in `derivedChecks` is one potential
 * downstream document type that could reference this document as its
 * source; the guard fails fast on the first one that still has an active
 * (non VOID/non CANCELLED, non soft-deleted) row pointing at it.
 *
 * This is intentionally a thin, explicit list rather than introspecting
 * Prisma relations at runtime — the FK fields that carry "this document is
 * my source" vary per pair (orderId, deliveryOrderId, goodsReceiptId,
 * invoiceId, sourceLineId...) and are not uniformly named, so each call
 * site states its own downstream checks rather than the helper guessing.
 */
export interface DerivedDocumentCheck {
  /** Human label for the error message, e.g. "Delivery Order", "Sales Invoice". */
  label: string;
  /** Prisma delegate for the downstream model, e.g. tx.erpSlsDeliveryOrder. */
  findFirst: (args: {
    where: Prisma.JsonObject | Record<string, unknown>;
    select: { id: true; docNumber: true };
  }) => Promise<{ id: bigint; docNumber: string } | null>;
  /** Where clause matching "points at this source document" + active status. */
  where: Record<string, unknown>;
}

export async function assertNoActiveDerivedDocuments(
  checks: DerivedDocumentCheck[],
): Promise<void> {
  for (const check of checks) {
    const found = await check.findFirst({
      where: check.where,
      select: { id: true, docNumber: true },
    });
    if (found) {
      throw new BadRequestException(
        `Dokumen tidak bisa dibatalkan/reopen: sudah ditarik oleh ${check.label} ${found.docNumber} yang masih aktif. Batalkan dokumen itu dulu.`,
      );
    }
  }
}

/** Status values that mean "this derived document no longer counts as active". */
export const INACTIVE_STATUSES = ['VOID', 'CANCELLED', 'REJECTED'] as const;
