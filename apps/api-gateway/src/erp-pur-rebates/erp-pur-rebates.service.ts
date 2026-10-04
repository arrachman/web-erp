import {
  ConflictException, Injectable, NotFoundException,
} from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { CreatePurRebateDto } from './dto/create-pur-rebate.dto';
import { QueryPurRebateDto } from './dto/query-pur-rebate.dto';
import { UpdatePurRebateDto } from './dto/update-pur-rebate.dto';

const round2 = (v: number) => Math.round(v * 100) / 100;

const include = {
  supplier: { select: { id: true, code: true, name: true } },
  category: { select: { id: true, code: true, name: true } },
} satisfies Prisma.ErpPurSupplierRebateInclude;

type RebateRow = Prisma.ErpPurSupplierRebateGetPayload<{ include: typeof include }>;

/** D2 — publisher/supplier rebate agreements + accrual computation. */
@Injectable()
export class ErpPurRebatesService {
  constructor(private readonly prisma: PrismaService) {}

  private mapRow(r: RebateRow) {
    return {
      id: Number(r.id),
      supplierId: Number(r.supplierId),
      supplier: r.supplier
        ? { id: Number(r.supplier.id), code: r.supplier.code, name: r.supplier.name }
        : null,
      categoryId: r.categoryId ? Number(r.categoryId) : null,
      category: r.category
        ? { id: Number(r.category.id), code: r.category.code, name: r.category.name }
        : null,
      percent: Number(r.percent),
      periodYear: r.periodYear,
      notes: r.notes,
      isActive: r.isActive,
    };
  }

  async list(q: QueryPurRebateDto) {
    const page = q.page ?? 1;
    const limit = q.limit ?? 25;
    const where: Prisma.ErpPurSupplierRebateWhereInput = { deletedAt: null };
    if (q.periodYear) where.periodYear = q.periodYear;
    if (q.supplierId) where.supplierId = BigInt(q.supplierId);
    if (q.search) {
      where.supplier = { name: { contains: q.search, mode: 'insensitive' } };
    }
    const [total, rows] = await this.prisma.$transaction([
      this.prisma.erpPurSupplierRebate.count({ where }),
      this.prisma.erpPurSupplierRebate.findMany({
        where, include, orderBy: [{ periodYear: 'desc' }, { id: 'asc' }],
        skip: (page - 1) * limit, take: limit,
      }),
    ]);
    return {
      data: rows.map((r) => this.mapRow(r as RebateRow)),
      meta: { total, page, limit, totalPages: Math.max(1, Math.ceil(total / limit)) },
    };
  }

  private async assertNoDuplicate(
    supplierId: bigint, categoryId: bigint | null, periodYear: number, exceptId?: bigint,
  ) {
    const dup = await this.prisma.erpPurSupplierRebate.findFirst({
      where: {
        supplierId, categoryId, periodYear, deletedAt: null,
        ...(exceptId ? { id: { not: exceptId } } : {}),
      },
      select: { id: true },
    });
    if (dup) {
      throw new ConflictException(
        'Perjanjian rabat untuk supplier/kategori/tahun ini sudah ada',
      );
    }
  }

  async create(dto: CreatePurRebateDto, actorId?: number) {
    const supplierId = BigInt(dto.supplierId);
    const supplier = await this.prisma.erpPartner.findFirst({
      where: { id: supplierId, deletedAt: null }, select: { id: true },
    });
    if (!supplier) throw new NotFoundException('Supplier tidak ditemukan');
    const categoryId = dto.categoryId ? BigInt(dto.categoryId) : null;
    await this.assertNoDuplicate(supplierId, categoryId, dto.periodYear);
    const row = await this.prisma.erpPurSupplierRebate.create({
      data: {
        supplierId, categoryId,
        percent: new Prisma.Decimal(dto.percent),
        periodYear: dto.periodYear, notes: dto.notes ?? null,
        createdById: actorId != null ? BigInt(actorId) : null,
      },
      include,
    });
    return this.mapRow(row as RebateRow);
  }

