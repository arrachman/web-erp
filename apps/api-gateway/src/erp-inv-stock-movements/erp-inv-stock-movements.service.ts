import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { InvStockMovementPostingService } from './inv-stock-movement-posting.service';
import { enrichMovements } from './inv-stock-movement-enrich';
import { CreateInvStockMovementDto } from './dto/create-inv-stock-movement.dto';
import { QueryInvStockMovementsDto } from './dto/query-inv-stock-movements.dto';
import { UpdateInvStockMovementDto } from './dto/update-inv-stock-movement.dto';
import {
  InvStockMovementTransitionAction as A,
  TransitionInvStockMovementDto,
} from './dto/transition-inv-stock-movement.dto';
import {
  toBigInt,
  EDITABLE,
  NEXT,
  DOC_CODE_BY_TYPE,
  buildInvMovementWhere,
  mapMovementLine,
} from './inv-stock-movement.helpers';

@Injectable()
export class ErpInvStockMovementsService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly posting: InvStockMovementPostingService,
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
    const count = await tx.erpInvStockMovement.count();
    return `${docCode}${String(count + 1).padStart(6, '0')}`;
  }

  private async findRaw(id: bigint) {
    const row = await this.prisma.erpInvStockMovement.findFirst({
      where: { id, deletedAt: null },
      include: { lines: { orderBy: { lineNo: 'asc' } } },
    });
    if (!row) throw new NotFoundException('Transaksi stok tidak ditemukan');
    return row;
  }

  private async one(id: bigint) {
    const row = await this.findRaw(id);
    const [enriched] = await enrichMovements(this.prisma, [row]);
    return { success: true, data: enriched };
  }

  // ── CRUD ────────────────────────────────────────────────────────────────────
  async create(dto: CreateInvStockMovementDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;

    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.movementDate);
      const wantAuto = dto.auto !== false && !dto.docNumber;
      const docNumber = wantAuto
        ? await this.genDocNumber(tx, DOC_CODE_BY_TYPE[dto.movementType])
        : dto.docNumber;
      if (!docNumber) throw new BadRequestException('No dokumen wajib diisi.');

      const row = await tx.erpInvStockMovement.create({
        data: {
          docNumber,
          autoNumber: wantAuto ? docNumber : null,
          movementType: dto.movementType as never,
          branchId: BigInt(dto.branchId),
          locationId: toBigInt(dto.locationId),
          sourceWarehouseId: toBigInt(dto.sourceWarehouseId),
          transitWarehouseId: toBigInt(dto.transitWarehouseId),
          destinationWarehouseId: toBigInt(dto.destinationWarehouseId),
          movementDate: new Date(dto.movementDate),
          fiscalPeriodId,
          requestedPartnerId: toBigInt(dto.requestedPartnerId),
          requestedTo: dto.requestedTo ?? null,
          neededDate: dto.neededDate ? new Date(dto.neededDate) : null,
          referenceNo: dto.referenceNo ?? null,
          referenceDate: dto.referenceDate ? new Date(dto.referenceDate) : null,
          description: dto.description ?? null,
          notes: dto.notes ?? null,
          status: 'DRAFT',
          postingStatus: 'UNPOSTED',
          legacyCode: dto.legacyCode ?? null,
          createdById: actor,
          updatedById: actor,
          lines: dto.lines.length
            ? { create: dto.lines.map((l) => mapMovementLine(l)) }
            : undefined,
        },
        select: { id: true },
      });
      return row;
    });
    return this.one(created.id);
  }

  async findAll(query: QueryInvStockMovementsDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const where = buildInvMovementWhere(query);

    const sortBy = query.sortBy ?? 'movementDate';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpInvStockMovement.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }],
        skip: (page - 1) * limit,
        take: limit,
        include: { lines: { orderBy: { lineNo: 'asc' } } },
      }),
      this.prisma.erpInvStockMovement.count({ where }),
    ]);

    return {
      success: true,
      data: await enrichMovements(this.prisma, items),
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  findOne(id: bigint) {
    return this.one(id);
  }

  async update(id: bigint, dto: UpdateInvStockMovementDto, actorId?: string) {
    const existing = await this.findRaw(id);
    if (!EDITABLE.has(existing.status)) {
      throw new BadRequestException(
        `Dokumen berstatus ${existing.status} tidak bisa diedit. Reopen dulu bila perlu.`,
      );
    }
    const actor = actorId ? BigInt(actorId) : null;

    await this.prisma.$transaction(async (tx) => {
      const data: Prisma.ErpInvStockMovementUpdateInput = { updatedById: actor };
      if (dto.docNumber !== undefined) data.docNumber = dto.docNumber;
      if (dto.movementType !== undefined) data.movementType = dto.movementType as never;
      if (dto.branchId !== undefined) data.branchId = BigInt(dto.branchId);
      if (dto.locationId !== undefined) data.locationId = toBigInt(dto.locationId);
      if (dto.sourceWarehouseId !== undefined) data.sourceWarehouseId = toBigInt(dto.sourceWarehouseId);
      if (dto.transitWarehouseId !== undefined) data.transitWarehouseId = toBigInt(dto.transitWarehouseId);
      if (dto.destinationWarehouseId !== undefined) {
        data.destinationWarehouseId = toBigInt(dto.destinationWarehouseId);
      }
      if (dto.requestedPartnerId !== undefined) data.requestedPartnerId = toBigInt(dto.requestedPartnerId);
      if (dto.requestedTo !== undefined) data.requestedTo = dto.requestedTo;
      if (dto.neededDate !== undefined) data.neededDate = dto.neededDate ? new Date(dto.neededDate) : null;
      if (dto.referenceNo !== undefined) data.referenceNo = dto.referenceNo;
      if (dto.referenceDate !== undefined) {
        data.referenceDate = dto.referenceDate ? new Date(dto.referenceDate) : null;
      }
      if (dto.description !== undefined) data.description = dto.description;
      if (dto.notes !== undefined) data.notes = dto.notes;
      if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;
      if (dto.movementDate !== undefined) {
        data.movementDate = new Date(dto.movementDate);
        data.fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.movementDate);
      } else if (dto.fiscalPeriodId !== undefined) {
        data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);
      }

      if (dto.lines !== undefined) {
        await tx.erpInvStockMovementLine.deleteMany({ where: { stockMovementId: id } });
        data.lines = { create: dto.lines.map((l) => mapMovementLine(l)) };
      }
      await tx.erpInvStockMovement.update({ where: { id }, data });
    });
    return this.one(id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status === 'POSTED') {
      throw new BadRequestException('Dokumen POSTED tidak bisa dihapus. Reopen dulu.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.erpInvStockMovement.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actor },
    });
    return { success: true, message: 'Transaksi stok dihapus' };
  }

  // ── workflow (§2.7 state machine) ────────────────────────────────────────────
  async transition(id: bigint, dto: TransitionInvStockMovementDto, actorId?: string) {
    const movement = await this.findRaw(id);
    const actor = actorId ? BigInt(actorId) : null;
    const next = NEXT[movement.status]?.[dto.action];
    if (!next) {
      throw new BadRequestException(`Aksi ${dto.action} tidak valid dari status ${movement.status}.`);
    }
    if (dto.action === A.REJECT && !dto.reason?.trim()) {
      throw new BadRequestException('Alasan reject wajib diisi.');
    }

    if (dto.action === A.POST) {
      const period = await this.prisma.erpFiscalPeriod.findUnique({
        where: { id: movement.fiscalPeriodId },
        select: { status: true },
      });
      if (period?.status === 'CLOSED') {
        throw new BadRequestException('Periode fiskal sudah ditutup — tidak bisa posting.');
      }
      await this.prisma.$transaction(async (tx) => {
        await this.posting.reverseMovement(tx, movement.id);
        await this.posting.postMovement(tx, movement, actor);
        await tx.erpInvStockMovement.update({
          where: { id },
          data: {
            status: 'POSTED',
            previousStatus: movement.status as never,
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
        await this.posting.reverseMovement(tx, movement.id);
        await tx.erpInvStockMovement.update({
          where: { id },
          data: {
            status: 'DRAFT',
            previousStatus: movement.status as never,
            postingStatus: 'UNPOSTED',
            postedAt: null,
            updatedById: actor,
          },
        });
      });
      return this.one(id);
    }

    await this.prisma.erpInvStockMovement.update({
      where: { id },
      data: {
        status: next as never,
        previousStatus: movement.status as never,
        updatedById: actor,
        ...(dto.action === A.REJECT
          ? { metadata: { ...((movement.metadata as object) ?? {}), rejectReason: dto.reason } }
          : {}),
      },
    });
    return this.one(id);
  }
}
