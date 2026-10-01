import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { ErpAccountKind, Prisma } from '@prisma/client';
import { isUniqueViolation, throwDuplicate } from '../common/errors/duplicate.util';
import { PrismaService } from '../prisma/prisma.service';
import { ErpAuditService } from '../erp-audit/erp-audit.service';
import { diffFields } from '../erp-common/utils/diff-fields.util';
import { BulkErpOtherCostDto, BulkStatusErpOtherCostDto } from './dto/bulk-other-costs.dto';
import { CreateErpOtherCostDto } from './dto/create-other-costs.dto';
import { QueryErpOtherCostDto } from './dto/query-other-costs.dto';
import { UpdateErpOtherCostDto } from './dto/update-other-costs.dto';

const ENTITY = 'ErpOtherCost';
const FIELD_LABEL = 'Other Cost code';
const UNIQUE_KEY = 'md_other_costs_code_key';
const LABEL_ID = 'Biaya Lain';

export interface AccountSummary {
  id: bigint;
  code: string;
  name: string;
}

interface AccountBackedOtherCost {
  debitAccountId: bigint | null;
  creditAccountId: bigint | null;
}

@Injectable()
export class ErpOtherCostsService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly audit: ErpAuditService,
  ) {}

  private parseRequiredAccountId(value: string | bigint | null | undefined, message: string) {
    if (value === null || value === undefined || value === '') {
      throw new BadRequestException(message);
    }

    try {
      return typeof value === 'bigint' ? value : BigInt(value);
    } catch {
      throw new BadRequestException(message);
    }
  }

  private async assertPostableAccount(id: bigint, label: string) {
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
      throw new BadRequestException(`${label} tidak valid atau bukan akun postable`);
    }
  }

  private async prepareAccountIds(
    dto: CreateErpOtherCostDto | UpdateErpOtherCostDto,
    existing?: { debitAccountId: bigint | null; creditAccountId: bigint | null; isHPP: boolean },
  ) {
    const isHPP = dto.isHPP ?? existing?.isHPP ?? false;
    const debitSource = dto.debitAccountId !== undefined ? dto.debitAccountId : existing?.debitAccountId;
    const creditSource = dto.creditAccountId !== undefined ? dto.creditAccountId : existing?.creditAccountId;
    const debitAccountId = isHPP
      ? null
      : this.parseRequiredAccountId(debitSource, 'Akun debit wajib diisi');
    const creditAccountId = this.parseRequiredAccountId(creditSource, 'Akun kredit wajib diisi');

    if (debitAccountId) {
      await this.assertPostableAccount(debitAccountId, 'Akun debit');
    }
    await this.assertPostableAccount(creditAccountId, 'Akun kredit');

    return { debitAccountId, creditAccountId, isHPP };
  }

  private async withAccounts<T extends AccountBackedOtherCost>(items: T[]) {
    const ids = Array.from(new Set(
      items.flatMap((item) => [item.debitAccountId, item.creditAccountId])
        .filter((id): id is bigint => id !== null)
        .map((id) => id.toString()),
    )).map((id) => BigInt(id));

    if (ids.length === 0) {
      return items.map((item) => ({ ...item, debitAccount: null, creditAccount: null }));
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
      debitAccount: item.debitAccountId ? accountMap.get(item.debitAccountId.toString()) ?? null : null,
      creditAccount: item.creditAccountId ? accountMap.get(item.creditAccountId.toString()) ?? null : null,
    }));
  }

  async create(dto: CreateErpOtherCostDto, actorId?: string) {
    const existing = await this.prisma.erpOtherCost.findFirst({ where: { code: dto.code }, select: { id: true, deletedAt: true } });
    if (existing) throwDuplicate({ fieldLabel: FIELD_LABEL, value: dto.code, isSoftDeleted: Boolean(existing.deletedAt) });
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const accountIds = await this.prepareAccountIds(dto);
    let created;
    try {
      created = await this.prisma.erpOtherCost.create({
        data: {
          code: dto.code,
          name: dto.name,
          debitAccountId: accountIds.debitAccountId,
          creditAccountId: accountIds.creditAccountId,
          isHPP: accountIds.isHPP,
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

  async findAll(query: QueryErpOtherCostDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;
    const where: Prisma.ErpOtherCostWhereInput = { deletedAt: null };
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
      this.prisma.erpOtherCost.findMany({ where, orderBy: [{ [sortBy]: sortDir }], skip, take: limit }),
      this.prisma.erpOtherCost.count({ where }),
    ]);
    const data = await this.withAccounts(items);
    return { success: true, data, meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 } };
  }

  async findOne(id: bigint) {
    const item = await this.prisma.erpOtherCost.findFirst({ where: { id, deletedAt: null } });
    if (!item) throw new NotFoundException(`${ENTITY} not found`);
    const [data] = await this.withAccounts([item]);
    return { success: true, data };
  }

  async update(id: bigint, dto: UpdateErpOtherCostDto, actorId?: string) {
    const existing = await this.prisma.erpOtherCost.findFirst({ where: { id, deletedAt: null } });
    if (!existing) throw new NotFoundException(`${ENTITY} not found`);
    if (dto.code && dto.code !== existing.code) {
      const duplicate = await this.prisma.erpOtherCost.findFirst({ where: { code: dto.code, NOT: { id } }, select: { id: true, deletedAt: true } });
      if (duplicate) throwDuplicate({ fieldLabel: FIELD_LABEL, value: dto.code, isSoftDeleted: Boolean(duplicate.deletedAt) });
    }
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const accountIds = await this.prepareAccountIds(dto, existing);
    let updated;
    try {
      updated = await this.prisma.erpOtherCost.update({
        where: { id },
        data: {
          code: dto.code,
          name: dto.name,
          debitAccountId: accountIds.debitAccountId,
          creditAccountId: accountIds.creditAccountId,
          isHPP: accountIds.isHPP,
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

  async bulkUpdateStatus(dto: BulkStatusErpOtherCostDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpOtherCost.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { isActive: dto.isActive, updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async bulkDelete(dto: BulkErpOtherCostDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpOtherCost.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpOtherCost.findFirst({ where: { id, deletedAt: null }, select: { id: true } });
    if (!existing) throw new NotFoundException(`${ENTITY} not found`);
    const actorBigInt = actorId ? BigInt(actorId) : null;
    await this.prisma.erpOtherCost.update({ where: { id }, data: { deletedAt: new Date(), updatedById: actorBigInt } });
    this.audit.log({ action: 'DELETE', entityName: ENTITY, entityId: id, summary: `${LABEL_ID} id=${id} dihapus`, actorId: actorBigInt ?? undefined });
    return { success: true, message: `${ENTITY} deleted` };
  }
}
