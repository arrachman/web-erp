import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { CreateArReceiptDto } from './dto/create-ar-receipt.dto';
import { QueryArReceiptDto } from './dto/query-ar-receipt.dto';
import { UpdateArReceiptDto } from './dto/update-ar-receipt.dto';

function toBigInt(v?: string | null): bigint | null {
  if (v === undefined || v === null || v === '') return null;
  return BigInt(v);
}

@Injectable()
export class ErpFinArReceiptsService {
  constructor(private readonly prisma: PrismaService) {}

  async create(dto: CreateArReceiptDto, actorId?: string) {
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const created = await this.prisma.erpFinArReceipt.create({
      data: {
        docNumber: dto.docNumber,
        autoNumber: dto.autoNumber ?? null,
        branchId: BigInt(dto.branchId),
        locationId: toBigInt(dto.locationId),
        source: dto.source ?? null,
        transactionDate: new Date(dto.transactionDate),
        fiscalPeriodId: BigInt(dto.fiscalPeriodId),
        partnerId: BigInt(dto.partnerId),
        contactPerson: dto.contactPerson ?? null,
        bankAccountId: toBigInt(dto.bankAccountId),
        description: dto.description,
        notes: dto.notes ?? null,
        currencyId: BigInt(dto.currencyId),
        exchangeRate: new Prisma.Decimal(dto.exchangeRate),
        amount: new Prisma.Decimal(dto.amount),
        amountFx: dto.amountFx ? new Prisma.Decimal(dto.amountFx) : null,
        allocatedAmount: new Prisma.Decimal(dto.allocatedAmount),
        paymentStatus: dto.paymentStatus ?? 'UNPAID',
        status: dto.status ?? 'DRAFT',
        postingStatus: dto.postingStatus ?? 'UNPOSTED',
        legacyCode: dto.legacyCode ?? null,
        createdById: actorBigInt,
        updatedById: actorBigInt,
      },
    });
    return { success: true, data: created };
  }

  async findAll(query: QueryArReceiptDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;
    const where: Prisma.ErpFinArReceiptWhereInput = { deletedAt: null };

    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { docNumber: { contains: q, mode: 'insensitive' } },
        { description: { contains: q, mode: 'insensitive' } },
      ];
    }
    if (query.status) where.status = query.status;
    if (query.paymentStatus) where.paymentStatus = query.paymentStatus;
    if (query.source) where.source = query.source;
    if (query.partnerId) where.partnerId = BigInt(query.partnerId);
    if (query.dateFrom || query.dateTo) {
      where.transactionDate = {
        ...(query.dateFrom ? { gte: new Date(query.dateFrom) } : {}),
        ...(query.dateTo ? { lte: new Date(query.dateTo) } : {}),
      };
    }

    const sortField = query.sortBy ?? 'transactionDate';
    const sortDir = query.sortDir ?? 'desc';
    const ALLOWED_SORT = ['transactionDate', 'docNumber', 'amount', 'createdAt', 'updatedAt'];
    const orderBy = ALLOWED_SORT.includes(sortField)
      ? [{ [sortField]: sortDir }, { createdAt: sortDir }]
      : [{ transactionDate: 'desc' as const }, { createdAt: 'desc' as const }];

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpFinArReceipt.findMany({
        where,
        orderBy,
        skip,
        take: limit,
      }),
      this.prisma.erpFinArReceipt.count({ where }),
    ]);

    return {
      success: true,
      data: items,
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  async findOne(id: bigint) {
    const item = await this.prisma.erpFinArReceipt.findFirst({
      where: { id, deletedAt: null },
    });
    if (!item) throw new NotFoundException('AR receipt not found');
    return { success: true, data: item };
  }

  async update(id: bigint, dto: UpdateArReceiptDto, actorId?: string) {
    const existing = await this.prisma.erpFinArReceipt.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) throw new NotFoundException('AR receipt not found');

    const data: Prisma.ErpFinArReceiptUpdateInput = {
      updatedById: actorId ? BigInt(actorId) : null,
    };
    if (dto.docNumber !== undefined) data.docNumber = dto.docNumber;
    if (dto.autoNumber !== undefined) data.autoNumber = dto.autoNumber;
    if (dto.branchId !== undefined) data.branchId = BigInt(dto.branchId);
    if (dto.locationId !== undefined) data.locationId = toBigInt(dto.locationId);
    if (dto.source !== undefined) data.source = dto.source;
    if (dto.transactionDate !== undefined)
      data.transactionDate = new Date(dto.transactionDate);
    if (dto.fiscalPeriodId !== undefined)
      data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);
    if (dto.partnerId !== undefined) data.partnerId = BigInt(dto.partnerId);
    if (dto.contactPerson !== undefined) data.contactPerson = dto.contactPerson;
    if (dto.bankAccountId !== undefined)
      data.bankAccountId = toBigInt(dto.bankAccountId);
    if (dto.description !== undefined) data.description = dto.description;
    if (dto.notes !== undefined) data.notes = dto.notes;
    if (dto.currencyId !== undefined) data.currencyId = BigInt(dto.currencyId);
    if (dto.exchangeRate !== undefined)
      data.exchangeRate = new Prisma.Decimal(dto.exchangeRate);
    if (dto.amount !== undefined) data.amount = new Prisma.Decimal(dto.amount);
    if (dto.amountFx !== undefined)
      data.amountFx = dto.amountFx ? new Prisma.Decimal(dto.amountFx) : null;
    if (dto.allocatedAmount !== undefined)
      data.allocatedAmount = new Prisma.Decimal(dto.allocatedAmount);
    if (dto.paymentStatus !== undefined) data.paymentStatus = dto.paymentStatus;
    if (dto.status !== undefined) data.status = dto.status;
    if (dto.postingStatus !== undefined) data.postingStatus = dto.postingStatus;
    if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;

    const updated = await this.prisma.erpFinArReceipt.update({
      where: { id },
      data,
    });
    return { success: true, data: updated };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpFinArReceipt.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) throw new NotFoundException('AR receipt not found');

    await this.prisma.erpFinArReceipt.update({
      where: { id },
      data: {
        deletedAt: new Date(),
        updatedById: actorId ? BigInt(actorId) : null,
      },
    });
    return { success: true, message: 'AR receipt deleted' };
  }
}
