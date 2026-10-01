import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { JournalPostingService } from './journal-posting.service';
import { CreateJournalEntryDto } from './dto/create-journal-entry.dto';
import { QueryJournalEntryDto } from './dto/query-journal-entry.dto';
import { UpdateJournalEntryDto } from './dto/update-journal-entry.dto';
import {
  JournalTransitionAction as A,
  TransitionJournalEntryDto,
} from './dto/transition-journal-entry.dto';
import {
  toBigInt,
  EDITABLE,
  NEXT,
  DOC_CODE,
  FALLBACK_PREFIX,
  mapLine,
  buildJournalWhere,
} from './journal-txn.helpers';

@Injectable()
export class ErpFinJournalEntriesService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly posting: JournalPostingService,
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

  /** Auto doc number per journalType via sys_document_numberings (fallback: count + prefix). */
  private async genDocNumber(tx: Prisma.TransactionClient, journalType: string): Promise<string> {
    const numbering = await tx.erpDocumentNumbering.findFirst({
      where: { documentCode: DOC_CODE[journalType] ?? 'JV', deletedAt: null },
    });
    if (numbering) {
      const seq = numbering.nextNumber;
      await tx.erpDocumentNumbering.update({
        where: { id: numbering.id },
        data: { nextNumber: seq + 1 },
      });
      return `${numbering.prefix}${String(seq).padStart(numbering.digitCount, '0')}`;
    }
    const count = await tx.erpFinJournalEntry.count({ where: { journalType: journalType as never } });
    return `${FALLBACK_PREFIX[journalType] ?? 'JV'}${String(count + 1).padStart(6, '0')}`;
  }

  private async findRaw(id: bigint) {
    const entry = await this.prisma.erpFinJournalEntry.findFirst({
      where: { id, deletedAt: null },
      include: { lines: { orderBy: { lineNo: 'asc' } } },
    });
    if (!entry) throw new NotFoundException('Journal entry tidak ditemukan');
    return entry;
  }

  // ── CRUD ────────────────────────────────────────────────────────────────────
  async create(dto: CreateJournalEntryDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;
    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.entryDate);
      const wantAuto = dto.auto !== false && !dto.docNumber;
      const docNumber = wantAuto ? await this.genDocNumber(tx, dto.journalType) : dto.docNumber;
      if (!docNumber) throw new BadRequestException('No transaksi wajib diisi.');

      return tx.erpFinJournalEntry.create({
        data: {
          docNumber,
          autoNumber: wantAuto ? docNumber : null,
          journalType: dto.journalType as never,
          branchId: BigInt(dto.branchId),
          locationId: toBigInt(dto.locationId),
          source: dto.source ?? null,
          entryDate: new Date(dto.entryDate),
          fiscalPeriodId,
          partnerId: toBigInt(dto.partnerId),
          contactPerson: dto.contactPerson ?? null,
          description: dto.description,
          notes: dto.notes ?? null,
          currencyId: BigInt(dto.currencyId),
          exchangeRate: new Prisma.Decimal(dto.exchangeRate),
          status: 'DRAFT',
          postingStatus: 'UNPOSTED',
          legacyCode: dto.legacyCode ?? null,
          createdById: actor,
          updatedById: actor,
          lines: dto.lines.length ? { create: dto.lines.map((l) => mapLine(l, dto)) } : undefined,
        },
        select: { id: true },
      });
    });
    return this.findOne(created.id);
  }

  async findAll(query: QueryJournalEntryDto) {
    const page = Math.max(1, query.page ?? 1);
    const limit = Math.min(Math.max(1, query.limit ?? 10), 100);
    const includeTotal = query.includeTotal !== false;
    // Default last 31 days + max 366 when range set (see buildJournalWhere).
    const where = buildJournalWhere(query);
    const sortBy = query.sortBy ?? 'entryDate';
    const sortDir = query.sortDir ?? 'desc';

    // List includes only line ids so existing grid can show count without
    // shipping full line payload; edit still fetches detail before opening.
    const items = await this.prisma.erpFinJournalEntry.findMany({
      where,
      orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }],
      skip: (page - 1) * limit,
      take: limit,
      include: { lines: { select: { id: true } } },
    });

    let total: number | null = null;
    if (includeTotal) {
      total = await this.prisma.erpFinJournalEntry.count({ where });
    }

    const hasMore =
      total != null ? page * limit < total : items.length >= limit;
    const approxTotal =
      total ??
      (page - 1) * limit + items.length + (hasMore ? 1 : 0);
    return {
      success: true,
      data: items,
      meta: {
        page,
        limit,
        total: approxTotal,
        totalPages:
          total != null
            ? Math.ceil(total / limit) || 1
            : hasMore
              ? page + 1
              : page,
        hasMore,
        totalExact: total != null,
      },
    };
  }

  async findOne(id: bigint) {
    return { success: true, data: await this.findRaw(id) };
  }

  async update(id: bigint, dto: UpdateJournalEntryDto, actorId?: string) {
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

    const updated = await this.prisma.$transaction(async (tx) => {
      const data: Prisma.ErpFinJournalEntryUpdateInput = { updatedById: actor };
      if (dto.docNumber !== undefined) data.docNumber = dto.docNumber;
      if (dto.journalType !== undefined) data.journalType = dto.journalType as never;
      if (dto.branchId !== undefined) data.branchId = BigInt(dto.branchId);
      if (dto.locationId !== undefined) data.locationId = toBigInt(dto.locationId);
      if (dto.source !== undefined) data.source = dto.source;
      if (dto.partnerId !== undefined) data.partnerId = toBigInt(dto.partnerId);
      if (dto.contactPerson !== undefined) data.contactPerson = dto.contactPerson;
      if (dto.description !== undefined) data.description = dto.description;
      if (dto.notes !== undefined) data.notes = dto.notes;
      if (dto.currencyId !== undefined) data.currencyId = BigInt(dto.currencyId);
      if (dto.exchangeRate !== undefined) data.exchangeRate = new Prisma.Decimal(dto.exchangeRate);
      if (dto.entryDate !== undefined) {
        data.entryDate = new Date(dto.entryDate);
        data.fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.entryDate);
      } else if (dto.fiscalPeriodId !== undefined) {
        data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);
      }
      if (dto.lines !== undefined) {
        await tx.erpFinJournalLine.deleteMany({ where: { journalEntryId: id } });
        data.lines = { create: dto.lines.map((l) => mapLine(l, header)) };
      }
      return tx.erpFinJournalEntry.update({ where: { id }, data, select: { id: true } });
    });
    return this.findOne(updated.id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status === 'POSTED') {
      throw new BadRequestException('Dokumen POSTED tidak bisa dihapus. Reopen dulu.');
    }
    await this.prisma.erpFinJournalEntry.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actorId ? BigInt(actorId) : null },
    });
    return { success: true, message: 'Journal entry dihapus' };
  }

  // ── workflow (§2.7 state machine) ────────────────────────────────────────────
  async transition(id: bigint, dto: TransitionJournalEntryDto, actorId?: string) {
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
        await this.posting.reverseLedger(tx, entry.id);
        await this.posting.postToLedger(tx, entry, actor);
        await tx.erpFinJournalEntry.update({
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
      await this.prisma.$transaction(async (tx) => {
        await this.posting.reverseLedger(tx, entry.id);
        await tx.erpFinJournalEntry.update({
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

    await this.prisma.erpFinJournalEntry.update({
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
