import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { CreateMfgWorkOrderDto } from './dto/create-mfg-work-order.dto';
import { QueryMfgWorkOrdersDto } from './dto/query-mfg-work-orders.dto';
import { UpdateMfgWorkOrderDto } from './dto/update-mfg-work-order.dto';
import {
  MfgWoTransitionAction as A,
  TransitionMfgWorkOrderDto,
} from './dto/transition-mfg-work-order.dto';
import {
  toBigInt,
  EDITABLE,
  NEXT,
  buildWorkOrderWhere,
  mapWorkOrderInputLine,
  mapWorkOrderOutputLine,
  computeInputTotal,
  computeOutputTotal,
} from './mfg-work-order.helpers';

const DOC_CODE = 'WO';
const FALLBACK_PREFIX = 'WO';

const WO_INCLUDE = {
  inputs: { orderBy: { lineNo: 'asc' as const } },
  outputs: { orderBy: { lineNo: 'asc' as const } },
};

@Injectable()
export class ErpMfgWorkOrdersService {
  constructor(private readonly prisma: PrismaService) {}

  // ── private helpers ──────────────────────────────────────────────────────────

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
    const count = await tx.erpMfgWorkOrder.count();
    return `${FALLBACK_PREFIX}${String(count + 1).padStart(6, '0')}`;
  }

  private async findRaw(id: bigint) {
    const wo = await this.prisma.erpMfgWorkOrder.findFirst({
      where: { id, deletedAt: null },
      include: WO_INCLUDE,
    });
    if (!wo) throw new NotFoundException('Work order tidak ditemukan');
    return wo;
  }

  // ── CRUD ────────────────────────────────────────────────────────────────────

  async create(dto: CreateMfgWorkOrderDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;
    const header = { currencyId: dto.currencyId, exchangeRate: dto.exchangeRate };
    const inputTotal = computeInputTotal(dto.inputs ?? []);
    const outputTotal = computeOutputTotal(dto.outputs ?? []);

    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.docDate);
      const wantAuto = dto.auto !== false && !dto.docNumber;
      const docNumber = wantAuto ? await this.genDocNumber(tx) : dto.docNumber;
      if (!docNumber) throw new BadRequestException('No dokumen wajib diisi.');

      const row = await tx.erpMfgWorkOrder.create({
        data: {
          docNumber,
          autoNumber: wantAuto ? docNumber : null,
          branchId: BigInt(dto.branchId),
          locationId: toBigInt(dto.locationId),
          sourceWarehouseId: toBigInt(dto.sourceWarehouseId),
          productionWarehouseId: toBigInt(dto.productionWarehouseId),
          destinationWarehouseId: toBigInt(dto.destinationWarehouseId),
          bomId: toBigInt(dto.bomId),
          docDate: new Date(dto.docDate),
          fiscalPeriodId,
          requestedById: toBigInt(dto.requestedById),
          requestedPartnerId: toBigInt(dto.requestedPartnerId),
          neededDate: dto.neededDate ? new Date(dto.neededDate) : null,
          workEstimate: dto.workEstimate != null ? new Prisma.Decimal(dto.workEstimate) : null,
          currencyId: BigInt(dto.currencyId),
          exchangeRate: new Prisma.Decimal(dto.exchangeRate),
          inputTotalPrice: inputTotal,
          outputTotalPrice: outputTotal,
          description: dto.description ?? null,
          notes: dto.notes ?? null,
          referenceNo: dto.referenceNo ?? null,
          referenceDate: dto.referenceDate ? new Date(dto.referenceDate) : null,
          legacyCode: dto.legacyCode ?? null,
          status: 'DRAFT',
          postingStatus: 'UNPOSTED',
          createdById: actor,
          updatedById: actor,
          inputs: (dto.inputs ?? []).length
            ? { create: dto.inputs.map((l) => mapWorkOrderInputLine(l, header)) }
            : undefined,
          outputs: (dto.outputs ?? []).length
            ? { create: dto.outputs.map((l) => mapWorkOrderOutputLine(l, header)) }
            : undefined,
        },
        select: { id: true },
      });
      return row;
    });

    return this.findOne(created.id);
  }

  async findAll(query: QueryMfgWorkOrdersDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const where = buildWorkOrderWhere(query);
    const sortBy = query.sortBy ?? 'docDate';
    const sortDir = query.sortDir ?? 'desc';

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpMfgWorkOrder.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }],
        skip: (page - 1) * limit,
        take: limit,
        include: WO_INCLUDE,
      }),
      this.prisma.erpMfgWorkOrder.count({ where }),
    ]);

    return {
      success: true,
      data: items,
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  async findOne(id: bigint) {
    const wo = await this.findRaw(id);
    return { success: true, data: wo };
  }

  async update(id: bigint, dto: UpdateMfgWorkOrderDto, actorId?: string) {
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
      const data: Prisma.ErpMfgWorkOrderUpdateInput = { updatedById: actor };

      if (dto.docNumber !== undefined) data.docNumber = dto.docNumber;
      if (dto.branchId !== undefined) data.branchId = BigInt(dto.branchId);
      if (dto.locationId !== undefined) data.locationId = toBigInt(dto.locationId);
      if (dto.sourceWarehouseId !== undefined) data.sourceWarehouseId = toBigInt(dto.sourceWarehouseId);
      if (dto.productionWarehouseId !== undefined) data.productionWarehouseId = toBigInt(dto.productionWarehouseId);
      if (dto.destinationWarehouseId !== undefined) data.destinationWarehouseId = toBigInt(dto.destinationWarehouseId);
      if (dto.bomId !== undefined) data.bom = dto.bomId ? { connect: { id: BigInt(dto.bomId) } } : { disconnect: true };
      if (dto.requestedById !== undefined) data.requestedById = toBigInt(dto.requestedById);
      if (dto.requestedPartnerId !== undefined) data.requestedPartnerId = toBigInt(dto.requestedPartnerId);
      if (dto.neededDate !== undefined) data.neededDate = dto.neededDate ? new Date(dto.neededDate) : null;
      if (dto.workEstimate !== undefined) data.workEstimate = dto.workEstimate != null ? new Prisma.Decimal(dto.workEstimate) : null;
      if (dto.currencyId !== undefined) data.currencyId = BigInt(dto.currencyId);
      if (dto.exchangeRate !== undefined) data.exchangeRate = new Prisma.Decimal(dto.exchangeRate);
      if (dto.description !== undefined) data.description = dto.description;
      if (dto.notes !== undefined) data.notes = dto.notes;
      if (dto.referenceNo !== undefined) data.referenceNo = dto.referenceNo;
      if (dto.referenceDate !== undefined) data.referenceDate = dto.referenceDate ? new Date(dto.referenceDate) : null;
      if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;
      if (dto.docDate !== undefined) {
        data.docDate = new Date(dto.docDate);
        data.fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.docDate);
      } else if (dto.fiscalPeriodId !== undefined) {
        data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);
      }

      if (dto.inputs !== undefined) {
        await tx.erpMfgWorkOrderInput.deleteMany({ where: { workOrderId: id } });
        data.inputs = { create: dto.inputs.map((l) => mapWorkOrderInputLine(l, header)) };
        data.inputTotalPrice = computeInputTotal(dto.inputs);
      }
      if (dto.outputs !== undefined) {
        await tx.erpMfgWorkOrderOutput.deleteMany({ where: { workOrderId: id } });
        data.outputs = { create: dto.outputs.map((l) => mapWorkOrderOutputLine(l, header)) };
        data.outputTotalPrice = computeOutputTotal(dto.outputs);
      }

      await tx.erpMfgWorkOrder.update({ where: { id }, data });
    });

    return this.findOne(id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status === 'APPROVED') {
      throw new BadRequestException('Dokumen APPROVED tidak bisa dihapus. Reopen dulu.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.erpMfgWorkOrder.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actor },
    });
    return { success: true, message: 'Work order dihapus' };
  }

  // ── state machine ────────────────────────────────────────────────────────────

  async transition(id: bigint, dto: TransitionMfgWorkOrderDto, actorId?: string) {
    const wo = await this.findRaw(id);
    const actor = actorId ? BigInt(actorId) : null;
    const next = NEXT[wo.status]?.[dto.action];
    if (!next) {
      throw new BadRequestException(`Aksi ${dto.action} tidak valid dari status ${wo.status}.`);
    }
    if (dto.action === A.REJECT && !dto.reason?.trim()) {
      throw new BadRequestException('Alasan reject wajib diisi.');
    }

    await this.prisma.erpMfgWorkOrder.update({
      where: { id },
      data: {
        status: next as never,
        previousStatus: wo.status as never,
        updatedById: actor,
        ...(dto.action === A.REOPEN
          ? { revisionCount: { increment: 1 } }
          : {}),
        ...(dto.action === A.REJECT
          ? {
              metadata: {
                ...((wo.metadata as object) ?? {}),
                rejectReason: dto.reason,
              },
            }
          : {}),
      },
    });

    return this.findOne(id);
  }
}
