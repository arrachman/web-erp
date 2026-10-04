import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import {
  CreateContractPriceDto,
  QueryContractPricesDto,
  UpdateContractPriceDto,
} from './dto/contracts.dto';

export type PriceSource = 'KONTRAK_ITEM' | 'KONTRAK_KATEGORI' | 'KONTRAK_SEKOLAH' | 'STANDAR';

export interface ResolvedPrice {
  price: string;
  source: PriceSource;
}

const id = (v: bigint | null | undefined) => (v == null ? null : v.toString());

function inWindow(row: { validFrom: Date | null; validTo: Date | null }, now: Date): boolean {
  if (row.validFrom && row.validFrom.getTime() > now.getTime()) return false;
  if (row.validTo) {
    const end = new Date(row.validTo);
    end.setUTCHours(23, 59, 59, 999);
    if (end.getTime() < now.getTime()) return false;
  }
  return true;
}

/**
 * Fase 3 W7 — mesin harga kontrak per sekolah (TANPA level yayasan; keputusan
 * 2026-10-05: per-sekolah saja). Resolusi berlapis untuk satu item:
 *   1. harga tetap kontrak per item,
 *   2. diskon kontrak per kategori item,
 *   3. diskon kontrak seluruh sekolah,
 *   4. harga jual standar item.
 * Dipakai katalog + checkout portal; admin ERP mengelola baris kontraknya.
 */
@Injectable()
export class ErpContractsService {
  constructor(private readonly prisma: PrismaService) {}

  // ── Resolver ─────────────────────────────────────────────────────────────

  private async activeRows(partnerId: bigint) {
    const rows = await this.prisma.erpSchoolContractPrice.findMany({
      where: { partnerId, isActive: true, deletedAt: null },
      orderBy: { createdAt: 'desc' },
    });
    const now = new Date();
    return rows.filter((r) => inWindow(r, now));
  }

  resolveFromRows(
    rows: Awaited<ReturnType<ErpContractsService['activeRows']>>,
    item: { id: bigint; categoryId: bigint | null; salePrice: Prisma.Decimal },
  ): ResolvedPrice {
    const sale = new Prisma.Decimal(item.salePrice);
    const itemRow = rows.find((r) => r.itemId === item.id && r.price != null);
    if (itemRow) return { price: new Prisma.Decimal(itemRow.price!).toString(), source: 'KONTRAK_ITEM' };
    const applyDiscount = (pct: Prisma.Decimal | null, source: PriceSource): ResolvedPrice | null => {
      if (pct == null) return null;
      const p = sale.mul(new Prisma.Decimal(100).minus(pct)).div(100);
      return { price: p.toDecimalPlaces(2).toString(), source };
    };
    if (item.categoryId) {
      const catRow = rows.find((r) => r.categoryId === item.categoryId && r.itemId == null);
      const res = catRow ? applyDiscount(catRow.discountPercent, 'KONTRAK_KATEGORI') : null;
      if (res) return res;
    }
    const schoolRow = rows.find((r) => r.itemId == null && r.categoryId == null);
    const schoolRes = schoolRow ? applyDiscount(schoolRow.discountPercent, 'KONTRAK_SEKOLAH') : null;
    if (schoolRes) return schoolRes;
    return { price: sale.toString(), source: 'STANDAR' };
  }

  async resolveMany(
    partnerId: bigint,
    items: { id: bigint; categoryId: bigint | null; salePrice: Prisma.Decimal }[],
  ): Promise<Map<string, ResolvedPrice>> {
    const rows = await this.activeRows(partnerId);
    const map = new Map<string, ResolvedPrice>();
    for (const item of items) map.set(item.id.toString(), this.resolveFromRows(rows, item));
    return map;
  }

  async resolvePrice(
    partnerId: bigint,
    item: { id: bigint; categoryId: bigint | null; salePrice: Prisma.Decimal },
  ): Promise<ResolvedPrice> {
    const map = await this.resolveMany(partnerId, [item]);
    return map.get(item.id.toString())!;
  }

  // ── Admin CRUD ───────────────────────────────────────────────────────────

