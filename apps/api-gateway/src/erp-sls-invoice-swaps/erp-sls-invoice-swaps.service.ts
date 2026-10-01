import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { SlsInvoiceSwapPostingService } from './sls-invoice-swap-posting.service';
import { enrichInvoiceSwaps } from './sls-invoice-swap-enrich';
import { CreateSlsInvoiceSwapDto, SlsInvoiceSwapLineDto } from './dto/create-sls-invoice-swap.dto';
import { QuerySlsInvoiceSwapsDto } from './dto/query-sls-invoice-swaps.dto';
import { UpdateSlsInvoiceSwapDto } from './dto/update-sls-invoice-swap.dto';
import {
  SlsInvoiceSwapTransitionAction as A,
  TransitionSlsInvoiceSwapDto,
} from './dto/transition-sls-invoice-swap.dto';
import { toBigInt, EDITABLE, NEXT, buildInvoiceSwapWhere } from './sls-invoice-swap.helpers';

const DOC_CODE = 'SIE';
const FALLBACK_PREFIX = 'SIE';

@Injectable()
export class ErpSlsInvoiceSwapsService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly posting: SlsInvoiceSwapPostingService,
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
    const count = await tx.erpSlsInvoiceSwap.count();
    return `${FALLBACK_PREFIX}${String(count + 1).padStart(6, '0')}`;
  }

  private async findRaw(id: bigint) {
    const doc = await this.prisma.erpSlsInvoiceSwap.findFirst({
      where: { id, deletedAt: null },
      include: { lines: { orderBy: { lineNo: 'asc' } } },
    });
    if (!doc) throw new NotFoundException('Invoice swap tidak ditemukan');
    return doc;
  }

  private async one(id: bigint) {
    const doc = await this.findRaw(id);
    const [enriched] = await enrichInvoiceSwaps(this.prisma, [doc]);
    return { success: true, data: enriched };
  }

  /** Map a line DTO → Prisma create input for ErpSlsInvoiceSwapLine. */
  private mapSwapLine(line: SlsInvoiceSwapLineDto): Prisma.ErpSlsInvoiceSwapLineCreateWithoutSwapInput {
    const toInvoiceId = toBigInt(line.toInvoiceId);
    return {
      fromInvoice: { connect: { id: BigInt(line.fromInvoiceId) } },
      ...(toInvoiceId ? { toInvoice: { connect: { id: toInvoiceId } } } : {}),
      amount: new Prisma.Decimal(line.amount),
      lineNo: line.lineNo,
    };
  }

  // ── CRUD ────────────────────────────────────────────────────────────────────

  async create(dto: CreateSlsInvoiceSwapDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;

    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.docDate);
      const wantAuto = dto.auto !== false && !dto.docNumber;
      const docNumber = wantAuto ? await this.genDocNumber(tx) : dto.docNumber;
      if (!docNumber) throw new BadRequestException('No dokumen wajib diisi.');

      const row = await tx.erpSlsInvoiceSwap.create({
        data: {
          code: docNumber,
          docNumber,
          branchId: BigInt(dto.branchId),
          docDate: new Date(dto.docDate),
          fiscalPeriodId,
          customerId: toBigInt(dto.customerId),
          currencyId: BigInt(dto.currencyId),
          exchangeRate: new Prisma.Decimal(dto.exchangeRate),
          description: dto.description ?? null,
          notes: dto.notes ?? null,
          referenceNo: dto.referenceNo ?? null,
          legacyCode: dto.legacyCode ?? null,
          status: 'DRAFT',
          postingStatus: 'UNPOSTED',
          createdById: actor,
          updatedById: actor,
          lines: dto.lines.length
            ? { create: dto.lines.map((l) => this.mapSwapLine(l)) }
            : undefined,
        },
        select: { id: true },
      });
      return row;
    });
    return this.one(created.id);
  }

  async findAll(query: QuerySlsInvoiceSwapsDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const where = buildInvoiceSwapWhere(query);

    const sortBy = query.sortBy ?? 'docDate';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpSlsInvoiceSwap.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }],
        skip: (page - 1) * limit,
        take: limit,
        include: { lines: { orderBy: { lineNo: 'asc' } } },
      }),
      this.prisma.erpSlsInvoiceSwap.count({ where }),
    ]);

    // Sum line amounts as the document "total"
    const lineAmountAgg = await this.prisma.erpSlsInvoiceSwapLine.aggregate({
      where: { swap: where },
      _sum: { amount: true },
    });

    return {
      success: true,
      data: await enrichInvoiceSwaps(this.prisma, items),
      meta: {
        page,
        limit,
        total,
        totalPages: Math.ceil(total / limit) || 1,
        sumGrandTotal: lineAmountAgg._sum.amount?.toString() ?? '0',
      },
    };
  }

  findOne(id: bigint) {
    return this.one(id);
  }

  async update(id: bigint, dto: UpdateSlsInvoiceSwapDto, actorId?: string) {
    const existing = await this.findRaw(id);
    if (!EDITABLE.has(existing.status)) {
      throw new BadRequestException(
        `Dokumen berstatus ${existing.status} tidak bisa diedit. Reopen dulu bila perlu.`,
      );
    }
    const actor = actorId ? BigInt(actorId) : null;

    await this.prisma.$transaction(async (tx) => {
      const data: Prisma.ErpSlsInvoiceSwapUpdateInput = { updatedById: actor };

      if (dto.docNumber !== undefined) {
        data.docNumber = dto.docNumber;
        data.code = dto.docNumber;
      }
      if (dto.branchId !== undefined) data.branchId = BigInt(dto.branchId);
      if (dto.customerId !== undefined) data.customerId = toBigInt(dto.customerId);
      if (dto.currencyId !== undefined) data.currencyId = BigInt(dto.currencyId);
      if (dto.exchangeRate !== undefined) data.exchangeRate = new Prisma.Decimal(dto.exchangeRate);
      if (dto.description !== undefined) data.description = dto.description;
      if (dto.notes !== undefined) data.notes = dto.notes;
      if (dto.referenceNo !== undefined) data.referenceNo = dto.referenceNo;
      if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;

      if (dto.docDate !== undefined) {
        data.docDate = new Date(dto.docDate);
        data.fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.docDate);
      } else if (dto.fiscalPeriodId !== undefined) {
        data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);
      }

      if (dto.lines !== undefined) {
        await tx.erpSlsInvoiceSwapLine.deleteMany({ where: { swapId: id } });
        data.lines = { create: dto.lines.map((l) => this.mapSwapLine(l)) };
      }

      await tx.erpSlsInvoiceSwap.update({ where: { id }, data });
    });
    return this.one(id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status === 'POSTED') {
      throw new BadRequestException('Dokumen POSTED tidak bisa dihapus. Reopen dulu.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.erpSlsInvoiceSwap.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actor },
    });
    return { success: true, message: 'Invoice swap dihapus' };
  }

  // ── workflow (§2.7 state machine) ────────────────────────────────────────────

  async transition(id: bigint, dto: TransitionSlsInvoiceSwapDto, actorId?: string) {
    const doc = await this.findRaw(id);
    const actor = actorId ? BigInt(actorId) : null;
    const next = NEXT[doc.status]?.[dto.action];
    if (!next) {
      throw new BadRequestException(`Aksi ${dto.action} tidak valid dari status ${doc.status}.`);
    }
    if (dto.action === A.REJECT && !dto.reason?.trim()) {
      throw new BadRequestException('Alasan reject wajib diisi.');
    }

    if (dto.action === A.POST) {
      const period = await this.prisma.erpFiscalPeriod.findUnique({
        where: { id: doc.fiscalPeriodId },
        select: { status: true },
      });
      if (period?.status === 'CLOSED') {
        throw new BadRequestException('Periode fiskal sudah ditutup — tidak bisa posting.');
      }
      await this.prisma.$transaction(async (tx) => {
        await this.posting.reverseLedger(tx, doc.id);
        await this.posting.postToLedger(tx, doc, actor);
        await tx.erpSlsInvoiceSwap.update({
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
        await this.posting.reverseLedger(tx, doc.id);
        await tx.erpSlsInvoiceSwap.update({
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

    await this.prisma.erpSlsInvoiceSwap.update({
      where: { id },
      data: {
        status: next as never,
        updatedById: actor,
        ...(dto.action === A.REJECT
          ? {
              metadata: {
                ...((doc.metadata as object) ?? {}),
                rejectReason: dto.reason,
              },
            }
          : {}),
      },
    });
    return this.one(id);
  }
}
