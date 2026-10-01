import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { isUniqueViolation, throwDuplicate } from '../common/errors/duplicate.util';
import { PrismaService } from '../prisma/prisma.service';
import { CreateErpWarehouseDto } from './dto/create-erp-warehouse.dto';
import { QueryErpWarehouseDto } from './dto/query-erp-warehouse.dto';
import { UpdateErpWarehouseDto } from './dto/update-erp-warehouse.dto';

@Injectable()
export class ErpWarehousesService {
  constructor(private readonly prisma: PrismaService) {}

  async create(dto: CreateErpWarehouseDto, actorId?: string) {
    const locationId = BigInt(dto.locationId);

    const location = await this.prisma.erpLocation.findFirst({
      where: { id: locationId, deletedAt: null },
      select: { id: true },
    });
    if (!location) {
      throw new NotFoundException(`ERP Location with id '${dto.locationId}' not found`);
    }

    const existing = await this.prisma.erpWarehouse.findFirst({
      where: { code: dto.code },
      select: { id: true, deletedAt: true },
    });
    if (existing) {
      throwDuplicate({
        fieldLabel: 'Warehouse code',
        value: dto.code,
        isSoftDeleted: Boolean(existing.deletedAt),
      });
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;

    let created;
    try {
      created = await this.prisma.erpWarehouse.create({
        data: {
          code: dto.code,
          name: dto.name,
          locationId,
          allowNegativeStock: dto.allowNegativeStock ?? false,
          notes: dto.notes,
          isActive: dto.isActive ?? true,
          createdById: actorBigInt,
          updatedById: actorBigInt,
        },
        include: {
          location: {
            select: {
              id: true,
              code: true,
              name: true,
              branch: { select: { id: true, code: true, name: true } },
            },
          },
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', 'md_warehouses_code_key'])) {
        throwDuplicate({ fieldLabel: 'Warehouse code', value: dto.code });
      }
      throw error;
    }

    return { success: true, data: created };
  }

  async findAll(query: QueryErpWarehouseDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    const where: Prisma.ErpWarehouseWhereInput = { deletedAt: null };

    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { code: { equals: q, mode: 'insensitive' } },
        { name: { contains: q, mode: 'insensitive' } },
      ];
    }

    if (query.locationId) {
      where.locationId = BigInt(query.locationId);
    }

    if (query.branchId) {
      where.location = { branchId: BigInt(query.branchId) };
    }

    if (query.isActive !== undefined) {
      where.isActive = query.isActive;
    }

    const sortBy = query.sortBy ?? 'createdAt';
    const sortDir = query.sortDir ?? 'desc';

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpWarehouse.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }],
        skip,
        take: limit,
        include: {
          location: {
            select: {
              id: true,
              code: true,
              name: true,
              branch: { select: { id: true, code: true, name: true } },
            },
          },
        },
      }),
      this.prisma.erpWarehouse.count({ where }),
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
    const item = await this.prisma.erpWarehouse.findFirst({
      where: { id, deletedAt: null },
      include: {
        location: {
          select: {
            id: true,
            code: true,
            name: true,
            branch: { select: { id: true, code: true, name: true } },
          },
        },
      },
    });
    if (!item) {
      throw new NotFoundException('ERP Warehouse not found');
    }
    return { success: true, data: item };
  }

  async update(id: bigint, dto: UpdateErpWarehouseDto, actorId?: string) {
    const existing = await this.prisma.erpWarehouse.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) {
      throw new NotFoundException('ERP Warehouse not found');
    }

    if (dto.locationId) {
      const locationId = BigInt(dto.locationId);
      const location = await this.prisma.erpLocation.findFirst({
        where: { id: locationId, deletedAt: null },
        select: { id: true },
      });
      if (!location) {
        throw new NotFoundException(`ERP Location with id '${dto.locationId}' not found`);
      }
    }

    if (dto.code && dto.code !== existing.code) {
      const duplicate = await this.prisma.erpWarehouse.findFirst({
        where: { code: dto.code, NOT: { id } },
        select: { id: true, deletedAt: true },
      });
      if (duplicate) {
        throwDuplicate({
          fieldLabel: 'Warehouse code',
          value: dto.code,
          isSoftDeleted: Boolean(duplicate.deletedAt),
        });
      }
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;

    let updated;
    try {
      updated = await this.prisma.erpWarehouse.update({
        where: { id },
        data: {
          code: dto.code,
          name: dto.name,
          locationId: dto.locationId ? BigInt(dto.locationId) : undefined,
          allowNegativeStock: dto.allowNegativeStock,
          notes: dto.notes,
          isActive: dto.isActive,
          updatedById: actorBigInt,
        },
        include: {
          location: {
            select: {
              id: true,
              code: true,
              name: true,
              branch: { select: { id: true, code: true, name: true } },
            },
          },
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', 'md_warehouses_code_key'])) {
        throwDuplicate({ fieldLabel: 'Warehouse code', value: dto.code ?? existing.code });
      }
      throw error;
    }

    return { success: true, data: updated };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpWarehouse.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('ERP Warehouse not found');
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;

    await this.prisma.erpWarehouse.update({
      where: { id },
      data: {
        deletedAt: new Date(),
        updatedById: actorBigInt,
      },
    });

    return { success: true, message: 'ERP Warehouse deleted' };
  }
}
