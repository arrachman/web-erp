import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { CreateArCollectionDto } from './dto/create-ar-collection.dto';
import { UpdateArCollectionDto } from './dto/update-ar-collection.dto';
import { QueryArCollectionDto } from './dto/query-ar-collection.dto';
import {
  ArCollectionTransitionAction as A,
  TransitionArCollectionDto,
} from './dto/transition-ar-collection.dto';

/** Source discriminator — all rows in fin_ar_receipts for IC module. */
const SOURCE = 'IC';
const DOC_CODE = 'IC';
const FALLBACK_PREFIX = 'IC';

/** Statuses that allow document edits. */
const EDITABLE = new Set(['DRAFT', 'REJECTED']);

/** State machine transitions. */
const NEXT: Record<string, Partial<Record<A, string>>> = {
  DRAFT:       { [A.SUBMIT]: 'NEED_APPROVE' },
  NEED_APPROVE:{ [A.APPROVE]: 'APPROVED', [A.REJECT]: 'REJECTED' },
  REJECTED:    { [A.SUBMIT]: 'NEED_APPROVE' },
  APPROVED:    { [A.POST]: 'POSTED', [A.REOPEN]: 'DRAFT' },
};

@Injectable()
export class ErpSlsArCollectionsService {
  constructor(private readonly prisma: PrismaService) {}

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
    const count = await tx.erpFinArReceipt.count({ where: { source: SOURCE } });
    return `${FALLBACK_PREFIX}${String(count + 1).padStart(6, '0')}`;
  }

  private async findRaw(id: bigint) {
    const item = await this.prisma.erpFinArReceipt.findFirst({
      where: { id, source: SOURCE, deletedAt: null },
    });
    if (!item) throw new NotFoundException('AR collection tidak ditemukan');
    return item;
  }

  /** Enrich a single receipt row with partner data (partnerId FK, no Prisma relation). */
  private async enrichOne(item: { partnerId: bigint; [k: string]: unknown }) {
    const partner = await this.prisma.erpPartner.findFirst({
      where: { id: item.partnerId },
      select: { id: true, code: true, name: true },
    });
    return { ...item, partner };
  }

  /** Enrich a list of receipt rows with partner data. */
  private async enrichMany(items: Array<{ partnerId: bigint; [k: string]: unknown }>) {
    if (!items.length) return items;
    const ids = [...new Set(items.map((i) => i.partnerId))];
    const partners = await this.prisma.erpPartner.findMany({
      where: { id: { in: ids } },
      select: { id: true, code: true, name: true },
    });
    const map = new Map(partners.map((p) => [p.id.toString(), p]));
    return items.map((i) => ({ ...i, partner: map.get(i.partnerId.toString()) ?? null }));
  }

  async create(dto: CreateArCollectionDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;

    const created = await this.prisma.$transaction(async (tx) => {
      const docNumber = dto.docNumber?.trim() || (await this.genDocNumber(tx));

      // Resolve fiscal period if not supplied — find period covering transactionDate.
      let fiscalPeriodId = BigInt(dto.fiscalPeriodId);
      if (!dto.fiscalPeriodId) {
        const d = new Date(dto.transactionDate);
        const period = await tx.erpFiscalPeriod.findFirst({
          where: { deletedAt: null, startDate: { lte: d }, endDate: { gte: d } },
          select: { id: true },
        });
        if (!period)
          throw new BadRequestException(
            `Tidak ada periode fiskal yang memuat tanggal ${dto.transactionDate}.`,
          );
        fiscalPeriodId = period.id;
      }

      return tx.erpFinArReceipt.create({
        data: {
          docNumber,
          source: SOURCE,
          transactionDate: new Date(dto.transactionDate),
          fiscalPeriodId,
          branchId: BigInt(dto.branchId),
          partnerId: BigInt(dto.partnerId),
          description: dto.description,
          notes: dto.notes ?? null,
          currencyId: dto.currencyId ? BigInt(dto.currencyId) : BigInt(1),
          exchangeRate: new Prisma.Decimal(dto.exchangeRate ?? '1'),
          amount: new Prisma.Decimal(dto.amount),
          allocatedAmount: new Prisma.Decimal('0'),
          paymentStatus: 'UNPAID',
          status: 'DRAFT',
          postingStatus: 'UNPOSTED', // TODO: implement GL posting when ledger mapping is ready
          createdById: actor,
          updatedById: actor,
        },
      });
    });

    return { success: true, data: await this.enrichOne(created) };
  }

  async findAll(query: QueryArCollectionDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    const where: Prisma.ErpFinArReceiptWhereInput = {
      source: SOURCE,
      deletedAt: null,
    };

    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { docNumber: { contains: q, mode: 'insensitive' } },
        { description: { contains: q, mode: 'insensitive' } },
      ];
    }
    if (query.status) where.status = query.status as never;
    if (query.partnerId) where.partnerId = BigInt(query.partnerId);
    if (query.dateFrom || query.dateTo) {
      where.transactionDate = {
        ...(query.dateFrom ? { gte: new Date(query.dateFrom) } : {}),
        ...(query.dateTo ? { lte: new Date(query.dateTo) } : {}),
      };
    }

    const ALLOWED_SORT = ['transactionDate', 'docNumber', 'amount', 'createdAt', 'updatedAt'];
    const sortField = query.sortBy ?? 'transactionDate';
    const sortDir = query.sortDir ?? 'desc';
    const orderBy = ALLOWED_SORT.includes(sortField)
      ? [{ [sortField]: sortDir }, { createdAt: sortDir }]
      : [{ transactionDate: 'desc' as const }, { createdAt: 'desc' as const }];

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpFinArReceipt.findMany({
        where,
        orderBy,
        skip,
        take: limit,
      }),
      this.prisma.erpFinArReceipt.count({ where }),
    ]);

    return {
      success: true,
      data: await this.enrichMany(items),
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  async findOne(id: bigint) {
    const item = await this.findRaw(id);
    return { success: true, data: await this.enrichOne(item) };
  }

  async update(id: bigint, dto: UpdateArCollectionDto, actorId?: string) {
    const existing = await this.findRaw(id);
    if (!EDITABLE.has(existing.status as string)) {
      throw new BadRequestException(
        `Dokumen berstatus ${existing.status} tidak bisa diedit. Reopen dulu bila perlu.`,
      );
    }

    const data: Prisma.ErpFinArReceiptUpdateInput = {
      updatedById: actorId ? BigInt(actorId) : null,
    };
    if (dto.docNumber !== undefined) data.docNumber = dto.docNumber;
    if (dto.transactionDate !== undefined) data.transactionDate = new Date(dto.transactionDate);
    if (dto.fiscalPeriodId !== undefined) data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);
    if (dto.branchId !== undefined) data.branchId = BigInt(dto.branchId);
    if (dto.partnerId !== undefined) data.partnerId = BigInt(dto.partnerId);
    if (dto.description !== undefined) data.description = dto.description;
    if (dto.currencyId !== undefined) data.currencyId = BigInt(dto.currencyId);
    if (dto.exchangeRate !== undefined) data.exchangeRate = new Prisma.Decimal(dto.exchangeRate);
    if (dto.amount !== undefined) data.amount = new Prisma.Decimal(dto.amount);
    if (dto.notes !== undefined) data.notes = dto.notes;

    const updated = await this.prisma.erpFinArReceipt.update({
      where: { id },
      data,
    });
    return { success: true, data: await this.enrichOne(updated) };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if ((existing.status as string) === 'POSTED') {
      throw new BadRequestException('Dokumen POSTED tidak bisa dihapus. Reopen dulu.');
    }
    await this.prisma.erpFinArReceipt.update({
      where: { id },
      data: {
        deletedAt: new Date(),
        updatedById: actorId ? BigInt(actorId) : null,
      },
    });
    return { success: true, message: 'AR collection dihapus' };
  }

  async transition(id: bigint, dto: TransitionArCollectionDto, actorId?: string) {
    const item = await this.findRaw(id);
    const actor = actorId ? BigInt(actorId) : null;
    const next = NEXT[item.status as string]?.[dto.action];
    if (!next) {
      throw new BadRequestException(
        `Aksi ${dto.action} tidak valid dari status ${item.status}.`,
      );
    }
    if (dto.action === A.REJECT && !dto.reason?.trim()) {
      throw new BadRequestException('Alasan reject wajib diisi.');
    }

    // TODO: implement GL posting when ledger mapping for IC is ready.
    const data: Prisma.ErpFinArReceiptUpdateInput = {
      status: next as never,
      previousStatus: item.status as never,
      updatedById: actor,
      ...(next === 'POSTED'
        ? { postingStatus: 'UNPOSTED', postedAt: new Date() } // TODO: change postingStatus to 'POSTED' after GL posting
        : {}),
      ...(dto.action === A.REOPEN
        ? { postingStatus: 'UNPOSTED', postedAt: null }
        : {}),
      ...(dto.action === A.REJECT
        ? {
            metadata: {
              ...((item.metadata as object) ?? {}),
              rejectReason: dto.reason,
            },
          }
        : {}),
    };

    const updated = await this.prisma.erpFinArReceipt.update({
      where: { id },
      data,
    });
    return { success: true, data: await this.enrichOne(updated) };
  }
}
