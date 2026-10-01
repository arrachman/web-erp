import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { isUniqueViolation, throwDuplicate } from '../common/errors/duplicate.util';
import { PrismaService } from '../prisma/prisma.service';
import { toAuditUserId } from '../common/utils/audit-user.util';
import { CreateErpBankAccountDto } from './dto/create-erp-bank-account.dto';
import { UpdateErpBankAccountDto } from './dto/update-erp-bank-account.dto';
import {
  BulkErpBankAccountDto,
  BulkStatusErpBankAccountDto,
  QueryErpBankAccountDto,
} from './dto/query-erp-bank-account.dto';

const FIELD_LABEL = 'Bank account code';

function toBigIntOrNull(value?: string | null): bigint | null {
  if (value === undefined || value === null || String(value).trim() === '') {
    return null;
  }
  return BigInt(value);
}

@Injectable()
export class ErpBankAccountsService {
  constructor(private prisma: PrismaService) {}

  async create(dto: CreateErpBankAccountDto, actorId?: string) {
    const existing = await this.prisma.erpBankAccount.findFirst({
      where: { code: dto.code },
      select: { id: true, deletedAt: true },
    });
    if (existing) {
      throwDuplicate({
        fieldLabel: FIELD_LABEL,
        value: dto.code,
        isSoftDeleted: Boolean(existing.deletedAt),
      });
    }

    let created;
    try {
      created = await this.prisma.erpBankAccount.create({
        data: {
          code: dto.code,
          name: dto.name,
          bankName: dto.bankName,
          accountNumber: dto.accountNumber,
          accountHolder: dto.accountHolder,
          branch: dto.branch,
          currencyId: toBigIntOrNull(dto.currencyId),
          glAccountId: toBigIntOrNull(dto.glAccountId),
          swiftCode: dto.swiftCode,
          isPrimary: dto.isPrimary ?? false,
          notes: dto.notes,
          isActive: dto.isActive ?? true,
          createdById: toAuditUserId(actorId),
          updatedById: toAuditUserId(actorId),
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code'])) {
        throwDuplicate({ fieldLabel: FIELD_LABEL, value: dto.code });
      }
      throw error;
    }

    return { success: true, data: created };
  }

  async findAll(query: QueryErpBankAccountDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    const where: Prisma.ErpBankAccountWhereInput = { deletedAt: null };
    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { code: { equals: q, mode: 'insensitive' } },
        { name: { contains: q, mode: 'insensitive' } },
        { bankName: { contains: q, mode: 'insensitive' } },
        { accountNumber: { contains: q, mode: 'insensitive' } },
      ];
    }
    if (query.isActive !== undefined) {
      where.isActive = query.isActive;
    }

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpBankAccount.findMany({
        where,
        orderBy: [{ [query.sortBy ?? 'createdAt']: query.sortDir ?? 'desc' }],
        skip,
        take: limit,
      }),
      this.prisma.erpBankAccount.count({ where }),
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
    const item = await this.prisma.erpBankAccount.findFirst({
      where: { id, deletedAt: null },
    });
    if (!item) {
      throw new NotFoundException('Bank account not found');
    }
    return { success: true, data: item };
  }

  async update(id: bigint, dto: UpdateErpBankAccountDto, actorId?: string) {
    const existing = await this.prisma.erpBankAccount.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) {
      throw new NotFoundException('Bank account not found');
    }

    if (dto.code && dto.code !== existing.code) {
      const duplicate = await this.prisma.erpBankAccount.findFirst({
        where: { code: dto.code, NOT: { id } },
        select: { id: true, deletedAt: true },
      });
      if (duplicate) {
        throwDuplicate({
          fieldLabel: FIELD_LABEL,
          value: dto.code,
          isSoftDeleted: Boolean(duplicate.deletedAt),
        });
      }
    }

    let updated;
    try {
      updated = await this.prisma.erpBankAccount.update({
        where: { id },
        data: {
          code: dto.code,
          name: dto.name,
          bankName: dto.bankName,
          accountNumber: dto.accountNumber,
          accountHolder: dto.accountHolder,
          branch: dto.branch,
          currencyId: dto.currencyId !== undefined ? toBigIntOrNull(dto.currencyId) : undefined,
          glAccountId:
            dto.glAccountId !== undefined ? toBigIntOrNull(dto.glAccountId) : undefined,
          swiftCode: dto.swiftCode,
          isPrimary: dto.isPrimary,
          notes: dto.notes,
          isActive: dto.isActive,
          updatedById: toAuditUserId(actorId),
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code'])) {
        throwDuplicate({ fieldLabel: FIELD_LABEL, value: dto.code ?? existing.code });
      }
      throw error;
    }

    return { success: true, data: updated };
  }

  async bulkUpdateStatus(dto: BulkStatusErpBankAccountDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const { count } = await this.prisma.erpBankAccount.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { isActive: dto.isActive, updatedById: toAuditUserId(actorId), updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async bulkDelete(dto: BulkErpBankAccountDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const { count } = await this.prisma.erpBankAccount.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: toAuditUserId(actorId), updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpBankAccount.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('Bank account not found');
    }

    await this.prisma.erpBankAccount.update({
      where: { id },
      data: {
        deletedAt: new Date(),
        updatedById: toAuditUserId(actorId),
      },
    });

    return { success: true, message: 'Bank account deleted' };
  }
}
