import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { isUniqueViolation, throwDuplicate } from '../common/errors/duplicate.util';
import { PrismaService } from '../prisma/prisma.service';
import { ErpAuditService } from '../erp-audit/erp-audit.service';
import { diffFields } from '../erp-common/utils/diff-fields.util';
import { BulkErpStorageBinDto, BulkStatusErpStorageBinDto } from './dto/bulk-storage-bins.dto';
import { CreateErpStorageBinDto } from './dto/create-storage-bin.dto';
import { QueryErpStorageBinDto } from './dto/query-storage-bin.dto';
import { UpdateErpStorageBinDto } from './dto/update-storage-bin.dto';

const ENTITY = 'ErpStorageBin';
const FIELD_LABEL = 'Storage Bin code';
const UNIQUE_KEY = 'md_storage_bins_warehouse_id_code_key';
const LABEL_ID = 'Lokasi Gudang';
const MAX_DEPTH = 10;

const baseInclude = {
  warehouse: { select: { id: true, code: true, name: true } },
  parent: { select: { id: true, code: true, name: true, binType: true } },
} satisfies Prisma.ErpStorageBinInclude;

@Injectable()
export class ErpStorageBinsService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly audit: ErpAuditService,
  ) {}

  /** Parent harus ada, belum dihapus, segudang, dan bukan dirinya/turunannya. */
  private async assertValidParent(parentId: bigint, warehouseId: bigint, selfId?: bigint) {
    const parent = await this.prisma.erpStorageBin.findFirst({
      where: { id: parentId, deletedAt: null },
      select: { id: true, warehouseId: true, parentId: true },
    });
    if (!parent) throw new BadRequestException('Parent bin tidak ditemukan');
    if (parent.warehouseId !== warehouseId) {
      throw new BadRequestException('Parent bin harus berada di gudang yang sama');
    }
    if (selfId !== undefined) {
      let cursor: { id: bigint; parentId: bigint | null } | null = parent;
      for (let depth = 0; cursor && depth < MAX_DEPTH; depth += 1) {
        if (cursor.id === selfId) {
          throw new BadRequestException('Parent bin tidak boleh dirinya sendiri atau turunannya');
        }
        cursor = cursor.parentId
          ? await this.prisma.erpStorageBin.findFirst({
              where: { id: cursor.parentId },
              select: { id: true, parentId: true },
            })
          : null;
      }
    }
  }

  async create(dto: CreateErpStorageBinDto, actorId?: string) {
    const warehouseId = BigInt(dto.warehouseId);
    const existing = await this.prisma.erpStorageBin.findFirst({
      where: { warehouseId, code: dto.code },
      select: { id: true, deletedAt: true },
    });
    if (existing)
      throwDuplicate({
        fieldLabel: FIELD_LABEL,
        value: dto.code,
        isSoftDeleted: Boolean(existing.deletedAt),
      });
    const parentId = dto.parentId ? BigInt(dto.parentId) : null;
    if (parentId) await this.assertValidParent(parentId, warehouseId);
    const actorBigInt = actorId ? BigInt(actorId) : null;
    let created;
    try {
      created = await this.prisma.erpStorageBin.create({
        data: {
          code: dto.code,
          name: dto.name,
          warehouseId,
          parentId,
          binType: dto.binType ?? 'BIN',
          notes: dto.notes,
          isActive: dto.isActive ?? true,
          createdById: actorBigInt,
          updatedById: actorBigInt,
        },
        include: baseInclude,
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

  async findAll(query: QueryErpStorageBinDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;
    const where: Prisma.ErpStorageBinWhereInput = { deletedAt: null };
    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { code: { contains: q, mode: 'insensitive' } },
        { name: { contains: q, mode: 'insensitive' } },
      ];
    }
    if (query.warehouseId) where.warehouseId = BigInt(query.warehouseId);
    if (query.parentId) where.parentId = BigInt(query.parentId);
    if (query.binType) where.binType = query.binType;
    if (query.isActive !== undefined) where.isActive = query.isActive;
    const sortBy = query.sortBy ?? 'createdAt';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpStorageBin.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }],
        skip,
        take: limit,
        include: baseInclude,
      }),
      this.prisma.erpStorageBin.count({ where }),
    ]);
    return {
      success: true,
      data: items,
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  /** Seluruh bin satu gudang (flat, terurut kode) — frontend yang menyusun tree. */
  async tree(warehouseId: bigint) {
    const items = await this.prisma.erpStorageBin.findMany({
      where: { warehouseId, deletedAt: null },
      orderBy: [{ code: 'asc' }],
      include: baseInclude,
    });
    return { success: true, data: items };
  }

  async findOne(id: bigint) {
    const item = await this.prisma.erpStorageBin.findFirst({
      where: { id, deletedAt: null },
      include: baseInclude,
    });
    if (!item) throw new NotFoundException(`${ENTITY} not found`);
    return { success: true, data: item };
  }

  async update(id: bigint, dto: UpdateErpStorageBinDto, actorId?: string) {
    const existing = await this.prisma.erpStorageBin.findFirst({ where: { id, deletedAt: null } });
    if (!existing) throw new NotFoundException(`${ENTITY} not found`);
    const warehouseId = dto.warehouseId ? BigInt(dto.warehouseId) : existing.warehouseId;
    const code = dto.code ?? existing.code;
    if (code !== existing.code || warehouseId !== existing.warehouseId) {
      const duplicate = await this.prisma.erpStorageBin.findFirst({
        where: { warehouseId, code, NOT: { id } },
        select: { id: true, deletedAt: true },
      });
      if (duplicate)
        throwDuplicate({
          fieldLabel: FIELD_LABEL,
          value: code,
          isSoftDeleted: Boolean(duplicate.deletedAt),
        });
    }
    if (warehouseId !== existing.warehouseId) {
      const childCount = await this.prisma.erpStorageBin.count({
        where: { parentId: id, deletedAt: null },
      });
      if (childCount > 0) {
        throw new BadRequestException('Tidak bisa pindah gudang: bin masih punya sub-lokasi');
      }
    }
    const parentId =
      dto.parentId === undefined ? existing.parentId : dto.parentId ? BigInt(dto.parentId) : null;
    if (parentId) await this.assertValidParent(parentId, warehouseId, id);
    const actorBigInt = actorId ? BigInt(actorId) : null;
    let updated;
    try {
      updated = await this.prisma.erpStorageBin.update({
        where: { id },
        data: {
          code: dto.code,
          name: dto.name,
          warehouseId: dto.warehouseId ? warehouseId : undefined,
          parentId,
          binType: dto.binType,
          notes: dto.notes,
          isActive: dto.isActive,
          updatedById: actorBigInt,
        },
        include: baseInclude,
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', UNIQUE_KEY])) {
        throwDuplicate({ fieldLabel: FIELD_LABEL, value: code });
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

  async bulkUpdateStatus(dto: BulkStatusErpStorageBinDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpStorageBin.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { isActive: dto.isActive, updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async bulkDelete(dto: BulkErpStorageBinDto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    await this.assertNoActiveChildren(ids);
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.erpStorageBin.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpStorageBin.findFirst({
      where: { id, deletedAt: null },
      select: { id: true },
    });
    if (!existing) throw new NotFoundException(`${ENTITY} not found`);
    await this.assertNoActiveChildren([id]);
    const actorBigInt = actorId ? BigInt(actorId) : null;
    await this.prisma.erpStorageBin.update({
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

  /** Soft-delete ditolak kalau masih ada sub-lokasi aktif di luar batch yang dihapus. */
  private async assertNoActiveChildren(ids: bigint[]) {
    const child = await this.prisma.erpStorageBin.findFirst({
      where: { parentId: { in: ids }, deletedAt: null, id: { notIn: ids } },
      select: { id: true, code: true },
    });
    if (child) {
      throw new BadRequestException(`Tidak bisa hapus: masih ada sub-lokasi aktif (${child.code})`);
    }
  }
}
