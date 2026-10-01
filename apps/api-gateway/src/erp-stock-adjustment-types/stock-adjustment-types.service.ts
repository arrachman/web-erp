import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { ErpAccountKind, Prisma } from '@prisma/client';
import { isUniqueViolation, throwDuplicate } from '../common/errors/duplicate.util';
import { PrismaService } from '../prisma/prisma.service';
import { ErpAuditService } from '../erp-audit/erp-audit.service';
import { diffFields } from '../erp-common/utils/diff-fields.util';
import { BulkErpStockAdjustmentTypeDto, BulkStatusErpStockAdjustmentTypeDto } from './dto/bulk-stock-adjustment-types.dto';
import { CreateErpStockAdjustmentTypeDto } from './dto/create-stock-adjustment-types.dto';
import { QueryErpStockAdjustmentTypeDto } from './dto/query-stock-adjustment-types.dto';
import { UpdateErpStockAdjustmentTypeDto } from './dto/update-stock-adjustment-types.dto';

const ENTITY = 'ErpStockAdjustmentType';
const FIELD_LABEL = 'Stock Adjustment Type code';
const UNIQUE_KEY = 'md_stock_adjustment_types_code_key';
const LABEL_ID = 'Tipe Penyesuaian Stok';

export interface AccountSummary {
  id: bigint;
  code: string;
  name: string;
}

interface AccountBackedStockAdjType {
  accountId: bigint | null;
}

@Injectable()
export class ErpStockAdjustmentTypesService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly audit: ErpAuditService,
  ) {}

  private parseOptionalAccountId(value: string | bigint | null | undefined) {
    if (value === null || value === undefined || value === '') return null;
    try {
      return typeof value === 'bigint' ? value : BigInt(value);
    } catch {
      throw new BadRequestException('No Akun tidak valid');
    }
  }

  private async assertPostableAccount(id: bigint) {
    const account = await this.prisma.erpAccount.findFirst({
      where: {
        id,
        deletedAt: null,
        isActive: true,
        kind: ErpAccountKind.POSTABLE,
      },
      select: { id: true },
    });
    if (!account) {
      throw new BadRequestException('No Akun tidak valid atau bukan akun postable');
    }
  }

  private async resolveAccountId(
    dto: CreateErpStockAdjustmentTypeDto | UpdateErpStockAdjustmentTypeDto,
    existing?: { accountId: bigint | null },
  ) {
    if (dto.accountId === undefined) {
      return existing?.accountId ?? null;
    }
    const accountId = this.parseOptionalAccountId(dto.accountId);
    if (accountId) {
      await this.assertPostableAccount(accountId);
    }
    return accountId;
  }

  private async withAccounts<T extends AccountBackedStockAdjType>(items: T[]) {
    const ids = Array.from(
      new Set(
        items
          .map((item) => item.accountId)
          .filter((id): id is bigint => id !== null)
          .map((id) => id.toString()),
      ),
    ).map((id) => BigInt(id));

    if (ids.length === 0) {
      return items.map((item) => ({ ...item, account: null }));
    }

    const accounts = await this.prisma.erpAccount.findMany({
      where: { id: { in: ids }, deletedAt: null },
      select: { id: true, code: true, name: true },
    });
    const accountMap = new Map<string, AccountSummary>(
      accounts.map((account) => [account.id.toString(), account]),
    );

    return items.map((item) => ({
      ...item,
      account: item.accountId ? accountMap.get(item.accountId.toString()) ?? null : null,
    }));
  }

  async create(dto: CreateErpStockAdjustmentTypeDto, actorId?: string) {
    const existing = await this.prisma.erpStockAdjustmentType.findFirst({
      where: { code: dto.code },
      select: { id: true, deletedAt: true },
    });
    if (existing) throwDuplicate({ fieldLabel: FIELD_LABEL, value: dto.code, isSoftDeleted: Boolean(existing.deletedAt) });
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const accountId = await this.resolveAccountId(dto);
    let created;
    try {
      created = await this.prisma.erpStockAdjustmentType.create({
        data: {
          code: dto.code,
          name: dto.name,
          direction: dto.direction,
          accountId,
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
      action: 'CREATE', entityName: ENTITY, entityId: created.id,
      summary: `${LABEL_ID} ${created.code} dibuat`, actorId: actorBigInt ?? undefined,
    });
    const [data] = await this.withAccounts([created]);
    return { success: true, data };
  }

  async findAll(query: QueryErpStockAdjustmentTypeDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;
    const where: Prisma.ErpStockAdjustmentTypeWhereInput = { deletedAt: null };
    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { code: { equals: q, mode: 'insensitive' } },
        { name: { contains: q, mode: 'insensitive' } },
      ];
    }
    if (query.isActive !== undefined) where.isActive = query.isActive;
    const sortBy = query.sortBy ?? 'createdAt';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpStockAdjustmentType.findMany({ where, orderBy: [{ [sortBy]: sortDir }], skip, take: limit }),
      this.prisma.erpStockAdjustmentType.count({ where }),
    ]);
    const data = await this.withAccounts(items);
    return { success: true, data, meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 } };
  }

  async findOne(id: bigint) {
    const item = await this.prisma.erpStockAdjustmentType.findFirst({ where: { id, deletedAt: null } });
    if (!item) throw new NotFoundException(`${ENTITY} not found`);
    const [data] = await this.withAccounts([item]);
    return { success: true, data };
  }

  async update(id: bigint, dto: UpdateErpStockAdjustmentTypeDto, actorId?: string) {
    const existing = await this.prisma.erpStockAdjustmentType.findFirst({ where: { id, deletedAt: null } });
    if (!existing) throw new NotFoundException(`${ENTITY} not found`);
    if (dto.code && dto.code !== existing.code) {
      const duplicate = await this.prisma.erpStockAdjustmentType.findFirst({
        where: { code: dto.code, NOT: { id } },
        select: { id: true, deletedAt: true },
      });
      if (duplicate) throwDuplicate({ fieldLabel: FIELD_LABEL, value: dto.code, isSoftDeleted: Boolean(duplicate.deletedAt) });
    }
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const accountId = await this.resolveAccountId(dto, existing);
    let updated;
    try {
      updated = await this.prisma.erpStockAdjustmentType.update({
        where: { id },
        data: {
          code: dto.code,
          name: dto.name,
          direction: dto.direction,
          accountId,
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
    const changes = diffFields(existing as unknown as Record<string, unknown>, updated as unknown as Record<string, unknown>);
    this.audit.log({
      action: 'UPDATE', entityName: ENTITY, entityId: id, changes,
      summary: `${LABEL_ID} ${updated.code} diperbarui`, actorId: actorBigInt ?? undefined,
    });
    const [data] = await this.withAccounts([updated]);
    return { success: true, data };
  }

  async bulkUpdateStatus(dto: BulkStatusErpStockAdjustmentTypeDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpStockAdjustmentType.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { isActive: dto.isActive, updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async bulkDelete(dto: BulkErpStockAdjustmentTypeDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpStockAdjustmentType.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpStockAdjustmentType.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) throw new NotFoundException(`${ENTITY} not found`);
    const actorBigInt = actorId ? BigInt(actorId) : null;
    await this.prisma.erpStockAdjustmentType.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actorBigInt },
    });
    this.audit.log({
      action: 'DELETE', entityName: ENTITY, entityId: id,
      summary: `${LABEL_ID} id=${id} dihapus`, actorId: actorBigInt ?? undefined,
    });
    return { success: true, message: `${ENTITY} deleted` };
  }
}
