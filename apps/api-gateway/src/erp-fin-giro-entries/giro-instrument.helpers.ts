import { BadRequestException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { GiroEntryRowDto } from './dto/create-giro-entry.dto';
import { toBigInt } from './giro-entry.helpers';

interface RegisterGiroContext {
  rows: GiroEntryRowDto[];
  type: string;
  partnerId?: string;
  branchId: string;
  currencyId: string;
  exchangeRate: string;
  giroAccountId?: string;
  fiscalPeriodId: bigint;
}

/** Build fin_giros create rows for a REGISTER header (validates required fields). */
export function buildRegisterGiros(ctx: RegisterGiroContext) {
  return ctx.rows.map((r, i) => {
    if (!r.giroNumber?.trim()) {
      throw new BadRequestException(`Baris ${i + 1}: nomor giro wajib diisi.`);
    }
    if (!r.dueDate) {
      throw new BadRequestException(`Baris ${i + 1}: jatuh tempo wajib diisi.`);
    }
    if (r.amount === undefined || r.amount === null || r.amount === '') {
      throw new BadRequestException(`Baris ${i + 1}: nominal wajib diisi.`);
    }
    const giroAccountId = r.giroAccountId ?? ctx.giroAccountId;
    return {
      giroNumber: r.giroNumber.trim(),
      type: ctx.type as never,
      status: 'OUTSTANDING' as never,
      partnerId: toBigInt(ctx.partnerId),
      branchId: BigInt(ctx.branchId),
      fiscalPeriodId: ctx.fiscalPeriodId,
      bankName: r.bankName ?? null,
      giroAccountId: toBigInt(giroAccountId),
      currencyId: BigInt(ctx.currencyId),
      exchangeRate: new Prisma.Decimal(ctx.exchangeRate),
      amount: new Prisma.Decimal(r.amount),
      dueDate: new Date(r.dueDate),
      notes: r.notes ?? null,
      lineNo: i + 1,
    };
  });
}

/**
 * Validate CLEAR rows reference outstanding, unlinked giros of the right type,
 * then link them (clearedByEntryId + clearedDate). Status stays OUTSTANDING.
 */
export async function linkClearGiros(
  tx: Prisma.TransactionClient,
  rows: GiroEntryRowDto[],
  header: { id: bigint; type: string },
) {
  for (const [i, r] of rows.entries()) {
    if (!r.giroId) throw new BadRequestException(`Baris ${i + 1}: giroId wajib untuk kliring.`);
    if (!r.clearedDate) {
      throw new BadRequestException(`Baris ${i + 1}: tanggal kliring wajib diisi.`);
    }
    const giro = await tx.erpFinGiro.findFirst({
      where: { id: BigInt(r.giroId), deletedAt: null },
    });
    if (!giro) throw new BadRequestException(`Baris ${i + 1}: giro tidak ditemukan.`);
    if (giro.status !== 'OUTSTANDING') {
      throw new BadRequestException(`Giro ${giro.giroNumber} tidak berstatus OUTSTANDING.`);
    }
    if (giro.type !== header.type) {
      throw new BadRequestException(`Giro ${giro.giroNumber} tipe-nya tidak cocok (${giro.type}).`);
    }
    if (giro.clearedByEntryId !== null && giro.clearedByEntryId !== header.id) {
      throw new BadRequestException(`Giro ${giro.giroNumber} sudah dikliring entri lain.`);
    }
    await tx.erpFinGiro.update({
      where: { id: giro.id },
      data: { clearedByEntryId: header.id, clearedDate: new Date(r.clearedDate) },
    });
  }
}

/** Unlink any giros currently pointing at this CLEAR header. */
export async function unlinkClearGiros(tx: Prisma.TransactionClient, entryId: bigint) {
  await tx.erpFinGiro.updateMany({
    where: { clearedByEntryId: entryId },
    data: { clearedByEntryId: null, clearedDate: null },
  });
}
