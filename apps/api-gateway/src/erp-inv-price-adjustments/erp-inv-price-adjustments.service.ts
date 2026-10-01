import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { enrichPriceAdjustments } from './inv-price-adjustment-enrich';
import { CreateInvPriceAdjustmentDto, ErpCostingMethodDto } from './dto/create-inv-price-adjustment.dto';
import { QueryInvPriceAdjustmentsDto } from './dto/query-inv-price-adjustments.dto';
import { toBigInt, buildPriceAdjWhere, genDocNumber } from './inv-price-adjustment.helpers';
import { InvMovingAverageCostService } from '../erp-inv-gl/inv-moving-average-cost.service';

@Injectable()
export class ErpInvPriceAdjustmentsService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly cost: InvMovingAverageCostService,
  ) {}

  private async resolvePeriod(
    tx: Prisma.TransactionClient,
    fiscalPeriodId: string | undefined,
    date: string,
  ): Promise<bigint> {
    if (fiscalPeriodId) return BigInt(fiscalPeriodId);
    const d = new Date(date);
    const period = await tx.erpFiscalPeriod.findFirst({
      where: { deletedAt: null, startDate: { lte: d }, endDate: { gte: d } },
      select: { id: true },
    });
    if (!period) {
      throw new BadRequestException(`Tidak ada periode fiskal yang memuat tanggal ${date}.`);
    }
    return period.id;
  }

  private async findRaw(id: bigint) {
    const row = await this.prisma.erpInvCostRecalculation.findFirst({
      where: { id, deletedAt: null },
    });
    if (!row) throw new NotFoundException('Price adjustment tidak ditemukan');
    return row;
  }

  private async one(id: bigint) {
    const row = await this.findRaw(id);
    const [enriched] = await enrichPriceAdjustments(this.prisma, [row]);
    return { success: true, data: enriched };
  }

  // ── CRUD ──────────────────────────────────────────────────────────────────

  async create(dto: CreateInvPriceAdjustmentDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;

    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.fromDate);
      const docNumber = dto.docNumber ?? (await genDocNumber(tx, 'PA'));

      const row = await tx.erpInvCostRecalculation.create({
        data: {
          docNumber,
          costingMethod: (dto.costingMethod ?? ErpCostingMethodDto.AVG) as never,
          triggerType: 'MANUAL',
          itemId: toBigInt(dto.itemId),
          warehouseId: toBigInt(dto.warehouseId),
          fromDate: new Date(dto.fromDate),
          toDate: dto.toDate ? new Date(dto.toDate) : null,
          fiscalPeriodId,
          status: 'PENDING',
          notes: dto.notes ?? null,
          legacyCode: dto.legacyCode ?? null,
          createdById: actor,
          updatedById: actor,
        },
        select: { id: true },
      });
      return row;
    });
    return this.one(created.id);
  }

  async findAll(query: QueryInvPriceAdjustmentsDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const where = buildPriceAdjWhere(query);

    const sortBy = query.sortBy ?? 'fromDate';
    const sortDir = query.sortDir ?? 'desc';

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpInvCostRecalculation.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }],
        skip: (page - 1) * limit,
        take: limit,
      }),
      this.prisma.erpInvCostRecalculation.count({ where }),
    ]);

    return {
      success: true,
      data: await enrichPriceAdjustments(this.prisma, items),
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  findOne(id: bigint) {
    return this.one(id);
  }

  // ── Process (cost recalculation) ───────────────────────────────────────────

  /**
   * Distinct item ids that carry POSTED stock — used when the PA header has no
   * itemId (recalc the whole catalogue), optionally scoped to a warehouse.
   */
  private async scopeItemIds(warehouseId: bigint | null): Promise<bigint[]> {
    const movWh = warehouseId
      ? Prisma.sql`AND COALESCE(l.destination_warehouse_id, l.source_warehouse_id) = ${warehouseId}::bigint`
      : Prisma.empty;
    const openWh = warehouseId
      ? Prisma.sql`AND ol.warehouse_id = ${warehouseId}::bigint`
      : Prisma.empty;

    const rows = await this.prisma.$queryRaw<{ item_id: bigint }[]>(Prisma.sql`
      SELECT DISTINCT item_id FROM (
        SELECT l.item_id AS item_id
        FROM inv_stock_movement_lines l
        JOIN inv_stock_movements m ON m.id = l.stock_movement_id
        WHERE m.status = 'POSTED' AND m.deleted_at IS NULL ${movWh}
        UNION ALL
        SELECT ol.item_id AS item_id
        FROM inv_opening_stock_lines ol
        JOIN inv_opening_stocks o ON o.id = ol.opening_stock_id
        WHERE o.status = 'POSTED' AND o.deleted_at IS NULL ${openWh}
      ) u
    `);
    return rows.map((r) => r.item_id);
  }

  /**
   * Run the cost recalculation: derive new moving-average cost + on-hand qty per
   * item SERVER-SIDE, compute delta = (newCost - oldCost) * qty, write recalc
   * lines, persist the new averageCost on each ErpItem, and mark COMPLETED.
   */
  async process(id: bigint, actorId: string | null) {
    const header = await this.findRaw(id);
    if (header.status !== 'PENDING' && header.status !== 'FAILED') {
      throw new BadRequestException('Hanya dokumen berstatus PENDING/FAILED yang bisa diproses.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    const warehouseId = header.warehouseId ?? null;

    // Lines require a non-null warehouse_id; company-wide PA (no warehouse) can't
    // attribute a line to a specific warehouse, so a warehouse is mandatory here.
    if (warehouseId === null) {
      const failMsg = 'Gudang wajib diisi pada header untuk memproses recalculation.';
      await this.prisma.erpInvCostRecalculation.update({
        where: { id },
        data: { status: 'FAILED' as never, startedAt: new Date(), updatedById: actor },
      });
      throw new BadRequestException(failMsg);
    }

    try {
      const itemIds = header.itemId
        ? [header.itemId]
        : await this.scopeItemIds(warehouseId);

      const snap = await this.cost.costAndQtyByItem(itemIds, warehouseId);

      const items = await this.prisma.erpItem.findMany({
        where: { id: { in: itemIds } },
        select: { id: true, averageCost: true, standardCost: true },
      });
      const oldCostById = new Map<string, Prisma.Decimal>();
      for (const it of items) {
        const old = it.averageCost ?? it.standardCost ?? new Prisma.Decimal(0);
        oldCostById.set(it.id.toString(), new Prisma.Decimal(old));
      }

      const lines: Prisma.ErpInvCostRecalculationLineCreateManyInput[] = [];
      const newCostById = new Map<string, Prisma.Decimal>();
      let totalDelta = new Prisma.Decimal(0);
      let lineNo = 0;

      for (const itemId of itemIds) {
        const key = itemId.toString();
        const s = snap.get(key);
        if (!s || s.qty.lessThanOrEqualTo(0)) continue;
        const oldUnitCost = oldCostById.get(key) ?? new Prisma.Decimal(0);
        const newUnitCost = s.avgCost;
        const affectedQty = s.qty;
        const deltaAmount = newUnitCost.minus(oldUnitCost).times(affectedQty);
        lineNo += 1;
        lines.push({
          costRecalculationId: id,
          itemId,
          warehouseId,
          oldUnitCost,
          newUnitCost,
          affectedQty,
          deltaAmount,
          lineNo,
        });
        newCostById.set(key, newUnitCost);
        totalDelta = totalDelta.plus(deltaAmount);
      }

      await this.prisma.$transaction(async (tx) => {
        await tx.erpInvCostRecalculationLine.deleteMany({
          where: { costRecalculationId: id },
        });
        if (lines.length) {
          await tx.erpInvCostRecalculationLine.createMany({ data: lines });
        }
        // Persist the recalculated moving average onto each item — the POINT of
        // a cost recalculation.
        for (const [itemKey, newCost] of newCostById) {
          await tx.erpItem.update({
            where: { id: BigInt(itemKey) },
            data: { averageCost: newCost },
          });
        }
        await tx.erpInvCostRecalculation.update({
          where: { id },
          data: {
            status: 'COMPLETED' as never,
            totalDelta,
            startedAt: new Date(),
            completedAt: new Date(),
            updatedById: actor,
          },
        });
      });
    } catch (err) {
      await this.prisma.erpInvCostRecalculation.update({
        where: { id },
        data: { status: 'FAILED' as never, startedAt: new Date(), updatedById: actor },
      });
      throw err;
    }

    return this.one(id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status !== 'PENDING' && existing.status !== 'FAILED') {
      throw new BadRequestException(
        `Hanya price adjustment berstatus PENDING atau FAILED yang bisa dihapus.`,
      );
    }
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.erpInvCostRecalculation.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actor },
    });
    return { success: true, message: 'Price adjustment dihapus' };
  }
}
