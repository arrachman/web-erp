import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { BulkErpLanguageDeleteDto, BulkErpLanguageStatusDto } from './dto/bulk-erp-language.dto';
import { CreateErpLanguageDto } from './dto/create-erp-language.dto';
import { QueryErpLanguageDto } from './dto/query-erp-language.dto';
import { UpdateErpLanguageDto } from './dto/update-erp-language.dto';

const ENTITY = 'ErpLanguage';

@Injectable()
export class ErpLanguagesService {
  constructor(private readonly prisma: PrismaService) {}

  async findAll(query: QueryErpLanguageDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    const where: Prisma.ErpLanguageWhereInput = { deletedAt: null };
    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { code: { equals: q, mode: 'insensitive' } },
        { name: { contains: q, mode: 'insensitive' } },
        { nativeName: { contains: q, mode: 'insensitive' } },
      ];
    }
    if (query.isActive !== undefined) where.isActive = query.isActive;

    const sortBy = query.sortBy ?? 'createdAt';
    const sortDir = query.sortDir ?? 'desc';

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpLanguage.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }],
        skip,
        take: limit,
      }),
      this.prisma.erpLanguage.count({ where }),
    ]);

    return {
      success: true,
      data: items,
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  async findOne(id: bigint) {
    const item = await this.prisma.erpLanguage.findFirst({ where: { id, deletedAt: null } });
    if (!item) throw new NotFoundException(`${ENTITY} not found`);
    return { success: true, data: item };
  }

  async create(dto: CreateErpLanguageDto, actorId?: string) {
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const created = await this.prisma.erpLanguage.create({
      data: {
        code: dto.code,
        name: dto.name,
        nativeName: dto.nativeName,
        isRtl: dto.isRtl ?? false,
        isDefault: dto.isDefault ?? false,
        isActive: dto.isActive ?? true,
        createdById: actorBigInt,
        updatedById: actorBigInt,
      },
    });
    return { success: true, data: created };
  }

  async update(id: bigint, dto: UpdateErpLanguageDto, actorId?: string) {
    const existing = await this.prisma.erpLanguage.findFirst({ where: { id, deletedAt: null } });
    if (!existing) throw new NotFoundException(`${ENTITY} not found`);

    const actorBigInt = actorId ? BigInt(actorId) : null;
    const updated = await this.prisma.erpLanguage.update({
      where: { id },
      data: {
        code: dto.code,
        name: dto.name,
        nativeName: dto.nativeName,
        isRtl: dto.isRtl,
        isDefault: dto.isDefault,
        isActive: dto.isActive,
        updatedById: actorBigInt,
      },
    });
    return { success: true, data: updated };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpLanguage.findFirst({ where: { id, deletedAt: null }, select: { id: true } });
    if (!existing) throw new NotFoundException(`${ENTITY} not found`);

    const actorBigInt = actorId ? BigInt(actorId) : null;
    await this.prisma.erpLanguage.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actorBigInt },
    });
    return { success: true, message: `${ENTITY} deleted` };
  }

  async bulkUpdateStatus(dto: BulkErpLanguageStatusDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpLanguage.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { isActive: dto.isActive, updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async bulkDelete(dto: BulkErpLanguageDeleteDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpLanguage.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }
}
