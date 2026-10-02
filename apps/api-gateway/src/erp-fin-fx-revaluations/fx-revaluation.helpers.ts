import { BadRequestException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import type { LedgerLeg } from '../erp-inv-gl/inv-gl-posting.helpers';

const ZERO = new Prisma.Decimal(0);
const IDR_SCALE = 4;

/** Posted ledger totals for one foreign-currency account (debit-credit already netted by caller). */
export interface FxAccountBalance {
  accountId: bigint;
  currencyId: bigint;
  /** Net balance in foreign currency (debitFx - creditFx). Positive = debit-side. */
  balanceFx: Prisma.Decimal;
  /** Net book balance in IDR (debit - credit). Positive = debit-side. */
  balanceIdr: Prisma.Decimal;
}

export interface FxRevaluationLine {
  accountId: bigint;
  currencyId: bigint;
  bookBalanceFx: Prisma.Decimal;
  bookBalanceIdr: Prisma.Decimal;
  revaluationRate: Prisma.Decimal;
  revaluedBalanceIdr: Prisma.Decimal;
  /** revalued - book. Positive = increase in debit-side balance. */
  adjustmentIdr: Prisma.Decimal;
  /** P&L effect; equals adjustmentIdr because balances are debit-signed (liability owed more = loss). */
  gainLossIdr: Prisma.Decimal;
  lineNo: number;
}

export interface FxRevaluationResult {
  lines: FxRevaluationLine[];
  totalGainLoss: Prisma.Decimal;
}

/**
 * Compute unrealized FX gain/loss per account at the period-end rate.
 * Balances are debit-signed (liabilities negative), so the adjustment is itself
 * the P&L effect: positive = gain, negative = loss. Accounts whose adjustment
 * is zero are skipped. A missing rate for any currency with a balance is an
 * error — never silently revalue at 1.
 */
export function computeFxRevaluation(
  balances: FxAccountBalance[],
  rates: Map<string, Prisma.Decimal>,
): FxRevaluationResult {
  const lines: FxRevaluationLine[] = [];
  let total = ZERO;

  for (const b of balances) {
    if (b.balanceFx.isZero() && b.balanceIdr.isZero()) continue;
    const rate = rates.get(b.currencyId.toString());
    if (!rate || rate.lessThanOrEqualTo(0)) {
      throw new BadRequestException(
        `Kurs periode belum diisi untuk mata uang id ${b.currencyId}; revaluasi dibatalkan.`,
      );
    }
    const revalued = b.balanceFx.mul(rate).toDecimalPlaces(IDR_SCALE);
    const adjustment = revalued.sub(b.balanceIdr);
    if (adjustment.isZero()) continue;
    const gainLoss = adjustment;
    total = total.add(gainLoss);
    lines.push({
      accountId: b.accountId,
      currencyId: b.currencyId,
      bookBalanceFx: b.balanceFx,
      bookBalanceIdr: b.balanceIdr,
      revaluationRate: rate,
      revaluedBalanceIdr: revalued,
      adjustmentIdr: adjustment,
      gainLossIdr: gainLoss,
      lineNo: lines.length + 1,
    });
  }
  return { lines, totalGainLoss: total };
}

/**
 * Balanced journal legs for a revaluation: each account is adjusted by its
 * delta, and the net P&L effect goes to the gain account (credit) or loss
 * account (debit). Throws if there is nothing to post.
 */
export function buildRevaluationLegs(
  result: FxRevaluationResult,
  gainAccountId: bigint,
  lossAccountId: bigint,
): LedgerLeg[] {
  if (!result.lines.length) {
    throw new BadRequestException('Tidak ada selisih kurs untuk direvaluasi pada periode ini.');
  }
  const legs: LedgerLeg[] = result.lines.map((l) => ({
    accountId: l.accountId,
    debit: l.adjustmentIdr.greaterThan(0) ? l.adjustmentIdr : ZERO,
    credit: l.adjustmentIdr.lessThan(0) ? l.adjustmentIdr.abs() : ZERO,
    description: 'Revaluasi valas',
  }));
  const net = result.totalGainLoss;
  if (net.greaterThan(0)) {
    legs.push({ accountId: gainAccountId, debit: ZERO, credit: net, description: 'Laba selisih kurs belum terealisasi' });
  } else if (net.lessThan(0)) {
    legs.push({ accountId: lossAccountId, debit: net.abs(), credit: ZERO, description: 'Rugi selisih kurs belum terealisasi' });
  }
  return legs;
}

/** Swap debit/credit of every leg — the auto-reversal posted on day 1 of the next period. */
export function reverseLegs(legs: LedgerLeg[]): LedgerLeg[] {
  return legs.map((l) => ({
    ...l,
    debit: l.credit,
    credit: l.debit,
    description: `Reversal: ${l.description ?? 'Revaluasi valas'}`,
  }));
}
