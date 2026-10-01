import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { SlsFreightReceivablePostingService } from './sls-freight-receivable-posting.service';
import { CreateFreightReceivableDto } from './dto/create-freight-receivable.dto';
import { UpdateFreightReceivableDto } from './dto/update-freight-receivable.dto';
import { QueryFreightReceivableDto } from './dto/query-freight-receivable.dto';
import {
  FreightReceivableTransitionAction as A,
  TransitionFreightReceivableDto,
} from './dto/transition-freight-receivable.dto';

const DOC_CODE = 'RP';
const FALLBACK_PREFIX = 'RP';

const EDITABLE = new Set(['DRAFT', 'REJECTED']);

/** State machine transitions (§2.7). */
const NEXT: Record<string, Partial<Record<A, string>>> = {
  DRAFT: { [A.SUBMIT]: 'NEED_APPROVE' },
  REJECTED: { [A.SUBMIT]: 'NEED_APPROVE' },
  NEED_APPROVE: { [A.APPROVE]: 'APPROVED', [A.REJECT]: 'REJECTED' },
  APPROVED: { [A.POST]: 'POSTED', [A.REOPEN]: 'DRAFT' },
  POSTED: { [A.REOPEN]: 'DRAFT' },
};

@Injectable()
export class ErpSlsFreightReceivablesService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly posting: SlsFreightReceivablePostingService,
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
    const count = await tx.erpSlsFreightReceivable.count();
    return `${FALLBACK_PREFIX}${String(count + 1).padStart(6, '0')}`;
  }

  private async findRaw(id: bigint) {
    const row = await this.prisma.erpSlsFreightReceivable.findFirst({
      where: { id, deletedAt: null },
    });
    if (!row) throw new NotFoundException('Freight receivable tidak ditemukan');
    return row;
  }

  private async enrichOne(item: { customerId: bigint; [k: string]: unknown }) {
    const customer = await this.prisma.erpPartner.findFirst({
      where: { id: item.customerId },
      select: { id: true, code: true, name: true },
    });
    return { ...item, customer };
  }

  private async enrichMany(items: Array<{ customerId: bigint; [k: string]: unknown }>) {
    if (!items.length) return items;
    const ids = [...new Set(items.map((i) => i.customerId))];
    const customers = await this.prisma.erpPartner.findMany({
      where: { id: { in: ids } },
      select: { id: true, code: true, name: true },
    });
    const map = new Map(customers.map((c) => [c.id.toString(), c]));
    return items.map((i) => ({ ...i, customer: map.get(i.customerId.toString()) ?? null }));
  }

  private async one(id: bigint) {
    const row = await this.findRaw(id);
    return { success: true, data: await this.enrichOne(row) };
  }

  // ── CRUD ────────────────────────────────────────────────────────────────────
  async create(dto: CreateFreightReceivableDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;

    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.transactionDate);
      const docNumber = dto.docNumber?.trim() || (await this.genDocNumber(tx));

      const row = await tx.erpSlsFreightReceivable.create({
        data: {
          docNumber,
          autoNumber: dto.docNumber?.trim() ? null : docNumber,
          branchId: BigInt(dto.branchId),
          transactionDate: new Date(dto.transactionDate),
          fiscalPeriodId,
          customerId: BigInt(dto.partnerId),
          description: dto.description,
          notes: dto.notes ?? null,
          currencyId: BigInt(dto.currencyId),
          exchangeRate: new Prisma.Decimal(dto.exchangeRate),
          amount: new Prisma.Decimal(dto.amount),
          receivableAccountId: dto.receivableAccountId ? BigInt(dto.receivableAccountId) : null,
          incomeAccountId: dto.incomeAccountId ? BigInt(dto.incomeAccountId) : null,
          settlementStatus: 'UNPAID',
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

  async findAll(query: QueryFreightReceivableDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    const where: Prisma.ErpSlsFreightReceivableWhereInput = { deletedAt: null };
    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { docNumber: { contains: q, mode: 'insensitive' } },
        { description: { contains: q, mode: 'insensitive' } },
      ];
    }
    if (query.status) where.status = query.status as never;
    if (query.partnerId) where.customerId = BigInt(query.partnerId);
    if (query.dateFrom || query.dateTo) {
      where.transactionDate = {
        ...(query.dateFrom ? { gte: new Date(query.dateFrom) } : {}),
        ...(query.dateTo ? { lte: new Date(query.dateTo) } : {}),
      };
    }

    const ALLOWED_SORT = ['transactionDate', 'docNumber', 'amount', 'createdAt', 'updatedAt'];
    const sortField = ALLOWED_SORT.includes(query.sortBy ?? '') ? query.sortBy! : 'transactionDate';
    const sortDir = query.sortDir ?? 'desc';

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpSlsFreightReceivable.findMany({
        where,
        orderBy: [{ [sortField]: sortDir }, { id: 'desc' }],
        skip,
        take: limit,
      }),
      this.prisma.erpSlsFreightReceivable.count({ where }),
    ]);

    return {
      success: true,
      data: await this.enrichMany(items),
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  findOne(id: bigint) {
    return this.one(id);
  }

  async update(id: bigint, dto: UpdateFreightReceivableDto, actorId?: string) {
    const existing = await this.findRaw(id);
    if (!EDITABLE.has(existing.status)) {
      throw new BadRequestException(
        `Dokumen berstatus ${existing.status} tidak bisa diedit. Reopen dulu bila perlu.`,
      );
    }
    const actor = actorId ? BigInt(actorId) : null;

    await this.prisma.$transaction(async (tx) => {
      const data: Prisma.ErpSlsFreightReceivableUpdateInput = { updatedById: actor };
      if (dto.docNumber !== undefined) data.docNumber = dto.docNumber;
      if (dto.branchId !== undefined) data.branchId = BigInt(dto.branchId);
      if (dto.partnerId !== undefined) data.customerId = BigInt(dto.partnerId);
      if (dto.description !== undefined) data.description = dto.description;
      if (dto.notes !== undefined) data.notes = dto.notes;
      if (dto.currencyId !== undefined) data.currencyId = BigInt(dto.currencyId);
      if (dto.exchangeRate !== undefined) data.exchangeRate = new Prisma.Decimal(dto.exchangeRate);
      if (dto.amount !== undefined) data.amount = new Prisma.Decimal(dto.amount);
      if (dto.receivableAccountId !== undefined) {
        data.receivableAccountId = dto.receivableAccountId ? BigInt(dto.receivableAccountId) : null;
      }
      if (dto.incomeAccountId !== undefined) {
        data.incomeAccountId = dto.incomeAccountId ? BigInt(dto.incomeAccountId) : null;
      }
      if (dto.transactionDate !== undefined) {
        data.transactionDate = new Date(dto.transactionDate);
        data.fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.transactionDate);
      } else if (dto.fiscalPeriodId !== undefined) {
        data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);
      }

      await tx.erpSlsFreightReceivable.update({ where: { id }, data });
    });
    return this.one(id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status === 'POSTED') {
      throw new BadRequestException('Dokumen POSTED tidak bisa dihapus. Reopen dulu.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.erpSlsFreightReceivable.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actor },
    });
    return { success: true, message: 'Freight receivable dihapus' };
  }

  // ── workflow (§2.7 state machine) ────────────────────────────────────────────
  async transition(id: bigint, dto: TransitionFreightReceivableDto, actorId?: string) {
    const row = await this.findRaw(id);
    const actor = actorId ? BigInt(actorId) : null;
    const next = NEXT[row.status]?.[dto.action];
    if (!next) {
      throw new BadRequestException(`Aksi ${dto.action} tidak valid dari status ${row.status}.`);
    }
    if (dto.action === A.REJECT && !dto.reason?.trim()) {
      throw new BadRequestException('Alasan reject wajib diisi.');
    }

    if (dto.action === A.POST) {
      const period = await this.prisma.erpFiscalPeriod.findUnique({
        where: { id: row.fiscalPeriodId },
        select: { status: true },
      });
      if (period?.status === 'CLOSED') {
        throw new BadRequestException('Periode fiskal sudah ditutup — tidak bisa posting.');
      }
      await this.prisma.$transaction(async (tx) => {
        await this.posting.reverseLedger(tx, row.id);
        await this.posting.postToLedger(tx, row, actor);
        await tx.erpSlsFreightReceivable.update({
          where: { id },
          data: {
            status: 'POSTED',
            previousStatus: row.status as never,
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
        await this.posting.reverseLedger(tx, row.id);
        await tx.erpSlsFreightReceivable.update({
          where: { id },
          data: {
            status: 'DRAFT',
            previousStatus: row.status as never,
            postingStatus: 'UNPOSTED',
            postedAt: null,
            updatedById: actor,
          },
        });
      });
      return this.one(id);
    }

    await this.prisma.erpSlsFreightReceivable.update({
      where: { id },
      data: {
        status: next as never,
        previousStatus: row.status as never,
        updatedById: actor,
        ...(dto.action === A.REJECT
          ? {
              metadata: {
                ...((row.metadata as object) ?? {}),
                rejectReason: dto.reason,
              },
            }
          : {}),
      },
    });
    return this.one(id);
  }
}
