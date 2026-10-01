import { BadRequestException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';

/**
 * Accepts either the root PrismaService or a transaction client — the setting
 * readers only use `.erpSetting`, present on both. Lets posting seams pass their
 * `tx` directly without casting.
 */
type PrismaLike = PrismaService | Prisma.TransactionClient;

/**
 * Pure building blocks for inventory → fin_ledger_entries posting. No NestJS
 * provider here — these are consumed by the (future) inventory posting wiring.
 * Mirrors the balanced double-entry shape of CashBankPostingService.
 */

/** `source` column tag for all inventory-originated ledger rows. */
export const INV_GL_SOURCE = 'INVENTORY';

/** `source_doc_type` discriminators per inventory document family. */
export const INV_GL_DOCTYPE = {
  adjustment: 'inv_stock_adjustments',
  opening: 'inv_opening_stocks',
  movement: 'inv_stock_movements',
} as const;

export type InvGlDocType = (typeof INV_GL_DOCTYPE)[keyof typeof INV_GL_DOCTYPE];

/**
 * Read an inventory `sys_settings` (module='inventory', group='accounts') row by
 * key. Mirrors the exact query shape used in inv-stock-adjustment.helpers.ts
 * (prisma.erpSetting with the `module_group_key` compound unique).
 */
async function readRawSetting(
  prisma: PrismaLike,
  key: string,
): Promise<string | null> {
  const row = await prisma.erpSetting.findUnique({
    where: { module_group_key: { module: 'inventory', group: 'accounts', key } },
    select: { value: true },
  });
  return row?.value ?? null;
}

/**
 * Whether inventory GL posting is switched on. Reads
 * module='inventory'/group='accounts'/key='glPostingEnabled'. Returns true ONLY
 * when the stored value is the string 'true' (or boolean true). Defaults false
 * when the row is absent — posting stays a no-op until explicitly enabled.
 */
export async function glPostingEnabled(prisma: PrismaLike): Promise<boolean> {
  const value = await readRawSetting(prisma, 'glPostingEnabled');
  return value === 'true' || (value as unknown) === true;
}

/**
 * Generic reader for an account-id inventory setting (module='inventory',
 * group='accounts'). Parses the stored value → BigInt, or null when absent /
 * unparseable. Use for 'defaultOpeningEquityAccountId', 'defaultCogsAccountId',
 * 'defaultInventoryAccountId', etc.
 */
export async function readInvSetting(
  prisma: PrismaLike,
  key: string,
): Promise<bigint | null> {
  const value = await readRawSetting(prisma, key);
  if (!value) return null;
  try {
    return BigInt(value);
  } catch {
    return null;
  }
}

/** One side of a balanced inventory journal. */
export interface LedgerLeg {
  accountId: bigint;
  debit: Prisma.Decimal;
  credit: Prisma.Decimal;
  description?: string | null;
  partnerId?: bigint | null;
  costCenterId?: bigint | null;
  divisionId?: bigint | null;
  subdivisionId?: bigint | null;
  projectId?: bigint | null;
}

/**
 * Shared header context for every leg of one inventory journal — mirrors the
 * `base` object built by CashBankPostingService and the NOT NULL columns of
 * ErpFinLedgerEntry.
 */
export interface LedgerBase {
  branchId: bigint;
  locationId: bigint | null;
  sourceDocType: InvGlDocType | string;
  sourceId: bigint;
  docNumber: string;
  entryDate: Date;
  fiscalPeriodId: bigint;
  currencyId: bigint;
  exchangeRate: Prisma.Decimal;
  actorId: bigint | null;
}

/**
 * Assemble balanced ledger rows for one inventory journal. Mirrors cash-bank's
 * `base` shape: source=INV_GL_SOURCE, status/postingStatus='POSTED',
 * postedAt=now, reconciliationStatus='UNRECONCILED', createdById/updatedById
 * from actorId, lineNo = index+1.
 *
 * Before returning, ASSERTS the journal is balanced: sum(debit) === sum(credit)
 * (Prisma.Decimal compare) and total > 0. Throws BadRequestException otherwise,
 * guaranteeing no unbalanced inventory journal ever reaches the ledger.
 */
export function buildLedgerRows(
  base: LedgerBase,
  legs: LedgerLeg[],
): Prisma.ErpFinLedgerEntryCreateManyInput[] {
  if (!legs.length) {
    throw new BadRequestException('Tidak bisa posting: tidak ada baris jurnal.');
  }

  const zero = new Prisma.Decimal(0);
  let totalDebit = zero;
  let totalCredit = zero;

  const rows: Prisma.ErpFinLedgerEntryCreateManyInput[] = legs.map((leg, i) => {
    totalDebit = totalDebit.add(leg.debit);
    totalCredit = totalCredit.add(leg.credit);
    return {
      branchId: base.branchId,
      locationId: base.locationId,
      source: INV_GL_SOURCE,
      sourceDocType: base.sourceDocType,
      sourceId: base.sourceId,
      docNumber: base.docNumber,
      entryDate: base.entryDate,
      fiscalPeriodId: base.fiscalPeriodId,
      currencyId: base.currencyId,
      exchangeRate: base.exchangeRate,
      accountId: leg.accountId,
      description: leg.description ?? null,
      partnerId: leg.partnerId ?? null,
      debit: leg.debit,
      credit: leg.credit,
      costCenterId: leg.costCenterId ?? null,
      divisionId: leg.divisionId ?? null,
      subdivisionId: leg.subdivisionId ?? null,
      projectId: leg.projectId ?? null,
      reconciliationStatus: 'UNRECONCILED',
      status: 'POSTED',
      postingStatus: 'POSTED',
      postedAt: new Date(),
      createdById: base.actorId,
      updatedById: base.actorId,
      lineNo: i + 1,
    };
  });

  if (!totalDebit.equals(totalCredit)) {
    throw new BadRequestException(
      `Jurnal persediaan tidak balance: debit (${totalDebit}) != kredit (${totalCredit}).`,
    );
  }
  if (totalDebit.lessThanOrEqualTo(0)) {
    throw new BadRequestException('Total jurnal persediaan harus lebih dari nol.');
  }

  return rows;
}

/**
 * Remove a document's posted inventory ledger rows (REOPEN / re-post). Mirrors
 * CashBankPostingService.reverseLedger — deleteMany by sourceDocType + sourceId.
 */
export async function reverseInvLedger(
  tx: Prisma.TransactionClient,
  sourceDocType: string,
  sourceId: bigint,
): Promise<void> {
  await tx.erpFinLedgerEntry.deleteMany({
    where: { sourceDocType, sourceId },
  });
}