  private present(r: any, names: { partner?: string; item?: string; category?: string }) {
    return {
      id: id(r.id),
      partnerId: id(r.partnerId),
      partnerName: names.partner ?? null,
      itemId: id(r.itemId),
      itemName: names.item ?? null,
      categoryId: id(r.categoryId),
      categoryName: names.category ?? null,
      scope: r.itemId ? 'ITEM' : r.categoryId ? 'KATEGORI' : 'SEKOLAH',
      price: r.price != null ? r.price.toString() : null,
      discountPercent: r.discountPercent != null ? r.discountPercent.toString() : null,
      validFrom: r.validFrom ? r.validFrom.toISOString().slice(0, 10) : null,
      validTo: r.validTo ? r.validTo.toISOString().slice(0, 10) : null,
      isActive: r.isActive,
      notes: r.notes ?? null,
      createdAt: r.createdAt,
    };
  }

  private async namesFor(rows: any[]) {
    const partnerIds = [...new Set(rows.map((r) => r.partnerId.toString()))];
    const itemIds = [...new Set(rows.map((r) => r.itemId?.toString()).filter(Boolean))] as string[];
    const catIds = [...new Set(rows.map((r) => r.categoryId?.toString()).filter(Boolean))] as string[];
    const [partners, items, cats] = await Promise.all([
      partnerIds.length ? this.prisma.erpPartner.findMany({ where: { id: { in: partnerIds.map(BigInt) } }, select: { id: true, name: true } }) : [],
      itemIds.length ? this.prisma.erpItem.findMany({ where: { id: { in: itemIds.map(BigInt) } }, select: { id: true, name: true } }) : [],
      catIds.length ? this.prisma.erpItemCategory.findMany({ where: { id: { in: catIds.map(BigInt) } }, select: { id: true, name: true } }) : [],
    ]);
    return {
      partner: new Map(partners.map((p) => [p.id.toString(), p.name])),
      item: new Map(items.map((p) => [p.id.toString(), p.name])),
      category: new Map(cats.map((p) => [p.id.toString(), p.name])),
    };
  }

  async list(query: QueryContractPricesDto) {
    const page = Math.max(1, Number(query.page ?? 1) || 1);
    const limit = Math.min(200, Math.max(1, Number(query.limit ?? 50) || 50));
    const where: Prisma.ErpSchoolContractPriceWhereInput = {
      deletedAt: null,
      ...(query.partnerId ? { partnerId: BigInt(query.partnerId) } : {}),
      ...(query.itemId ? { itemId: BigInt(query.itemId) } : {}),
    };
    const [rows, total] = await Promise.all([
      this.prisma.erpSchoolContractPrice.findMany({
        where, orderBy: { createdAt: 'desc' }, skip: (page - 1) * limit, take: limit,
      }),
      this.prisma.erpSchoolContractPrice.count({ where }),
    ]);
    const names = await this.namesFor(rows);
    return {
      data: rows.map((r) => this.present(r, {
        partner: names.partner.get(r.partnerId.toString()),
        item: r.itemId ? names.item.get(r.itemId.toString()) : undefined,
        category: r.categoryId ? names.category.get(r.categoryId.toString()) : undefined,
      })),
      meta: { page, limit, total, totalPages: Math.max(1, Math.ceil(total / limit)) },
    };
  }

  private validateShape(dto: { itemId?: string; categoryId?: string; price?: string; discountPercent?: string; validFrom?: string; validTo?: string }) {
    if (dto.itemId && dto.categoryId) {
      throw new BadRequestException('Pilih salah satu cakupan: item ATAU kategori (atau kosongkan keduanya untuk seluruh sekolah).');
    }
    if (dto.itemId) {
      if (dto.price == null) throw new BadRequestException('Baris per item wajib berisi harga tetap.');
      if (dto.discountPercent != null) throw new BadRequestException('Baris per item memakai harga tetap, bukan diskon.');
    } else {
      if (dto.price != null) throw new BadRequestException('Baris diskon tidak boleh berisi harga tetap.');
      if (dto.discountPercent == null) throw new BadRequestException('Baris kategori/sekolah wajib berisi diskon persen.');
      const pct = Number(dto.discountPercent);
      if (!(pct > 0 && pct <= 100)) throw new BadRequestException('Diskon harus di antara 0 dan 100 persen.');
    }
    if (dto.validFrom && dto.validTo && dto.validTo < dto.validFrom) {
      throw new BadRequestException('Tanggal berlaku sampai harus setelah dari.');
    }
  }

