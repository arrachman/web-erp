import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { QueryItemCatalogDto } from './dto/query-item-catalog.dto';
import { UpsertItemCatalogDto } from './dto/upsert-item-catalog.dto';

type ItemWithCatalog = Prisma.ErpItemGetPayload<{
  include: {
    category: { select: { id: true; code: true; name: true } };
    catalogProfile: true;
  };
}>;

/** D1 — school-catalog profile layered on the item master (md_items). */
@Injectable()
export class ErpItemCatalogService {
  constructor(private readonly prisma: PrismaService) {}

  private mapProfile(p: ItemWithCatalog['catalogProfile']) {
    if (!p) {
      return {
        publisherName: null, jenjang: null, gradeLevel: null, curriculum: null,
        subject: null, hetPrice: null, isCustomPrint: false,
        stockClass: 'DAGANGAN', channels: null, isActive: true,
      };
    }
    return {
      publisherName: p.publisherName, jenjang: p.jenjang,
      gradeLevel: p.gradeLevel, curriculum: p.curriculum, subject: p.subject,
      hetPrice: p.hetPrice != null ? Number(p.hetPrice) : null,
      isCustomPrint: p.isCustomPrint, stockClass: p.stockClass,
      channels: p.channels ?? null, isActive: p.isActive,
    };
  }

  private mapRow(r: ItemWithCatalog) {
    return {
      id: Number(r.id), code: r.code, name: r.name, barcode: r.barcode,
      salePrice: r.salePrice != null ? Number(r.salePrice) : null,
      categoryId: Number(r.categoryId),
      category: r.category
        ? { id: Number(r.category.id), code: r.category.code, name: r.category.name }
        : null,
      vendorId: r.vendorId != null ? Number(r.vendorId) : null,
      profile: this.mapProfile(r.catalogProfile),
    };
  }

  async list(q: QueryItemCatalogDto) {
    const page = q.page ?? 1;
    const limit = q.limit ?? 25;
    const where: Prisma.ErpItemWhereInput = { deletedAt: null };
    if (q.search) {
      where.OR = [
        { code: { contains: q.search, mode: 'insensitive' } },
        { name: { contains: q.search, mode: 'insensitive' } },
      ];
    }
    if (q.categoryId) where.categoryId = BigInt(q.categoryId);
    const profileWhere: Prisma.ErpItemCatalogProfileWhereInput = {};
    if (q.jenjang) profileWhere.jenjang = q.jenjang;
    if (q.curriculum) profileWhere.curriculum = q.curriculum;
    if (q.stockClass) profileWhere.stockClass = q.stockClass;
    if (Object.keys(profileWhere).length) where.catalogProfile = { is: profileWhere };

    const include = {
      category: { select: { id: true, code: true, name: true } },
      catalogProfile: true,
    } as const;
    const [total, rows] = await this.prisma.$transaction([
      this.prisma.erpItem.count({ where }),
      this.prisma.erpItem.findMany({
        where, include, orderBy: { code: 'asc' },
        skip: (page - 1) * limit, take: limit,
      }),
    ]);
    return {
      data: rows.map((r) => this.mapRow(r as ItemWithCatalog)),
      meta: { total, page, limit, totalPages: Math.max(1, Math.ceil(total / limit)) },
    };
  }

  async getOne(itemId: string) {
    const row = await this.prisma.erpItem.findFirst({
      where: { id: BigInt(itemId), deletedAt: null },
      include: {
        category: { select: { id: true, code: true, name: true } },
        catalogProfile: true,
      },
    });
    if (!row) throw new NotFoundException('Item tidak ditemukan');
    return this.mapRow(row as ItemWithCatalog);
  }

  async upsert(itemId: string, dto: UpsertItemCatalogDto, actorId?: number) {
    const id = BigInt(itemId);
    const item = await this.prisma.erpItem.findFirst({
      where: { id, deletedAt: null }, select: { id: true },
    });
    if (!item) throw new NotFoundException('Item tidak ditemukan');
    const data: Prisma.ErpItemCatalogProfileUncheckedCreateInput = { itemId: id };
    if (dto.publisherName !== undefined) data.publisherName = dto.publisherName;
    if (dto.jenjang !== undefined) data.jenjang = dto.jenjang;
    if (dto.gradeLevel !== undefined) data.gradeLevel = dto.gradeLevel;
    if (dto.curriculum !== undefined) data.curriculum = dto.curriculum;
    if (dto.subject !== undefined) data.subject = dto.subject;
    if (dto.hetPrice !== undefined) data.hetPrice = new Prisma.Decimal(dto.hetPrice);
    if (dto.isCustomPrint !== undefined) data.isCustomPrint = dto.isCustomPrint;
    if (dto.stockClass !== undefined) data.stockClass = dto.stockClass;
    if (dto.channels !== undefined) data.channels = dto.channels as Prisma.InputJsonValue;
    if (dto.isActive !== undefined) data.isActive = dto.isActive;
    const actor = actorId != null ? BigInt(actorId) : null;
    await this.prisma.erpItemCatalogProfile.upsert({
      where: { itemId: id },
      create: { ...data, createdById: actor },
      update: { ...data, itemId: undefined, updatedById: actor },
    });
    return this.getOne(itemId);
  }
}
