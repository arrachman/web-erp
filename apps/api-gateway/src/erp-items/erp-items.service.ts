import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { isUniqueViolation, throwDuplicate } from '../common/errors/duplicate.util';
import { PrismaService } from '../prisma/prisma.service';
import { ErpAuditService } from '../erp-audit/erp-audit.service';
import { diffFields } from '../erp-common/utils/diff-fields.util';
import { BulkErpItemDto, BulkStatusErpItemDto } from './dto/bulk-erp-item.dto';
import { CreateErpItemDto } from './dto/create-erp-item.dto';
import { QueryErpItemDto } from './dto/query-erp-item.dto';
import { UpdateErpItemDto } from './dto/update-erp-item.dto';

import {
  ITEM_INCLUDE,
  buildPriceRows,
  buildDistributorRows,
  buildWarehouseStockRows,
  buildDimRows,
  buildDimSync,
  buildFkData,
  buildDecimalData,
  buildItemMetadata,
  deriveSalePriceFromTiers,
  mapItem,
} from './erp-items.mappers';

@Injectable()
export class ErpItemsService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly audit: ErpAuditService,
  ) {}

  async create(dto: CreateErpItemDto, actorId?: string) {
    const existing = await this.prisma.erpItem.findFirst({
      where: { code: dto.code },
      select: { id: true, deletedAt: true },
    });
    if (existing) {
      throwDuplicate({
        fieldLabel: 'Item code',
        value: dto.code,
        isSoftDeleted: Boolean(existing.deletedAt),
      });
    }

    let created;
    try {
      created = await this.prisma.erpItem.create({
        data: {
          code: dto.code,
          name: dto.name,
          type: dto.itemType,
          costMethod: dto.costMethod ?? undefined,
          categoryId: BigInt(dto.categoryId),
          baseUnitId: BigInt(dto.unitId),
          barcode: dto.barcode ?? null,
          ...buildDecimalData(dto as unknown as Record<string, unknown>),
          tracksSerial: dto.tracksSerial ?? false,
          tracksBatch: dto.tracksBatch ?? false,
          tracksBin: dto.tracksBin ?? false,
          ageCategory: dto.ageCategory ?? null,
          validUntil: dto.validUntil ? new Date(dto.validUntil) : null,
          isVatable: dto.isVatable ?? true,
          isSpecial: dto.isSpecial ?? false,
          registrationNo: dto.registrationNo ?? null,
          isReturnable: dto.isReturnable ?? true,
          isMobile: dto.isMobile ?? false,
          ...buildFkData(dto as unknown as Record<string, unknown>),
          // Multi-select GL dims: single columns mirror the first selection.
          ...buildDimSync(dto),
          // salePrice cache = level-1 tier price (§2.32) — overrides client-sent salePrice.
          ...(deriveSalePriceFromTiers(dto.prices) !== undefined
            ? { salePrice: deriveSalePriceFromTiers(dto.prices) }
            : {}),
          isActive: dto.isActive ?? true,
          // Lain-lain + Custom tabs → md_items.metadata (§2.38).
          ...(buildItemMetadata(dto) !== undefined ? { metadata: buildItemMetadata(dto) } : {}),
          createdById: actorId ? BigInt(actorId) : null,
          updatedById: actorId ? BigInt(actorId) : null,
          ...(buildPriceRows(dto.prices, actorId)
            ? { prices: { create: buildPriceRows(dto.prices, actorId) } }
            : {}),
          ...(buildDimRows(dto.branchIds, 'branchId')
            ? { dimBranches: { create: buildDimRows(dto.branchIds, 'branchId') } }
            : {}),
          ...(buildDimRows(dto.defaultWarehouseIds, 'warehouseId')
            ? { dimWarehouses: { create: buildDimRows(dto.defaultWarehouseIds, 'warehouseId') } }
            : {}),
          ...(buildDimRows(dto.defaultLocationIds, 'locationId')
            ? { dimLocations: { create: buildDimRows(dto.defaultLocationIds, 'locationId') } }
            : {}),
          ...(buildDistributorRows(dto.distributors, actorId)
            ? { distributors: { create: buildDistributorRows(dto.distributors, actorId) } }
            : {}),
          ...(buildWarehouseStockRows(dto.warehouseStocks, actorId)
            ? { warehouseStocks: { create: buildWarehouseStockRows(dto.warehouseStocks, actorId) } }
            : {}),
        },
        include: ITEM_INCLUDE,
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', 'md_items_code_key'])) {
        throwDuplicate({ fieldLabel: 'Item code', value: dto.code });
      }
      throw error;
    }

    this.audit.log({
      action: 'CREATE',
      entityName: 'ErpItem',
      entityId: created.id,
      summary: `Item ${created.code} dibuat`,
      actorId: actorId ? BigInt(actorId) : undefined,
    });

    return { success: true, data: mapItem(created) };
  }

  async findAll(query: QueryErpItemDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    const where: Prisma.ErpItemWhereInput = { deletedAt: null };

    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { code: { equals: q, mode: 'insensitive' } },
        { name: { contains: q, mode: 'insensitive' } },
        { barcode: { contains: q, mode: 'insensitive' } },
      ];
    }

    if (query.itemType !== undefined) where.type = query.itemType;
    if (query.categoryId !== undefined) where.categoryId = BigInt(query.categoryId);
    if (query.unitId !== undefined) where.baseUnitId = BigInt(query.unitId);
    if (query.kindId !== undefined) where.kindId = BigInt(query.kindId);
    if (query.productClassId !== undefined) where.productClassId = BigInt(query.productClassId);
    if (query.branchId !== undefined) where.branchId = BigInt(query.branchId);
    if (query.defaultWarehouseId !== undefined)
      where.defaultWarehouseId = BigInt(query.defaultWarehouseId);
    if (query.isActive !== undefined) where.isActive = query.isActive;

    const sortField = query.sortBy ?? 'createdAt';
    const sortDir = query.sortDir ?? 'desc';

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpItem.findMany({
        where,
        include: ITEM_INCLUDE,
        orderBy: [{ [sortField]: sortDir }],
        skip,
        take: limit,
      }),
      this.prisma.erpItem.count({ where }),
    ]);

    return {
      success: true,
      data: items.map(mapItem),
      meta: {
        page,
        limit,
        total,
        totalPages: Math.ceil(total / limit) || 1,
      },
    };
  }

  async findOne(id: bigint) {
    const item = await this.prisma.erpItem.findFirst({
      where: { id, deletedAt: null },
      include: ITEM_INCLUDE,
    });
    if (!item) {
      throw new NotFoundException('ERP item not found');
    }
    return { success: true, data: mapItem(item) };
  }

  async update(id: bigint, dto: UpdateErpItemDto, actorId?: string) {
    const existing = await this.prisma.erpItem.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) {
      throw new NotFoundException('ERP item not found');
    }

    if (dto.code && dto.code !== existing.code) {
      const duplicate = await this.prisma.erpItem.findFirst({
        where: { code: dto.code, NOT: { id } },
        select: { id: true, deletedAt: true },
      });
      if (duplicate) {
        throwDuplicate({
          fieldLabel: 'Item code',
          value: dto.code,
          isSoftDeleted: Boolean(duplicate.deletedAt),
        });
      }
    }

    let updated;
    try {
      updated = await this.prisma.erpItem.update({
        where: { id },
        data: {
          code: dto.code,
          name: dto.name,
          type: dto.itemType,
          costMethod: dto.costMethod,
          categoryId: dto.categoryId ? BigInt(dto.categoryId) : undefined,
          baseUnitId: dto.unitId ? BigInt(dto.unitId) : undefined,
          barcode: dto.barcode,
          ...buildDecimalData(dto as unknown as Record<string, unknown>),
          tracksSerial: dto.tracksSerial,
          tracksBatch: dto.tracksBatch,
          tracksBin: dto.tracksBin,
          ageCategory: dto.ageCategory,
          validUntil:
            dto.validUntil === undefined
              ? undefined
              : dto.validUntil === null || dto.validUntil === ''
                ? null
                : new Date(dto.validUntil),
          isVatable: dto.isVatable,
          isSpecial: dto.isSpecial,
          registrationNo: dto.registrationNo,
          isReturnable: dto.isReturnable,
          isMobile: dto.isMobile,
          ...buildFkData(dto as unknown as Record<string, unknown>),
          // Multi-select GL dims: single columns mirror the first selection.
          ...buildDimSync(dto),
          // salePrice cache = level-1 tier price (§2.32) — overrides client-sent salePrice.
          ...(deriveSalePriceFromTiers(dto.prices) !== undefined
            ? { salePrice: deriveSalePriceFromTiers(dto.prices) }
            : {}),
          isActive: dto.isActive,
          // Lain-lain + Custom tabs → md_items.metadata, merged onto existing (§2.38).
          ...(buildItemMetadata(dto, existing.metadata) !== undefined
            ? { metadata: buildItemMetadata(dto, existing.metadata) }
            : {}),
          updatedById: actorId ? BigInt(actorId) : null,
          ...(dto.prices !== undefined
            ? { prices: { deleteMany: {}, create: buildPriceRows(dto.prices, actorId) } }
            : {}),
          ...(dto.distributors !== undefined
            ? {
                distributors: {
                  deleteMany: {},
                  create: buildDistributorRows(dto.distributors, actorId),
                },
              }
            : {}),
          ...(dto.warehouseStocks !== undefined
            ? {
                warehouseStocks: {
                  deleteMany: {},
                  create: buildWarehouseStockRows(dto.warehouseStocks, actorId),
                },
              }
            : {}),
          ...(dto.branchIds !== undefined
            ? { dimBranches: { deleteMany: {}, create: buildDimRows(dto.branchIds, 'branchId') } }
            : {}),
          ...(dto.defaultWarehouseIds !== undefined
            ? {
                dimWarehouses: {
                  deleteMany: {},
                  create: buildDimRows(dto.defaultWarehouseIds, 'warehouseId'),
                },
              }
            : {}),
          ...(dto.defaultLocationIds !== undefined
            ? {
                dimLocations: {
                  deleteMany: {},
                  create: buildDimRows(dto.defaultLocationIds, 'locationId'),
                },
              }
            : {}),
        },
        include: ITEM_INCLUDE,
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', 'md_items_code_key'])) {
        throwDuplicate({ fieldLabel: 'Item code', value: dto.code ?? existing.code });
      }
      throw error;
    }

    const changes = diffFields(
      existing as unknown as Record<string, unknown>,
      updated as unknown as Record<string, unknown>,
    );
    this.audit.log({
      action: 'UPDATE',
      entityName: 'ErpItem',
      entityId: id,
      changes,
      summary: `Item ${updated.code} diperbarui`,
      actorId: actorId ? BigInt(actorId) : undefined,
    });

    return { success: true, data: mapItem(updated) };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpItem.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('ERP item not found');
    }

    await this.prisma.erpItem.update({
      where: { id },
      data: {
        deletedAt: new Date(),
        updatedById: actorId ? BigInt(actorId) : null,
      },
    });

    this.audit.log({
      action: 'DELETE',
      entityName: 'ErpItem',
      entityId: id,
      summary: `Item id=${id} dihapus`,
      actorId: actorId ? BigInt(actorId) : undefined,
    });

    return { success: true, message: 'ERP item deleted' };
  }

  async bulkUpdateStatus(dto: BulkStatusErpItemDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpItem.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { isActive: dto.isActive, updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async bulkDelete(dto: BulkErpItemDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpItem.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }
}
