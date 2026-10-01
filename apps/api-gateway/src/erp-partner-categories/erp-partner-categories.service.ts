import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { isUniqueViolation, throwDuplicate } from '../common/errors/duplicate.util';
import { PrismaService } from '../prisma/prisma.service';
import { BulkErpPartnerCategoryDto, BulkStatusErpPartnerCategoryDto } from './dto/bulk-erp-partner-category.dto';
import { CreateErpPartnerCategoryDto } from './dto/create-erp-partner-category.dto';
import { QueryErpPartnerCategoryDto } from './dto/query-erp-partner-category.dto';
import { UpdateErpPartnerCategoryDto } from './dto/update-erp-partner-category.dto';

@Injectable()
export class ErpPartnerCategoriesService {
  constructor(private readonly prisma: PrismaService) {}

  async create(dto: CreateErpPartnerCategoryDto, actorId?: string) {
    const existing = await this.prisma.erpPartnerCategory.findFirst({
      where: { code: dto.code },
      select: { id: true, deletedAt: true },
    });
    if (existing) {
      throwDuplicate({
        fieldLabel: 'Partner category code',
        value: dto.code,
        isSoftDeleted: Boolean(existing.deletedAt),
      });
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;

    let created;
    try {
      created = await this.prisma.erpPartnerCategory.create({
        data: {
          code: dto.code,
          name: dto.name,
          kind: dto.kind,
          salesTier: dto.salesTier ?? null,
          isActive: dto.isActive ?? true,
          createdById: actorBigInt,
          updatedById: actorBigInt,
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', 'md_partner_categories_code_key'])) {
        throwDuplicate({ fieldLabel: 'Partner category code', value: dto.code });
      }
      throw error;
    }

    return { success: true, data: created };
  }

  async findAll(query: QueryErpPartnerCategoryDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    const where: Prisma.ErpPartnerCategoryWhereInput = { deletedAt: null };

    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { code: { equals: q, mode: 'insensitive' } },
        { name: { contains: q, mode: 'insensitive' } },
      ];
    }

    if (query.kind !== undefined) {
      where.kind = query.kind;
    }

    if (query.isActive !== undefined) {
      where.isActive = query.isActive;
    }

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpPartnerCategory.findMany({
        where,
        orderBy: [{ [query.sortBy ?? 'createdAt']: query.sortDir ?? 'desc' }],
        skip,
        take: limit,
      }),
      this.prisma.erpPartnerCategory.count({ where }),
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
    const item = await this.prisma.erpPartnerCategory.findFirst({
      where: { id, deletedAt: null },
    });
    if (!item) {
      throw new NotFoundException('ERP Partner Category not found');
    }
    return { success: true, data: item };
  }

  async update(id: bigint, dto: UpdateErpPartnerCategoryDto, actorId?: string) {
    const existing = await this.prisma.erpPartnerCategory.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) {
      throw new NotFoundException('ERP Partner Category not found');
    }

    if (dto.code && dto.code !== existing.code) {
      const duplicate = await this.prisma.erpPartnerCategory.findFirst({
        where: { code: dto.code, NOT: { id } },
        select: { id: true, deletedAt: true },
      });
      if (duplicate) {
        throwDuplicate({
          fieldLabel: 'Partner category code',
          value: dto.code,
          isSoftDeleted: Boolean(duplicate.deletedAt),
        });
      }
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;

    let updated;
    try {
      updated = await this.prisma.erpPartnerCategory.update({
        where: { id },
        data: {
          code: dto.code,
          name: dto.name,
          kind: dto.kind,
          salesTier: dto.salesTier,
          isActive: dto.isActive,
          updatedById: actorBigInt,
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', 'md_partner_categories_code_key'])) {
        throwDuplicate({ fieldLabel: 'Partner category code', value: dto.code ?? existing.code });
      }
      throw error;
    }

    return { success: true, data: updated };
  }

  async bulkUpdateStatus(dto: BulkStatusErpPartnerCategoryDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpPartnerCategory.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { isActive: dto.isActive, updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async bulkDelete(dto: BulkErpPartnerCategoryDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpPartnerCategory.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpPartnerCategory.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('ERP Partner Category not found');
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;

    await this.prisma.erpPartnerCategory.update({
      where: { id },
      data: {
        deletedAt: new Date(),
        updatedById: actorBigInt,
      },
    });

    return { success: true, message: 'ERP Partner Category deleted' };
  }
}
