import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { isUniqueViolation, throwDuplicate } from '../common/errors/duplicate.util';
import { PrismaService } from '../prisma/prisma.service';
import { toAuditUserId } from '../common/utils/audit-user.util';
import { BulkErpPaymentTermDto, BulkStatusErpPaymentTermDto } from './dto/bulk-erp-payment-term.dto';
import { CreateErpPaymentTermDto } from './dto/create-erp-payment-term.dto';
import { QueryErpPaymentTermDto } from './dto/query-erp-payment-term.dto';
import { UpdateErpPaymentTermDto } from './dto/update-erp-payment-term.dto';

@Injectable()
export class ErpPaymentTermsService {
  constructor(private prisma: PrismaService) {}

  async create(dto: CreateErpPaymentTermDto, actorId?: string) {
    const existing = await this.prisma.erpPaymentTerm.findFirst({
      where: { code: dto.code },
      select: { id: true, deletedAt: true },
    });
    if (existing) {
      throwDuplicate({
        fieldLabel: 'Payment term code',
        value: dto.code,
        isSoftDeleted: Boolean(existing.deletedAt),
      });
    }

    let created;
    try {
      created = await this.prisma.erpPaymentTerm.create({
        data: {
          code: dto.code,
          name: dto.name,
          netDays: dto.netDays,
          discountDays1: dto.discountDays1,
          discountPercent1: dto.discountPercent1 ? new Prisma.Decimal(dto.discountPercent1) : null,
          discountDays2: dto.discountDays2,
          discountPercent2: dto.discountPercent2 ? new Prisma.Decimal(dto.discountPercent2) : null,
          penaltyPercent: dto.penaltyPercent ? new Prisma.Decimal(dto.penaltyPercent) : null,
          penaltyPeriod: dto.penaltyPeriod,
          isActive: dto.isActive ?? true,
          createdById: toAuditUserId(actorId),
          updatedById: toAuditUserId(actorId),
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code'])) {
        throwDuplicate({ fieldLabel: 'Payment term code', value: dto.code });
      }
      throw error;
    }

    return { success: true, data: created };
  }

  async findAll(query: QueryErpPaymentTermDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    const where: Prisma.ErpPaymentTermWhereInput = { deletedAt: null };
    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { code: { equals: q, mode: 'insensitive' } },
        { name: { contains: q, mode: 'insensitive' } },
      ];
    }
    if (query.isActive !== undefined) {
      where.isActive = query.isActive;
    }

    const sortBy = query.sortBy ?? 'createdAt';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpPaymentTerm.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }],
        skip,
        take: limit,
      }),
      this.prisma.erpPaymentTerm.count({ where }),
    ]);

    return {
      success: true,
      data: items,
      meta: {
        page,
        limit,
        total,
        totalPages: Math.ceil(total / limit) || 1,
      },
    };
  }

  async findOne(id: bigint) {
    const item = await this.prisma.erpPaymentTerm.findFirst({
      where: { id, deletedAt: null },
    });
    if (!item) {
      throw new NotFoundException('Payment term not found');
    }
    return { success: true, data: item };
  }

  async update(id: bigint, dto: UpdateErpPaymentTermDto, actorId?: string) {
    const existing = await this.prisma.erpPaymentTerm.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) {
      throw new NotFoundException('Payment term not found');
    }

    if (dto.code && dto.code !== existing.code) {
      const duplicate = await this.prisma.erpPaymentTerm.findFirst({
        where: { code: dto.code, NOT: { id } },
        select: { id: true, deletedAt: true },
      });
      if (duplicate) {
        throwDuplicate({
          fieldLabel: 'Payment term code',
          value: dto.code,
          isSoftDeleted: Boolean(duplicate.deletedAt),
        });
      }
    }

    let updated;
    try {
      updated = await this.prisma.erpPaymentTerm.update({
        where: { id },
        data: {
          code: dto.code,
          name: dto.name,
          netDays: dto.netDays,
          discountDays1: dto.discountDays1,
          discountPercent1: dto.discountPercent1 !== undefined
            ? (dto.discountPercent1 ? new Prisma.Decimal(dto.discountPercent1) : null)
            : undefined,
          discountDays2: dto.discountDays2,
          discountPercent2: dto.discountPercent2 !== undefined
            ? (dto.discountPercent2 ? new Prisma.Decimal(dto.discountPercent2) : null)
            : undefined,
          penaltyPercent: dto.penaltyPercent !== undefined
            ? (dto.penaltyPercent ? new Prisma.Decimal(dto.penaltyPercent) : null)
            : undefined,
          penaltyPeriod: dto.penaltyPeriod,
          isActive: dto.isActive,
          updatedById: toAuditUserId(actorId),
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code'])) {
        throwDuplicate({ fieldLabel: 'Payment term code', value: dto.code ?? existing.code });
      }
      throw error;
    }

    return { success: true, data: updated };
  }

  async bulkUpdateStatus(dto: BulkStatusErpPaymentTermDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = toAuditUserId(actorId);
    const { count } = await this.prisma.erpPaymentTerm.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { isActive: dto.isActive, updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async bulkDelete(dto: BulkErpPaymentTermDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = toAuditUserId(actorId);
    const { count } = await this.prisma.erpPaymentTerm.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpPaymentTerm.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('Payment term not found');
    }

    await this.prisma.erpPaymentTerm.update({
      where: { id },
      data: {
        deletedAt: new Date(),
        updatedById: toAuditUserId(actorId),
      },
    });

    return { success: true, message: 'Payment term deleted' };
  }
}
