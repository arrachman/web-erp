import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { enrichBoms } from './mfg-bom-enrich';
import { CreateMfgBomDto } from './dto/create-mfg-bom.dto';
import { QueryMfgBomsDto } from './dto/query-mfg-boms.dto';
import { UpdateMfgBomDto } from './dto/update-mfg-bom.dto';
import {
  MfgBomTransitionAction as A,
  TransitionMfgBomDto,
} from './dto/transition-mfg-bom.dto';
import {
  toBigInt,
  EDITABLE,
  NEXT,
  buildBomWhere,
  mapBomInputLine,
  mapBomOutputLine,
  computeBomTotals,
} from './mfg-bom.helpers';

const DOC_CODE = 'BOM';
const FALLBACK_PREFIX = 'BOM';

const BOM_INCLUDE = {
  inputs: { orderBy: { lineNo: 'asc' as const } },
  outputs: { orderBy: { lineNo: 'asc' as const } },
};

@Injectable()
export class ErpMfgBomsService {
  constructor(private readonly prisma: PrismaService) {}

  // ── Private helpers ──────────────────────────────────────────────────────────

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
    const count = await tx.erpMfgBom.count();
    return `${FALLBACK_PREFIX}${String(count + 1).padStart(6, '0')}`;
  }

  private async findRaw(id: bigint) {
    const bom = await this.prisma.erpMfgBom.findFirst({
      where: { id, deletedAt: null },
      include: BOM_INCLUDE,
    });
    if (!bom) throw new NotFoundException('BOM tidak ditemukan');
    return bom;
  }

  private async one(id: bigint) {
    const bom = await this.findRaw(id);
    const [enriched] = await enrichBoms(this.prisma, [bom]);
    return { success: true, data: enriched };
  }

  // ── CRUD ─────────────────────────────────────────────────────────────────────

  async create(dto: CreateMfgBomDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;
    const header = { currencyId: dto.currencyId, exchangeRate: dto.exchangeRate };
    const totals = computeBomTotals(dto.inputs ?? [], dto.outputs ?? []);

    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.docDate);
      const wantAuto = dto.auto !== false && !dto.docNumber;
      const docNumber = wantAuto ? await this.genDocNumber(tx) : dto.docNumber;
      if (!docNumber) throw new BadRequestException('No dokumen wajib diisi.');

      const row = await tx.erpMfgBom.create({
        data: {
          docNumber,
          autoNumber: wantAuto ? docNumber : null,
          branchId: BigInt(dto.branchId),
          locationId: toBigInt(dto.locationId),
          sourceWarehouseId: toBigInt(dto.sourceWarehouseId),
          productionWarehouseId: toBigInt(dto.productionWarehouseId),
          destinationWarehouseId: toBigInt(dto.destinationWarehouseId),
          docDate: new Date(dto.docDate),
          fiscalPeriodId,
          currencyId: BigInt(dto.currencyId),
          exchangeRate: new Prisma.Decimal(dto.exchangeRate),
          neededDate: dto.neededDate ? new Date(dto.neededDate) : null,
          workEstimate: dto.workEstimate != null ? new Prisma.Decimal(dto.workEstimate) : null,
          inputTotalPrice: totals.inputTotalPrice,
          inputTotalCost: totals.inputTotalCost,
          outputTotalPrice: totals.outputTotalPrice,
          outputTotalCost: totals.outputTotalCost,
          description: dto.description ?? null,
          notes: dto.notes ?? null,
          referenceNo: dto.referenceNo ?? null,
          referenceDate: dto.referenceDate ? new Date(dto.referenceDate) : null,
          requestedById: toBigInt(dto.requestedById),
          requestedPartnerId: toBigInt(dto.requestedPartnerId),
          legacyCode: dto.legacyCode ?? null,
          status: 'DRAFT',
          postingStatus: 'UNPOSTED',
          createdById: actor,
          updatedById: actor,
          inputs: dto.inputs?.length
            ? { create: dto.inputs.map((l) => mapBomInputLine(l, header)) }
            : undefined,
          outputs: dto.outputs?.length
            ? { create: dto.outputs.map((l) => mapBomOutputLine(l, header)) }
            : undefined,
        },
        select: { id: true },
      });
      return row;
    });
    return this.one(created.id);
  }

  async findAll(query: QueryMfgBomsDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const where = buildBomWhere(query);
    const sortBy = query.sortBy ?? 'docDate';
    const sortDir = query.sortDir ?? 'desc';

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpMfgBom.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }],
        skip: (page - 1) * limit,
        take: limit,
        include: BOM_INCLUDE,
      }),
      this.prisma.erpMfgBom.count({ where }),
    ]);

    return {
      success: true,
      data: await enrichBoms(this.prisma, items),
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  findOne(id: bigint) {
    return this.one(id);
  }

  async update(id: bigint, dto: UpdateMfgBomDto, actorId?: string) {
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
      const data: Prisma.ErpMfgBomUpdateInput = { updatedById: actor };

      if (dto.docNumber !== undefined) data.docNumber = dto.docNumber;
      if (dto.branchId !== undefined) data.branchId = BigInt(dto.branchId);
      if (dto.locationId !== undefined) data.locationId = toBigInt(dto.locationId);
      if (dto.sourceWarehouseId !== undefined) data.sourceWarehouseId = toBigInt(dto.sourceWarehouseId);
      if (dto.productionWarehouseId !== undefined) data.productionWarehouseId = toBigInt(dto.productionWarehouseId);
      if (dto.destinationWarehouseId !== undefined) data.destinationWarehouseId = toBigInt(dto.destinationWarehouseId);
      if (dto.currencyId !== undefined) data.currencyId = BigInt(dto.currencyId);
      if (dto.exchangeRate !== undefined) data.exchangeRate = new Prisma.Decimal(dto.exchangeRate);
      if (dto.neededDate !== undefined) data.neededDate = dto.neededDate ? new Date(dto.neededDate) : null;
      if (dto.workEstimate !== undefined) {
        data.workEstimate = dto.workEstimate != null ? new Prisma.Decimal(dto.workEstimate) : null;
      }
      if (dto.description !== undefined) data.description = dto.description;
      if (dto.notes !== undefined) data.notes = dto.notes;
      if (dto.referenceNo !== undefined) data.referenceNo = dto.referenceNo;
      if (dto.referenceDate !== undefined) {
        data.referenceDate = dto.referenceDate ? new Date(dto.referenceDate) : null;
      }
      if (dto.requestedById !== undefined) data.requestedById = toBigInt(dto.requestedById);
      if (dto.requestedPartnerId !== undefined) data.requestedPartnerId = toBigInt(dto.requestedPartnerId);
      if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;
      if (dto.docDate !== undefined) {
        data.docDate = new Date(dto.docDate);
        data.fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.docDate);
      } else if (dto.fiscalPeriodId !== undefined) {
        data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);
      }

      // Recompute totals if lines changed
      if (dto.inputs !== undefined || dto.outputs !== undefined) {
        const newInputs = dto.inputs ?? existing.inputs.map((l) => ({
          itemId: l.itemId.toString(), quantity: l.quantity.toString(),
          unitId: l.unitId.toString(), unitPrice: l.unitPrice.toString(),
          unitCost: l.unitCost.toString(), lineNo: l.lineNo,
        }));
        const newOutputs = dto.outputs ?? existing.outputs.map((l) => ({
          itemId: l.itemId.toString(), quantity: l.quantity.toString(),
          unitId: l.unitId.toString(), unitPrice: l.unitPrice.toString(),
          unitCost: l.unitCost.toString(), lineNo: l.lineNo,
        }));
        const totals = computeBomTotals(newInputs as never, newOutputs as never);
        Object.assign(data, totals);
      }

      if (dto.inputs !== undefined) {
        await tx.erpMfgBomInput.deleteMany({ where: { bomId: id } });
        data.inputs = { create: dto.inputs.map((l) => mapBomInputLine(l, header)) };
      }
      if (dto.outputs !== undefined) {
        await tx.erpMfgBomOutput.deleteMany({ where: { bomId: id } });
        data.outputs = { create: dto.outputs.map((l) => mapBomOutputLine(l, header)) };
      }

      await tx.erpMfgBom.update({ where: { id }, data });
    });
    return this.one(id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status === 'APPROVED') {
      throw new BadRequestException('Dokumen APPROVED tidak bisa dihapus. Reopen dulu.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.erpMfgBom.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actor },
    });
    return { success: true, message: 'BOM dihapus' };
  }

  // ── State machine (§2.7) ─────────────────────────────────────────────────────

  async transition(id: bigint, dto: TransitionMfgBomDto, actorId?: string) {
    const bom = await this.findRaw(id);
    const actor = actorId ? BigInt(actorId) : null;
    const next = NEXT[bom.status]?.[dto.action];
    if (!next) {
      throw new BadRequestException(`Aksi ${dto.action} tidak valid dari status ${bom.status}.`);
    }
    if (dto.action === A.REJECT && !dto.reason?.trim()) {
      throw new BadRequestException('Alasan reject wajib diisi.');
    }

    await this.prisma.erpMfgBom.update({
      where: { id },
      data: {
        status: next as never,
        previousStatus: bom.status as never,
        updatedById: actor,
        ...(dto.action === A.REJECT
          ? {
              metadata: {
                ...((bom.metadata as object) ?? {}),
                rejectReason: dto.reason,
              },
            }
          : {}),
        ...(dto.action === A.REOPEN
          ? { postingStatus: 'UNPOSTED' as never }
          : {}),
      },
    });
    return this.one(id);
  }
}
