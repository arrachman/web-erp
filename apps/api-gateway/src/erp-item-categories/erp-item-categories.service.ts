import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { isUniqueViolation, throwDuplicate } from '../common/errors/duplicate.util';
import { PrismaService } from '../prisma/prisma.service';
import {
  BulkErpItemCategoryDto,
  BulkStatusErpItemCategoryDto,
} from './dto/bulk-erp-item-category.dto';
import { CreateErpItemCategoryDto } from './dto/create-erp-item-category.dto';
import { QueryErpItemCategoryDto } from './dto/query-erp-item-category.dto';
import { UpdateErpItemCategoryDto } from './dto/update-erp-item-category.dto';

const ACCOUNT_SELECT = { select: { id: true, code: true, name: true } } as const;

const ACCOUNT_INCLUDES = {
  inventoryAccount: ACCOUNT_SELECT,
  cogsAccount: ACCOUNT_SELECT,
  salesAccount: ACCOUNT_SELECT,
  salesReturnAccount: ACCOUNT_SELECT,
  salesDiscountAccount: ACCOUNT_SELECT,
  purchaseReturnAccount: ACCOUNT_SELECT,
  purchaseDiscountAccount: ACCOUNT_SELECT,
  consignmentAccount: ACCOUNT_SELECT,
} as const;

@Injectable()
export class ErpItemCategoriesService {
  constructor(private readonly prisma: PrismaService) {}

