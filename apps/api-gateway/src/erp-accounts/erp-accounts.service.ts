import {
  BadRequestException,
  ConflictException,
  Injectable,
  NotFoundException,
} from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { isUniqueViolation, throwDuplicate } from '../common/errors/duplicate.util';
import { PrismaService } from '../prisma/prisma.service';
import { toAuditUserId } from '../common/utils/audit-user.util';
import {
  AccountCodeFormat,
  buildAccountCodeFormat,
  parseSegments,
  parseSeparator,
  validateAccountCode,
} from './account-code-format';
import { toBigIntId, toOptionalBigIntId } from './account-hierarchy';
import { ACCOUNT_DIM_INCLUDE, buildAccountDimRows } from './account-dim';
import { ErpAccountsHierarchy } from './erp-accounts.hierarchy';
import { ErpAccountKind, ErpAccountType } from '@prisma/client';
import { BulkErpAccountDto, BulkStatusErpAccountDto } from './dto/bulk-erp-account.dto';
import { CreateErpAccountDto } from './dto/create-erp-account.dto';
import { QueryErpAccountDto } from './dto/query-erp-account.dto';
import { UpdateErpAccountDto } from './dto/update-erp-account.dto';

const SETTING_GROUP = 'account-code';
const KEY_SEGMENTS = 'account_code_segments';
const KEY_SEPARATOR = 'account_code_separator';

@Injectable()
export class ErpAccountsService {
  constructor(
    private prisma: PrismaService,
    private hierarchy: ErpAccountsHierarchy,
  ) {}

  async getCodeFormat(): Promise<{
    segments: number[];
    separator: string;
    patternSource: string;
    maxLength: number;
    example: string;
    accountCount: number;
    locked: boolean;
  }> {
    const format = await this.loadFormat();
    const accountCount = await this.prisma.erpAccount.count({
      where: { deletedAt: null },
    });
    return {
      segments: format.segments,
      separator: format.separator,
      patternSource: format.patternSource,
      maxLength: format.maxLength,
      example: format.example,
      accountCount,
      locked: accountCount > 0,
    };
  }

  async updateCodeFormat(
    segments: number[],
    separator: string,
    actorId?: string,
  ): Promise<{
    segments: number[];
    separator: string;
    patternSource: string;
    maxLength: number;
    example: string;
  }> {
    const accountCount = await this.prisma.erpAccount.count({
      where: { deletedAt: null },
    });
    if (accountCount > 0) {
      throw new ConflictException(
        `Format kode akun tidak bisa diubah: sudah ada ${accountCount} akun. Hapus semua akun dulu untuk reset format.`,
      );
    }
    const format = buildAccountCodeFormat(
      parseSegments(JSON.stringify(segments)),
      parseSeparator(separator),
    );
    const updatedById = toAuditUserId(actorId);
    const writes: Array<{ key: string; value: string; name: string }> = [
      {
        key: KEY_SEGMENTS,
        value: JSON.stringify(format.segments),
        name: 'Segmen Kode Akun',
      },
      {
        key: KEY_SEPARATOR,
        value: format.separator,
        name: 'Pemisah Segmen Kode Akun',
      },
    ];
    for (const w of writes) {
      await this.prisma.erpSetting.upsert({
        where: {
          module_group_key: {
            module: 'system',
            group: SETTING_GROUP,
            key: w.key,
          },
        },
        create: {
          module: 'system',
          group: SETTING_GROUP,
          key: w.key,
          name: w.name,
          value: w.value,
          dataType: w.key === KEY_SEGMENTS ? 'json' : 'string',
        },
        update: { value: w.value, updatedById },
      });
    }
    return {
      segments: format.segments,
      separator: format.separator,
      patternSource: format.patternSource,
      maxLength: format.maxLength,
      example: format.example,
    };
  }

  private async loadFormat(): Promise<AccountCodeFormat> {
    const rows = await this.prisma.erpSetting.findMany({
      where: {
        group: SETTING_GROUP,
        key: { in: [KEY_SEGMENTS, KEY_SEPARATOR] },
        deletedAt: null,
      },
    });
    const map = new Map(rows.map((r) => [r.key, r.value]));
    const segments = parseSegments(map.get(KEY_SEGMENTS));
    const separator = parseSeparator(map.get(KEY_SEPARATOR));
    return buildAccountCodeFormat(segments, separator);
  }