  async update(id: string, dto: UpdatePurRebateDto, actorId?: number) {
    const rid = BigInt(id);
    const existing = await this.prisma.erpPurSupplierRebate.findFirst({
      where: { id: rid, deletedAt: null },
    });
    if (!existing) throw new NotFoundException('Perjanjian rabat tidak ditemukan');
    const supplierId = dto.supplierId ? BigInt(dto.supplierId) : existing.supplierId;
    const categoryId =
      dto.categoryId !== undefined
        ? dto.categoryId ? BigInt(dto.categoryId) : null
        : existing.categoryId;
    const periodYear = dto.periodYear ?? existing.periodYear;
    await this.assertNoDuplicate(supplierId, categoryId, periodYear, rid);
    const row = await this.prisma.erpPurSupplierRebate.update({
      where: { id: rid },
      data: {
        supplierId, categoryId, periodYear,
        ...(dto.percent !== undefined ? { percent: new Prisma.Decimal(dto.percent) } : {}),
        ...(dto.notes !== undefined ? { notes: dto.notes } : {}),
        ...(dto.isActive !== undefined ? { isActive: dto.isActive } : {}),
        updatedById: actorId != null ? BigInt(actorId) : null,
      },
      include,
    });
    return this.mapRow(row as RebateRow);
  }

  async remove(id: string, actorId?: number) {
    const rid = BigInt(id);
    const existing = await this.prisma.erpPurSupplierRebate.findFirst({
      where: { id: rid, deletedAt: null }, select: { id: true },
    });
    if (!existing) throw new NotFoundException('Perjanjian rabat tidak ditemukan');
    await this.prisma.erpPurSupplierRebate.update({
      where: { id: rid },
      data: {
        deletedAt: new Date(),
        updatedById: actorId != null ? BigInt(actorId) : null,
      },
    });
    return { id: Number(rid), deleted: true };
  }

  async bulkStatus(ids: number[], isActive: boolean) {
    const res = await this.prisma.erpPurSupplierRebate.updateMany({
      where: { id: { in: ids.map((i) => BigInt(i)) }, deletedAt: null },
      data: { isActive },
    });
    return { affected: res.count };
  }

  async bulkRemove(ids: number[]) {
    const res = await this.prisma.erpPurSupplierRebate.updateMany({
      where: { id: { in: ids.map((i) => BigInt(i)) }, deletedAt: null },
      data: { deletedAt: new Date() },
    });
    return { affected: res.count };
  }

  /**
   * Accrued rebate per agreement for a year: percent × net posted purchase
   * base (invoice subtotal − header discount; per-line net when the
   * agreement is scoped to a category). Informational — no GL posting.
   */
  async accrual(year?: number, supplierId?: string) {
    const y = year ?? new Date().getFullYear();
    const agreements = await this.prisma.erpPurSupplierRebate.findMany({
      where: {
        deletedAt: null, isActive: true, periodYear: y,
        ...(supplierId ? { supplierId: BigInt(supplierId) } : {}),
      },
      include, orderBy: { id: 'asc' },
    });
    const from = new Date(Date.UTC(y, 0, 1));
    const to = new Date(Date.UTC(y, 11, 31, 23, 59, 59));
    const data = [];
    for (const a of agreements) {
      const invoices = await this.prisma.erpPurInvoice.findMany({
        where: {
          supplierId: a.supplierId, deletedAt: null,
          postingStatus: 'POSTED', docDate: { gte: from, lte: to },
        },
        select: {
          subtotal: true, discountAmount: true,
          lines: {
            select: { itemId: true, baseQuantity: true, unitPrice: true, discountAmount: true },
          },
        },
      });
      let base = 0;
      if (!a.categoryId) {
        base = invoices.reduce(
          (s, inv) => s + Number(inv.subtotal) - Number(inv.discountAmount), 0,
        );
      } else {
        const itemIds = [...new Set(invoices.flatMap((inv) => inv.lines.map((l) => l.itemId)))];
        const items = itemIds.length
          ? await this.prisma.erpItem.findMany({
              where: { id: { in: itemIds } }, select: { id: true, categoryId: true },
            })
          : [];
        const catOf = new Map(items.map((i) => [i.id.toString(), i.categoryId]));
        for (const inv of invoices) {
          for (const l of inv.lines) {
            if (catOf.get(l.itemId.toString()) === a.categoryId) {
              base += Number(l.baseQuantity) * Number(l.unitPrice) - Number(l.discountAmount);
            }
          }
        }
      }
      const percent = Number(a.percent);
      data.push({
        ...this.mapRow(a as RebateRow),
        baseAmount: round2(base),
        accruedAmount: round2((base * percent) / 100),
        invoiceCount: invoices.length,
      });
    }
    return {
      periodYear: y,
      data,
      totalAccrued: round2(data.reduce((s, r) => s + r.accruedAmount, 0)),
    };
  }
}
