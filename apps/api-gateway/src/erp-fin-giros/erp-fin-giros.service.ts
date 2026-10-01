import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { CreateGiroDto } from './dto/create-giro.dto';
import { QueryGiroDto } from './dto/query-giro.dto';
import { UpdateGiroDto } from './dto/update-giro.dto';

function toBigInt(v?: string | null): bigint | null {
  if (v === undefined || v === null || v === '') return null;
  return BigInt(v);
}

@Injectable()
export class ErpFinGirosService {
  constructor(private readonly prisma: PrismaService) {}

  async create(dto: CreateGiroDto, actorId?: string) {
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const created = await this.prisma.erpFinGiro.create({
      data: {
        giroNumber: dto.giroNumber,
        type: dto.type,
        source: dto.source ?? null,
        sourceTransactionId: toBigInt(dto.sourceTransactionId),
        partnerId: toBigInt(dto.partnerId),
        branchId: toBigInt(dto.branchId),
        fiscalPeriodId: toBigInt(dto.fiscalPeriodId),
        bankName: dto.bankName ?? null,
        bankAccountNo: dto.bankAccountNo ?? null,
        bankAccountId: toBigInt(dto.bankAccountId),
        giroAccountId: toBigInt(dto.giroAccountId),
        currencyId: BigInt(dto.currencyId),
        exchangeRate: new Prisma.Decimal(dto.exchangeRate),
        amount: new Prisma.Decimal(dto.amount),
        amountFx: dto.amountFx ? new Prisma.Decimal(dto.amountFx) : null,
        dueDate: new Date(dto.dueDate),
        clearedDate: dto.clearedDate ? new Date(dto.clearedDate) : null,
        status: dto.status ?? 'OUTSTANDING',
        description: dto.description ?? null,
        notes: dto.notes ?? null,
        lineNo: dto.lineNo,
        legacyCode: dto.legacyCode ?? null,
        createdById: actorBigInt,
        updatedById: actorBigInt,
      },
    });
    return { success: true, data: created };
  }

  async findAll(query: QueryGiroDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;
    const where: Prisma.ErpFinGiroWhereInput = { deletedAt: null };

    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { giroNumber: { contains: q, mode: 'insensitive' } },
        { bankName: { contains: q, mode: 'insensitive' } },
      ];
    }
    if (query.type) where.type = query.type;
    if (query.status) where.status = query.status;

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpFinGiro.findMany({
        where,
        orderBy: [{ dueDate: 'desc' }, { createdAt: 'desc' }],
        skip,
        take: limit,
      }),
      this.prisma.erpFinGiro.count({ where }),
    ]);

    return {
      success: true,
      data: items,
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  async findOne(id: bigint) {
    const item = await this.prisma.erpFinGiro.findFirst({
      where: { id, deletedAt: null },
    });
    if (!item) throw new NotFoundException('Giro not found');
    return { success: true, data: item };
  }

  async update(id: bigint, dto: UpdateGiroDto, actorId?: string) {
    const existing = await this.prisma.erpFinGiro.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) throw new NotFoundException('Giro not found');

    const data: Prisma.ErpFinGiroUpdateInput = {
      updatedById: actorId ? BigInt(actorId) : null,
    };
    if (dto.giroNumber !== undefined) data.giroNumber = dto.giroNumber;
    if (dto.type !== undefined) data.type = dto.type;
    if (dto.source !== undefined) data.source = dto.source;
    if (dto.sourceTransactionId !== undefined)
      data.sourceTransactionId = toBigInt(dto.sourceTransactionId);
    if (dto.partnerId !== undefined) data.partnerId = toBigInt(dto.partnerId);
    if (dto.branchId !== undefined) data.branchId = toBigInt(dto.branchId);
    if (dto.fiscalPeriodId !== undefined)
      data.fiscalPeriodId = toBigInt(dto.fiscalPeriodId);
    if (dto.bankName !== undefined) data.bankName = dto.bankName;
    if (dto.bankAccountNo !== undefined) data.bankAccountNo = dto.bankAccountNo;
    if (dto.bankAccountId !== undefined)
      data.bankAccountId = toBigInt(dto.bankAccountId);
    if (dto.giroAccountId !== undefined)
      data.giroAccountId = toBigInt(dto.giroAccountId);
    if (dto.currencyId !== undefined) data.currencyId = BigInt(dto.currencyId);
    if (dto.exchangeRate !== undefined)
      data.exchangeRate = new Prisma.Decimal(dto.exchangeRate);
    if (dto.amount !== undefined) data.amount = new Prisma.Decimal(dto.amount);
    if (dto.amountFx !== undefined)
      data.amountFx = dto.amountFx ? new Prisma.Decimal(dto.amountFx) : null;
    if (dto.dueDate !== undefined) data.dueDate = new Date(dto.dueDate);
    if (dto.clearedDate !== undefined)
      data.clearedDate = dto.clearedDate ? new Date(dto.clearedDate) : null;
    if (dto.status !== undefined) data.status = dto.status;
    if (dto.description !== undefined) data.description = dto.description;
    if (dto.notes !== undefined) data.notes = dto.notes;
    if (dto.lineNo !== undefined) data.lineNo = dto.lineNo;
    if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;

    const updated = await this.prisma.erpFinGiro.update({
      where: { id },
      data,
    });
    return { success: true, data: updated };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpFinGiro.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) throw new NotFoundException('Giro not found');

    await this.prisma.erpFinGiro.update({
      where: { id },
      data: {
        deletedAt: new Date(),
        updatedById: actorId ? BigInt(actorId) : null,
      },
    });
    return { success: true, message: 'Giro deleted' };
  }
}
