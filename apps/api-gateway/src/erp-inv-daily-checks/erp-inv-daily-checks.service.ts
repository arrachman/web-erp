import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { InvDailyCheckPostingService } from './inv-daily-check-posting.service';
import { enrichDailyChecks } from './inv-daily-check-enrich';
import { CreateInvDailyCheckDto } from './dto/create-inv-daily-check.dto';
import { QueryInvDailyChecksDto } from './dto/query-inv-daily-checks.dto';
import { UpdateInvDailyCheckDto } from './dto/update-inv-daily-check.dto';
import {
  InvDailyCheckTransitionAction as A,
  TransitionInvDailyCheckDto,
} from './dto/transition-inv-daily-check.dto';
import {
  toBigInt,
  EDITABLE,
  NEXT,
  DC_DOC_CODE,
  buildDailyCheckWhere,
  mapDailyCheckLine,
} from './inv-daily-check.helpers';

@Injectable()
export class ErpInvDailyChecksService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly posting: InvDailyCheckPostingService,
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
      where: { documentCode: DC_DOC_CODE, deletedAt: null },
    });
    if (numbering) {
      const seq = numbering.nextNumber;
      await tx.erpDocumentNumbering.update({
        where: { id: numbering.id },
        data: { nextNumber: seq + 1 },
      });
      return `${numbering.prefix}${String(seq).padStart(numbering.digitCount, '0')}`;
    }
    const count = await tx.erpInvDailyCheck.count();
    return `${DC_DOC_CODE}${String(count + 1).padStart(6, '0')}`;
  }

  private async findRaw(id: bigint) {
    const row = await this.prisma.erpInvDailyCheck.findFirst({
      where: { id, deletedAt: null },
      include: { lines: { orderBy: { lineNo: 'asc' } } },
    });
    if (!row) throw new NotFoundException('Daily check tidak ditemukan');
    return row;
  }

  private async one(id: bigint) {
    const row = await this.findRaw(id);
    const [enriched] = await enrichDailyChecks(this.prisma, [row]);
    return { success: true, data: enriched };
  }

  // ── CRUD ────────────────────────────────────────────────────────────────────
  async create(dto: CreateInvDailyCheckDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;

    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.checkDate);
      const wantAuto = dto.auto !== false && !dto.docNumber;
      const docNumber = wantAuto ? await this.genDocNumber(tx) : dto.docNumber;
      if (!docNumber) throw new BadRequestException('No dokumen wajib diisi.');

      const row = await tx.erpInvDailyCheck.create({
        data: {
          docNumber,
          autoNumber: wantAuto ? docNumber : null,
          branchId: BigInt(dto.branchId),
          locationId: toBigInt(dto.locationId),
          checkDate: new Date(dto.checkDate),
          fiscalPeriodId,
          machineRef: dto.machineRef ?? null,
          operatorRef: dto.operatorRef ?? null,
          description: dto.description ?? null,
          notes: dto.notes ?? null,
          status: 'DRAFT',
          postingStatus: 'UNPOSTED',
          legacyCode: dto.legacyCode ?? null,
          createdById: actor,
          updatedById: actor,
          lines: dto.lines.length
            ? { create: dto.lines.map((l) => mapDailyCheckLine(l)) }
            : undefined,
        },
        select: { id: true },
      });
      return row;
    });
    return this.one(created.id);
  }

  async findAll(query: QueryInvDailyChecksDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const where = buildDailyCheckWhere(query);

    const sortBy = query.sortBy ?? 'checkDate';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpInvDailyCheck.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }],
        skip: (page - 1) * limit,
        take: limit,
        include: { lines: { orderBy: { lineNo: 'asc' } } },
      }),
      this.prisma.erpInvDailyCheck.count({ where }),
    ]);

    return {
      success: true,
      data: await enrichDailyChecks(this.prisma, items),
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  findOne(id: bigint) {
    return this.one(id);
  }

  async update(id: bigint, dto: UpdateInvDailyCheckDto, actorId?: string) {
    const existing = await this.findRaw(id);
    if (!EDITABLE.has(existing.status)) {
      throw new BadRequestException(
        `Dokumen berstatus ${existing.status} tidak bisa diedit. Reopen dulu bila perlu.`,
      );
    }
    const actor = actorId ? BigInt(actorId) : null;

    await this.prisma.$transaction(async (tx) => {
      const data: Prisma.ErpInvDailyCheckUpdateInput = { updatedById: actor };
      if (dto.docNumber !== undefined) data.docNumber = dto.docNumber;
      if (dto.branchId !== undefined) data.branchId = BigInt(dto.branchId);
      if (dto.locationId !== undefined) data.locationId = toBigInt(dto.locationId);
      if (dto.machineRef !== undefined) data.machineRef = dto.machineRef;
      if (dto.operatorRef !== undefined) data.operatorRef = dto.operatorRef;
      if (dto.description !== undefined) data.description = dto.description;
      if (dto.notes !== undefined) data.notes = dto.notes;
      if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;
      if (dto.checkDate !== undefined) {
        data.checkDate = new Date(dto.checkDate);
        data.fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.checkDate);
      } else if (dto.fiscalPeriodId !== undefined) {
        data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);
      }

      if (dto.lines !== undefined) {
        await tx.erpInvDailyCheckLine.deleteMany({ where: { dailyCheckId: id } });
        data.lines = { create: dto.lines.map((l) => mapDailyCheckLine(l)) };
      }
      await tx.erpInvDailyCheck.update({ where: { id }, data });
    });
    return this.one(id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status === 'POSTED') {
      throw new BadRequestException('Dokumen POSTED tidak bisa dihapus. Reopen dulu.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.erpInvDailyCheck.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actor },
    });
    return { success: true, message: 'Daily check dihapus' };
  }

  // ── workflow (§2.7 state machine) ────────────────────────────────────────────
  async transition(id: bigint, dto: TransitionInvDailyCheckDto, actorId?: string) {
    const dailyCheck = await this.findRaw(id);
    const actor = actorId ? BigInt(actorId) : null;
    const next = NEXT[dailyCheck.status]?.[dto.action];
    if (!next) {
      throw new BadRequestException(
        `Aksi ${dto.action} tidak valid dari status ${dailyCheck.status}.`,
      );
    }
    if (dto.action === A.REJECT && !dto.reason?.trim()) {
      throw new BadRequestException('Alasan reject wajib diisi.');
    }

    if (dto.action === A.POST) {
      const period = await this.prisma.erpFiscalPeriod.findUnique({
        where: { id: dailyCheck.fiscalPeriodId },
        select: { status: true },
      });
      if (period?.status === 'CLOSED') {
        throw new BadRequestException('Periode fiskal sudah ditutup — tidak bisa posting.');
      }
      await this.prisma.$transaction(async (tx) => {
        await this.posting.reverseDailyCheck(tx, dailyCheck.id);
        await this.posting.postDailyCheck(tx, dailyCheck, actor);
        await tx.erpInvDailyCheck.update({
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
        await this.posting.reverseDailyCheck(tx, dailyCheck.id);
        await tx.erpInvDailyCheck.update({
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

    await this.prisma.erpInvDailyCheck.update({
      where: { id },
      data: {
        status: next as never,
        updatedById: actor,
        ...(dto.action === A.REJECT
          ? { metadata: { ...((dailyCheck.metadata as object) ?? {}), rejectReason: dto.reason } }
          : {}),
      },
    });
    return this.one(id);
  }
}