  async create(dto: CreateErpAccountDto, actorId?: string) {
    const format = await this.loadFormat();
    validateAccountCode(dto.code, format);

    const existing = await this.prisma.erpAccount.findFirst({
      where: { code: dto.code },
      select: { id: true, deletedAt: true },
    });
    if (existing) {
      throwDuplicate({
        fieldLabel: 'Account code',
        value: dto.code,
        isSoftDeleted: Boolean(existing.deletedAt),
      });
    }

    const parent = dto.parentId
      ? await this.hierarchy.loadParent(toBigIntId(dto.parentId, 'parentId'))
      : null;

    const effectiveType = parent
      ? this.hierarchy.assertTypeMatchesParent(dto.accountType, parent.type)
      : (dto.accountType as ErpAccountType | undefined);
    if (!effectiveType) {
      throw new BadRequestException('Root account wajib menentukan tipe akun');
    }
    const effectiveLevel = this.hierarchy.deriveLevel(parent);

    const isLeaf = this.hierarchy.isLeaf(dto.code, format);
    const effectiveKind = this.hierarchy.kindFromCode(dto.code, format);
    const currencyIdBig = toOptionalBigIntId(dto.currencyId, 'currencyId');
    const bankIdBig = toOptionalBigIntId(dto.bankId, 'bankId');
    const hasDetails = Boolean(
      currencyIdBig ||
        bankIdBig ||
        dto.bankName?.trim() ||
        dto.bankAccountNo?.trim(),
    );
    this.hierarchy.assertLeafDetails(hasDetails, isLeaf);

    if (currencyIdBig && isLeaf) {
      await this.hierarchy.validateCurrency(currencyIdBig);
    }
    if (bankIdBig && isLeaf) {
      await this.hierarchy.validateBank(bankIdBig);
    }

    const branchRows = isLeaf ? buildAccountDimRows(dto.branchIds, 'branchId') : [];
    const locationRows = isLeaf ? buildAccountDimRows(dto.locationIds, 'locationId') : [];
    const divisionRows = isLeaf ? buildAccountDimRows(dto.divisionIds, 'divisionId') : [];

    let created;
    try {
      created = await this.prisma.erpAccount.create({
        data: {
          code: dto.code,
          name: dto.name,
          alias: dto.alias,
          type: effectiveType,
          kind: effectiveKind,
          normalBalance: this.hierarchy.normalBalanceOf(effectiveType),
          cashFlowCategory: dto.cashFlowCategory,
          parentId: parent ? parent.id : null,
          currencyId: isLeaf ? (currencyIdBig ?? null) : null,
          level: effectiveLevel,
          isActive: dto.isActive ?? true,
          // Control Account removed from UI; keep column for seed/AR-AP (default false).
          isControlAccount: false,
          bankId: isLeaf ? (bankIdBig ?? null) : null,
          bankName: isLeaf ? dto.bankName : undefined,
          bankAccountNo: isLeaf ? dto.bankAccountNo : undefined,
          notes: dto.notes,
          createdById: toAuditUserId(actorId),
          updatedById: toAuditUserId(actorId),
          ...(branchRows?.length ? { dimBranches: { create: branchRows } } : {}),
          ...(locationRows?.length ? { dimLocations: { create: locationRows } } : {}),
          ...(divisionRows?.length ? { dimDivisions: { create: divisionRows } } : {}),
        },
        include: {
          ...ACCOUNT_DIM_INCLUDE,
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code'])) {
        throwDuplicate({ fieldLabel: 'Account code', value: dto.code });
      }
      throw error;
    }

    return { success: true, data: created };
  }

  async findAll(query: QueryErpAccountDto) {
    const page = query.page ?? 1;
    const limit = Math.min(query.limit ?? 10, 500);
    const skip = (page - 1) * limit;

    const where: Prisma.ErpAccountWhereInput = { deletedAt: null };
    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { code: { equals: q, mode: 'insensitive' } },
        { name: { contains: q, mode: 'insensitive' } },
        { alias: { contains: q, mode: 'insensitive' } },
      ];
    }
    if (query.accountType) where.type = query.accountType;
    if (query.accountKind) where.kind = query.accountKind;
    if (query.normalBalance) where.normalBalance = query.normalBalance;
    if (query.parentId !== undefined) {
      where.parentId = query.parentId === 'null' ? null : toBigIntId(query.parentId, 'parentId');
    }
    if (query.isActive !== undefined) where.isActive = query.isActive;

    // List: thin select (dims only on findOne / form).
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpAccount.findMany({
        where,
        orderBy: [{ [query.sortBy ?? 'code']: query.sortDir ?? 'asc' }],
        skip,
        take: limit,
        select: {
          id: true,
          code: true,
          name: true,
          alias: true,
          type: true,
          kind: true,
          normalBalance: true,
          cashFlowCategory: true,
          parentId: true,
          currencyId: true,
          bankId: true,
          level: true,
          isActive: true,
          createdAt: true,
          updatedAt: true,
          parent: { select: { id: true, code: true, name: true } },
        },
      }),
      this.prisma.erpAccount.count({ where }),
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

  /**
   * One level of the CoA tree (roots when parentId omitted/null).
   * Each row includes hasChildren for expand chevron without loading siblings.
   */
  async findTreeChildren(query: {
    parentId?: string;
    accountType?: import('@prisma/client').ErpAccountType;
    accountKind?: import('@prisma/client').ErpAccountKind;
    isActive?: boolean;
  }) {
    const where: Prisma.ErpAccountWhereInput = { deletedAt: null };
    if (query.parentId === undefined || query.parentId === '' || query.parentId === 'null') {
      where.parentId = null;
    } else {
      where.parentId = toBigIntId(query.parentId, 'parentId');
    }
    if (query.accountType) where.type = query.accountType;
    if (query.accountKind) where.kind = query.accountKind;
    if (query.isActive !== undefined) where.isActive = query.isActive;

    const items = await this.prisma.erpAccount.findMany({
      where,
      orderBy: [{ code: 'asc' }],
      select: {
        id: true,
        code: true,
        name: true,
        alias: true,
        type: true,
        kind: true,
        normalBalance: true,
        cashFlowCategory: true,
        parentId: true,
        currencyId: true,
        bankId: true,
        level: true,
        isActive: true,
        createdAt: true,
        updatedAt: true,
        _count: { select: { children: { where: { deletedAt: null } } } },
      },
    });

    const data = items.map(({ _count, ...row }) => ({
      ...row,
      hasChildren: _count.children > 0,
    }));

    return { success: true, data };
  }

  async findOne(id: bigint) {
    const item = await this.prisma.erpAccount.findFirst({
      where: { id, deletedAt: null },
      include: {
        ...ACCOUNT_DIM_INCLUDE,
        children: { where: { deletedAt: null }, select: { id: true, code: true, name: true } },
      },
    });
    if (!item) {
      throw new NotFoundException('Account not found');
    }
    return { success: true, data: item };
  }

  async update(id: bigint, dto: UpdateErpAccountDto, actorId?: string) {
    const existing = await this.prisma.erpAccount.findFirst({
      where: { id, deletedAt: null },
    });
    if (!existing) {
      throw new NotFoundException('Account not found');
    }

    const format = await this.loadFormat();

    if (dto.code && dto.code !== existing.code) {
      validateAccountCode(dto.code, format);
      const duplicate = await this.prisma.erpAccount.findFirst({
        where: { code: dto.code, NOT: { id } },
        select: { id: true, deletedAt: true },
      });
      if (duplicate) {
        throwDuplicate({
          fieldLabel: 'Account code',
          value: dto.code,
          isSoftDeleted: Boolean(duplicate.deletedAt),
        });
      }
    }

    // Resolve parent: when moved, validate header + no cycle + recompute sub-tree levels.
    let parentIdEffective: bigint | null | undefined = undefined;
    let parentMoved = false;
    if (dto.parentId !== undefined) {
      const newParentId = dto.parentId ? toBigIntId(dto.parentId, 'parentId') : null;
      if (newParentId && newParentId !== existing.parentId) {
        parentMoved = true;
        const parent = await this.hierarchy.loadParent(newParentId);
        await this.hierarchy.assertNotSelfOrDescendant(id, newParentId);
        parentIdEffective = parent.id;
      } else if (!newParentId) {
        parentIdEffective = null;
      } else {
        parentIdEffective = existing.parentId;
      }
    }

    // Resolve effective type & kind for validation below.
    const parentForType = parentIdEffective !== undefined
      ? (parentIdEffective ? await this.hierarchy.loadParent(parentIdEffective) : null)
      : (existing.parentId ? await this.hierarchy.loadParent(existing.parentId) : null);
    const effectiveType = parentForType
      ? this.hierarchy.assertTypeMatchesParent(dto.accountType, parentForType.type)
      : dto.accountType ?? existing.type;

    // Leaf + kind derived from effective code (new or existing) + active format.
    const effectiveCode = dto.code ?? existing.code;
    const isLeaf = this.hierarchy.isLeaf(effectiveCode, format);
    const effectiveKind = this.hierarchy.kindFromCode(effectiveCode, format);
    if (effectiveKind === ErpAccountKind.POSTABLE) {
      const childCount = await this.hierarchy.countChildren(id);
      this.hierarchy.assertPostableHasNoChildren(effectiveKind, childCount);
    }

    const currencyIdBig = toOptionalBigIntId(dto.currencyId, 'currencyId');
    const bankIdBig = toOptionalBigIntId(dto.bankId, 'bankId');
    const bankFields: {
      bankName?: string | null;
      bankAccountNo?: string | null;
      bankId?: bigint | null;
    } = {
      bankName: dto.bankName,
      bankAccountNo: dto.bankAccountNo,
      bankId: bankIdBig === undefined ? undefined : bankIdBig,
    };
    const hasDetails =
      Boolean(currencyIdBig) ||
      Boolean(bankIdBig) ||
      Boolean(dto.bankName?.trim()) ||
      Boolean(dto.bankAccountNo?.trim());
    this.hierarchy.assertLeafDetails(hasDetails, isLeaf);
    if (!isLeaf) {
      bankFields.bankName = null;
      bankFields.bankAccountNo = null;
      bankFields.bankId = null;
    }

    if (currencyIdBig && isLeaf) {
      await this.hierarchy.validateCurrency(currencyIdBig);
    }
    if (bankIdBig && isLeaf) {
      await this.hierarchy.validateBank(bankIdBig);
    }

    let updated;
    try {
      const result = await this.prisma.$transaction(async (tx) => {
        // Use UncheckedUpdateInput so scalar FKs (parentId/currencyId/bankId)
        // can coexist with nested dim junction writes.
        const data: Prisma.ErpAccountUncheckedUpdateInput = {
          code: dto.code,
          name: dto.name,
          alias: dto.alias,
          type: effectiveType,
          kind: effectiveKind,
          normalBalance: this.hierarchy.normalBalanceOf(effectiveType),
          cashFlowCategory: dto.cashFlowCategory,
          parentId: parentIdEffective,
          currencyId: isLeaf
            ? currencyIdBig === undefined
              ? undefined
              : currencyIdBig
            : null,
          isActive: dto.isActive,
          // Do not accept UI writes for control account; leave existing DB value.
          bankId: bankFields.bankId,
          bankName: bankFields.bankName,
          bankAccountNo: bankFields.bankAccountNo,
          notes: dto.notes,
          updatedById: toAuditUserId(actorId),
        };
        if (dto.branchIds !== undefined) {
          data.dimBranches = {
            deleteMany: {},
            create: isLeaf ? (buildAccountDimRows(dto.branchIds, 'branchId') ?? []) : [],
          };
        }
        if (dto.locationIds !== undefined) {
          data.dimLocations = {
            deleteMany: {},
            create: isLeaf ? (buildAccountDimRows(dto.locationIds, 'locationId') ?? []) : [],
          };
        }
        if (dto.divisionIds !== undefined) {
          data.dimDivisions = {
            deleteMany: {},
            create: isLeaf ? (buildAccountDimRows(dto.divisionIds, 'divisionId') ?? []) : [],
          };
        }

        const updatedRow = await tx.erpAccount.update({
          where: { id },
          data,
          include: {
            ...ACCOUNT_DIM_INCLUDE,
          },
        });

        if (parentMoved) {
          const newLevel = parentForType ? parentForType.level + 1 : 1;
          await this.recomputeSubtreeWithinTx(tx, id, newLevel);
        }
        return updatedRow;
      });
      updated = result;
    } catch (error) {
      if (isUniqueViolation(error, ['code'])) {
        throwDuplicate({ fieldLabel: 'Account code', value: dto.code ?? existing.code });
      }
      throw error;
    }

    return { success: true, data: updated };
  }

  private async recomputeSubtreeWithinTx(
    tx: Prisma.TransactionClient,
    accountId: bigint,
    level: number,
  ): Promise<void> {
    await tx.erpAccount.update({
      where: { id: accountId },
      data: { level },
    });
    const children = await tx.erpAccount.findMany({
      where: { parentId: accountId, deletedAt: null },
      select: { id: true },
    });
    for (const child of children) {
      await this.recomputeSubtreeWithinTx(tx, child.id, level + 1);
    }
  }

  async bulkUpdateStatus(dto: BulkStatusErpAccountDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpAccount.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { isActive: dto.isActive, updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async bulkDelete(dto: BulkErpAccountDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpAccount.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpAccount.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) {
      throw new NotFoundException('Account not found');
    }

    const childCount = await this.hierarchy.countChildren(id);
    if (childCount > 0) {
      throw new BadRequestException(
        'Hapus semua akun anak terlebih dahulu sebelum menghapus induk',
      );
    }

    await this.prisma.erpAccount.update({
      where: { id },
      data: {
        deletedAt: new Date(),
        updatedById: toAuditUserId(actorId),
      },
    });

    return { success: true, message: 'Account deleted' };
  }
}
