import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { isUniqueViolation, throwDuplicate } from '../common/errors/duplicate.util';
import { PrismaService } from '../prisma/prisma.service';
import { BulkErpUnitDto, BulkStatusErpUnitDto } from './dto/bulk-erp-unit.dto';
import { CreateErpUnitDto } from './dto/create-erp-unit.dto';
import { QueryErpUnitDto } from './dto/query-erp-unit.dto';
import { UpdateErpUnitDto } from './dto/update-erp-unit.dto';

@Injectable()
export class ErpUnitsService {
  constructor(private readonly prisma: PrismaService) {}

  async create(dto: CreateErpUnitDto, actorId?: string) {
    const existing = await this.prisma.erpUnit.findFirst({
      where: { code: dto.code },
      select: { id: true, deletedAt: true },
    });
    if (existing) {
      throwDuplicate({
        fieldLabel: 'Unit code',
        value: dto.code,
        isSoftDeleted: Boolean(existing.deletedAt),
      });
    }

    let created;
    try {
      created = await this.prisma.erpUnit.create({
        data: {
          code: dto.code,
          name: dto.name,
          isActive: dto.isActive ?? true,
          conversionFactor: dto.conversionFactor ? new Prisma.Decimal(dto.conversionFactor) : undefined,
          createdById: actorId ? BigInt(actorId) : null,
          updatedById: actorId ? BigInt(actorId) : null,
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', 'md_units_code_key'])) {
        throwDuplicate({ fieldLabel: 'Unit code', value: dto.code });
      }
      throw error;
    }

    return { success: true, data: created };
  }

  async findAll(query: QueryErpUnitDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    const where: Prisma.ErpUnitWhereInput = { deletedAt: null };

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

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpUnit.findMany({
        where,
        orderBy: [{ [query.sortBy ?? 'createdAt']: query.sortDir ?? 'desc' }],
        skip,
        take: limit,
      }),
      this.prisma.erpUnit.count({ where }),
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
    const item = await this.prisma.erpUnit.findFirst({
      where: { id, deletedAt: null },
    });
    if (!item) {
      throw new NotFoundException('ERP unit not found');
    }
    return { success: true, data: item };
  }

  async update(id: bigint, dto: UpdateErpUnitDto, actorId?: string) {
    const existing = await this.prisma.erpUnit.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) {
      throw new NotFoundException('ERP unit not found');
    }

    if (dto.code && dto.code !== existing.code) {
      const duplicate = await this.prisma.erpUnit.findFirst({
        where: { code: dto.code, NOT: { id } },
        select: { id: true, deletedAt: true },
      });
      if (duplicate) {
        throwDuplicate({
          fieldLabel: 'Unit code',
          value: dto.code,
          isSoftDeleted: Boolean(duplicate.deletedAt),
        });
      }
    }

    let updated;
    try {
      updated = await this.prisma.erpUnit.update({
        where: { id },
        data: {
          code: dto.code,
          name: dto.name,
          isActive: dto.isActive,
          conversionFactor: dto.conversionFactor ? new Prisma.Decimal(dto.conversionFactor) : undefined,
          updatedById: actorId ? BigInt(actorId) : null,
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', 'md_units_code_key'])) {
        throwDuplicate({ fieldLabel: 'Unit code', value: dto.code ?? existing.code });
      }
      throw error;
    }

    return { success: true, data: updated };
  }

  async bulkUpdateStatus(dto: BulkStatusErpUnitDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpUnit.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { isActive: dto.isActive, updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async bulkDelete(dto: BulkErpUnitDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpUnit.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpUnit.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('ERP unit not found');
    }

    await this.prisma.erpUnit.update({
      where: { id },
      data: {
        deletedAt: new Date(),
        updatedById: actorId ? BigInt(actorId) : null,
      },
    });

    return { success: true, message: 'ERP unit deleted' };
  }
}
