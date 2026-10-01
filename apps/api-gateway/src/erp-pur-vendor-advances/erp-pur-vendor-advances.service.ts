import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { CreateVendorAdvanceDto } from './dto/create-vendor-advance.dto';
import { UpdateVendorAdvanceDto } from './dto/update-vendor-advance.dto';
import { QueryVendorAdvanceDto } from './dto/query-vendor-advance.dto';
import {
  VendorAdvanceTransitionAction as A,
  TransitionVendorAdvanceDto,
} from './dto/transition-vendor-advance.dto';

/** Source discriminator — all vendor advance rows use 'AP' in fin_ap_payments. */
const SOURCE = 'AP';
const DOC_CODE = 'AP';
const FALLBACK_PREFIX = 'AP';

/** Statuses that allow document edits. */
const EDITABLE = new Set(['DRAFT', 'REJECTED']);

/** State machine transitions. */
const NEXT: Record<string, Partial<Record<A, string>>> = {
  DRAFT:        { [A.SUBMIT]: 'NEED_APPROVE' },
  NEED_APPROVE: { [A.APPROVE]: 'APPROVED', [A.REJECT]: 'REJECTED' },
  REJECTED:     { [A.SUBMIT]: 'NEED_APPROVE' },
  APPROVED:     { [A.POST]: 'POSTED', [A.REOPEN]: 'DRAFT' },
};

@Injectable()
export class ErpPurVendorAdvancesService {
  constructor(private readonly prisma: PrismaService) {}

  // ---------------------------------------------------------------------------
  // Helpers
  // ---------------------------------------------------------------------------

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
    const count = await tx.erpFinApPayment.count({ where: { source: SOURCE } });
    return `${FALLBACK_PREFIX}${String(count + 1).padStart(6, '0')}`;
  }

  private async findRaw(id: bigint) {
    const item = await this.prisma.erpFinApPayment.findFirst({
      where: { id, source: SOURCE, deletedAt: null },
    });
    if (!item) throw new NotFoundException('Vendor advance tidak ditemukan');
    return item;
  }

  /** Enrich a single payment row with partner data (no Prisma relation on this model). */
  private async enrichOne(item: { partnerId: bigint; [k: string]: unknown }) {
    const partner = await this.prisma.erpPartner.findFirst({
      where: { id: item.partnerId },
      select: { id: true, code: true, name: true },
    });
    return { ...item, partner };
  }

  /** Enrich a list of payment rows with partner data (batch lookup). */
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

  // ---------------------------------------------------------------------------
  // CRUD
  // ---------------------------------------------------------------------------

  async create(dto: CreateVendorAdvanceDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;

    const created = await this.prisma.$transaction(async (tx) => {
      const docNumber = dto.docNumber?.trim() || (await this.genDocNumber(tx));

      // Resolve fiscal period if not supplied — find period covering transactionDate.
      let fiscalPeriodId = dto.fiscalPeriodId ? BigInt(dto.fiscalPeriodId) : null;
      if (!fiscalPeriodId) {
        const d = new Date(dto.transactionDate);
        const period = await tx.erpFiscalPeriod.findFirst({
          where: { deletedAt: null, startDate: { lte: d }, endDate: { gte: d } },
          select: { id: true },
        });
        if (!period) {
          throw new BadRequestException(
            `Tidak ada periode fiskal yang memuat tanggal ${dto.transactionDate}.`,
          );
        }
        fiscalPeriodId = period.id;
      }

      return tx.erpFinApPayment.create({
        data: {
          docNumber,
          source: SOURCE,
          transactionDate: new Date(dto.transactionDate),
          fiscalPeriodId,
          branchId: BigInt(dto.branchId),
          partnerId: BigInt(dto.partnerId),
          description: dto.description,
          notes: dto.notes ?? null,
          currencyId: BigInt(dto.currencyId),
          exchangeRate: new Prisma.Decimal(dto.exchangeRate),
          amount: new Prisma.Decimal(dto.amount),
          allocatedAmount: new Prisma.Decimal('0'),
          paymentStatus: 'UNPAID',
          status: 'DRAFT',
          postingStatus: 'UNPOSTED', // TODO: implement GL posting when ledger mapping for AP is ready
          createdById: actor,
          updatedById: actor,
        },
      });
    });

    return { success: true, data: await this.enrichOne(created) };
  }

  async findAll(query: QueryVendorAdvanceDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;

    const where: Prisma.ErpFinApPaymentWhereInput = {
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
      this.prisma.erpFinApPayment.findMany({ where, orderBy, skip, take: limit }),
      this.prisma.erpFinApPayment.count({ where }),
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

  async update(id: bigint, dto: UpdateVendorAdvanceDto, actorId?: string) {
    const existing = await this.findRaw(id);
    if (!EDITABLE.has(existing.status as string)) {
      throw new BadRequestException(
        `Dokumen berstatus ${existing.status} tidak bisa diedit. Reopen dulu bila perlu.`,
      );
    }

    const data: Prisma.ErpFinApPaymentUpdateInput = {
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

    const updated = await this.prisma.erpFinApPayment.update({ where: { id }, data });
    return { success: true, data: await this.enrichOne(updated) };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if ((existing.status as string) === 'POSTED') {
      throw new BadRequestException('Dokumen POSTED tidak bisa dihapus. Reopen dulu.');
    }
    await this.prisma.erpFinApPayment.update({
      where: { id },
      data: {
        deletedAt: new Date(),
        updatedById: actorId ? BigInt(actorId) : null,
      },
    });
    return { success: true, message: 'Vendor advance dihapus' };
  }

  // ---------------------------------------------------------------------------
  // Workflow transition
  // ---------------------------------------------------------------------------

  async transition(id: bigint, dto: TransitionVendorAdvanceDto, actorId?: string) {
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

    // TODO: implement GL posting when ledger mapping for AP Vendor Advance is ready.
    const data: Prisma.ErpFinApPaymentUpdateInput = {
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

    const updated = await this.prisma.erpFinApPayment.update({ where: { id }, data });
    return { success: true, data: await this.enrichOne(updated) };
  }
}
