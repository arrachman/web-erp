import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { CreateInvLotDto, QueryInvLotsDto, UpdateInvLotDto } from './dto/inv-lot.dto';
import { InvLotAllocationService } from './inv-lot-allocation.service';

/**
 * Fase 2 T1 — Lot/Batch & expiry tracking. The ErpInvLot master + lot_id on
 * movement lines predate this module; what was missing is the API: CRUD lot
 * (metadata corrections never touch balances), saldo per lot derived from
 * POSTED movement lines, riwayat pergerakan, and the FEFO resolver endpoint.
 * Lots are born at goods receipt (see pur-goods-receipt posting hook) or
 * created manually here (e.g. opening stock).
 */
@Injectable()
export class ErpInvLotsService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly allocation: InvLotAllocationService,
  ) {}

  private present(row: any, balance?: Prisma.Decimal, perWarehouse?: { warehouseId: string | null; balance: string }[]) {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const exp = row.expiryDate ? new Date(row.expiryDate) : null;
    return {
      id: row.id?.toString(),
      lotNumber: row.lotNumber,
      itemId: row.itemId?.toString(),
      itemName: row._itemName ?? null,
      itemCode: row._itemCode ?? null,
      supplierLotNo: row.supplierLotNo,
      manufactureDate: row.manufactureDate ? new Date(row.manufactureDate).toISOString().slice(0, 10) : null,
      expiryDate: exp ? exp.toISOString().slice(0, 10) : null,
      daysToExpiry: exp ? Math.round((exp.getTime() - today.getTime()) / 86400000) : null,
      isExpired: exp ? exp.getTime() < today.getTime() : false,
      originGoodsReceiptId: row.originGoodsReceiptId?.toString() ?? null,
      status: row.status,
      notes: row.notes,
      balance: balance !== undefined ? balance.toString() : undefined,
      perWarehouse,
    };
  }

  private async withItemNames(rows: any[]): Promise<any[]> {
    const ids = [...new Set(rows.map((r) => r.itemId?.toString()).filter(Boolean))];
    if (!ids.length) return rows;
    const items = await this.prisma.erpItem.findMany({
      where: { id: { in: ids.map((i) => BigInt(i as string)) } },
      select: { id: true, name: true, code: true },
    });
    const byId = new Map(items.map((i) => [i.id.toString(), i]));
    for (const r of rows) {
      const it = byId.get(r.itemId?.toString());
      r._itemName = it?.name ?? null;
      r._itemCode = it?.code ?? null;
    }
    return rows;
  }

  async list(query: QueryInvLotsDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 20;
    const where: Prisma.ErpInvLotWhereInput = { deletedAt: null };
    if (query.itemId) where.itemId = BigInt(query.itemId);
    if (query.status) where.status = query.status as any;
    if (query.search) {
      const named = await this.prisma.erpItem.findMany({
        where: { name: { contains: query.search, mode: 'insensitive' }, deletedAt: null },
        select: { id: true },
        take: 200,
      });
      where.OR = [
        { lotNumber: { contains: query.search, mode: 'insensitive' } },
        ...(named.length ? [{ itemId: { in: named.map((n) => n.id) } }] : []),
      ];
    }
    if (query.expiringWithinDays !== undefined) {
      const cutoff = new Date();
      cutoff.setDate(cutoff.getDate() + query.expiringWithinDays);
      where.expiryDate = { lte: cutoff };
    }
    const [total, rows] = await this.prisma.$transaction([
      this.prisma.erpInvLot.count({ where }),
      this.prisma.erpInvLot.findMany({
        where,
        orderBy: [{ expiryDate: { sort: 'asc', nulls: 'last' } }, { createdAt: 'asc' }],
        skip: (page - 1) * limit,
        take: limit,
      }),
    ]);
    const bals = await this.allocation.balances(this.prisma, {});
    const totals = new Map<string, Prisma.Decimal>();
    for (const b of bals) {
      const k = b.lotId.toString();
      totals.set(k, (totals.get(k) ?? new Prisma.Decimal(0)).add(b.balance));
    }
    await this.withItemNames(rows as any[]);
    const data = rows.map((r) => this.present(r, totals.get(r.id.toString()) ?? new Prisma.Decimal(0)));
    return { data, meta: { page, limit, total, totalPages: Math.max(1, Math.ceil(total / limit)) } };
  }

  async get(id: string) {
    const row = await this.prisma.erpInvLot.findFirst({
      where: { id: BigInt(id), deletedAt: null },
    });
    if (!row) throw new NotFoundException('Lot tidak ditemukan.');
    await this.withItemNames([row as any]);
    const bals = await this.allocation.balances(this.prisma, { lotId: row.id });
    const perWarehouse = bals.map((b) => ({
      warehouseId: b.warehouseId?.toString() ?? null,
      balance: b.balance.toString(),
    }));
    const total = bals.reduce((s, b) => s.add(b.balance), new Prisma.Decimal(0));
    const movements = await this.prisma.erpInvStockMovementLine.findMany({
      where: { lotId: row.id },
      include: {
        stockMovement: {
          select: { docNumber: true, movementType: true, movementDate: true, status: true },
        },
      },
      orderBy: { id: 'desc' },
      take: 50,
    });
    return {
      ...this.present(row, total, perWarehouse),
      movements: movements.map((m) => ({
        movementDocNumber: m.stockMovement.docNumber,
        movementType: m.stockMovement.movementType,
        movementDate: m.stockMovement.movementDate,
        status: m.stockMovement.status,
        baseQuantity: m.baseQuantity.toString(),
      })),
    };
  }

  async create(dto: CreateInvLotDto, actorId?: string) {
    const item = await this.prisma.erpItem.findFirst({
      where: { id: BigInt(dto.itemId), deletedAt: null },
      select: { id: true },
    });
    if (!item) throw new NotFoundException('Item tidak ditemukan.');
    const dup = await this.prisma.erpInvLot.findFirst({
      where: { itemId: item.id, lotNumber: dto.lotNumber, deletedAt: null },
    });
    if (dup) throw new BadRequestException(`Lot ${dto.lotNumber} untuk item ini sudah ada.`);
    if (dto.manufactureDate && dto.expiryDate && dto.expiryDate < dto.manufactureDate) {
      throw new BadRequestException('Tanggal expiry tidak boleh sebelum tanggal produksi.');
    }
    const row = await this.prisma.erpInvLot.create({
      data: {
        lotNumber: dto.lotNumber,
        itemId: item.id,
        supplierLotNo: dto.supplierLotNo ?? null,
        manufactureDate: dto.manufactureDate ? new Date(dto.manufactureDate) : null,
        expiryDate: dto.expiryDate ? new Date(dto.expiryDate) : null,
        status: (dto.status as any) ?? 'ACTIVE',
        notes: dto.notes ?? null,
        createdById: actorId ? BigInt(actorId) : null,
        updatedById: actorId ? BigInt(actorId) : null,
      },
    });
    await this.withItemNames([row as any]);
    return this.present(row, new Prisma.Decimal(0));
  }

  async update(id: string, dto: UpdateInvLotDto, actorId?: string) {
    const row = await this.prisma.erpInvLot.findFirst({
      where: { id: BigInt(id), deletedAt: null },
    });
    if (!row) throw new NotFoundException('Lot tidak ditemukan.');
    if (dto.lotNumber && dto.lotNumber !== row.lotNumber) {
      const dup = await this.prisma.erpInvLot.findFirst({
        where: { itemId: row.itemId, lotNumber: dto.lotNumber, deletedAt: null, id: { not: row.id } },
      });
      if (dup) throw new BadRequestException(`Lot ${dto.lotNumber} untuk item ini sudah ada.`);
    }
    const mfg = dto.manufactureDate !== undefined ? (dto.manufactureDate ? new Date(dto.manufactureDate) : null) : row.manufactureDate;
    const exp = dto.expiryDate !== undefined ? (dto.expiryDate ? new Date(dto.expiryDate) : null) : row.expiryDate;
    if (mfg && exp && exp < mfg) {
      throw new BadRequestException('Tanggal expiry tidak boleh sebelum tanggal produksi.');
    }
    const updated = await this.prisma.erpInvLot.update({
      where: { id: row.id },
      data: {
        lotNumber: dto.lotNumber ?? row.lotNumber,
        supplierLotNo: dto.supplierLotNo !== undefined ? dto.supplierLotNo : row.supplierLotNo,
        manufactureDate: mfg,
        expiryDate: exp,
        status: (dto.status as any) ?? row.status,
        notes: dto.notes !== undefined ? dto.notes : row.notes,
        updatedById: actorId ? BigInt(actorId) : null,
      },
    });
    await this.withItemNames([updated as any]);
    const total = await this.allocation.lotBalance(this.prisma, row.id);
    return this.present(updated, total);
  }

  async remove(id: string) {
    const row = await this.prisma.erpInvLot.findFirst({
      where: { id: BigInt(id), deletedAt: null },
    });
    if (!row) throw new NotFoundException('Lot tidak ditemukan.');
    const used = await this.prisma.erpInvStockMovementLine.count({ where: { lotId: row.id } });
    if (used) {
      throw new BadRequestException('Lot sudah punya riwayat pergerakan — tidak dapat dihapus (ubah status jadi BLOCKED).');
    }
    await this.prisma.erpInvLot.update({ where: { id: row.id }, data: { deletedAt: new Date() } });
    return { success: true };
  }

  async fefo(itemId: string, warehouseId: string, quantity: string) {
    const item = await this.prisma.erpItem.findFirst({
      where: { id: BigInt(itemId), deletedAt: null },
      select: { id: true },
    });
    if (!item) throw new NotFoundException('Item tidak ditemukan.');
    const plan = await this.allocation.planFefo(
      this.prisma,
      item.id,
      BigInt(warehouseId),
      new Prisma.Decimal(quantity),
    );
    return { itemId, warehouseId, requested: quantity, ...plan };
  }
}
