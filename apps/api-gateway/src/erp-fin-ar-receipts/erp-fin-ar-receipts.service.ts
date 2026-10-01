import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { ArReceiptPostingService } from './ar-receipt-posting.service';
import { CreateArReceiptDto } from './dto/create-ar-receipt.dto';
import { QueryArReceiptDto } from './dto/query-ar-receipt.dto';
import { UpdateArReceiptDto } from './dto/update-ar-receipt.dto';
import {
  ArReceiptTransitionAction as A,
  TransitionArReceiptDto,
} from './dto/transition-ar-receipt.dto';
import { EDITABLE, NEXT, buildArReceiptWhere, toBigInt } from './ar-receipt.helpers';

const DOC_CODE = 'IP';
const FALLBACK_PREFIX = 'IP';

@Injectable()
export class ErpFinArReceiptsService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly posting: ArReceiptPostingService,
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
      // Atomic increment: row lock serializes concurrent saves (no duplicate / skipped numbers).
      const bumped = await tx.erpDocumentNumbering.update({
        where: { id: numbering.id },
        data: { nextNumber: { increment: 1 } },
        select: { nextNumber: true },
      });
      const seq = bumped.nextNumber - 1;
      return `${numbering.prefix}${String(seq).padStart(numbering.digitCount, '0')}`;
    }
    const count = await tx.erpFinArReceipt.count();
    return `${FALLBACK_PREFIX}${String(count + 1).padStart(6, '0')}`;
  }

  private async findRaw(id: bigint) {
    const receipt = await this.prisma.erpFinArReceipt.findFirst({
      where: { id, deletedAt: null },
      include: { instruments: { orderBy: { lineNo: 'asc' } }, allocations: { orderBy: { lineNo: 'asc' } } },
    });
    if (!receipt) throw new NotFoundException('AR receipt tidak ditemukan');
    return receipt;
  }

  private one(id: bigint) {
    return this.findRaw(id).then((data) => ({ success: true, data }));
  }

  // ── CRUD ────────────────────────────────────────────────────────────────────
  async create(dto: CreateArReceiptDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;
    this.validateTotals(dto);

    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.transactionDate);
      const wantAuto = dto.auto !== false && !dto.docNumber;
      const docNumber = wantAuto ? await this.genDocNumber(tx) : dto.docNumber;
      if (!docNumber) throw new BadRequestException('No dokumen wajib diisi.');

      const row = await tx.erpFinArReceipt.create({
        data: {
          docNumber,
          autoNumber: wantAuto ? docNumber : null,
          branchId: BigInt(dto.branchId),
          locationId: toBigInt(dto.locationId),
          source: dto.source ?? DOC_CODE,
          transactionDate: new Date(dto.transactionDate),
          fiscalPeriodId,
          partnerId: BigInt(dto.partnerId),
          contactPerson: dto.contactPerson ?? null,
          description: dto.description,
          notes: dto.notes ?? null,
          currencyId: BigInt(dto.currencyId),
          exchangeRate: new Prisma.Decimal(dto.exchangeRate),
          amount: this.sumInstruments(dto),
          allocatedAmount: this.sumAllocations(dto),
          paymentStatus: 'UNPAID',
          status: 'DRAFT',
          postingStatus: 'UNPOSTED',
          legacyCode: dto.legacyCode ?? null,
          metadata: { draftAllocations: dto.allocations.map((a) => ({ ...a })) },
          createdById: actor,
          updatedById: actor,
          instruments: {
            create: dto.instruments.map((i) => ({
              method: i.method as never,
              bankAccountId: BigInt(i.bankAccountId),
              currencyId: BigInt(dto.currencyId),
              exchangeRate: new Prisma.Decimal(dto.exchangeRate),
              amount: new Prisma.Decimal(i.amount),
              bankName: i.bankName ?? null,
              bankAccountNo: i.bankAccountNo ?? null,
              notes: i.notes ?? null,
              lineNo: i.lineNo,
            })),
          },
        },
        select: { id: true },
      });
      return row;
    });
    return this.one(created.id);
  }

  async findAll(query: QueryArReceiptDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const where = buildArReceiptWhere(query);

    const sortBy = query.sortBy ?? 'transactionDate';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpFinArReceipt.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }],
        skip: (page - 1) * limit,
        take: limit,
        include: { instruments: true, allocations: true },
      }),
      this.prisma.erpFinArReceipt.count({ where }),
    ]);

    return {
      success: true,
      data: items,
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  findOne(id: bigint) {
    return this.one(id);
  }

  async update(id: bigint, dto: UpdateArReceiptDto, actorId?: string) {
    const existing = await this.findRaw(id);
    if (!EDITABLE.has(existing.status)) {
      throw new BadRequestException(
        `Dokumen berstatus ${existing.status} tidak bisa diedit. Reopen dulu bila perlu.`,
      );
    }
    const actor = actorId ? BigInt(actorId) : null;

    await this.prisma.$transaction(async (tx) => {
      const data: Prisma.ErpFinArReceiptUpdateInput = { updatedById: actor };
      if (dto.branchId !== undefined) data.branchId = BigInt(dto.branchId);
      if (dto.locationId !== undefined) data.locationId = toBigInt(dto.locationId);
      if (dto.partnerId !== undefined) data.partnerId = BigInt(dto.partnerId);
      if (dto.contactPerson !== undefined) data.contactPerson = dto.contactPerson;
      if (dto.description !== undefined) data.description = dto.description;
      if (dto.notes !== undefined) data.notes = dto.notes;
      if (dto.currencyId !== undefined) data.currencyId = BigInt(dto.currencyId);
      if (dto.exchangeRate !== undefined) data.exchangeRate = new Prisma.Decimal(dto.exchangeRate);
      if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;

      if (dto.transactionDate !== undefined) {
        data.transactionDate = new Date(dto.transactionDate);
        data.fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.transactionDate);
      } else if (dto.fiscalPeriodId !== undefined) {
        data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);
      }

      if (dto.instruments !== undefined) {
        await tx.erpFinPaymentInstrument.deleteMany({ where: { arReceiptId: id } });
        const currencyId = dto.currencyId ?? existing.currencyId.toString();
        const exchangeRate = dto.exchangeRate ?? existing.exchangeRate.toString();
        data.instruments = {
          create: dto.instruments.map((i) => ({
            method: i.method as never,
            bankAccountId: BigInt(i.bankAccountId),
            currencyId: BigInt(currencyId),
            exchangeRate: new Prisma.Decimal(exchangeRate),
            amount: new Prisma.Decimal(i.amount),
            bankName: i.bankName ?? null,
            bankAccountNo: i.bankAccountNo ?? null,
            notes: i.notes ?? null,
            lineNo: i.lineNo,
          })),
        };
        data.amount = i_sum(dto.instruments);
      }
      if (dto.allocations !== undefined) {
        data.metadata = { draftAllocations: dto.allocations.map((a) => ({ ...a })) };
        data.allocatedAmount = a_sum(dto.allocations);
      }

      await tx.erpFinArReceipt.update({ where: { id }, data });
    });
    return this.one(id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status === 'POSTED') {
      throw new BadRequestException('Dokumen POSTED tidak bisa dihapus. Reopen dulu.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.erpFinArReceipt.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actor },
    });
    return { success: true, message: 'AR receipt dihapus' };
  }

  // ── workflow (§2.7 state machine) ────────────────────────────────────────────
  async transition(id: bigint, dto: TransitionArReceiptDto, actorId?: string) {
    const receipt = await this.findRaw(id);
    const actor = actorId ? BigInt(actorId) : null;
    const next = NEXT[receipt.status]?.[dto.action];
    if (!next) {
      throw new BadRequestException(`Aksi ${dto.action} tidak valid dari status ${receipt.status}.`);
    }
    if (dto.action === A.REJECT && !dto.reason?.trim()) {
      throw new BadRequestException('Alasan reject wajib diisi.');
    }

    if (dto.action === A.POST) {
      const period = await this.prisma.erpFiscalPeriod.findUnique({
        where: { id: receipt.fiscalPeriodId },
        select: { status: true },
      });
      if (period?.status === 'CLOSED') {
        throw new BadRequestException('Periode fiskal sudah ditutup — tidak bisa posting.');
      }
      await this.prisma.$transaction(async (tx) => {
        await this.posting.reverseLedger(tx, receipt.id);
        const fresh = await tx.erpFinArReceipt.findUniqueOrThrow({
          where: { id: receipt.id },
          include: { instruments: true },
        });
        await this.posting.postToLedger(tx, fresh, actor);
        const paid = this.posting.readDraftAllocations(fresh.metadata).reduce(
          (s, a) => s.add(new Prisma.Decimal(a.amount)),
          new Prisma.Decimal(0),
        );
        await tx.erpFinArReceipt.update({
          where: { id },
          data: {
            status: 'POSTED',
            previousStatus: receipt.status as never,
            postingStatus: 'POSTED',
            postedAt: new Date(),
            paymentStatus: 'PAID',
            allocatedAmount: paid,
            updatedById: actor,
          },
        });
      });
      return this.one(id);
    }

    if (dto.action === A.REOPEN) {
      await this.prisma.$transaction(async (tx) => {
        await this.posting.reverseLedger(tx, receipt.id);
        await tx.erpFinArReceipt.update({
          where: { id },
          data: {
            status: 'DRAFT',
            previousStatus: receipt.status as never,
            postingStatus: 'UNPOSTED',
            postedAt: null,
            paymentStatus: 'UNPAID',
            updatedById: actor,
          },
        });
      });
      return this.one(id);
    }

    await this.prisma.erpFinArReceipt.update({
      where: { id },
      data: {
        status: next as never,
        previousStatus: receipt.status as never,
        updatedById: actor,
        ...(dto.action === A.REJECT
          ? {
              metadata: {
                ...((receipt.metadata as object) ?? {}),
                rejectReason: dto.reason,
              },
            }
          : {}),
      },
    });
    return this.one(id);
  }

  private validateTotals(dto: CreateArReceiptDto): void {
    const instrumentTotal = this.sumInstruments(dto);
    const allocationTotal = this.sumAllocations(dto);
    if (!instrumentTotal.equals(allocationTotal)) {
      throw new BadRequestException(
        `Total cara bayar (${instrumentTotal}) tidak sama dengan total alokasi invoice (${allocationTotal}).`,
      );
    }
  }

  private sumInstruments(dto: CreateArReceiptDto): Prisma.Decimal {
    return i_sum(dto.instruments);
  }

  private sumAllocations(dto: CreateArReceiptDto): Prisma.Decimal {
    return a_sum(dto.allocations);
  }
}

function i_sum(instruments: { amount: string }[]): Prisma.Decimal {
  return instruments.reduce((s, i) => s.add(new Prisma.Decimal(i.amount)), new Prisma.Decimal(0));
}
function a_sum(allocations: { amount: string }[]): Prisma.Decimal {
  return allocations.reduce((s, a) => s.add(new Prisma.Decimal(a.amount)), new Prisma.Decimal(0));
}
