import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { ApPaymentPostingService } from './ap-payment-posting.service';
import { CreateApPaymentDto } from './dto/create-ap-payment.dto';
import { QueryApPaymentDto } from './dto/query-ap-payment.dto';
import { UpdateApPaymentDto } from './dto/update-ap-payment.dto';
import {
  ApPaymentTransitionAction as A,
  TransitionApPaymentDto,
} from './dto/transition-ap-payment.dto';
import { EDITABLE, NEXT, buildApPaymentWhere, toBigInt } from './ap-payment.helpers';

const DOC_CODE = 'VP';
const FALLBACK_PREFIX = 'VP';

@Injectable()
export class ErpFinApPaymentsService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly posting: ApPaymentPostingService,
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
    const count = await tx.erpFinApPayment.count();
    return `${FALLBACK_PREFIX}${String(count + 1).padStart(6, '0')}`;
  }

  private async findRaw(id: bigint) {
    const payment = await this.prisma.erpFinApPayment.findFirst({
      where: { id, deletedAt: null },
      include: { instruments: { orderBy: { lineNo: 'asc' } }, allocations: { orderBy: { lineNo: 'asc' } } },
    });
    if (!payment) throw new NotFoundException('AP payment tidak ditemukan');
    return payment;
  }

  private one(id: bigint) {
    return this.findRaw(id).then((data) => ({ success: true, data }));
  }

  // ── CRUD ────────────────────────────────────────────────────────────────────
  async create(dto: CreateApPaymentDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;
    this.validateTotals(dto);

    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.transactionDate);
      const wantAuto = dto.auto !== false && !dto.docNumber;
      const docNumber = wantAuto ? await this.genDocNumber(tx) : dto.docNumber;
      if (!docNumber) throw new BadRequestException('No dokumen wajib diisi.');

      const row = await tx.erpFinApPayment.create({
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
          fxGainLossAccountId: toBigInt(dto.fxGainLossAccountId),
          termDiscountAccountId: toBigInt(dto.termDiscountAccountId),
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

  async findAll(query: QueryApPaymentDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const where = buildApPaymentWhere(query);

    const sortBy = query.sortBy ?? 'transactionDate';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpFinApPayment.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }],
        skip: (page - 1) * limit,
        take: limit,
        include: { instruments: true, allocations: true },
      }),
      this.prisma.erpFinApPayment.count({ where }),
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

  async update(id: bigint, dto: UpdateApPaymentDto, actorId?: string) {
    const existing = await this.findRaw(id);
    if (!EDITABLE.has(existing.status)) {
      throw new BadRequestException(
        `Dokumen berstatus ${existing.status} tidak bisa diedit. Reopen dulu bila perlu.`,
      );
    }
    const actor = actorId ? BigInt(actorId) : null;

    await this.prisma.$transaction(async (tx) => {
      const data: Prisma.ErpFinApPaymentUpdateInput = { updatedById: actor };
      if (dto.branchId !== undefined) data.branchId = BigInt(dto.branchId);
      if (dto.locationId !== undefined) data.locationId = toBigInt(dto.locationId);
      if (dto.partnerId !== undefined) data.partnerId = BigInt(dto.partnerId);
      if (dto.contactPerson !== undefined) data.contactPerson = dto.contactPerson;
      if (dto.description !== undefined) data.description = dto.description;
      if (dto.notes !== undefined) data.notes = dto.notes;
      if (dto.currencyId !== undefined) data.currencyId = BigInt(dto.currencyId);
      if (dto.exchangeRate !== undefined) data.exchangeRate = new Prisma.Decimal(dto.exchangeRate);
      if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;
      if (dto.fxGainLossAccountId !== undefined) data.fxGainLossAccountId = toBigInt(dto.fxGainLossAccountId);
      if (dto.termDiscountAccountId !== undefined) data.termDiscountAccountId = toBigInt(dto.termDiscountAccountId);

      if (dto.transactionDate !== undefined) {
        data.transactionDate = new Date(dto.transactionDate);
        data.fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.transactionDate);
      } else if (dto.fiscalPeriodId !== undefined) {
        data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);
      }

      if (dto.instruments !== undefined) {
        await tx.erpFinPaymentInstrument.deleteMany({ where: { apPaymentId: id } });
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
        data.amount = sumDecimals(dto.instruments);
      }
      if (dto.allocations !== undefined) {
        data.metadata = { draftAllocations: dto.allocations.map((a) => ({ ...a })) };
        data.allocatedAmount = sumDecimals(dto.allocations);
      }

      await tx.erpFinApPayment.update({ where: { id }, data });
    });
    return this.one(id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status === 'POSTED') {
      throw new BadRequestException('Dokumen POSTED tidak bisa dihapus. Reopen dulu.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.erpFinApPayment.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actor },
    });
    return { success: true, message: 'AP payment dihapus' };
  }

  // ── workflow (§2.7 state machine) ────────────────────────────────────────────
  async transition(id: bigint, dto: TransitionApPaymentDto, actorId?: string) {
    const payment = await this.findRaw(id);
    const actor = actorId ? BigInt(actorId) : null;
    const next = NEXT[payment.status]?.[dto.action];
    if (!next) {
      throw new BadRequestException(`Aksi ${dto.action} tidak valid dari status ${payment.status}.`);
    }
    if (dto.action === A.REJECT && !dto.reason?.trim()) {
      throw new BadRequestException('Alasan reject wajib diisi.');
    }

    if (dto.action === A.POST) {
      const period = await this.prisma.erpFiscalPeriod.findUnique({
        where: { id: payment.fiscalPeriodId },
        select: { status: true },
      });
      if (period?.status === 'CLOSED') {
        throw new BadRequestException('Periode fiskal sudah ditutup — tidak bisa posting.');
      }
      await this.prisma.$transaction(async (tx) => {
        await this.posting.reverseLedger(tx, payment.id);
        const fresh = await tx.erpFinApPayment.findUniqueOrThrow({
          where: { id: payment.id },
          include: { instruments: true },
        });
        await this.posting.postToLedger(tx, fresh, actor);
        const paid = this.posting
          .readDraftAllocations(fresh.metadata)
          .reduce((s, a) => s.add(new Prisma.Decimal(a.amount)), new Prisma.Decimal(0));
        await tx.erpFinApPayment.update({
          where: { id },
          data: {
            status: 'POSTED',
            previousStatus: payment.status as never,
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
        await this.posting.reverseLedger(tx, payment.id);
        await tx.erpFinApPayment.update({
          where: { id },
          data: {
            status: 'DRAFT',
            previousStatus: payment.status as never,
            postingStatus: 'UNPOSTED',
            postedAt: null,
            paymentStatus: 'UNPAID',
            updatedById: actor,
          },
        });
      });
      return this.one(id);
    }

    await this.prisma.erpFinApPayment.update({
      where: { id },
      data: {
        status: next as never,
        previousStatus: payment.status as never,
        updatedById: actor,
        ...(dto.action === A.REJECT
          ? {
              metadata: {
                ...((payment.metadata as object) ?? {}),
                rejectReason: dto.reason,
              },
            }
          : {}),
      },
    });
    return this.one(id);
  }

  private validateTotals(dto: CreateApPaymentDto): void {
    const instrumentTotal = this.sumInstruments(dto);
    const allocationTotal = this.sumAllocations(dto);
    const fxNet = dto.allocations.reduce(
      (s, a) => s.add(new Prisma.Decimal(a.fxGainLossAmount ?? 0)),
      new Prisma.Decimal(0),
    );
    const discountNet = dto.allocations.reduce(
      (s, a) => s.add(new Prisma.Decimal(a.termDiscountAmount ?? 0)),
      new Prisma.Decimal(0),
    );
    const expectedInstrumentTotal = allocationTotal.sub(fxNet).sub(discountNet);
    if (!instrumentTotal.equals(expectedInstrumentTotal)) {
      throw new BadRequestException(
        `Total cara bayar (${instrumentTotal}) tidak sama dengan alokasi (${allocationTotal}) dikurangi selisih kurs (${fxNet}) dan potongan termin (${discountNet}) = ${expectedInstrumentTotal}.`,
      );
    }
  }

  private sumInstruments(dto: CreateApPaymentDto): Prisma.Decimal {
    return sumDecimals(dto.instruments);
  }

  private sumAllocations(dto: CreateApPaymentDto): Prisma.Decimal {
    return sumDecimals(dto.allocations);
  }
}

function sumDecimals(items: { amount: string }[]): Prisma.Decimal {
  return items.reduce((s, i) => s.add(new Prisma.Decimal(i.amount)), new Prisma.Decimal(0));
}
