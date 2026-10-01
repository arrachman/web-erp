import {
  ConflictException,
  Injectable,
  NotFoundException,
} from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { toAuditUserId } from '../common/utils/audit-user.util';
import { CREATION_STATUSES, CreateRoleDocPolicyDto } from './dto/create-role-doc-policy.dto';
import { QueryRoleDocPolicyDto } from './dto/query-role-doc-policy.dto';
import { UpdateRoleDocPolicyDto } from './dto/update-role-doc-policy.dto';
import { BulkUpsertRoleDocPolicyDto } from './dto/bulk-upsert-role-doc-policy.dto';

@Injectable()
export class ErpRoleDocPoliciesService {
  constructor(private prisma: PrismaService) {}

  async create(dto: CreateRoleDocPolicyDto, actorId?: string) {
    const roleId = BigInt(dto.roleId);
    const existing = await this.prisma.erpRoleDocPolicy.findUnique({
      where: { roleId_documentType: { roleId, documentType: dto.documentType } },
      select: { id: true, deletedAt: true },
    });
    if (existing && !existing.deletedAt) {
      throw new ConflictException(
        `Policy untuk role ${dto.roleId} + document type "${dto.documentType}" sudah ada.`,
      );
    }

    const actor = toAuditUserId(actorId);
    const data = {
      roleId,
      documentType: dto.documentType,
      allowedStatuses: dto.allowedStatuses,
      isActive: dto.isActive ?? true,
      createdById: actor,
      updatedById: actor,
    };

    let row;
    if (existing?.deletedAt) {
      row = await this.prisma.erpRoleDocPolicy.update({
        where: { id: existing.id },
        data: { ...data, deletedAt: null },
      });
    } else {
      row = await this.prisma.erpRoleDocPolicy.create({ data });
    }
    return { success: true, data: this.serialize(row) };
  }

  async findAll(query: QueryRoleDocPolicyDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 20;
    const skip = (page - 1) * limit;

    const where: Prisma.ErpRoleDocPolicyWhereInput = { deletedAt: null };
    if (query.roleId) where.roleId = BigInt(query.roleId);
    if (query.documentType) where.documentType = { contains: query.documentType, mode: 'insensitive' };
    if (query.isActive !== undefined) where.isActive = query.isActive;
    if (query.search?.trim()) {
      where.documentType = { contains: query.search.trim(), mode: 'insensitive' };
    }

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpRoleDocPolicy.findMany({
        where,
        orderBy: [{ [query.sortBy ?? 'createdAt']: query.sortDir ?? 'desc' }],
        skip,
        take: limit,
      }),
      this.prisma.erpRoleDocPolicy.count({ where }),
    ]);

    return {
      success: true,
      data: items.map((it) => this.serialize(it)),
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  async findOne(id: bigint) {
    const item = await this.prisma.erpRoleDocPolicy.findFirst({
      where: { id, deletedAt: null },
    });
    if (!item) throw new NotFoundException('Policy not found');
    return { success: true, data: this.serialize(item) };
  }

  async update(id: bigint, dto: UpdateRoleDocPolicyDto, actorId?: string) {
    const existing = await this.prisma.erpRoleDocPolicy.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) throw new NotFoundException('Policy not found');

    const updated = await this.prisma.erpRoleDocPolicy.update({
      where: { id },
      data: {
        ...(dto.allowedStatuses !== undefined && { allowedStatuses: dto.allowedStatuses }),
        ...(dto.isActive !== undefined && { isActive: dto.isActive }),
        updatedById: toAuditUserId(actorId),
      },
    });
    return { success: true, data: this.serialize(updated) };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpRoleDocPolicy.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) throw new NotFoundException('Policy not found');
    await this.prisma.erpRoleDocPolicy.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: toAuditUserId(actorId) },
    });
    return { success: true, message: 'Policy deleted' };
  }

  async listAllForRole(roleId: string) {
    const items = await this.prisma.erpRoleDocPolicy.findMany({
      where: { roleId: BigInt(roleId), deletedAt: null },
      orderBy: { documentType: 'asc' },
    });
    return { success: true, data: items.map((it) => this.serialize(it)) };
  }

  async bulkUpsert(dto: BulkUpsertRoleDocPolicyDto, actorId?: string) {
    const roleId = BigInt(dto.roleId);
    const actor = toAuditUserId(actorId);

    await this.prisma.$transaction(
      async (tx) => {
        for (const entry of dto.entries) {
          const existing = await tx.erpRoleDocPolicy.findUnique({
            where: { roleId_documentType: { roleId, documentType: entry.documentType } },
          });

          if (entry.allowedStatuses.length === 0) {
            // No statuses checked → soft-delete if exists
            if (existing && !existing.deletedAt) {
              await tx.erpRoleDocPolicy.update({
                where: { id: existing.id },
                data: { deletedAt: new Date(), updatedById: actor },
              });
            }
          } else if (existing) {
            // Upsert (restore if soft-deleted)
            await tx.erpRoleDocPolicy.update({
              where: { id: existing.id },
              data: {
                allowedStatuses: entry.allowedStatuses,
                isActive: true,
                deletedAt: null,
                updatedById: actor,
              },
            });
          } else {
            await tx.erpRoleDocPolicy.create({
              data: {
                roleId,
                documentType: entry.documentType,
                allowedStatuses: entry.allowedStatuses,
                isActive: true,
                createdById: actor,
                updatedById: actor,
              },
            });
          }
        }
      },
      { timeout: 30000 },
    );

    return { success: true, message: 'Policies updated' };
  }

  /**
   * Returns union of allowed creation statuses for a given user across all their roles.
   * If no active policy exists for the doc type, falls back to ['DRAFT'].
   */
  async getAllowedStatusesForUser(userId: string, documentType: string): Promise<string[]> {
    const userRoles = await this.prisma.erpUserRole.findMany({
      where: { userId: BigInt(userId) },
      select: { roleId: true },
    });
    if (!userRoles.length) return ['DRAFT'];

    const roleIds = userRoles.map((r) => r.roleId);
    const policies = await this.prisma.erpRoleDocPolicy.findMany({
      where: {
        roleId: { in: roleIds },
        documentType,
        isActive: true,
        deletedAt: null,
      },
      select: { allowedStatuses: true },
    });

    if (!policies.length) return ['DRAFT'];

    // Union all arrays, preserve order DRAFT < NEED_APPROVE < APPROVED
    const union = new Set<string>();
    for (const p of policies) {
      for (const s of p.allowedStatuses) union.add(s);
    }
    return CREATION_STATUSES.filter((s) => union.has(s));
  }

  private serialize(row: {
    id: bigint;
    roleId: bigint;
    documentType: string;
    allowedStatuses: string[];
    isActive: boolean;
    createdAt: Date;
    updatedAt: Date;
  }) {
    return {
      id: row.id.toString(),
      roleId: row.roleId.toString(),
      documentType: row.documentType,
      allowedStatuses: row.allowedStatuses,
      isActive: row.isActive,
      createdAt: row.createdAt,
      updatedAt: row.updatedAt,
    };
  }
}
