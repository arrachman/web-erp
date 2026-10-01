import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { InvStockAdjustmentPostingService } from './inv-stock-adjustment-posting.service';
import { enrichAdjustments } from './inv-stock-adjustment-enrich';
import { CreateInvStockAdjustmentDto } from './dto/create-inv-stock-adjustment.dto';
import { QueryInvStockAdjustmentsDto } from './dto/query-inv-stock-adjustments.dto';
import { UpdateInvStockAdjustmentDto } from './dto/update-inv-stock-adjustment.dto';
import {
  InvStockAdjustmentTransitionAction as A,
  TransitionInvStockAdjustmentDto,
} from './dto/transition-inv-stock-adjustment.dto';
import {
  toBigInt,
  EDITABLE,
  NEXT,
  DOC_CODE,
  buildInvAdjustmentWhere,
  mapAdjustmentLine,
  resolveLineAccounts,
} from './inv-stock-adjustment.helpers';

@Injectable()
export class ErpInvStockAdjustmentsService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly posting: InvStockAdjustmentPostingService,
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
    const count = await tx.erpInvStockAdjustment.count();
    return `${docCode}${String(count + 1).padStart(6, '0')}`;
  }

  private async findRaw(id: bigint) {
    const row = await this.prisma.erpInvStockAdjustment.findFirst({
      where: { id, deletedAt: null },
      include: { lines: { orderBy: { lineNo: 'asc' } } },
    });
    if (!row) throw new NotFoundException('Penyesuaian stok tidak ditemukan');
    return row;
  }

  private async one(id: bigint) {
    const row = await this.findRaw(id);
    const [enriched] = await enrichAdjustments(this.prisma, [row]);
    return { success: true, data: enriched };
  }

  // ── CRUD ────────────────────────────────────────────────────────────────────
  async create(dto: CreateInvStockAdjustmentDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;
    const resolveAccounts = await resolveLineAccounts(this.prisma, dto.lines);
    const headerWarehouseId = BigInt(dto.warehouseId);

    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.adjustmentDate);
      const wantAuto = dto.auto !== false && !dto.docNumber;
      const docNumber = wantAuto ? await this.genDocNumber(tx, DOC_CODE) : dto.docNumber;
      if (!docNumber) throw new BadRequestException('No dokumen wajib diisi.');

      const row = await tx.erpInvStockAdjustment.create({
        data: {
          docNumber,
          autoNumber: wantAuto ? docNumber : null,
          branchId: BigInt(dto.branchId),
          warehouseId: headerWarehouseId,
          adjustmentDate: new Date(dto.adjustmentDate),
          fiscalPeriodId,
          kind: dto.kind ?? null,
          stockCountId: toBigInt(dto.stockCountId),
          description: dto.description ?? null,
          notes: dto.notes ?? null,
          status: 'DRAFT',
          postingStatus: 'UNPOSTED',
          legacyCode: dto.legacyCode ?? null,
          createdById: actor,
          updatedById: actor,
          lines: dto.lines.length
            ? {
                create: dto.lines.map((l) =>
                  mapAdjustmentLine(l, resolveAccounts(l), headerWarehouseId),
                ),
              }
            : undefined,
        },
        select: { id: true },
      });
      return row;
    });
    return this.one(created.id);
  }

  async findAll(query: QueryInvStockAdjustmentsDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const where = buildInvAdjustmentWhere(query);

    const sortBy = query.sortBy ?? 'adjustmentDate';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpInvStockAdjustment.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }],
        skip: (page - 1) * limit,
        take: limit,
        include: { lines: { orderBy: { lineNo: 'asc' } } },
      }),
      this.prisma.erpInvStockAdjustment.count({ where }),
    ]);

    return {
      success: true,
      data: await enrichAdjustments(this.prisma, items),
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  findOne(id: bigint) {
    return this.one(id);
  }

  async update(id: bigint, dto: UpdateInvStockAdjustmentDto, actorId?: string) {
    const existing = await this.findRaw(id);
    if (!EDITABLE.has(existing.status)) {
      throw new BadRequestException(
        `Dokumen berstatus ${existing.status} tidak bisa diedit. Reopen dulu bila perlu.`,
      );
    }
    const actor = actorId ? BigInt(actorId) : null;
    const headerWarehouseId =
      dto.warehouseId !== undefined ? BigInt(dto.warehouseId) : existing.warehouseId;
    const resolveAccounts =
      dto.lines !== undefined ? await resolveLineAccounts(this.prisma, dto.lines) : null;

    await this.prisma.$transaction(async (tx) => {
      const data: Prisma.ErpInvStockAdjustmentUpdateInput = { updatedById: actor };
      if (dto.docNumber !== undefined) data.docNumber = dto.docNumber;
      if (dto.branchId !== undefined) data.branchId = BigInt(dto.branchId);
      if (dto.warehouseId !== undefined) data.warehouseId = headerWarehouseId;
      if (dto.kind !== undefined) data.kind = dto.kind;
      if (dto.stockCountId !== undefined) {
        const scId = toBigInt(dto.stockCountId);
        data.stockCount = scId != null ? { connect: { id: scId } } : { disconnect: true };
      }
      if (dto.description !== undefined) data.description = dto.description;
      if (dto.notes !== undefined) data.notes = dto.notes;
      if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;
      if (dto.adjustmentDate !== undefined) {
        data.adjustmentDate = new Date(dto.adjustmentDate);
        data.fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.adjustmentDate);
      } else if (dto.fiscalPeriodId !== undefined) {
        data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);
      }

      if (dto.lines !== undefined && resolveAccounts) {
        await tx.erpInvStockAdjustmentLine.deleteMany({ where: { stockAdjustmentId: id } });
        data.lines = {
          create: dto.lines.map((l) =>
            mapAdjustmentLine(l, resolveAccounts(l), headerWarehouseId),
          ),
        };
      }
      await tx.erpInvStockAdjustment.update({ where: { id }, data });
    });
    return this.one(id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status === 'POSTED') {
      throw new BadRequestException('Dokumen POSTED tidak bisa dihapus. Reopen dulu.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.erpInvStockAdjustment.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actor },
    });
    return { success: true, message: 'Penyesuaian stok dihapus' };
  }

  // ── workflow (§2.7 state machine) ────────────────────────────────────────────
  async transition(id: bigint, dto: TransitionInvStockAdjustmentDto, actorId?: string) {
    const adjustment = await this.findRaw(id);
    const actor = actorId ? BigInt(actorId) : null;
    const next = NEXT[adjustment.status]?.[dto.action];
    if (!next) {
      throw new BadRequestException(`Aksi ${dto.action} tidak valid dari status ${adjustment.status}.`);
    }
    if (dto.action === A.REJECT && !dto.reason?.trim()) {
      throw new BadRequestException('Alasan reject wajib diisi.');
    }

    if (dto.action === A.POST) {
      const period = await this.prisma.erpFiscalPeriod.findUnique({
        where: { id: adjustment.fiscalPeriodId },
        select: { status: true },
      });
      if (period?.status === 'CLOSED') {
        throw new BadRequestException('Periode fiskal sudah ditutup — tidak bisa posting.');
      }
      await this.prisma.$transaction(async (tx) => {
        await this.posting.reverseAdjustment(tx, adjustment.id);
        await this.posting.postAdjustment(tx, adjustment, actor);
        await tx.erpInvStockAdjustment.update({
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
        await this.posting.reverseAdjustment(tx, adjustment.id);
        await tx.erpInvStockAdjustment.update({
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

    await this.prisma.erpInvStockAdjustment.update({
      where: { id },
      data: {
        status: next as never,
        updatedById: actor,
        ...(dto.action === A.REJECT
          ? { metadata: { ...((adjustment.metadata as object) ?? {}), rejectReason: dto.reason } }
          : {}),
      },
    });
    return this.one(id);
  }
}
