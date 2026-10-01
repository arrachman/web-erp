import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { GiroPostingService } from './giro-posting.service';
import { CreateGiroEntryDto } from './dto/create-giro-entry.dto';
import { QueryGiroEntryDto } from './dto/query-giro-entry.dto';
import { UpdateGiroEntryDto } from './dto/update-giro-entry.dto';
import {
  GiroTransitionAction as A,
  TransitionGiroEntryDto,
} from './dto/transition-giro-entry.dto';
import {
  toBigInt,
  EDITABLE,
  NEXT,
  DOC_CODE,
  FALLBACK_PREFIX,
  codeKeyFor,
  buildGiroEntryWhere,
} from './giro-entry.helpers';
import {
  buildRegisterGiros,
  linkClearGiros,
  unlinkClearGiros,
} from './giro-instrument.helpers';

const GIRO_INCLUDE = {
  registeredGiros: { orderBy: { lineNo: 'asc' as const } },
  clearedGiros: { orderBy: { lineNo: 'asc' as const } },
};

@Injectable()
export class ErpFinGiroEntriesService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly posting: GiroPostingService,
  ) {}

  /** Resolve the fiscal period: explicit id, else the period containing the entry date. */
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

  /** Auto doc number per (kind,type) via sys_document_numberings (fallback: count + prefix). */
  private async genDocNumber(
    tx: Prisma.TransactionClient,
    kind: string,
    type: string,
  ): Promise<string> {
    const key = codeKeyFor(kind, type);
    const numbering = await tx.erpDocumentNumbering.findFirst({
      where: { documentCode: DOC_CODE[key], deletedAt: null },
    });
    if (numbering) {
      const seq = numbering.nextNumber;
      await tx.erpDocumentNumbering.update({
        where: { id: numbering.id },
        data: { nextNumber: seq + 1 },
      });
      return `${numbering.prefix}${String(seq).padStart(numbering.digitCount, '0')}`;
    }
    const count = await tx.erpFinGiroEntry.count({
      where: { kind: kind as never, type: type as never },
    });
    return `${FALLBACK_PREFIX[key]}${String(count + 1).padStart(6, '0')}`;
  }

  private async findRaw(id: bigint) {
    const entry = await this.prisma.erpFinGiroEntry.findFirst({
      where: { id, deletedAt: null },
      include: GIRO_INCLUDE,
    });
    if (!entry) throw new NotFoundException('Giro entry tidak ditemukan');
    return entry;
  }

  // ── CRUD ──────────────────────────────────────────────────────────────────────
  async create(dto: CreateGiroEntryDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;
    if (dto.kind === 'CLEAR' && !dto.bankAccountId) {
      throw new BadRequestException('Bank settlement (bankAccountId) wajib untuk entri kliring.');
    }
    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.entryDate);
      const wantAuto = dto.auto !== false && !dto.docNumber;
      const docNumber = wantAuto
        ? await this.genDocNumber(tx, dto.kind, dto.type)
        : dto.docNumber;
      if (!docNumber) throw new BadRequestException('No transaksi wajib diisi.');

      const entry = await tx.erpFinGiroEntry.create({
        data: {
          docNumber,
          autoNumber: wantAuto ? docNumber : null,
          kind: dto.kind as never,
          type: dto.type as never,
          branchId: BigInt(dto.branchId),
          partnerId: toBigInt(dto.partnerId),
          entryDate: new Date(dto.entryDate),
          fiscalPeriodId,
          bankAccountId: toBigInt(dto.bankAccountId),
          currencyId: BigInt(dto.currencyId),
          exchangeRate: new Prisma.Decimal(dto.exchangeRate),
          description: dto.description ?? null,
          notes: dto.notes ?? null,
          status: 'DRAFT',
          postingStatus: 'UNPOSTED',
          legacyCode: dto.legacyCode ?? null,
          createdById: actor,
          updatedById: actor,
        },
        select: { id: true },
      });

      if (dto.kind === 'REGISTER') {
        const giros = buildRegisterGiros({
          rows: dto.rows,
          type: dto.type,
          partnerId: dto.partnerId,
          branchId: dto.branchId,
          currencyId: dto.currencyId,
          exchangeRate: dto.exchangeRate,
          giroAccountId: dto.giroAccountId,
          fiscalPeriodId,
        });
        for (const g of giros) {
          await tx.erpFinGiro.create({ data: { ...g, giroEntryId: entry.id } });
        }
      } else {
        await linkClearGiros(tx, dto.rows, { id: entry.id, type: dto.type });
      }
      return entry;
    });
    return this.findOne(created.id);
  }

  async findAll(query: QueryGiroEntryDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const where = buildGiroEntryWhere(query);
    const sortBy = query.sortBy ?? 'entryDate';
    const sortDir = query.sortDir ?? 'desc';

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpFinGiroEntry.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }],
        skip: (page - 1) * limit,
        take: limit,
        include: GIRO_INCLUDE,
      }),
      this.prisma.erpFinGiroEntry.count({ where }),
    ]);

    return {
      success: true,
      data: items,
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  async findOne(id: bigint) {
    return { success: true, data: await this.findRaw(id) };
  }

  async update(id: bigint, dto: UpdateGiroEntryDto, actorId?: string) {
    const existing = await this.findRaw(id);
    if (!EDITABLE.has(existing.status)) {
      throw new BadRequestException(
        `Dokumen berstatus ${existing.status} tidak bisa diedit. Reopen dulu bila perlu.`,
      );
    }
    if (existing.kind === 'REGISTER' && existing.registeredGiros.some((g) => g.status === 'CLEARED')) {
      throw new BadRequestException('Ada giro yang sudah CLEARED — register tidak bisa diedit.');
    }
    const actor = actorId ? BigInt(actorId) : null;

    const updated = await this.prisma.$transaction(async (tx) => {
      const data: Prisma.ErpFinGiroEntryUpdateInput = { updatedById: actor };
      if (dto.docNumber !== undefined) data.docNumber = dto.docNumber;
      if (dto.branchId !== undefined) data.branchId = BigInt(dto.branchId);
      if (dto.partnerId !== undefined) data.partnerId = toBigInt(dto.partnerId);
      if (dto.bankAccountId !== undefined) data.bankAccountId = toBigInt(dto.bankAccountId);
      if (dto.description !== undefined) data.description = dto.description ?? null;
      if (dto.notes !== undefined) data.notes = dto.notes ?? null;
      if (dto.currencyId !== undefined) data.currencyId = BigInt(dto.currencyId);
      if (dto.exchangeRate !== undefined) data.exchangeRate = new Prisma.Decimal(dto.exchangeRate);
      if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode ?? null;

      let fiscalPeriodId = existing.fiscalPeriodId;
      if (dto.entryDate !== undefined) {
        data.entryDate = new Date(dto.entryDate);
        fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.entryDate);
        data.fiscalPeriodId = fiscalPeriodId;
      } else if (dto.fiscalPeriodId !== undefined) {
        fiscalPeriodId = BigInt(dto.fiscalPeriodId);
        data.fiscalPeriodId = fiscalPeriodId;
      }

      await tx.erpFinGiroEntry.update({ where: { id }, data });

      if (dto.rows !== undefined) {
        if (existing.kind === 'REGISTER') {
          await tx.erpFinGiro.deleteMany({ where: { giroEntryId: id } });
          const giros = buildRegisterGiros({
            rows: dto.rows,
            type: existing.type,
            partnerId: dto.partnerId ?? existing.partnerId?.toString(),
            branchId: (dto.branchId ?? existing.branchId.toString()),
            currencyId: dto.currencyId ?? existing.currencyId.toString(),
            exchangeRate: dto.exchangeRate ?? existing.exchangeRate.toString(),
            giroAccountId: dto.giroAccountId,
            fiscalPeriodId,
          });
          for (const g of giros) {
            await tx.erpFinGiro.create({ data: { ...g, giroEntryId: id } });
          }
        } else {
          await unlinkClearGiros(tx, id);
          await linkClearGiros(tx, dto.rows, { id, type: existing.type });
        }
      }
      return { id };
    });
    return this.findOne(updated.id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status === 'POSTED') {
      throw new BadRequestException('Dokumen POSTED tidak bisa dihapus. Reopen dulu.');
    }
    if (existing.kind === 'REGISTER' && existing.registeredGiros.some((g) => g.status === 'CLEARED')) {
      throw new BadRequestException('Ada giro yang sudah CLEARED — register tidak bisa dihapus.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.$transaction(async (tx) => {
      if (existing.kind === 'REGISTER') {
        await tx.erpFinGiro.updateMany({
          where: { giroEntryId: id },
          data: { deletedAt: new Date(), updatedById: actor },
        });
      } else {
        await unlinkClearGiros(tx, id);
      }
      await tx.erpFinGiroEntry.update({
        where: { id },
        data: { deletedAt: new Date(), updatedById: actor },
      });
    });
    return { success: true, message: 'Giro entry dihapus' };
  }

  // ── outstanding lookup (CLEAR form row picker) ───────────────────────────────
  async findOutstandingGiros(type: string, search?: string, partnerId?: string) {
    const where: Prisma.ErpFinGiroWhereInput = {
      deletedAt: null,
      status: 'OUTSTANDING',
      clearedByEntryId: null,
      type: type as never,
    };
    if (partnerId) where.partnerId = BigInt(partnerId);
    if (search?.trim()) {
      const q = search.trim();
      where.OR = [
        { giroNumber: { contains: q, mode: 'insensitive' } },
        { bankName: { contains: q, mode: 'insensitive' } },
      ];
    }
    const giros = await this.prisma.erpFinGiro.findMany({
      where,
      orderBy: [{ dueDate: 'asc' }, { id: 'asc' }],
      select: {
        id: true,
        giroNumber: true,
        bankName: true,
        dueDate: true,
        amount: true,
        partnerId: true,
        giroAccountId: true,
      },
    });
    return { success: true, data: giros };
  }

  // ── workflow (§2.7 state machine) ────────────────────────────────────────────
  async transition(id: bigint, dto: TransitionGiroEntryDto, actorId?: string) {
    const entry = await this.findRaw(id);
    const actor = actorId ? BigInt(actorId) : null;
    const next = NEXT[entry.status]?.[dto.action];
    if (!next) {
      throw new BadRequestException(`Aksi ${dto.action} tidak valid dari status ${entry.status}.`);
    }
    if (dto.action === A.REJECT && !dto.reason?.trim()) {
      throw new BadRequestException('Alasan reject wajib diisi.');
    }

    if (dto.action === A.POST) {
      const period = await this.prisma.erpFiscalPeriod.findUnique({
        where: { id: entry.fiscalPeriodId },
        select: { status: true },
      });
      if (period?.status === 'CLOSED') {
        throw new BadRequestException('Periode fiskal sudah ditutup — tidak bisa posting.');
      }
      await this.prisma.$transaction(async (tx) => {
        if (entry.kind === 'CLEAR') {
          await this.posting.reverseLedger(tx, entry.id);
          const linked = await tx.erpFinGiro.findMany({
            where: { clearedByEntryId: entry.id, deletedAt: null },
          });
          await this.posting.postClearing(tx, entry, linked, actor);
          await tx.erpFinGiro.updateMany({
            where: { clearedByEntryId: entry.id },
            data: { status: 'CLEARED', previousStatus: 'OUTSTANDING' },
          });
        }
        await tx.erpFinGiroEntry.update({
          where: { id },
          data: {
            status: 'POSTED',
            previousStatus: entry.status as never,
            postingStatus: 'POSTED',
            postedAt: new Date(),
            postedById: actor,
            updatedById: actor,
          },
        });
      });
      return this.findOne(id);
    }

    if (dto.action === A.REOPEN) {
      if (entry.kind === 'REGISTER' && entry.registeredGiros.some((g) => g.status === 'CLEARED')) {
        throw new BadRequestException(
          'Register tidak bisa di-reopen: ada giro yang sudah CLEARED.',
        );
      }
      await this.prisma.$transaction(async (tx) => {
        if (entry.kind === 'CLEAR') {
          await this.posting.reverseLedger(tx, entry.id);
          // Keep clearedByEntryId + clearedDate (still-DRAFT clearing intent).
          await tx.erpFinGiro.updateMany({
            where: { clearedByEntryId: entry.id },
            data: { status: 'OUTSTANDING', previousStatus: 'CLEARED' },
          });
        }
        await tx.erpFinGiroEntry.update({
          where: { id },
          data: {
            status: 'DRAFT',
            previousStatus: entry.status as never,
            postingStatus: 'UNPOSTED',
            postedAt: null,
            postedById: null,
            updatedById: actor,
          },
        });
      });
      return this.findOne(id);
    }

    await this.prisma.erpFinGiroEntry.update({
      where: { id },
      data: {
        status: next as never,
        previousStatus: entry.status as never,
        updatedById: actor,
        ...(dto.action === A.REJECT
          ? { metadata: { ...((entry.metadata as object) ?? {}), rejectReason: dto.reason } }
          : {}),
      },
    });
    return this.findOne(id);
  }
}