  async create(dto: CreateContractPriceDto, actorId?: string) {
    this.validateShape(dto);
    const partner = await this.prisma.erpPartner.findFirst({
      where: { id: BigInt(dto.partnerId), deletedAt: null },
    });
    if (!partner) throw new BadRequestException('Partner sekolah tidak ditemukan.');
    if (dto.itemId) {
      const item = await this.prisma.erpItem.findFirst({ where: { id: BigInt(dto.itemId), deletedAt: null } });
      if (!item) throw new BadRequestException('Item tidak ditemukan.');
    }
    if (dto.categoryId) {
      const cat = await this.prisma.erpItemCategory.findFirst({ where: { id: BigInt(dto.categoryId), deletedAt: null } });
      if (!cat) throw new BadRequestException('Kategori tidak ditemukan.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    const row = await this.prisma.erpSchoolContractPrice.create({
      data: {
        partnerId: partner.id,
        itemId: dto.itemId ? BigInt(dto.itemId) : null,
        categoryId: dto.categoryId ? BigInt(dto.categoryId) : null,
        price: dto.price != null ? new Prisma.Decimal(dto.price) : null,
        discountPercent: dto.discountPercent != null ? new Prisma.Decimal(dto.discountPercent) : null,
        validFrom: dto.validFrom ? new Date(dto.validFrom) : null,
        validTo: dto.validTo ? new Date(dto.validTo) : null,
        notes: dto.notes,
        createdById: actor,
        updatedById: actor,
      },
    });
    const names = await this.namesFor([row]);
    return this.present(row, {
      partner: names.partner.get(row.partnerId.toString()),
      item: row.itemId ? names.item.get(row.itemId.toString()) : undefined,
      category: row.categoryId ? names.category.get(row.categoryId.toString()) : undefined,
    });
  }

  async update(rowId: string, dto: UpdateContractPriceDto, actorId?: string) {
    const row = await this.prisma.erpSchoolContractPrice.findFirst({
      where: { id: BigInt(rowId), deletedAt: null },
    });
    if (!row) throw new NotFoundException('Baris harga kontrak tidak ditemukan.');
    if (row.itemId) {
      if (dto.discountPercent != null) throw new BadRequestException('Baris per item memakai harga tetap, bukan diskon.');
    } else if (dto.price != null) {
      throw new BadRequestException('Baris diskon tidak boleh berisi harga tetap.');
    }
    if (dto.validFrom && dto.validTo && dto.validTo < dto.validFrom) {
      throw new BadRequestException('Tanggal berlaku sampai harus setelah dari.');
    }
    const updated = await this.prisma.erpSchoolContractPrice.update({
      where: { id: row.id },
      data: {
        price: dto.price != null ? new Prisma.Decimal(dto.price) : undefined,
        discountPercent: dto.discountPercent != null ? new Prisma.Decimal(dto.discountPercent) : undefined,
        validFrom: dto.validFrom === null ? null : dto.validFrom ? new Date(dto.validFrom) : undefined,
        validTo: dto.validTo === null ? null : dto.validTo ? new Date(dto.validTo) : undefined,
        isActive: dto.isActive,
        notes: dto.notes === null ? null : dto.notes,
        updatedById: actorId ? BigInt(actorId) : undefined,
      },
    });
    const names = await this.namesFor([updated]);
    return this.present(updated, {
      partner: names.partner.get(updated.partnerId.toString()),
      item: updated.itemId ? names.item.get(updated.itemId.toString()) : undefined,
      category: updated.categoryId ? names.category.get(updated.categoryId.toString()) : undefined,
    });
  }

  async remove(rowId: string) {
    const row = await this.prisma.erpSchoolContractPrice.findFirst({
      where: { id: BigInt(rowId), deletedAt: null },
    });
    if (!row) throw new NotFoundException('Baris harga kontrak tidak ditemukan.');
    await this.prisma.erpSchoolContractPrice.update({
      where: { id: row.id },
      data: { deletedAt: new Date() },
    });
    return { success: true };
  }
}
