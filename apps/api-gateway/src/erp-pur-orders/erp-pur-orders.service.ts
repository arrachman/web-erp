import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { PurOrderPostingService } from './pur-order-posting.service';
import { enrichOrders } from './pur-order-enrich';
import { CreatePurOrderDto } from './dto/create-pur-order.dto';
import { QueryPurOrdersDto } from './dto/query-pur-orders.dto';
import { UpdatePurOrderDto } from './dto/update-pur-order.dto';
import {
  PurOrderTransitionAction as A,
  TransitionPurOrderDto,
} from './dto/transition-pur-order.dto';
import {
  toBigInt,
  EDITABLE,
  NEXT,
  buildPurOrderWhere,
  mapOrderLine,
  computeTotals,
} from './pur-order.helpers';

const DOC_CODE = 'PO';
const FALLBACK_PREFIX = 'PO';

@Injectable()
export class ErpPurOrdersService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly posting: PurOrderPostingService,
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
    const count = await tx.erpPurOrder.count();
    return `${FALLBACK_PREFIX}${String(count + 1).padStart(6, '0')}`;
  }

  private async findRaw(id: bigint) {
    const order = await this.prisma.erpPurOrder.findFirst({
      where: { id, deletedAt: null },
      include: { lines: { orderBy: { lineNo: 'asc' } } },
    });
    if (!order) throw new NotFoundException('Purchase order tidak ditemukan');
    return order;
  }

  private async one(id: bigint) {
    const order = await this.findRaw(id);
    const [enriched] = await enrichOrders(this.prisma, [order]);
    return { success: true, data: enriched };
  }

  // ── CRUD ────────────────────────────────────────────────────────────────────
  async create(dto: CreatePurOrderDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;
    const header = { currencyId: dto.currencyId, exchangeRate: dto.exchangeRate };
    const { subtotal, grandTotal } = computeTotals(dto.lines, dto);

    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.docDate);
      const wantAuto = dto.auto !== false && !dto.docNumber;
      const docNumber = wantAuto ? await this.genDocNumber(tx) : dto.docNumber;
      if (!docNumber) throw new BadRequestException('No dokumen wajib diisi.');

      const row = await tx.erpPurOrder.create({
        data: {
          docNumber,
          autoNumber: wantAuto ? docNumber : null,
          branchId: BigInt(dto.branchId),
          locationId: toBigInt(dto.locationId),
          warehouseId: toBigInt(dto.warehouseId),
          docDate: new Date(dto.docDate),
          fiscalPeriodId,
          supplierId: toBigInt(dto.supplierId),
          paymentTermId: toBigInt(dto.paymentTermId),
          dueDate: dto.dueDate ? new Date(dto.dueDate) : null,
          currencyId: BigInt(dto.currencyId),
          exchangeRate: new Prisma.Decimal(dto.exchangeRate),
          priceMode: (dto.priceMode ?? 'TAX_EXCLUSIVE') as never,
          subtotal,
          discountPercent: dto.discountPercent != null ? new Prisma.Decimal(dto.discountPercent) : null,
          discountAmount: dto.discountAmount != null ? new Prisma.Decimal(dto.discountAmount) : null,
          tax1Amount: dto.tax1Amount != null ? new Prisma.Decimal(dto.tax1Amount) : null,
          tax2Amount: dto.tax2Amount != null ? new Prisma.Decimal(dto.tax2Amount) : null,
          otherCostAmount: dto.otherCostAmount != null ? new Prisma.Decimal(dto.otherCostAmount) : null,
          grandTotal,
          description: dto.description ?? null,
          notes: dto.notes ?? null,
          referenceNo: dto.referenceNo ?? null,
          referenceDate: dto.referenceDate ? new Date(dto.referenceDate) : null,
          payableAccountId: toBigInt(dto.payableAccountId),
          requisitionId: toBigInt(dto.requisitionId),
          quotationId: toBigInt(dto.quotationId),
          bidSelectionId: toBigInt(dto.bidSelectionId),
          status: 'DRAFT',
          postingStatus: 'UNPOSTED',
          legacyCode: dto.legacyCode ?? null,
          createdById: actor,
          updatedById: actor,
          lines: dto.lines.length
            ? { create: dto.lines.map((l) => mapOrderLine(l, header)) }
            : undefined,
        },
        select: { id: true },
      });
      return row;
    });
    return this.one(created.id);
  }

  async findAll(query: QueryPurOrdersDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const where = buildPurOrderWhere(query);

    const sortBy = query.sortBy ?? 'docDate';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpPurOrder.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }],
        skip: (page - 1) * limit,
        take: limit,
        include: { lines: { orderBy: { lineNo: 'asc' } } },
      }),
      this.prisma.erpPurOrder.count({ where }),
    ]);

    return {
      success: true,
      data: await enrichOrders(this.prisma, items),
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  findOne(id: bigint) {
    return this.one(id);
  }

  async update(id: bigint, dto: UpdatePurOrderDto, actorId?: string) {
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
      const data: Prisma.ErpPurOrderUpdateInput = { updatedById: actor };
      if (dto.docNumber !== undefined) data.docNumber = dto.docNumber;
      if (dto.branchId !== undefined) data.branchId = BigInt(dto.branchId);
      if (dto.locationId !== undefined) data.locationId = toBigInt(dto.locationId);
      if (dto.warehouseId !== undefined) data.warehouseId = toBigInt(dto.warehouseId);
      if (dto.supplierId !== undefined) data.supplierId = toBigInt(dto.supplierId);
      if (dto.paymentTermId !== undefined) data.paymentTermId = toBigInt(dto.paymentTermId);
      if (dto.dueDate !== undefined) data.dueDate = dto.dueDate ? new Date(dto.dueDate) : null;
      if (dto.currencyId !== undefined) data.currencyId = BigInt(dto.currencyId);
      if (dto.exchangeRate !== undefined) data.exchangeRate = new Prisma.Decimal(dto.exchangeRate);
      if (dto.priceMode !== undefined) data.priceMode = dto.priceMode as never;
      if (dto.description !== undefined) data.description = dto.description;
      if (dto.notes !== undefined) data.notes = dto.notes;
      if (dto.referenceNo !== undefined) data.referenceNo = dto.referenceNo;
      if (dto.referenceDate !== undefined) {
        data.referenceDate = dto.referenceDate ? new Date(dto.referenceDate) : null;
      }
      if (dto.payableAccountId !== undefined) {
        data.payableAccountId = toBigInt(dto.payableAccountId);
      }
      if (dto.discountPercent !== undefined) {
        data.discountPercent = dto.discountPercent != null ? new Prisma.Decimal(dto.discountPercent) : null;
      }
      if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;
      if (dto.docDate !== undefined) {
        data.docDate = new Date(dto.docDate);
        data.fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.docDate);
      } else if (dto.fiscalPeriodId !== undefined) {
        data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);
      }

      // Header money fields participate in totals — recompute against merged values.
      const discountAmount =
        dto.discountAmount !== undefined ? dto.discountAmount : existing.discountAmount?.toString();
      const tax1Amount =
        dto.tax1Amount !== undefined ? dto.tax1Amount : existing.tax1Amount?.toString();
      const tax2Amount =
        dto.tax2Amount !== undefined ? dto.tax2Amount : existing.tax2Amount?.toString();
      const otherCostAmount =
        dto.otherCostAmount !== undefined
          ? dto.otherCostAmount
          : existing.otherCostAmount?.toString();
      if (dto.discountAmount !== undefined) {
        data.discountAmount = dto.discountAmount != null ? new Prisma.Decimal(dto.discountAmount) : null;
      }
      if (dto.tax1Amount !== undefined) {
        data.tax1Amount = dto.tax1Amount != null ? new Prisma.Decimal(dto.tax1Amount) : null;
      }
      if (dto.tax2Amount !== undefined) {
        data.tax2Amount = dto.tax2Amount != null ? new Prisma.Decimal(dto.tax2Amount) : null;
      }
      if (dto.otherCostAmount !== undefined) {
        data.otherCostAmount = dto.otherCostAmount != null ? new Prisma.Decimal(dto.otherCostAmount) : null;
      }

      const lines = dto.lines ?? existing.lines.map((l) => ({
        itemId: l.itemId.toString(),
        quantity: l.quantity.toString(),
        unitId: l.unitId.toString(),
        unitPrice: l.unitPrice.toString(),
        discountPercent: l.discountPercent?.toString(),
        discountAmount: l.discountAmount?.toString(),
        tax1Amount: l.tax1Amount?.toString(),
        tax2Amount: l.tax2Amount?.toString(),
        lineNo: l.lineNo,
      }));
      const { subtotal, grandTotal } = computeTotals(lines as never, {
        discountAmount,
        tax1Amount,
        tax2Amount,
        otherCostAmount,
      });
      data.subtotal = subtotal;
      data.grandTotal = grandTotal;

      if (dto.lines !== undefined) {
        await tx.erpPurOrderLine.deleteMany({ where: { orderId: id } });
        data.lines = { create: dto.lines.map((l) => mapOrderLine(l, header)) };
      }
      await tx.erpPurOrder.update({ where: { id }, data });
    });
    return this.one(id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status === 'POSTED') {
      throw new BadRequestException('Dokumen POSTED tidak bisa dihapus. Reopen dulu.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.erpPurOrder.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actor },
    });
    return { success: true, message: 'Purchase order dihapus' };
  }

  // ── workflow (§2.7 state machine) ────────────────────────────────────────────
  async transition(id: bigint, dto: TransitionPurOrderDto, actorId?: string) {
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
        await tx.erpPurOrder.update({
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
        await tx.erpPurOrder.update({
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

    await this.prisma.erpPurOrder.update({
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
