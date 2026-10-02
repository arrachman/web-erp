import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { assertLedgerRowsPeriodOpen } from '../erp-common/utils/ledger-period-guard';
import { buildLedgerRows, type LedgerBase } from '../erp-inv-gl/inv-gl-posting.helpers';
import {
  buildRevaluationLegs,
  computeFxRevaluation,
  reverseLegs,
  type FxAccountBalance,
} from './fx-revaluation.helpers';

const ZERO = new Prisma.Decimal(0);
const SOURCE_DOC_TYPE = 'fin_fx_revaluation_runs';
const REVERSAL_DOC_TYPE = 'fin_fx_revaluation_reversals';

export interface RunRevaluationInput {
  fiscalPeriodId: string;
  branchId: string;
  notes?: string;
}

/**
 * FR-FIN-05: revaluasi valas akhir periode. Saldo akun bermata uang asing
 * (kas/bank, piutang, utang) dinilai ulang ke kurs akhir periode; selisih
 * (belum terealisasi) diposting ke akun laba/rugi dari sys_settings
 * (module=finance, group=accounts: fxUnrealizedGainAccountId / fxUnrealizedLossAccountId)
 * dan di-reverse otomatis pada hari pertama periode berikutnya.
 */
@Injectable()
export class ErpFinFxRevaluationsService {
  constructor(private readonly prisma: PrismaService) {}

  private async readAccountSetting(tx: Prisma.TransactionClient, key: string): Promise<bigint> {
    const row = await tx.erpSetting.findUnique({
      where: { module_group_key: { module: 'finance', group: 'accounts', key } },
      select: { value: true },
    });
    if (!row?.value) {
      throw new BadRequestException(`Setting Finance › Akun › ${key} belum diisi; revaluasi dibatalkan.`);
    }
    try {
      return BigInt(row.value);
    } catch {
      throw new BadRequestException(`Setting Finance › Akun › ${key} tidak valid.`);
    }
  }

  /** Net foreign (debitFx-creditFx) and IDR (debit-credit) balance per foreign-currency account. */
  private async loadBalances(
    tx: Prisma.TransactionClient,
    periodEnd: Date,
    baseCurrencyId: bigint,
  ): Promise<FxAccountBalance[]> {
    const accounts = await tx.erpAccount.findMany({
      where: { deletedAt: null, currencyId: { not: null, notIn: [baseCurrencyId] } },
      select: { id: true, currencyId: true },
    });
    if (!accounts.length) return [];
    const grouped = await tx.erpFinLedgerEntry.groupBy({
      by: ['accountId'],
      where: {
        accountId: { in: accounts.map((a) => a.id) },
        postingStatus: 'POSTED',
        deletedAt: null,
        entryDate: { lte: periodEnd },
      },
      _sum: { debit: true, credit: true, debitFx: true, creditFx: true },
    });
    const byAccount = new Map(accounts.map((a) => [a.id.toString(), a.currencyId as bigint]));
    return grouped.map((g) => ({
      accountId: g.accountId,
      currencyId: byAccount.get(g.accountId.toString()) as bigint,
      balanceFx: new Prisma.Decimal(g._sum.debitFx ?? 0).sub(g._sum.creditFx ?? 0),
      balanceIdr: new Prisma.Decimal(g._sum.debit ?? 0).sub(g._sum.credit ?? 0),
    }));
  }

  private async loadRates(
    tx: Prisma.TransactionClient,
    currencyIds: bigint[],
    periodEnd: Date,
  ): Promise<Map<string, Prisma.Decimal>> {
    const rates = new Map<string, Prisma.Decimal>();
    for (const currencyId of new Set(currencyIds.map(String))) {
      const row = await tx.erpCurrencyRate.findFirst({
        where: { currencyId: BigInt(currencyId), isActive: true, rateDate: { lte: periodEnd } },
        orderBy: { rateDate: 'desc' },
        select: { rate: true },
      });
      if (row) rates.set(currencyId, new Prisma.Decimal(row.rate));
    }
    return rates;
  }

  private nextDay(d: Date): Date {
    const n = new Date(d);
    n.setUTCDate(n.getUTCDate() + 1);
    return n;
  }

