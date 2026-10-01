import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import {
  buildListMeta,
  clampPageLimit,
} from '../common/dto/query-pagination.dto';
import {
  prismaDateFilter,
  resolveDateRange,
} from '../common/utils/date-range.util';
import { PrismaService } from '../prisma/prisma.service';
import { QueryLedgerDto } from './dto/query-ledger.dto';

/** Thin list projection — no over-fetch for grid. */
const LEDGER_LIST_SELECT = {
  id: true,
  branchId: true,
  source: true,
  sourceDocType: true,
  sourceId: true,
  docNumber: true,
  entryDate: true,
  fiscalPeriodId: true,
  partnerId: true,
  accountId: true,
  description: true,
  currencyId: true,
  exchangeRate: true,
  referenceNo: true,
  debit: true,
  credit: true,
  status: true,
  postingStatus: true,
  reconciliationStatus: true,
  createdAt: true,
  updatedAt: true,
} satisfies Prisma.ErpFinLedgerEntrySelect;

@Injectable()
export class ErpFinLedgerService {
  constructor(private readonly prisma: PrismaService) {}

  async findAll(query: QueryLedgerDto) {
    const { page, limit, skip } = clampPageLimit(query.page, query.limit);
    const includeTotal = query.includeTotal !== false;

    // Require a bounded date window (default last 31 days) and cap at 366 days.
    const { from, to } = resolveDateRange({
      dateFrom: query.dateFrom,
      dateTo: query.dateTo,
      requireRange: true,
      defaultSpanDays: 31,
      maxSpanDays: 366,
      fieldLabel: 'Ledger entryDate',
    });

    const where: Prisma.ErpFinLedgerEntryWhereInput = { deletedAt: null };
    const dateFilter = prismaDateFilter(from, to);
    if (dateFilter) where.entryDate = dateFilter;

    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { docNumber: { contains: q, mode: 'insensitive' } },
        { description: { contains: q, mode: 'insensitive' } },
        { referenceNo: { contains: q, mode: 'insensitive' } },
      ];
    }
    if (query.accountId) where.accountId = BigInt(query.accountId);
    if (query.partnerId) where.partnerId = BigInt(query.partnerId);

    const items = await this.prisma.erpFinLedgerEntry.findMany({
      where,
      orderBy: [{ entryDate: 'desc' }, { id: 'desc' }],
      skip,
      take: limit,
      select: LEDGER_LIST_SELECT,
    });

    let total: number | null = null;
    if (includeTotal) {
      total = await this.prisma.erpFinLedgerEntry.count({ where });
    }

    return {
      success: true,
      data: items,
      meta: buildListMeta({
        page,
        limit,
        total,
        rowCount: items.length,
        includeTotal,
      }),
    };
  }

  async findOne(id: bigint) {
    const item = await this.prisma.erpFinLedgerEntry.findFirst({
      where: { id, deletedAt: null },
    });
    if (!item) throw new NotFoundException('Ledger entry not found');
    return { success: true, data: item };
  }
}
