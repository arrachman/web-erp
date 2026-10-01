import { ConflictException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { toAuditUserId } from '../common/utils/audit-user.util';
import {
  BulkErpApprovalRuleDto,
  BulkStatusErpApprovalRuleDto,
} from './dto/bulk-erp-approval-rule.dto';
import { CreateErpApprovalRuleDto } from './dto/create-erp-approval-rule.dto';
import { QueryErpApprovalRuleDto } from './dto/query-erp-approval-rule.dto';
import { UpdateErpApprovalRuleDto } from './dto/update-erp-approval-rule.dto';

@Injectable()
export class ErpApprovalRulesService {
  constructor(private prisma: PrismaService) {}

  private duplicateMessage(documentType: string, level: number) {
    return `Aturan persetujuan untuk dokumen "${documentType}" level ${level} sudah ada.`;
  }

  async create(dto: CreateErpApprovalRuleDto, actorId?: string) {
    const level = dto.level ?? 1;
    const existing = await this.prisma.erpApprovalRule.findFirst({
      where: { documentType: dto.documentType, level, deletedAt: null },
      select: { id: true },
    });
    if (existing) {
      throw new ConflictException(this.duplicateMessage(dto.documentType, level));
    }

    let created;
    try {
      created = await this.prisma.erpApprovalRule.create({
        data: {
          documentType: dto.documentType,
          name: dto.name,
          level,
          requiresApproval: dto.requiresApproval ?? true,
          minAmount: dto.minAmount != null ? new Prisma.Decimal(dto.minAmount) : null,
          approverRoleId: dto.approverRoleId ? BigInt(dto.approverRoleId) : null,
          notes: dto.notes,
          isActive: dto.isActive ?? true,
          createdById: toAuditUserId(actorId),
          updatedById: toAuditUserId(actorId),
        },
      });
    } catch (error) {
      if (
        error instanceof Prisma.PrismaClientKnownRequestError &&
        error.code === 'P2002'
      ) {
        throw new ConflictException(this.duplicateMessage(dto.documentType, level));
      }
      throw error;
    }

    return { success: true, data: this.serialize(created) };
  }

  async findAll(query: QueryErpApprovalRuleDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    const where: Prisma.ErpApprovalRuleWhereInput = { deletedAt: null };
    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { documentType: { contains: q, mode: 'insensitive' } },
        { name: { contains: q, mode: 'insensitive' } },
      ];
    }
    if (query.isActive !== undefined) {
      where.isActive = query.isActive;
    }

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpApprovalRule.findMany({
        where,
        orderBy: [{ [query.sortBy ?? 'createdAt']: query.sortDir ?? 'desc' }],
        skip,
        take: limit,
      }),
      this.prisma.erpApprovalRule.count({ where }),
    ]);

    return {
      success: true,
      data: items.map((it) => this.serialize(it)),
      meta: {
        page,
        limit,
        total,
        totalPages: Math.ceil(total / limit) || 1,
      },
    };
  }

  async findOne(id: bigint) {
    const item = await this.prisma.erpApprovalRule.findFirst({
      where: { id, deletedAt: null },
    });
    if (!item) {
      throw new NotFoundException('Approval rule not found');
    }
    return { success: true, data: this.serialize(item) };
  }

  async update(id: bigint, dto: UpdateErpApprovalRuleDto, actorId?: string) {
    const existing = await this.prisma.erpApprovalRule.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) {
      throw new NotFoundException('Approval rule not found');
    }

    const nextDocumentType = dto.documentType ?? existing.documentType;
    const nextLevel = dto.level ?? existing.level;
    const pairChanged =
      nextDocumentType !== existing.documentType || nextLevel !== existing.level;
    if (pairChanged) {
      const duplicate = await this.prisma.erpApprovalRule.findFirst({
        where: {
          documentType: nextDocumentType,
          level: nextLevel,
          deletedAt: null,
          NOT: { id },
        },
        select: { id: true },
      });
      if (duplicate) {
        throw new ConflictException(this.duplicateMessage(nextDocumentType, nextLevel));
      }
    }

    let updated;
    try {
      updated = await this.prisma.erpApprovalRule.update({
        where: { id },
        data: {
          documentType: dto.documentType,
          name: dto.name,
          level: dto.level,
          requiresApproval: dto.requiresApproval,
          minAmount:
            dto.minAmount !== undefined
              ? dto.minAmount === null || dto.minAmount === ''
                ? null
                : new Prisma.Decimal(dto.minAmount)
              : undefined,
          approverRoleId:
            dto.approverRoleId !== undefined
              ? dto.approverRoleId
                ? BigInt(dto.approverRoleId)
                : null
              : undefined,
          notes: dto.notes,
          isActive: dto.isActive,
          updatedById: toAuditUserId(actorId),
        },
      });
    } catch (error) {
      if (
        error instanceof Prisma.PrismaClientKnownRequestError &&
        error.code === 'P2002'
      ) {
        throw new ConflictException(this.duplicateMessage(nextDocumentType, nextLevel));
      }
      throw error;
    }

    return { success: true, data: this.serialize(updated) };
  }

  async bulkUpdateStatus(dto: BulkStatusErpApprovalRuleDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const { count } = await this.prisma.erpApprovalRule.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { isActive: dto.isActive, updatedById: toAuditUserId(actorId), updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async bulkDelete(dto: BulkErpApprovalRuleDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const { count } = await this.prisma.erpApprovalRule.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: toAuditUserId(actorId), updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpApprovalRule.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('Approval rule not found');
    }

    await this.prisma.erpApprovalRule.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: toAuditUserId(actorId) },
    });

    return { success: true, message: 'Approval rule deleted' };
  }

  private serialize(row: {
    id: bigint;
    documentType: string;
    name: string;
    level: number;
    requiresApproval: boolean;
    minAmount: Prisma.Decimal | null;
    approverRoleId: bigint | null;
    notes: string | null;
    isActive: boolean;
    legacyCode: string | null;
    createdAt: Date;
    updatedAt: Date;
  }) {
    return {
      id: row.id.toString(),
      documentType: row.documentType,
      name: row.name,
      level: row.level,
      requiresApproval: row.requiresApproval,
      minAmount: row.minAmount != null ? row.minAmount.toString() : null,
      approverRoleId: row.approverRoleId != null ? row.approverRoleId.toString() : null,
      notes: row.notes,
      isActive: row.isActive,
      legacyCode: row.legacyCode,
      createdAt: row.createdAt,
      updatedAt: row.updatedAt,
    };
  }
}
