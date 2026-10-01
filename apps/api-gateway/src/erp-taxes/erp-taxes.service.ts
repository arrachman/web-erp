import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { isUniqueViolation, throwDuplicate } from '../common/errors/duplicate.util';
import { PrismaService } from '../prisma/prisma.service';
import { toAuditUserId } from '../common/utils/audit-user.util';
import { BulkErpTaxDto, BulkStatusErpTaxDto } from './dto/bulk-erp-tax.dto';
import { CreateErpTaxDto } from './dto/create-erp-tax.dto';
import { QueryErpTaxDto } from './dto/query-erp-tax.dto';
import { UpdateErpTaxDto } from './dto/update-erp-tax.dto';

@Injectable()
export class ErpTaxesService {
  constructor(private prisma: PrismaService) {}

  async create(dto: CreateErpTaxDto, actorId?: string) {
    const existing = await this.prisma.erpTax.findFirst({
      where: { code: dto.code },
      select: { id: true, deletedAt: true },
    });
    if (existing) {
      throwDuplicate({
        fieldLabel: 'Tax code',
        value: dto.code,
        isSoftDeleted: Boolean(existing.deletedAt),
      });
    }

    let created;
    try {
      created = await this.prisma.erpTax.create({
        data: {
          code: dto.code,
          name: dto.name,
          rate: new Prisma.Decimal(dto.rate),
          saleAccountId: dto.saleAccountId ? BigInt(dto.saleAccountId) : null,
          purchaseAccountId: dto.purchaseAccountId ? BigInt(dto.purchaseAccountId) : null,
          isActive: dto.isActive ?? true,
          createdById: toAuditUserId(actorId),
          updatedById: toAuditUserId(actorId),
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code'])) {
        throwDuplicate({ fieldLabel: 'Tax code', value: dto.code });
      }
      throw error;
    }

    return { success: true, data: created };
  }

  async findAll(query: QueryErpTaxDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    const where: Prisma.ErpTaxWhereInput = { deletedAt: null };
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
      this.prisma.erpTax.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }],
        skip,
        take: limit,
        include: {
          saleAccount: { select: { id: true, code: true, name: true } },
          purchaseAccount: { select: { id: true, code: true, name: true } },
        },
      }),
      this.prisma.erpTax.count({ where }),
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
    const item = await this.prisma.erpTax.findFirst({
      where: { id, deletedAt: null },
      include: {
        saleAccount: { select: { id: true, code: true, name: true } },
        purchaseAccount: { select: { id: true, code: true, name: true } },
      },
    });
    if (!item) {
      throw new NotFoundException('Tax not found');
    }
    return { success: true, data: item };
  }

  async update(id: bigint, dto: UpdateErpTaxDto, actorId?: string) {
    const existing = await this.prisma.erpTax.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) {
      throw new NotFoundException('Tax not found');
    }

    if (dto.code && dto.code !== existing.code) {
      const duplicate = await this.prisma.erpTax.findFirst({
        where: { code: dto.code, NOT: { id } },
        select: { id: true, deletedAt: true },
      });
      if (duplicate) {
        throwDuplicate({
          fieldLabel: 'Tax code',
          value: dto.code,
          isSoftDeleted: Boolean(duplicate.deletedAt),
        });
      }
    }

    let updated;
    try {
      updated = await this.prisma.erpTax.update({
        where: { id },
        data: {
          code: dto.code,
          name: dto.name,
          rate: dto.rate !== undefined ? new Prisma.Decimal(dto.rate) : undefined,
          saleAccountId: dto.saleAccountId !== undefined
            ? (dto.saleAccountId ? BigInt(dto.saleAccountId) : null)
            : undefined,
          purchaseAccountId: dto.purchaseAccountId !== undefined
            ? (dto.purchaseAccountId ? BigInt(dto.purchaseAccountId) : null)
            : undefined,
          isActive: dto.isActive,
          updatedById: toAuditUserId(actorId),
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code'])) {
        throwDuplicate({ fieldLabel: 'Tax code', value: dto.code ?? existing.code });
      }
      throw error;
    }

    return { success: true, data: updated };
  }

  async bulkUpdateStatus(dto: BulkStatusErpTaxDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = toAuditUserId(actorId);
    const { count } = await this.prisma.erpTax.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { isActive: dto.isActive, updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async bulkDelete(dto: BulkErpTaxDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = toAuditUserId(actorId);
    const { count } = await this.prisma.erpTax.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpTax.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('Tax not found');
    }

    await this.prisma.erpTax.update({
      where: { id },
      data: {
        deletedAt: new Date(),
        updatedById: toAuditUserId(actorId),
      },
    });

    return { success: true, message: 'Tax deleted' };
  }
}
