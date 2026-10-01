import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { ErpAuditService } from '../erp-audit/erp-audit.service';
import { diffFields } from '../erp-common/utils/diff-fields.util';
import { CreateErpItemInformationDto } from './dto/create-item-information.dto';
import { QueryErpItemInformationDto } from './dto/query-item-information.dto';
import { UpdateErpItemInformationDto } from './dto/update-item-information.dto';

const ENTITY = 'ErpItemInformation';
const LABEL_ID = 'Item Information';

@Injectable()
export class ErpItemInformationsService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly audit: ErpAuditService,
  ) {}

  private readonly ITEM_SELECT = { id: true, code: true, name: true };

  async create(dto: CreateErpItemInformationDto, actorId?: string) {
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const itemId = BigInt(dto.itemId);
    const created = await this.prisma.erpItemInformation.create({
      data: {
        itemId,
        longDescription: dto.longDescription,
        specifications: dto.specifications,
        manufacturer: dto.manufacturer,
        countryOfOrigin: dto.countryOfOrigin,
        warrantyPeriodMonths: dto.warrantyPeriodMonths,
        tags: dto.tags,
        notes: dto.notes,
        createdById: actorBigInt,
        updatedById: actorBigInt,
      },
      include: { item: { select: this.ITEM_SELECT } },
    });
    this.audit.log({
      action: 'CREATE', entityName: ENTITY, entityId: created.id,
      summary: `${LABEL_ID} for itemId=${itemId} dibuat`, actorId: actorBigInt ?? undefined,
    });
    return { success: true, data: created };
  }

  async findAll(query: QueryErpItemInformationDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;
    const where: Prisma.ErpItemInformationWhereInput = { deletedAt: null };
    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { manufacturer: { contains: q, mode: 'insensitive' } },
        { countryOfOrigin: { contains: q, mode: 'insensitive' } },
        { tags: { contains: q, mode: 'insensitive' } },
        { notes: { contains: q, mode: 'insensitive' } },
      ];
    }
    const sortBy = query.sortBy ?? 'createdAt';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpItemInformation.findMany({
        where, orderBy: [{ [sortBy]: sortDir }], skip, take: limit,
        include: { item: { select: this.ITEM_SELECT } },
      }),
      this.prisma.erpItemInformation.count({ where }),
    ]);
    return { success: true, data: items, meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 } };
  }

  async findOne(id: bigint) {
    const item = await this.prisma.erpItemInformation.findFirst({
      where: { id, deletedAt: null },
      include: { item: { select: this.ITEM_SELECT } },
    });
    if (!item) throw new NotFoundException(`${ENTITY} not found`);
    return { success: true, data: item };
  }

  async findByItemId(itemId: bigint) {
    const item = await this.prisma.erpItemInformation.findFirst({
      where: { itemId, deletedAt: null },
      include: { item: { select: this.ITEM_SELECT } },
    });
    if (!item) throw new NotFoundException(`${ENTITY} for itemId=${itemId} not found`);
    return { success: true, data: item };
  }

  async update(id: bigint, dto: UpdateErpItemInformationDto, actorId?: string) {
    const existing = await this.prisma.erpItemInformation.findFirst({ where: { id, deletedAt: null } });
    if (!existing) throw new NotFoundException(`${ENTITY} not found`);
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const updated = await this.prisma.erpItemInformation.update({
      where: { id },
      data: {
        longDescription: dto.longDescription,
        specifications: dto.specifications,
        manufacturer: dto.manufacturer,
        countryOfOrigin: dto.countryOfOrigin,
        warrantyPeriodMonths: dto.warrantyPeriodMonths,
        tags: dto.tags,
        notes: dto.notes,
        updatedById: actorBigInt,
      },
      include: { item: { select: this.ITEM_SELECT } },
    });
    const changes = diffFields(existing as unknown as Record<string, unknown>, updated as unknown as Record<string, unknown>);
    this.audit.log({
      action: 'UPDATE', entityName: ENTITY, entityId: id, changes,
      summary: `${LABEL_ID} id=${id} diperbarui`, actorId: actorBigInt ?? undefined,
    });
    return { success: true, data: updated };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.erpItemInformation.findFirst({ where: { id, deletedAt: null }, select: { id: true } });
    if (!existing) throw new NotFoundException(`${ENTITY} not found`);
    const actorBigInt = actorId ? BigInt(actorId) : null;
    await this.prisma.erpItemInformation.update({ where: { id }, data: { deletedAt: new Date(), updatedById: actorBigInt } });
    this.audit.log({ action: 'DELETE', entityName: ENTITY, entityId: id, summary: `${LABEL_ID} id=${id} dihapus`, actorId: actorBigInt ?? undefined });
    return { success: true, message: `${ENTITY} deleted` };
  }
}
