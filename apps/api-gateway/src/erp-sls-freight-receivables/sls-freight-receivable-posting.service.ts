import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import {
  buildLedgerRows,
  reverseInvLedger,
  type LedgerBase,
  type LedgerLeg,
} from '../erp-inv-gl/inv-gl-posting.helpers';
import { assertLedgerRowsPeriodOpen } from '../erp-common/utils/ledger-period-guard';

const SLS_GL_SOURCE = 'SALES';
const SLS_GL_DOCTYPE = 'sls_freight_receivables';

type FreightReceivableRow = Prisma.ErpSlsFreightReceivableGetPayload<Record<string, never>>;

/**
 * GL posting for Freight Receivable (RP) → fin_ledger_entries.
 *
 * Confirmed with user: freight cost billed to the customer is recorded
 * SEPARATE from HPP/sales margin — other income, not a deduction from
 * goods revenue. Standalone document (own table, sls_freight_receivables)
 * — not an AR Receipt (no cash yet, nothing to allocate) or a Sales
 * Invoice (no stock, no item lines).
 *
 * §2 posting matrix (DECISIONS.md "Pola jurnal usulan" — RP row,
 * user-confirmed direction):
 *   Dr  Piutang Usaha (receivableAccountId, fallback customer)   amount
 *   Cr  Pendapatan Jasa Angkut / Freight Income (incomeAccountId) amount
 */
@Injectable()
export class SlsFreightReceivablePostingService {
  async postToLedger(
    tx: Prisma.TransactionClient,
    freightReceivable: FreightReceivableRow,
    actorId: bigint | null,
  ): Promise<void> {
    const amount = new Prisma.Decimal(freightReceivable.amount?.toString() ?? 0);
    if (amount.lte(0)) {
      throw new BadRequestException('Tidak bisa posting: amount harus lebih besar dari 0.');
    }
    const receivableAccountId = await this.resolveReceivableAccount(tx, freightReceivable);
    if (!freightReceivable.incomeAccountId) {
      throw new BadRequestException(
        'Tidak bisa posting: akun Pendapatan Jasa Angkut (incomeAccountId) belum diisi untuk dokumen ini.',
      );
    }

    const legs: LedgerLeg[] = [
      {
        accountId: receivableAccountId,
        debit: amount,
        credit: new Prisma.Decimal(0),
        description: freightReceivable.description,
        partnerId: freightReceivable.customerId,
      },
      {
        accountId: freightReceivable.incomeAccountId,
        debit: new Prisma.Decimal(0),
        credit: amount,
        description: 'Pendapatan jasa angkut',
        partnerId: freightReceivable.customerId,
      },
    ];

    const base: LedgerBase = {
      branchId: freightReceivable.branchId,
      locationId: freightReceivable.locationId,
      sourceDocType: SLS_GL_DOCTYPE,
      sourceId: freightReceivable.id,
      docNumber: freightReceivable.docNumber,
      entryDate: freightReceivable.transactionDate,
      fiscalPeriodId: freightReceivable.fiscalPeriodId,
      currencyId: freightReceivable.currencyId,
      exchangeRate: freightReceivable.exchangeRate,
      actorId,
    };
    const rows = buildLedgerRows(base, legs).map((r) => ({ ...r, source: SLS_GL_SOURCE }));
    await assertLedgerRowsPeriodOpen(tx, rows);
    await tx.erpFinLedgerEntry.createMany({ data: rows });
  }

  async reverseLedger(tx: Prisma.TransactionClient, freightReceivableId: bigint): Promise<void> {
    await reverseInvLedger(tx, SLS_GL_DOCTYPE, freightReceivableId);
  }

  private async resolveReceivableAccount(
    tx: Prisma.TransactionClient,
    freightReceivable: FreightReceivableRow,
  ): Promise<bigint> {
    if (freightReceivable.receivableAccountId) return freightReceivable.receivableAccountId;
    const customer = await tx.erpPartner.findUnique({
      where: { id: freightReceivable.customerId },
      select: { receivableAccountId: true },
    });
    if (customer?.receivableAccountId) return customer.receivableAccountId;
    throw new BadRequestException(
      'Tidak bisa posting: akun piutang (receivable account) tidak ditemukan di dokumen maupun master pelanggan.',
    );
  }
}
