import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { isUniqueViolation, throwDuplicate } from '../common/errors/duplicate.util';
import { PrismaService } from '../prisma/prisma.service';
import { ErpAuditService } from '../erp-audit/erp-audit.service';
import { diffFields } from '../erp-common/utils/diff-fields.util';
import { BulkErpBranchDto, BulkStatusErpBranchDto } from './dto/bulk-erp-branch.dto';
import { CreateErpBranchDto } from './dto/create-erp-branch.dto';
import { QueryErpBranchDto } from './dto/query-erp-branch.dto';
import { UpdateErpBranchDto } from './dto/update-erp-branch.dto';

@Injectable()
export class ErpBranchesService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly audit: ErpAuditService,
  ) {}

  async create(dto: CreateErpBranchDto, actorId?: string) {
    const existing = await this.prisma.erpBranch.findFirst({
      where: { code: dto.code },
      select: { id: true, deletedAt: true },
    });
    if (existing) {
      throwDuplicate({
        fieldLabel: 'Branch code',
        value: dto.code,
        isSoftDeleted: Boolean(existing.deletedAt),
      });
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;

    let created;
    try {
      created = await this.prisma.erpBranch.create({
        data: {
          code: dto.code,
          name: dto.name,
          addressLine1: dto.addressLine1,
          addressLine2: dto.addressLine2,
          city: dto.city,
          postalCode: dto.postalCode,
          phone: dto.phone,
          fax: dto.fax,
          notes: dto.notes,
          isActive: dto.isActive ?? true,
          createdById: actorBigInt,
          updatedById: actorBigInt,
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', 'md_branches_code_key'])) {
        throwDuplicate({ fieldLabel: 'Branch code', value: dto.code });
      }
      throw error;
    }

    this.audit.log({
      action: 'CREATE',
      entityName: 'ErpBranch',
      entityId: created.id,
      summary: `Cabang ${created.code} dibuat`,
      actorId: actorBigInt ?? undefined,
    });

    return { success: true, data: created };
  }

  async findAll(query: QueryErpBranchDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    const where: Prisma.ErpBranchWhereInput = { deletedAt: null };

    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { code: { equals: q, mode: 'insensitive' } },
        { name: { contains: q, mode: 'insensitive' } },
        { city: { contains: q, mode: 'insensitive' } },
      ];
    }

    if (query.isActive !== undefined) {
      where.isActive = query.isActive;
    }

    const sortBy = query.sortBy ?? 'createdAt';
    const sortDir = query.sortDir ?? 'desc';

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpBranch.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }],
        skip,
        take: limit,
      }),
      this.prisma.erpBranch.count({ where }),
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
    const item = await this.prisma.erpBranch.findFirst({
      where: { id, deletedAt: null },
    });
    if (!item) {
      throw new NotFoundException('ERP Branch not found');
    }
    return { success: true, data: item };
  }

  async update(id: bigint, dto: UpdateErpBranchDto, actorId?: string) {
    const existing = await this.prisma.erpBranch.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) {
      throw new NotFoundException('ERP Branch not found');
    }

    if (dto.code && dto.code !== existing.code) {
      const duplicate = await this.prisma.erpBranch.findFirst({
        where: { code: dto.code, NOT: { id } },
        select: { id: true, deletedAt: true },
      });
      if (duplicate) {
        throwDuplicate({
          fieldLabel: 'Branch code',
          value: dto.code,
          isSoftDeleted: Boolean(duplicate.deletedAt),
        });
      }
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;

    let updated;
    try {
      updated = await this.prisma.erpBranch.update({
        where: { id },
        data: {
          code: dto.code,
          name: dto.name,
          addressLine1: dto.addressLine1,
          addressLine2: dto.addressLine2,
          city: dto.city,
          postalCode: dto.postalCode,
          phone: dto.phone,
          fax: dto.fax,
          notes: dto.notes,
          isActive: dto.isActive,
          updatedById: actorBigInt,
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', 'md_branches_code_key'])) {
        throwDuplicate({ fieldLabel: 'Branch code', value: dto.code ?? existing.code });
      }
      throw error;
    }

    const changes = diffFields(
      existing as unknown as Record<string, unknown>,
      updated as unknown as Record<string, unknown>,
    );
    this.audit.log({
      action: 'UPDATE',
      entityName: 'ErpBranch',
      entityId: id,
      changes,
      summary: `Cabang ${updated.code} diperbarui`,
      actorId: actorBigInt ?? undefined,
    });

    return { success: true, data: updated };
  }

  async bulkUpdateStatus(dto: BulkStatusErpBranchDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpBranch.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { isActive: dto.isActive, updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async bulkDelete(dto: BulkErpBranchDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpBranch.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpBranch.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('ERP Branch not found');
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;

    await this.prisma.erpBranch.update({
      where: { id },
      data: {
        deletedAt: new Date(),
        updatedById: actorBigInt,
      },
    });

    this.audit.log({
      action: 'DELETE',
      entityName: 'ErpBranch',
      entityId: id,
      summary: `Cabang id=${id} dihapus`,
      actorId: actorBigInt ?? undefined,
    });

    return { success: true, message: 'ERP Branch deleted' };
  }
}