  async create(dto: CreateErpItemCategoryDto, actorId?: string) {
    const existing = await this.prisma.erpItemCategory.findFirst({
      where: { code: dto.code },
      select: { id: true, deletedAt: true },
    });
    if (existing) {
      throwDuplicate({
        fieldLabel: 'Item category code',
        value: dto.code,
        isSoftDeleted: Boolean(existing.deletedAt),
      });
    }

    let created;
    try {
      created = await this.prisma.erpItemCategory.create({
        data: {
          code: dto.code,
          name: dto.name,
          isActive: dto.isActive ?? true,
          parentId: dto.parentId ? BigInt(dto.parentId) : null,
          inventoryAccountId: dto.inventoryAccountId ? BigInt(dto.inventoryAccountId) : null,
          cogsAccountId: dto.cogsAccountId ? BigInt(dto.cogsAccountId) : null,
          salesAccountId: dto.salesAccountId ? BigInt(dto.salesAccountId) : null,
          salesReturnAccountId: dto.salesReturnAccountId ? BigInt(dto.salesReturnAccountId) : null,
          salesDiscountAccountId: dto.salesDiscountAccountId
            ? BigInt(dto.salesDiscountAccountId)
            : null,
          purchaseReturnAccountId: dto.purchaseReturnAccountId
            ? BigInt(dto.purchaseReturnAccountId)
            : null,
          purchaseDiscountAccountId: dto.purchaseDiscountAccountId
            ? BigInt(dto.purchaseDiscountAccountId)
            : null,
          consignmentAccountId: dto.consignmentAccountId ? BigInt(dto.consignmentAccountId) : null,
          createdById: actorId ? BigInt(actorId) : null,
          updatedById: actorId ? BigInt(actorId) : null,
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', 'md_item_categories_code_key'])) {
        throwDuplicate({ fieldLabel: 'Item category code', value: dto.code });
      }
      throw error;
    }

    return { success: true, data: created };
  }

  async findAll(query: QueryErpItemCategoryDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    const where: Prisma.ErpItemCategoryWhereInput = { deletedAt: null };

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

    if (query.parentId !== undefined) {
      where.parentId = BigInt(query.parentId);
    }

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpItemCategory.findMany({
        where,
        include: {
          parent: { select: { id: true, code: true, name: true } },
          ...ACCOUNT_INCLUDES,
        },
        orderBy: [{ [query.sortBy ?? 'createdAt']: query.sortDir ?? 'desc' }],
        skip,
        take: limit,
      }),
      this.prisma.erpItemCategory.count({ where }),
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
    const item = await this.prisma.erpItemCategory.findFirst({
      where: { id, deletedAt: null },
      include: {
        parent: { select: { id: true, code: true, name: true } },
        ...ACCOUNT_INCLUDES,
        children: {
          where: { deletedAt: null },
          select: { id: true, code: true, name: true, isActive: true },
        },
      },
    });
    if (!item) {
      throw new NotFoundException('ERP item category not found');
    }
    return { success: true, data: item };
  }

  async update(id: bigint, dto: UpdateErpItemCategoryDto, actorId?: string) {
    const existing = await this.prisma.erpItemCategory.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) {
      throw new NotFoundException('ERP item category not found');
    }

    if (dto.code && dto.code !== existing.code) {
      const duplicate = await this.prisma.erpItemCategory.findFirst({
        where: { code: dto.code, NOT: { id } },
        select: { id: true, deletedAt: true },
      });
      if (duplicate) {
        throwDuplicate({
          fieldLabel: 'Item category code',
          value: dto.code,
          isSoftDeleted: Boolean(duplicate.deletedAt),
        });
      }
    }

    let updated;
    try {
      updated = await this.prisma.erpItemCategory.update({
        where: { id },
        data: {
          code: dto.code,
          name: dto.name,
          isActive: dto.isActive,
          parentId:
            dto.parentId !== undefined ? (dto.parentId ? BigInt(dto.parentId) : null) : undefined,
          inventoryAccountId:
            dto.inventoryAccountId !== undefined
              ? dto.inventoryAccountId
                ? BigInt(dto.inventoryAccountId)
                : null
              : undefined,
          cogsAccountId:
            dto.cogsAccountId !== undefined
              ? dto.cogsAccountId
                ? BigInt(dto.cogsAccountId)
                : null
              : undefined,
          salesAccountId:
            dto.salesAccountId !== undefined
              ? dto.salesAccountId
                ? BigInt(dto.salesAccountId)
                : null
              : undefined,
          salesReturnAccountId:
            dto.salesReturnAccountId !== undefined
              ? dto.salesReturnAccountId
                ? BigInt(dto.salesReturnAccountId)
                : null
              : undefined,
          salesDiscountAccountId:
            dto.salesDiscountAccountId !== undefined
              ? dto.salesDiscountAccountId
                ? BigInt(dto.salesDiscountAccountId)
                : null
              : undefined,
          purchaseReturnAccountId:
            dto.purchaseReturnAccountId !== undefined
              ? dto.purchaseReturnAccountId
                ? BigInt(dto.purchaseReturnAccountId)
                : null
              : undefined,
          purchaseDiscountAccountId:
            dto.purchaseDiscountAccountId !== undefined
              ? dto.purchaseDiscountAccountId
                ? BigInt(dto.purchaseDiscountAccountId)
                : null
              : undefined,
          consignmentAccountId:
            dto.consignmentAccountId !== undefined
              ? dto.consignmentAccountId
                ? BigInt(dto.consignmentAccountId)
                : null
              : undefined,
          updatedById: actorId ? BigInt(actorId) : null,
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', 'md_item_categories_code_key'])) {
        throwDuplicate({
          fieldLabel: 'Item category code',
          value: dto.code ?? existing.code,
        });
      }
      throw error;
    }

    return { success: true, data: updated };
  }

  async bulkUpdateStatus(dto: BulkStatusErpItemCategoryDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpItemCategory.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { isActive: dto.isActive, updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async bulkDelete(dto: BulkErpItemCategoryDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpItemCategory.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpItemCategory.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('ERP item category not found');
    }

    await this.prisma.erpItemCategory.update({
      where: { id },
      data: {
        deletedAt: new Date(),
        updatedById: actorId ? BigInt(actorId) : null,
      },
    });

    return { success: true, message: 'ERP item category deleted' };
  }
}
