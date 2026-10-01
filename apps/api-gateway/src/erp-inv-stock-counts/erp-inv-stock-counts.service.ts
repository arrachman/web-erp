import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { InvStockCountPostingService } from './inv-stock-count-posting.service';
import { enrichCounts } from './inv-stock-count-enrich';
import {
  CreateInvStockCountDto,
  ErpStockCountTypeDto,
} from './dto/create-inv-stock-count.dto';
import { QueryInvStockCountsDto } from './dto/query-inv-stock-counts.dto';
import { UpdateInvStockCountDto } from './dto/update-inv-stock-count.dto';
import {
  InvStockCountTransitionAction as A,
  TransitionInvStockCountDto,
} from './dto/transition-inv-stock-count.dto';
import {
  EDITABLE,
  NEXT,
  DOC_CODE,
  buildInvCountWhere,
  mapCountLine,
} from './inv-stock-count.helpers';

@Injectable()
export class ErpInvStockCountsService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly posting: InvStockCountPostingService,
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
    const count = await tx.erpInvStockCount.count();
    return `${DOC_CODE}${String(count + 1).padStart(6, '0')}`;
  }

  private async findRaw(id: bigint) {
    const row = await this.prisma.erpInvStockCount.findFirst({
      where: { id, deletedAt: null },
      include: { lines: { orderBy: { lineNo: 'asc' } } },
    });
    if (!row) throw new NotFoundException('Stock count tidak ditemukan');
    return row;
  }

  private async one(id: bigint) {
    const row = await this.findRaw(id);
    const [enriched] = await enrichCounts(this.prisma, [row]);
    return { success: true, data: enriched };
  }

  // ── CRUD ────────────────────────────────────────────────────────────────────
  async create(dto: CreateInvStockCountDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;
    const warehouseId = BigInt(dto.warehouseId);
    const countType = dto.countType ?? ErpStockCountTypeDto.FULL;

    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.countDate);
      const wantAuto = dto.auto !== false && !dto.docNumber;
      const docNumber = wantAuto ? await this.genDocNumber(tx) : dto.docNumber;
      if (!docNumber) throw new BadRequestException('No dokumen wajib diisi.');

      const row = await tx.erpInvStockCount.create({
        data: {
          docNumber,
          autoNumber: wantAuto ? docNumber : null,
          branchId: BigInt(dto.branchId),
          warehouseId,
          countType: countType as never,
          countDate: new Date(dto.countDate),
          fiscalPeriodId,
          stepNo: dto.stepNo ?? null,
          description: dto.description ?? null,
          notes: dto.notes ?? null,
          status: 'DRAFT',
          postingStatus: 'UNPOSTED',
          legacyCode: dto.legacyCode ?? null,
          createdById: actor,
          updatedById: actor,
          lines: dto.lines.length
            ? { create: dto.lines.map((l) => mapCountLine(l, warehouseId)) }
            : undefined,
        },
        select: { id: true },
      });
      return row;
    });
    return this.one(created.id);
  }

  async findAll(query: QueryInvStockCountsDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const where = buildInvCountWhere(query);

    const sortBy = query.sortBy ?? 'countDate';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpInvStockCount.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }],
        skip: (page - 1) * limit,
        take: limit,
        include: { lines: { orderBy: { lineNo: 'asc' } } },
      }),
      this.prisma.erpInvStockCount.count({ where }),
    ]);

    return {
      success: true,
      data: await enrichCounts(this.prisma, items),
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  findOne(id: bigint) {
    return this.one(id);
  }

  async update(id: bigint, dto: UpdateInvStockCountDto, actorId?: string) {
    const existing = await this.findRaw(id);
    if (!EDITABLE.has(existing.status)) {
      throw new BadRequestException(
        `Dokumen berstatus ${existing.status} tidak bisa diedit. Reopen dulu bila perlu.`,
      );
    }
    const actor = actorId ? BigInt(actorId) : null;

    await this.prisma.$transaction(async (tx) => {
      const data: Prisma.ErpInvStockCountUpdateInput = { updatedById: actor };
      if (dto.docNumber !== undefined) data.docNumber = dto.docNumber;
      if (dto.branchId !== undefined) data.branchId = BigInt(dto.branchId);
      if (dto.warehouseId !== undefined) data.warehouseId = BigInt(dto.warehouseId);
      if (dto.countType !== undefined) data.countType = dto.countType as never;
      if (dto.stepNo !== undefined) data.stepNo = dto.stepNo;
      if (dto.description !== undefined) data.description = dto.description;
      if (dto.notes !== undefined) data.notes = dto.notes;
      if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;
      if (dto.countDate !== undefined) {
        data.countDate = new Date(dto.countDate);
        data.fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.countDate);
      } else if (dto.fiscalPeriodId !== undefined) {
        data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);
      }

      if (dto.lines !== undefined) {
        // Header warehouse used as per-line fallback for any line that omits it.
        const headerWarehouseId =
          dto.warehouseId !== undefined ? BigInt(dto.warehouseId) : existing.warehouseId;
        await tx.erpInvStockCountLine.deleteMany({ where: { stockCountId: id } });
        data.lines = { create: dto.lines.map((l) => mapCountLine(l, headerWarehouseId)) };
      }
      await tx.erpInvStockCount.update({ where: { id }, data });
    });
    return this.one(id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status === 'POSTED') {
      throw new BadRequestException('Dokumen POSTED tidak bisa dihapus. Reopen dulu.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.erpInvStockCount.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actor },
    });
    return { success: true, message: 'Stock count dihapus' };
  }

  // ── workflow (§2.7 state machine) ────────────────────────────────────────────
  async transition(id: bigint, dto: TransitionInvStockCountDto, actorId?: string) {
    const count = await this.findRaw(id);
    const actor = actorId ? BigInt(actorId) : null;
    const next = NEXT[count.status]?.[dto.action];
    if (!next) {
      throw new BadRequestException(`Aksi ${dto.action} tidak valid dari status ${count.status}.`);
    }
    if (dto.action === A.REJECT && !dto.reason?.trim()) {
      throw new BadRequestException('Alasan reject wajib diisi.');
    }

    if (dto.action === A.POST) {
      const period = await this.prisma.erpFiscalPeriod.findUnique({
        where: { id: count.fiscalPeriodId },
        select: { status: true },
      });
      if (period?.status === 'CLOSED') {
        throw new BadRequestException('Periode fiskal sudah ditutup — tidak bisa posting.');
      }
      await this.prisma.$transaction(async (tx) => {
        await this.posting.reverseCount(tx, count.id);
        await this.posting.postCount(tx, count, actor);
        // NOTE: this model has no postedAt column — POST only flips status + postingStatus.
        await tx.erpInvStockCount.update({
          where: { id },
          data: {
            status: 'POSTED',
            postingStatus: 'POSTED',
            updatedById: actor,
          },
        });
      });
      return this.one(id);
    }

    if (dto.action === A.REOPEN) {
      await this.prisma.$transaction(async (tx) => {
        await this.posting.reverseCount(tx, count.id);
        await tx.erpInvStockCount.update({
          where: { id },
          data: {
            status: 'DRAFT',
            postingStatus: 'UNPOSTED',
            updatedById: actor,
          },
        });
      });
      return this.one(id);
    }

    await this.prisma.erpInvStockCount.update({
      where: { id },
      data: {
        status: next as never,
        updatedById: actor,
        ...(dto.action === A.REJECT
          ? { metadata: { ...((count.metadata as object) ?? {}), rejectReason: dto.reason } }
          : {}),
      },
    });
    return this.one(id);
  }
}
