import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { isUniqueViolation, throwDuplicate } from '../common/errors/duplicate.util';
import { PrismaService } from '../prisma/prisma.service';
import { BulkErpPartnerTypeDto, BulkStatusErpPartnerTypeDto } from './dto/bulk-erp-partner-type.dto';
import { CreateErpPartnerTypeDto } from './dto/create-erp-partner-type.dto';
import { QueryErpPartnerTypeDto } from './dto/query-erp-partner-type.dto';
import { UpdateErpPartnerTypeDto } from './dto/update-erp-partner-type.dto';

const PROTECTED_CODES = ['CUST', 'SUP', 'SLS'];

/** System kind is derived from protected codes — not client-editable. */
function deriveKindFromCode(code: string): 'CUSTOMER' | 'SUPPLIER' | 'SALESMAN' | 'GENERAL' {
  const key = code.trim().toUpperCase();
  if (key === 'CUST') return 'CUSTOMER';
  if (key === 'SUP') return 'SUPPLIER';
  if (key === 'SLS') return 'SALESMAN';
  return 'GENERAL';
}

@Injectable()
export class ErpPartnerTypesService {
  constructor(private readonly prisma: PrismaService) {}

  async create(dto: CreateErpPartnerTypeDto, actorId?: string) {
    const existing = await this.prisma.erpPartnerType.findFirst({
      where: { code: dto.code },
      select: { id: true, deletedAt: true },
    });
    if (existing) {
      throwDuplicate({
        fieldLabel: 'Partner type code',
        value: dto.code,
        isSoftDeleted: Boolean(existing.deletedAt),
      });
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;
    const kind = deriveKindFromCode(dto.code);

    let created;
    try {
      created = await this.prisma.erpPartnerType.create({
        data: {
          code: dto.code,
          name: dto.name,
          kind,
          isActive: dto.isActive ?? true,
          createdById: actorBigInt,
          updatedById: actorBigInt,
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', 'md_partner_types_code_key'])) {
        throwDuplicate({ fieldLabel: 'Partner type code', value: dto.code });
      }
      throw error;
    }

    return { success: true, data: created };
  }

  async findAll(query: QueryErpPartnerTypeDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    // Pin locked system codes (CUST, SUP, SLS) to the top of every page,
    // then apply the caller's secondary sort. CASE cannot be expressed in
    // Prisma orderBy, so list uses $queryRaw with the same filters.
    const sortBy = query.sortBy ?? 'createdAt';
    const sortDir = query.sortDir ?? 'desc';
    const colMap: Record<string, string> = {
      code: 'code',
      name: 'name',
      isActive: 'is_active',
      createdAt: 'created_at',
    };
    const sortCol = colMap[sortBy] ?? 'created_at';
    const sortDirSql = sortDir === 'asc' ? Prisma.sql`ASC` : Prisma.sql`DESC`;

    const filters: Prisma.Sql[] = [Prisma.sql`deleted_at IS NULL`];
    if (query.search?.trim()) {
      const q = query.search.trim();
      filters.push(
        Prisma.sql`(code ILIKE ${q} OR name ILIKE ${'%' + q + '%'})`,
      );
    }
    if (query.kind !== undefined) {
      filters.push(Prisma.sql`kind = ${query.kind}::"ErpPartnerTypeKind"`);
    }
    if (query.isActive !== undefined) {
      filters.push(Prisma.sql`is_active = ${query.isActive}`);
    }
    const whereSql = Prisma.join(filters, ' AND ');

    const [items, countRows] = await this.prisma.$transaction([
      this.prisma.$queryRaw<
        Array<{
          id: bigint;
          code: string;
          name: string;
          kind: 'CUSTOMER' | 'SUPPLIER' | 'SALESMAN' | 'GENERAL';
          is_active: boolean;
          legacy_code: string | null;
          created_at: Date;
          updated_at: Date;
          created_by_id: bigint | null;
          updated_by_id: bigint | null;
          deleted_at: Date | null;
        }>
      >`
        SELECT *
        FROM md_partner_types
        WHERE ${whereSql}
        ORDER BY
          CASE code
            WHEN 'CUST' THEN 0
            WHEN 'SUP'  THEN 1
            WHEN 'SLS'  THEN 2
            ELSE 3
          END ASC,
          ${Prisma.raw(`"${sortCol}"`)} ${sortDirSql}
        OFFSET ${skip}
        LIMIT ${limit}
      `,
      this.prisma.$queryRaw<Array<{ count: bigint }>>`
        SELECT COUNT(*)::bigint AS count
        FROM md_partner_types
        WHERE ${whereSql}
      `,
    ]);

    const total = Number(countRows[0]?.count ?? 0);
    const data = items.map((row) => ({
      id: row.id,
      code: row.code,
      name: row.name,
      kind: row.kind,
      isActive: row.is_active,
      legacyCode: row.legacy_code,
      createdAt: row.created_at,
      updatedAt: row.updated_at,
      createdById: row.created_by_id,
      updatedById: row.updated_by_id,
      deletedAt: row.deleted_at,
    }));

    return {
      success: true,
      data,
      meta: {
        page,
        limit,
        total,
        totalPages: Math.ceil(total / limit) || 1,
      },
    };
  }

  async findOne(id: bigint) {
    const item = await this.prisma.erpPartnerType.findFirst({
      where: { id, deletedAt: null },
    });
    if (!item) {
      throw new NotFoundException('ERP Partner Type not found');
    }
    return { success: true, data: item };
  }

  async update(id: bigint, dto: UpdateErpPartnerTypeDto, actorId?: string) {
    const existing = await this.prisma.erpPartnerType.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) {
      throw new NotFoundException('ERP Partner Type not found');
    }

    if (PROTECTED_CODES.includes(existing.code) && dto.code && dto.code !== existing.code) {
      throw new BadRequestException(`Cannot change the code of a protected partner type.`);
    }

    if (dto.code && dto.code !== existing.code) {
      const duplicate = await this.prisma.erpPartnerType.findFirst({
        where: { code: dto.code, NOT: { id } },
        select: { id: true, deletedAt: true },
      });
      if (duplicate) {
        throwDuplicate({
          fieldLabel: 'Partner type code',
          value: dto.code,
          isSoftDeleted: Boolean(duplicate.deletedAt),
        });
      }
    }

    const actorBigInt = actorId ? BigInt(actorId) : null;
    const nextCode = dto.code ?? existing.code;
    const kind = deriveKindFromCode(nextCode);

    let updated;
    try {
      updated = await this.prisma.erpPartnerType.update({
        where: { id },
        data: {
          code: dto.code,
          name: dto.name,
          kind,
          isActive: dto.isActive,
          updatedById: actorBigInt,
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', 'md_partner_types_code_key'])) {
        throwDuplicate({ fieldLabel: 'Partner type code', value: dto.code ?? existing.code });
      }
      throw error;
    }

    return { success: true, data: updated };
  }

  async bulkUpdateStatus(dto: BulkStatusErpPartnerTypeDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpPartnerType.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { isActive: dto.isActive, updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async bulkDelete(dto: BulkErpPartnerTypeDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));

    // Exclude protected types from deletion
    const toDelete = await this.prisma.erpPartnerType.findMany({
      where: {
        id: { in: ids },
        code: { notIn: PROTECTED_CODES },
        deletedAt: null,
      },
      select: { id: true }
    });

    const deleteIds = toDelete.map(t => t.id);

    if (deleteIds.length === 0) {
      return { success: true, affected: 0 };
    }

    await this.assertNoPartnerUsage(deleteIds);
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpPartnerType.updateMany({
      where: { id: { in: deleteIds } },
      data: { deletedAt: new Date(), updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpPartnerType.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) {
      throw new NotFoundException('ERP Partner Type not found');
    }

    if (PROTECTED_CODES.includes(existing.code)) {
      throw new BadRequestException('Cannot delete a protected partner type.');
    }

    await this.assertNoPartnerUsage([id]);
    const actorBigInt = actorId ? BigInt(actorId) : null;

    await this.prisma.erpPartnerType.update({
      where: { id },
      data: {
        deletedAt: new Date(),
        updatedById: actorBigInt,
      },
    });

    return { success: true, message: 'ERP Partner Type deleted' };
  }

  private async assertNoPartnerUsage(ids: bigint[]) {
    const used = await this.prisma.erpPartner.count({
      where: { partnerTypeId: { in: ids }, deletedAt: null },
    });
    if (used > 0) {
      throw new BadRequestException('Partner type is still used by active partners');
    }
  }
}
