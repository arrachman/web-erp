import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { SlsOrderPostingService } from './sls-order-posting.service';
import { enrichOrders } from './sls-order-enrich';
import { CreateSlsOrderDto } from './dto/create-sls-order.dto';
import { QuerySlsOrdersDto } from './dto/query-sls-orders.dto';
import { UpdateSlsOrderDto } from './dto/update-sls-order.dto';
import {
  SlsOrderTransitionAction as A,
  TransitionSlsOrderDto,
} from './dto/transition-sls-order.dto';
import {
  toBigInt,
  EDITABLE,
  NEXT,
  buildSlsOrderWhere,
  mapOrderLine,
  computeOrderTotals,
} from './sls-order.helpers';
import {
  buildSlsOrderCreateData,
  buildSlsOrderUpdatePatch,
  buildSlsOrderTotalsInput,
  mapExistingSlsOrderLines,
} from './sls-order-persistence.mapper';

const DOC_CODE = 'SO';
const FALLBACK_PREFIX = 'SO';

@Injectable()
export class ErpSlsOrdersService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly posting: SlsOrderPostingService,
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

  private async genDocNumber(tx: Prisma.TransactionClient): Promise<string> {
    const numbering = await tx.erpDocumentNumbering.findFirst({
      where: { documentCode: DOC_CODE, deletedAt: null },
    });
    if (numbering) {
      const seq = numbering.nextNumber;
      await tx.erpDocumentNumbering.update({
        where: { id: numbering.id },
        data: { nextNumber: seq + 1 },
      });
      return `${numbering.prefix}${String(seq).padStart(numbering.digitCount, '0')}`;
    }
    const count = await this.prisma.erpSlsOrder.count();
    return `${FALLBACK_PREFIX}${String(count + 1).padStart(6, '0')}`;
  }

  private async findRaw(id: bigint) {
    const order = await this.prisma.erpSlsOrder.findFirst({
      where: { id, deletedAt: null },
      include: { lines: { orderBy: { lineNo: 'asc' } } },
    });
    if (!order) throw new NotFoundException('Sales order tidak ditemukan');
    return order;
  }

  private async one(id: bigint) {
    const order = await this.findRaw(id);
    const [enriched] = await enrichOrders(this.prisma, [order]);
    return { success: true, data: enriched };
  }

  /** Fetch rate map for all tax ids referenced by a line set. */
  private async taxRateMap(taxIds: (string | undefined)[]): Promise<Map<string, Prisma.Decimal>> {
    const ids = [...new Set(taxIds.filter((v): v is string => !!v))];
    if (!ids.length) return new Map();
    const taxes = await this.prisma.erpTax.findMany({
      where: { id: { in: ids.map(BigInt) }, deletedAt: null },
      select: { id: true, rate: true },
    });
    const m = new Map<string, Prisma.Decimal>();
    for (const t of taxes) m.set(t.id.toString(), t.rate);
    return m;
  }

  /** Derive dueDate from payment term netDays if caller did not supply it. */
  private async resolveDueDate(
    paymentTermId: string | undefined,
    docDate: string,
    explicitDueDate?: string,
  ): Promise<Date | null> {
    if (explicitDueDate) return new Date(explicitDueDate);
    if (!paymentTermId) return null;
    const term = await this.prisma.erpPaymentTerm.findUnique({
      where: { id: BigInt(paymentTermId) },
      select: { netDays: true },
    });
    if (!term) return null;
    const d = new Date(docDate);
    d.setDate(d.getDate() + term.netDays);
    return d;
  }

  // ── CRUD ────────────────────────────────────────────────────────────────────
  async create(dto: CreateSlsOrderDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;
    const priceMode = (dto.priceMode ?? 'TAX_EXCLUSIVE') as 'TAX_EXCLUSIVE' | 'TAX_INCLUSIVE';
    const header = { currencyId: dto.currencyId, exchangeRate: dto.exchangeRate };

    const taxIds = dto.lines.flatMap((l) => [l.tax1Id, l.tax2Id]);
    const rateById = await this.taxRateMap(taxIds);
    const { subtotal, grandTotal, lines: computedLines, discountAmount, otherCostAmount } =
      computeOrderTotals(dto.lines, dto, rateById, priceMode);

    const dueDate = await this.resolveDueDate(dto.paymentTermId, dto.docDate, dto.dueDate);

    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.docDate);
      const wantAuto = dto.auto !== false && !dto.docNumber;
      const docNumber = wantAuto ? await this.genDocNumber(tx) : dto.docNumber;
      if (!docNumber) throw new BadRequestException('No dokumen wajib diisi.');

      const data = buildSlsOrderCreateData(dto, {
        actor,
        priceMode,
        fiscalPeriodId,
        docNumber,
        wantAuto,
        dueDate,
        subtotal,
        grandTotal,
        discountAmount,
        otherCostAmount,
        computedLines,
        header,
      });
      const row = await tx.erpSlsOrder.create({ data, select: { id: true } });
      return row;
    });
    return this.one(created.id);
  }

  async findAll(query: QuerySlsOrdersDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const where = buildSlsOrderWhere(query);

    const sortBy = query.sortBy ?? 'docDate';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total, agg] = await this.prisma.$transaction([
      this.prisma.erpSlsOrder.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }],
        skip: (page - 1) * limit,
        take: limit,
        include: { lines: { orderBy: { lineNo: 'asc' } } },
      }),
      this.prisma.erpSlsOrder.count({ where }),
      this.prisma.erpSlsOrder.aggregate({ where, _sum: { grandTotal: true } }),
    ]);

    return {
      success: true,
      data: await enrichOrders(this.prisma, items),
      meta: {
        page,
        limit,
        total,
        totalPages: Math.ceil(total / limit) || 1,
        sumGrandTotal: agg._sum.grandTotal?.toString() ?? '0',
      },
    };
  }

  findOne(id: bigint) {
    return this.one(id);
  }

  async update(id: bigint, dto: UpdateSlsOrderDto, actorId?: string) {
    const existing = await this.findRaw(id);
    if (!EDITABLE.has(existing.status)) {
      throw new BadRequestException(
        `Dokumen berstatus ${existing.status} tidak bisa diedit. Reopen dulu bila perlu.`,
      );
    }
    const actor = actorId ? BigInt(actorId) : null;
    const header = {
      currencyId: dto.currencyId ?? existing.currencyId.toString(),
      exchangeRate: dto.exchangeRate ?? existing.exchangeRate.toString(),
    };

    await this.prisma.$transaction(async (tx) => {
      const data = buildSlsOrderUpdatePatch(dto, actor);

      if (dto.docDate !== undefined) {
        data.docDate = new Date(dto.docDate);
        data.fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.docDate);
      } else if (dto.fiscalPeriodId !== undefined) {
        data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);
      }

      const mergedLines =
        dto.lines ?? mapExistingSlsOrderLines(existing.lines as Parameters<typeof mapExistingSlsOrderLines>[0]);

      const priceMode = ((dto.priceMode ?? existing.priceMode) as string) as 'TAX_EXCLUSIVE' | 'TAX_INCLUSIVE';
      const taxIds = mergedLines.flatMap((l) => [l.tax1Id, l.tax2Id]);
      const rateById = await this.taxRateMap(taxIds);
      const totals = computeOrderTotals(
        mergedLines,
        buildSlsOrderTotalsInput(dto, {
          discountAmount: existing.discountAmount,
          tax1Amount: existing.tax1Amount,
          tax2Amount: existing.tax2Amount,
          otherCostAmount: existing.otherCostAmount,
        }),
        rateById,
        priceMode,
      );
      data.subtotal = totals.subtotal;
      data.grandTotal = totals.grandTotal;
      if (totals.discountAmount !== null) data.discountAmount = totals.discountAmount;
      if (totals.otherCostAmount !== null) data.otherCostAmount = totals.otherCostAmount;

      // Recompute dueDate if paymentTerm changed but dueDate not explicitly set.
      if (dto.paymentTermId !== undefined && dto.dueDate === undefined) {
        const termId = dto.paymentTermId ?? null;
        const docDateStr = dto.docDate ?? existing.docDate.toISOString().slice(0, 10);
        data.dueDate = await this.resolveDueDate(termId ?? undefined, docDateStr, undefined);
      }

      if (dto.lines !== undefined) {
        await tx.erpSlsOrderLine.deleteMany({ where: { orderId: id } });
        data.lines = { create: totals.lines.map((l) => mapOrderLine(l, header)) };
      }
      await tx.erpSlsOrder.update({ where: { id }, data });
    });
    return this.one(id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status === 'POSTED') {
      throw new BadRequestException('Dokumen POSTED tidak bisa dihapus. Reopen dulu.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.erpSlsOrder.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actor },
    });
    return { success: true, message: 'Sales order dihapus' };
  }

  // ── workflow (§2.7 state machine) ────────────────────────────────────────────
  async transition(id: bigint, dto: TransitionSlsOrderDto, actorId?: string) {
    const order = await this.findRaw(id);
    const actor = actorId ? BigInt(actorId) : null;
    const next = NEXT[order.status]?.[dto.action];
    if (!next) {
      throw new BadRequestException(`Aksi ${dto.action} tidak valid dari status ${order.status}.`);
    }
    if (dto.action === A.REJECT && !dto.reason?.trim()) {
      throw new BadRequestException('Alasan reject wajib diisi.');
    }

    if (dto.action === A.POST) {
      const period = await this.prisma.erpFiscalPeriod.findUnique({
        where: { id: order.fiscalPeriodId },
        select: { status: true },
      });
      if (period?.status === 'CLOSED') {
        throw new BadRequestException('Periode fiskal sudah ditutup — tidak bisa posting.');
      }
      await this.prisma.$transaction(async (tx) => {
        await this.posting.reverseLedger(tx, order.id);
        await this.posting.postToLedger(tx, order, actor);
        await tx.erpSlsOrder.update({
          where: { id },
          data: {
            status: 'POSTED',
            previousStatus: order.status as never,
            postingStatus: 'POSTED',
            postedAt: new Date(),
            updatedById: actor,
          },
        });
      });
      return this.one(id);
    }

    if (dto.action === A.REOPEN) {
      await this.prisma.$transaction(async (tx) => {
        await this.posting.reverseLedger(tx, order.id);
        await tx.erpSlsOrder.update({
          where: { id },
          data: {
            status: 'DRAFT',
            previousStatus: order.status as never,
            postingStatus: 'UNPOSTED',
            postedAt: null,
            updatedById: actor,
          },
        });
      });
      return this.one(id);
    }

    await this.prisma.erpSlsOrder.update({
      where: { id },
      data: {
        status: next as never,
        previousStatus: order.status as never,
        updatedById: actor,
        ...(dto.action === A.REJECT
          ? {
              metadata: {
                ...((order.metadata as object) ?? {}),
                rejectReason: dto.reason,
              },
            }
          : {}),
      },
    });
    return this.one(id);
  }
}