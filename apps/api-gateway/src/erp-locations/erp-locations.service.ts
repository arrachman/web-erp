import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { isUniqueViolation, throwDuplicate } from '../common/errors/duplicate.util';
import { PrismaService } from '../prisma/prisma.service';
import { BulkErpLocationDto, BulkStatusErpLocationDto } from './dto/bulk-erp-location.dto';
import { CreateErpLocationDto } from './dto/create-erp-location.dto';
import { QueryErpLocationDto } from './dto/query-erp-location.dto';
import { UpdateErpLocationDto } from './dto/update-erp-location.dto';

@Injectable()
export class ErpLocationsService {
  constructor(private readonly prisma: PrismaService) {}

  async create(dto: CreateErpLocationDto, actorId?: string) {
    const branchId = BigInt(dto.branchId);

    const branch = await this.prisma.erpBranch.findFirst({
      where: { id: branchId, deletedAt: null },
      select: { id: true },
    });
    if (!branch) {
      throw new NotFoundException(`ERP Branch with id '${dto.branchId}' not found`);
    }

    const existing = await this.prisma.erpLocation.findFirst({
      where: { code: dto.code },
      select: { id: true, deletedAt: true },
    });
    if (existing) {
      throwDuplicate({
        fieldLabel: 'Location code',
        value: dto.code,
        isSoftDeleted: Boolean(existing.deletedAt),
      });
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;

    let created;
    try {
      created = await this.prisma.erpLocation.create({
        data: {
          code: dto.code,
          name: dto.name,
          branchId,
          addressLine1: dto.addressLine1,
          city: dto.city,
          postalCode: dto.postalCode,
          phone: dto.phone,
          notes: dto.notes,
          isActive: dto.isActive ?? true,
          createdById: actorBigInt,
          updatedById: actorBigInt,
        },
        include: { branch: { select: { id: true, code: true, name: true } } },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', 'md_locations_code_key'])) {
        throwDuplicate({ fieldLabel: 'Location code', value: dto.code });
      }
      throw error;
    }

    return { success: true, data: created };
  }

  async findAll(query: QueryErpLocationDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    const where: Prisma.ErpLocationWhereInput = { deletedAt: null };

    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { code: { equals: q, mode: 'insensitive' } },
        { name: { contains: q, mode: 'insensitive' } },
        { city: { contains: q, mode: 'insensitive' } },
      ];
    }

    if (query.branchId) {
      where.branchId = BigInt(query.branchId);
    }

    if (query.isActive !== undefined) {
      where.isActive = query.isActive;
    }

    const sortBy = query.sortBy ?? 'createdAt';
    const sortDir = query.sortDir ?? 'desc';

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpLocation.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }],
        skip,
        take: limit,
        include: { branch: { select: { id: true, code: true, name: true } } },
      }),
      this.prisma.erpLocation.count({ where }),
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
    const item = await this.prisma.erpLocation.findFirst({
      where: { id, deletedAt: null },
      include: { branch: { select: { id: true, code: true, name: true } } },
    });
    if (!item) {
      throw new NotFoundException('ERP Location not found');
    }
    return { success: true, data: item };
  }

  async update(id: bigint, dto: UpdateErpLocationDto, actorId?: string) {
    const existing = await this.prisma.erpLocation.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) {
      throw new NotFoundException('ERP Location not found');
    }

    if (dto.branchId) {
      const branchId = BigInt(dto.branchId);
      const branch = await this.prisma.erpBranch.findFirst({
        where: { id: branchId, deletedAt: null },
        select: { id: true },
      });
      if (!branch) {
        throw new NotFoundException(`ERP Branch with id '${dto.branchId}' not found`);
      }
    }

    if (dto.code && dto.code !== existing.code) {
      const duplicate = await this.prisma.erpLocation.findFirst({
        where: { code: dto.code, NOT: { id } },
        select: { id: true, deletedAt: true },
      });
      if (duplicate) {
        throwDuplicate({
          fieldLabel: 'Location code',
          value: dto.code,
          isSoftDeleted: Boolean(duplicate.deletedAt),
        });
      }
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;

    let updated;
    try {
      updated = await this.prisma.erpLocation.update({
        where: { id },
        data: {
          code: dto.code,
          name: dto.name,
          branchId: dto.branchId ? BigInt(dto.branchId) : undefined,
          addressLine1: dto.addressLine1,
          city: dto.city,
          postalCode: dto.postalCode,
          phone: dto.phone,
          notes: dto.notes,
          isActive: dto.isActive,
          updatedById: actorBigInt,
        },
        include: { branch: { select: { id: true, code: true, name: true } } },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', 'md_locations_code_key'])) {
        throwDuplicate({ fieldLabel: 'Location code', value: dto.code ?? existing.code });
      }
      throw error;
    }

    return { success: true, data: updated };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpLocation.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('ERP Location not found');
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;

    await this.prisma.erpLocation.update({
      where: { id },
      data: {
        deletedAt: new Date(),
        updatedById: actorBigInt,
      },
    });

    return { success: true, message: 'ERP Location deleted' };
  }

  async bulkUpdateStatus(dto: BulkStatusErpLocationDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpLocation.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { isActive: dto.isActive, updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async bulkDelete(dto: BulkErpLocationDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpLocation.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }
}