  async run(input: RunRevaluationInput, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;
    const branchId = BigInt(input.branchId);
    const result = await this.prisma.$transaction(async (tx) => {
      const period = await tx.erpFiscalPeriod.findFirst({
        where: { id: BigInt(input.fiscalPeriodId), deletedAt: null },
      });
      if (!period) throw new NotFoundException('Periode fiskal tidak ditemukan.');
      const existing = await tx.erpFinFxRevaluationRun.findFirst({
        where: { fiscalPeriodId: period.id, status: 'COMPLETED', deletedAt: null },
        select: { docNumber: true },
      });
      if (existing) {
        throw new BadRequestException(`Periode ini sudah direvaluasi (${existing.docNumber}).`);
      }
      const base = await tx.erpCurrency.findFirst({
        where: { isBase: true, isActive: true, deletedAt: null },
        select: { id: true },
      });
      if (!base) throw new BadRequestException('Mata uang dasar belum diset.');
      const gainAccountId = await this.readAccountSetting(tx, 'fxUnrealizedGainAccountId');
      const lossAccountId = await this.readAccountSetting(tx, 'fxUnrealizedLossAccountId');

      const balances = await this.loadBalances(tx, period.endDate, base.id);
      const rates = await this.loadRates(tx, balances.map((b) => b.currencyId), period.endDate);
      const calc = computeFxRevaluation(balances, rates);
      const legs = buildRevaluationLegs(calc, gainAccountId, lossAccountId);

      const docNumber = `RV-${period.year}${String(period.periodNo).padStart(2, '0')}`;
      const run = await tx.erpFinFxRevaluationRun.create({
        data: {
          docNumber,
          fiscalPeriodId: period.id,
          revaluationDate: period.endDate,
          status: 'COMPLETED',
          totalGainLoss: calc.totalGainLoss,
          gainAccountId,
          lossAccountId,
          startedAt: new Date(),
          completedAt: new Date(),
          notes: input.notes ?? null,
          createdById: actor,
          updatedById: actor,
          lines: {
            create: calc.lines.map((l) => ({
              accountId: l.accountId,
              currencyId: l.currencyId,
              bookBalanceFx: l.bookBalanceFx,
              bookBalanceIdr: l.bookBalanceIdr,
              revaluationRate: l.revaluationRate,
              revaluedBalanceIdr: l.revaluedBalanceIdr,
              gainLossAmount: l.gainLossIdr,
              lineNo: l.lineNo,
            })),
          },
        },
        select: { id: true },
      });

      const ledgerBase: LedgerBase = {
        branchId,
        locationId: null,
        sourceDocType: SOURCE_DOC_TYPE,
        sourceId: run.id,
        docNumber,
        entryDate: period.endDate,
        fiscalPeriodId: period.id,
        currencyId: base.id,
        exchangeRate: new Prisma.Decimal(1),
        actorId: actor,
      };
      const rows = buildLedgerRows(ledgerBase, legs);
      await assertLedgerRowsPeriodOpen(tx, rows);
      await tx.erpFinLedgerEntry.createMany({ data: rows });

      const reversalDate = this.nextDay(period.endDate);
      const nextPeriod = await tx.erpFiscalPeriod.findFirst({
        where: { deletedAt: null, startDate: { lte: reversalDate }, endDate: { gte: reversalDate } },
        select: { id: true },
      });
      let reversed = false;
      if (nextPeriod) {
        const revRows = buildLedgerRows(
          {
            ...ledgerBase,
            sourceDocType: REVERSAL_DOC_TYPE,
            docNumber: `${docNumber}-R`,
            entryDate: reversalDate,
            fiscalPeriodId: nextPeriod.id,
          },
          reverseLegs(legs),
        );
        await assertLedgerRowsPeriodOpen(tx, revRows);
        await tx.erpFinLedgerEntry.createMany({ data: revRows });
        reversed = true;
      }
      return { id: run.id, docNumber, totalGainLoss: calc.totalGainLoss, lines: calc.lines.length, reversed };
    });
    return {
      success: true,
      data: {
        id: result.id.toString(),
        docNumber: result.docNumber,
        totalGainLoss: result.totalGainLoss.toString(),
        lineCount: result.lines,
        autoReversed: result.reversed,
      },
    };
  }

  async list() {
    const rows = await this.prisma.erpFinFxRevaluationRun.findMany({
      where: { deletedAt: null },
      orderBy: { revaluationDate: 'desc' },
      include: { lines: true },
    });
    return { success: true, data: rows };
  }
}
