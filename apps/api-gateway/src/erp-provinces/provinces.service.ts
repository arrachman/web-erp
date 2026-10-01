import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { isUniqueViolation, throwDuplicate } from '../common/errors/duplicate.util';
import { PrismaService } from '../prisma/prisma.service';
import { ErpAuditService } from '../erp-audit/erp-audit.service';
import { diffFields } from '../erp-common/utils/diff-fields.util';
import { BulkErpProvinceDto, BulkStatusErpProvinceDto } from './dto/bulk-provinces.dto';
import { CreateErpProvinceDto } from './dto/create-provinces.dto';
import { QueryErpProvinceDto } from './dto/query-provinces.dto';
import { UpdateErpProvinceDto } from './dto/update-provinces.dto';

const ENTITY = 'ErpProvince';
const FIELD_LABEL = 'Province code';
const UNIQUE_KEY = 'md_provinces_code_key';
const LABEL_ID = 'Provinsi';

@Injectable()
export class ErpProvincesService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly audit: ErpAuditService,
  ) {}

  async create(dto: CreateErpProvinceDto, actorId?: string) {
    const existing = await this.prisma.erpProvince.findFirst({
      where: { code: dto.code },
      select: { id: true, deletedAt: true },
    });
    if (existing)
      throwDuplicate({
        fieldLabel: FIELD_LABEL,
        value: dto.code,
        isSoftDeleted: Boolean(existing.deletedAt),
      });
    const actorBigInt = actorId ? BigInt(actorId) : null;
    let created;
    try {
      created = await this.prisma.erpProvince.create({
        data: {
          code: dto.code,
          name: dto.name,
          countryId: BigInt(dto.countryId),
          isActive: dto.isActive ?? true,
          createdById: actorBigInt,
          updatedById: actorBigInt,
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', UNIQUE_KEY])) {
        throwDuplicate({ fieldLabel: FIELD_LABEL, value: dto.code });
      }
      throw error;
    }
    this.audit.log({
      action: 'CREATE',
      entityName: ENTITY,
      entityId: created.id,
      summary: `${LABEL_ID} ${created.code} dibuat`,
      actorId: actorBigInt ?? undefined,
    });
    return { success: true, data: created };
  }

  async findAll(query: QueryErpProvinceDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;
    const where: Prisma.ErpProvinceWhereInput = { deletedAt: null };
    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { code: { equals: q, mode: 'insensitive' } },
        { name: { contains: q, mode: 'insensitive' } },
      ];
    }
    if (query.isActive !== undefined) where.isActive = query.isActive;
    if (query.countryId) where.countryId = BigInt(query.countryId);
    const sortBy = query.sortBy ?? 'createdAt';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpProvince.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }],
        skip,
        take: limit,
        include: { country: { select: { name: true } } },
      }),
      this.prisma.erpProvince.count({ where }),
    ]);
    const data = items.map(({ country, ...rest }) => ({
      ...rest,
      countryName: country?.name ?? null,
    }));
    return {
      success: true,
      data,
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  async findOne(id: bigint) {
    const item = await this.prisma.erpProvince.findFirst({
      where: { id, deletedAt: null },
      include: { country: { select: { name: true } } },
    });
    if (!item) throw new NotFoundException(`${ENTITY} not found`);
    const { country, ...rest } = item;
    return { success: true, data: { ...rest, countryName: country?.name ?? null } };
  }

  async update(id: bigint, dto: UpdateErpProvinceDto, actorId?: string) {
    const existing = await this.prisma.erpProvince.findFirst({ where: { id, deletedAt: null } });
    if (!existing) throw new NotFoundException(`${ENTITY} not found`);
    if (dto.code && dto.code !== existing.code) {
      const duplicate = await this.prisma.erpProvince.findFirst({
        where: { code: dto.code, NOT: { id } },
        select: { id: true, deletedAt: true },
      });
      if (duplicate)
        throwDuplicate({
          fieldLabel: FIELD_LABEL,
          value: dto.code,
          isSoftDeleted: Boolean(duplicate.deletedAt),
        });
    }
    const actorBigInt = actorId ? BigInt(actorId) : null;
    let updated;
    try {
      updated = await this.prisma.erpProvince.update({
        where: { id },
        data: {
          code: dto.code,
          name: dto.name,
          countryId: dto.countryId !== undefined ? BigInt(dto.countryId) : undefined,
          isActive: dto.isActive,
          updatedById: actorBigInt,
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', UNIQUE_KEY])) {
        throwDuplicate({ fieldLabel: FIELD_LABEL, value: dto.code ?? existing.code });
      }
      throw error;
    }
    const changes = diffFields(
      existing as unknown as Record<string, unknown>,
      updated as unknown as Record<string, unknown>,
    );
    this.audit.log({
      action: 'UPDATE',
      entityName: ENTITY,
      entityId: id,
      changes,
      summary: `${LABEL_ID} ${updated.code} diperbarui`,
      actorId: actorBigInt ?? undefined,
    });
    return { success: true, data: updated };
  }

  async bulkUpdateStatus(dto: BulkStatusErpProvinceDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpProvince.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { isActive: dto.isActive, updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async bulkDelete(dto: BulkErpProvinceDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpProvince.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpProvince.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) throw new NotFoundException(`${ENTITY} not found`);
    const actorBigInt = actorId ? BigInt(actorId) : null;
    await this.prisma.erpProvince.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actorBigInt },
    });
    this.audit.log({
      action: 'DELETE',
      entityName: ENTITY,
      entityId: id,
      summary: `${LABEL_ID} id=${id} dihapus`,
      actorId: actorBigInt ?? undefined,
    });
    return { success: true, message: `${ENTITY} deleted` };
  }
}
