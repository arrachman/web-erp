import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { InvOpeningStockPostingService } from './inv-opening-stock-posting.service';
import { enrichOpeningStocks } from './inv-opening-stock-enrich';
import {
  CreateInvOpeningStockDto,
  InvOpeningStockLineDto,
} from './dto/create-inv-opening-stock.dto';
import { QueryInvOpeningStocksDto } from './dto/query-inv-opening-stocks.dto';
import { UpdateInvOpeningStockDto } from './dto/update-inv-opening-stock.dto';
import {
  InvOpeningStockTransitionAction as A,
  TransitionInvOpeningStockDto,
} from './dto/transition-inv-opening-stock.dto';
import {
  toBigInt,
  EDITABLE,
  NEXT,
  DOC_CODE,
  buildInvOpeningStockWhere,
  mapOpeningStockLine,
  resolveInventoryAccount,
} from './inv-opening-stock.helpers';

@Injectable()
export class ErpInvOpeningStocksService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly posting: InvOpeningStockPostingService,
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

  private async genDocNumber(tx: Prisma.TransactionClient, docCode: string): Promise<string> {
    const numbering = await tx.erpDocumentNumbering.findFirst({
      where: { documentCode: docCode, deletedAt: null },
    });
    if (numbering) {
      const seq = numbering.nextNumber;
      await tx.erpDocumentNumbering.update({
        where: { id: numbering.id },
        data: { nextNumber: seq + 1 },
      });
      return `${numbering.prefix}${String(seq).padStart(numbering.digitCount, '0')}`;
    }
    const count = await tx.erpInvOpeningStock.count();
    return `${docCode}${String(count + 1).padStart(6, '0')}`;
  }

  /**
   * Resolve every line's NOT NULL columns that depend on header context or
   * master data (warehouse falls back to header; inventory account resolves via
   * line → item → inventory setting) and map to Prisma create inputs.
   */
  private async buildLines(
    lines: InvOpeningStockLineDto[],
    headerWarehouseId: bigint,
  ): Promise<Prisma.ErpInvOpeningStockLineCreateWithoutOpeningStockInput[]> {
    return Promise.all(
      lines.map(async (l) => {
        const inventoryAccountId = await resolveInventoryAccount(this.prisma, l);
        const warehouseId = toBigInt(l.warehouseId) ?? headerWarehouseId;
        return mapOpeningStockLine(l, { inventoryAccountId, warehouseId });
      }),
    );
  }

  private async findRaw(id: bigint) {
    const row = await this.prisma.erpInvOpeningStock.findFirst({
      where: { id, deletedAt: null },
      include: { lines: { orderBy: { lineNo: 'asc' } } },
    });
    if (!row) throw new NotFoundException('Transaksi saldo awal tidak ditemukan');
    return row;
  }

  private async one(id: bigint) {
    const row = await this.findRaw(id);
    const [enriched] = await enrichOpeningStocks(this.prisma, [row]);
    return { success: true, data: enriched };
  }

  // ── CRUD ────────────────────────────────────────────────────────────────────
  async create(dto: CreateInvOpeningStockDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;
    const headerWarehouseId = BigInt(dto.warehouseId);
    const lineData = await this.buildLines(dto.lines, headerWarehouseId);

    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.openingDate);
      const wantAuto = dto.auto !== false && !dto.docNumber;
      const docNumber = wantAuto ? await this.genDocNumber(tx, DOC_CODE) : dto.docNumber;
      if (!docNumber) throw new BadRequestException('No dokumen wajib diisi.');

      const row = await tx.erpInvOpeningStock.create({
        data: {
          docNumber,
          autoNumber: wantAuto ? docNumber : null,
          branchId: BigInt(dto.branchId),
          warehouseId: headerWarehouseId,
          locationId: toBigInt(dto.locationId),
          kind: dto.kind ?? null,
          openingDate: new Date(dto.openingDate),
          fiscalPeriodId,
          currencyId: BigInt(dto.currencyId),
          exchangeRate: new Prisma.Decimal(dto.exchangeRate),
          description: dto.description ?? null,
          notes: dto.notes ?? null,
          status: 'DRAFT',
          postingStatus: 'UNPOSTED',
          legacyCode: dto.legacyCode ?? null,
          createdById: actor,
          updatedById: actor,
          lines: lineData.length ? { create: lineData } : undefined,
        },
        select: { id: true },
      });
      return row;
    });
    return this.one(created.id);
  }

  async findAll(query: QueryInvOpeningStocksDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const where = buildInvOpeningStockWhere(query);

    const sortBy = query.sortBy ?? 'openingDate';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpInvOpeningStock.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }],
        skip: (page - 1) * limit,
        take: limit,
        include: { lines: { orderBy: { lineNo: 'asc' } } },
      }),
      this.prisma.erpInvOpeningStock.count({ where }),
    ]);

    return {
      success: true,
      data: await enrichOpeningStocks(this.prisma, items),
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  findOne(id: bigint) {
    return this.one(id);
  }

  async update(id: bigint, dto: UpdateInvOpeningStockDto, actorId?: string) {
    const existing = await this.findRaw(id);
    if (!EDITABLE.has(existing.status)) {
      throw new BadRequestException(
        `Dokumen berstatus ${existing.status} tidak bisa diedit. Reopen dulu bila perlu.`,
      );
    }
    const actor = actorId ? BigInt(actorId) : null;

    // Resolve replacement lines outside the tx (master-data lookups).
    const headerWarehouseId =
      dto.warehouseId !== undefined ? BigInt(dto.warehouseId) : existing.warehouseId;
    const lineData =
      dto.lines !== undefined ? await this.buildLines(dto.lines, headerWarehouseId) : null;

    await this.prisma.$transaction(async (tx) => {
      const data: Prisma.ErpInvOpeningStockUpdateInput = { updatedById: actor };
      if (dto.docNumber !== undefined) data.docNumber = dto.docNumber;
      if (dto.branchId !== undefined) data.branchId = BigInt(dto.branchId);
      if (dto.warehouseId !== undefined) data.warehouseId = BigInt(dto.warehouseId);
      if (dto.locationId !== undefined) data.locationId = toBigInt(dto.locationId);
      if (dto.kind !== undefined) data.kind = dto.kind;
      if (dto.currencyId !== undefined) data.currencyId = BigInt(dto.currencyId);
      if (dto.exchangeRate !== undefined) data.exchangeRate = new Prisma.Decimal(dto.exchangeRate);
      if (dto.description !== undefined) data.description = dto.description;
      if (dto.notes !== undefined) data.notes = dto.notes;
      if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;
      if (dto.openingDate !== undefined) {
        data.openingDate = new Date(dto.openingDate);
        data.fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.openingDate);
      } else if (dto.fiscalPeriodId !== undefined) {
        data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);
      }

      if (lineData !== null) {
        await tx.erpInvOpeningStockLine.deleteMany({ where: { openingStockId: id } });
        data.lines = { create: lineData };
      }
      await tx.erpInvOpeningStock.update({ where: { id }, data });
    });
    return this.one(id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status === 'POSTED') {
      throw new BadRequestException('Dokumen POSTED tidak bisa dihapus. Reopen dulu.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.erpInvOpeningStock.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actor },
    });
    return { success: true, message: 'Transaksi saldo awal dihapus' };
  }

  // ── workflow (§2.7 state machine) ────────────────────────────────────────────
  async transition(id: bigint, dto: TransitionInvOpeningStockDto, actorId?: string) {
    const opening = await this.findRaw(id);
    const actor = actorId ? BigInt(actorId) : null;
    const next = NEXT[opening.status]?.[dto.action];
    if (!next) {
      throw new BadRequestException(`Aksi ${dto.action} tidak valid dari status ${opening.status}.`);
    }
    if (dto.action === A.REJECT && !dto.reason?.trim()) {
      throw new BadRequestException('Alasan reject wajib diisi.');
    }

    if (dto.action === A.POST) {
      const period = await this.prisma.erpFiscalPeriod.findUnique({
        where: { id: opening.fiscalPeriodId },
        select: { status: true },
      });
      if (period?.status === 'CLOSED') {
        throw new BadRequestException('Periode fiskal sudah ditutup — tidak bisa posting.');
      }
      await this.prisma.$transaction(async (tx) => {
        await this.posting.reverseOpeningStock(tx, opening.id);
        await this.posting.postOpeningStock(tx, opening, actor);
        await tx.erpInvOpeningStock.update({
          where: { id },
          data: {
            status: 'POSTED',
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
        await this.posting.reverseOpeningStock(tx, opening.id);
        await tx.erpInvOpeningStock.update({
          where: { id },
          data: {
            status: 'DRAFT',
            postingStatus: 'UNPOSTED',
            postedAt: null,
            updatedById: actor,
          },
        });
      });
      return this.one(id);
    }

    await this.prisma.erpInvOpeningStock.update({
      where: { id },
      data: {
        status: next as never,
        updatedById: actor,
        ...(dto.action === A.REJECT
          ? { metadata: { ...((opening.metadata as object) ?? {}), rejectReason: dto.reason } }
          : {}),
      },
    });
    return this.one(id);
  }
}
