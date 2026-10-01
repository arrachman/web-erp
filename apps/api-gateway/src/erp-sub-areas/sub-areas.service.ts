import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { isUniqueViolation, throwDuplicate } from '../common/errors/duplicate.util';
import { PrismaService } from '../prisma/prisma.service';
import { ErpAuditService } from '../erp-audit/erp-audit.service';
import { diffFields } from '../erp-common/utils/diff-fields.util';
import { BulkErpSubAreaDto, BulkStatusErpSubAreaDto } from './dto/bulk-sub-areas.dto';
import { CreateErpSubAreaDto } from './dto/create-sub-areas.dto';
import { QueryErpSubAreaDto } from './dto/query-sub-areas.dto';
import { UpdateErpSubAreaDto } from './dto/update-sub-areas.dto';

const ENTITY = 'ErpSubArea';
const FIELD_LABEL = 'Sub-area code';
const UNIQUE_KEY = 'md_sub_areas_code_key';
const LABEL_ID = 'Kelurahan';

@Injectable()
export class ErpSubAreasService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly audit: ErpAuditService,
  ) {}

  async create(dto: CreateErpSubAreaDto, actorId?: string) {
    const existing = await this.prisma.erpSubArea.findFirst({
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
      created = await this.prisma.erpSubArea.create({
        data: {
          code: dto.code,
          name: dto.name,
          areaId: BigInt(dto.areaId),
          postalCode: dto.postalCode,
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

  async findAll(query: QueryErpSubAreaDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;
    const where: Prisma.ErpSubAreaWhereInput = { deletedAt: null };
    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { code: { equals: q, mode: 'insensitive' } },
        { name: { contains: q, mode: 'insensitive' } },
      ];
    }
    if (query.isActive !== undefined) where.isActive = query.isActive;
    if (query.areaId) where.areaId = BigInt(query.areaId);
    const sortBy = query.sortBy ?? 'createdAt';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpSubArea.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }],
        skip,
        take: limit,
        select: {
          id: true,
          code: true,
          name: true,
          areaId: true,
          postalCode: true,
          isActive: true,
          createdAt: true,
          updatedAt: true,
        },
      }),
      this.prisma.erpSubArea.count({ where }),
    ]);
    return {
      success: true,
      data: items,
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  async findOne(id: bigint) {
    const item = await this.prisma.erpSubArea.findFirst({ where: { id, deletedAt: null } });
    if (!item) throw new NotFoundException(`${ENTITY} not found`);
    return { success: true, data: item };
  }

  async update(id: bigint, dto: UpdateErpSubAreaDto, actorId?: string) {
    const existing = await this.prisma.erpSubArea.findFirst({ where: { id, deletedAt: null } });
    if (!existing) throw new NotFoundException(`${ENTITY} not found`);
    if (dto.code && dto.code !== existing.code) {
      const duplicate = await this.prisma.erpSubArea.findFirst({
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
      updated = await this.prisma.erpSubArea.update({
        where: { id },
        data: {
          code: dto.code,
          name: dto.name,
          areaId: dto.areaId !== undefined ? BigInt(dto.areaId) : undefined,
          postalCode: dto.postalCode,
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

  async bulkUpdateStatus(dto: BulkStatusErpSubAreaDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpSubArea.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { isActive: dto.isActive, updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async bulkDelete(dto: BulkErpSubAreaDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpSubArea.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpSubArea.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) throw new NotFoundException(`${ENTITY} not found`);
    const actorBigInt = actorId ? BigInt(actorId) : null;
    await this.prisma.erpSubArea.update({
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
