import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { SlsCustomerAdvancePostingService } from './sls-customer-advance-posting.service';
import { enrichCustomerAdvances } from './sls-customer-advance-enrich';
import { CreateSlsCustomerAdvanceDto } from './dto/create-sls-customer-advance.dto';
import { QuerySlsCustomerAdvancesDto } from './dto/query-sls-customer-advances.dto';
import { UpdateSlsCustomerAdvanceDto } from './dto/update-sls-customer-advance.dto';
import {
  SlsCustomerAdvanceTransitionAction as A,
  TransitionSlsCustomerAdvanceDto,
} from './dto/transition-sls-customer-advance.dto';
import { toBigInt, EDITABLE, NEXT, buildCustomerAdvanceWhere } from './sls-customer-advance.helpers';

const DOC_CODE = 'AS';
const FALLBACK_PREFIX = 'AS';

@Injectable()
export class ErpSlsCustomerAdvancesService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly posting: SlsCustomerAdvancePostingService,
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
    const count = await tx.erpSlsCustomerAdvance.count();
    return `${FALLBACK_PREFIX}${String(count + 1).padStart(6, '0')}`;
  }

  private async findRaw(id: bigint) {
    const doc = await this.prisma.erpSlsCustomerAdvance.findFirst({
      where: { id, deletedAt: null },
    });
    if (!doc) throw new NotFoundException('Customer advance tidak ditemukan');
    return doc;
  }

  private async one(id: bigint) {
    const doc = await this.findRaw(id);
    const [enriched] = await enrichCustomerAdvances(this.prisma, [doc]);
    return { success: true, data: enriched };
  }

  private async resolveDueDate(
    paymentTermId: string | undefined,
    docDate: string,
    explicitDueDate?: string,
  ): Promise<Date | null> {
    if (explicitDueDate) return new Date(explicitDueDate);
    if (!paymentTermId) return null;
    const term = await this.prisma.erpPaymentTerm.findUnique({
      where: { id: BigInt(paymentTermId) },
      select: { netDays: true },
    });
    if (!term) return null;
    const d = new Date(docDate);
    d.setDate(d.getDate() + term.netDays);
    return d;
  }

  // ── CRUD ────────────────────────────────────────────────────────────────────

  async create(dto: CreateSlsCustomerAdvanceDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;
    await this.resolveDueDate(dto.paymentTermId, dto.docDate, dto.dueDate);

    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.docDate);
      const wantAuto = dto.auto !== false && !dto.docNumber;
      const docNumber = wantAuto ? await this.genDocNumber(tx) : dto.docNumber;
      if (!docNumber) throw new BadRequestException('No dokumen wajib diisi.');

      const amount = new Prisma.Decimal(dto.amount);
      // amountFx = amount * exchangeRate
      const amountFx = amount.mul(new Prisma.Decimal(dto.exchangeRate));

      const row = await tx.erpSlsCustomerAdvance.create({
        data: {
          code: docNumber,
          docNumber,
          autoNumber: wantAuto ? docNumber : null,
          branchId: BigInt(dto.branchId),
          docDate: new Date(dto.docDate),
          fiscalPeriodId,
          customerId: toBigInt(dto.customerId),
          paymentTermId: toBigInt(dto.paymentTermId),
          currencyId: BigInt(dto.currencyId),
          exchangeRate: new Prisma.Decimal(dto.exchangeRate),
          amount,
          amountFx,
          appliedAmount: new Prisma.Decimal(0),
          settlementStatus: 'UNPAID',
          description: dto.description ?? null,
          notes: dto.notes ?? null,
          referenceNo: dto.referenceNo ?? null,
          costCenterId: toBigInt(dto.costCenterId),
          divisionId: toBigInt(dto.divisionId),
          projectId: toBigInt(dto.projectId),
          orderId: toBigInt(dto.orderId),
          legacyCode: dto.legacyCode ?? null,
          status: 'DRAFT',
          postingStatus: 'UNPOSTED',
          createdById: actor,
          updatedById: actor,
        },
        select: { id: true },
      });
      return row;
    });
    return this.one(created.id);
  }

  async findAll(query: QuerySlsCustomerAdvancesDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const where = buildCustomerAdvanceWhere(query);

    const sortBy = query.sortBy ?? 'docDate';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total, agg] = await this.prisma.$transaction([
      this.prisma.erpSlsCustomerAdvance.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }],
        skip: (page - 1) * limit,
        take: limit,
      }),
      this.prisma.erpSlsCustomerAdvance.count({ where }),
      this.prisma.erpSlsCustomerAdvance.aggregate({ where, _sum: { amount: true } }),
    ]);

    return {
      success: true,
      data: await enrichCustomerAdvances(this.prisma, items),
      meta: {
        page,
        limit,
        total,
        totalPages: Math.ceil(total / limit) || 1,
        sumGrandTotal: agg._sum.amount?.toString() ?? '0',
      },
    };
  }

  findOne(id: bigint) {
    return this.one(id);
  }

  async update(id: bigint, dto: UpdateSlsCustomerAdvanceDto, actorId?: string) {
    const existing = await this.findRaw(id);
    if (!EDITABLE.has(existing.status)) {
      throw new BadRequestException(
        `Dokumen berstatus ${existing.status} tidak bisa diedit. Reopen dulu bila perlu.`,
      );
    }
    const actor = actorId ? BigInt(actorId) : null;

    await this.prisma.$transaction(async (tx) => {
      const data: Prisma.ErpSlsCustomerAdvanceUpdateInput = { updatedById: actor };

      if (dto.docNumber !== undefined) {
        data.docNumber = dto.docNumber;
        data.code = dto.docNumber;
      }
      if (dto.branchId !== undefined) data.branchId = BigInt(dto.branchId);
      if (dto.customerId !== undefined) data.customerId = toBigInt(dto.customerId);
      if (dto.currencyId !== undefined) data.currencyId = BigInt(dto.currencyId);
      if (dto.exchangeRate !== undefined) data.exchangeRate = new Prisma.Decimal(dto.exchangeRate);
      if (dto.paymentTermId !== undefined) data.paymentTermId = toBigInt(dto.paymentTermId);
      if (dto.dueDate !== undefined) {
        // dueDate is not stored on the model — handled via paymentTerm
      }
      if (dto.description !== undefined) data.description = dto.description;
      if (dto.notes !== undefined) data.notes = dto.notes;
      if (dto.referenceNo !== undefined) data.referenceNo = dto.referenceNo;
      if (dto.costCenterId !== undefined) data.costCenterId = toBigInt(dto.costCenterId);
      if (dto.divisionId !== undefined) data.divisionId = toBigInt(dto.divisionId);
      if (dto.projectId !== undefined) data.projectId = toBigInt(dto.projectId);
      if (dto.orderId !== undefined) {
        const oid = toBigInt(dto.orderId);
        data.order = oid ? { connect: { id: oid } } : { disconnect: true };
      }
      if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;

      if (dto.docDate !== undefined) {
        data.docDate = new Date(dto.docDate);
        data.fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.docDate);
      } else if (dto.fiscalPeriodId !== undefined) {
        data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);
      }

      // Recompute amountFx if amount or exchangeRate changed
      if (dto.amount !== undefined || dto.exchangeRate !== undefined) {
        const newAmount = dto.amount !== undefined
          ? new Prisma.Decimal(dto.amount)
          : existing.amount;
        const newRate = dto.exchangeRate !== undefined
          ? new Prisma.Decimal(dto.exchangeRate)
          : existing.exchangeRate;
        if (dto.amount !== undefined) data.amount = newAmount;
        data.amountFx = newAmount.mul(newRate);
      }

      await tx.erpSlsCustomerAdvance.update({ where: { id }, data });
    });
    return this.one(id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status === 'POSTED') {
      throw new BadRequestException('Dokumen POSTED tidak bisa dihapus. Reopen dulu.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.erpSlsCustomerAdvance.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actor },
    });
    return { success: true, message: 'Customer advance dihapus' };
  }

  // ── workflow (§2.7 state machine) ────────────────────────────────────────────

  async transition(id: bigint, dto: TransitionSlsCustomerAdvanceDto, actorId?: string) {
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
        await tx.erpSlsCustomerAdvance.update({
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
        await tx.erpSlsCustomerAdvance.update({
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

    await this.prisma.erpSlsCustomerAdvance.update({
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
