import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { CreatePurBidSelectionDto } from './dto/create-pur-bid-selection.dto';
import { QueryPurBidSelectionsDto } from './dto/query-pur-bid-selections.dto';
import { UpdatePurBidSelectionDto } from './dto/update-pur-bid-selection.dto';
import { PurBidSelectionTransitionAction as A, TransitionPurBidSelectionDto } from './dto/transition-pur-bid-selection.dto';

const DOC_CODE = 'BS';
const EDITABLE = new Set(['DRAFT', 'NEED_APPROVE', 'REJECTED']);
const NEXT: Record<string, Partial<Record<A, string>>> = {
  DRAFT: { [A.SUBMIT]: 'NEED_APPROVE' }, REJECTED: { [A.SUBMIT]: 'NEED_APPROVE' },
  NEED_APPROVE: { [A.APPROVE]: 'APPROVED', [A.REJECT]: 'REJECTED' },
  APPROVED: { [A.POST]: 'POSTED', [A.REOPEN]: 'DRAFT' },
};

function bi(v?: string | null): bigint | null { return (v === undefined || v === null || v === '') ? null : BigInt(v); }

@Injectable()
export class ErpPurBidSelectionsService {
  constructor(private readonly prisma: PrismaService) {}

  private async resolvePeriod(tx: Prisma.TransactionClient, fpId: string | undefined, date: string): Promise<bigint> {
    if (fpId) return BigInt(fpId);
    const p = await tx.erpFiscalPeriod.findFirst({ where: { deletedAt: null, startDate: { lte: new Date(date) }, endDate: { gte: new Date(date) } }, select: { id: true } });
    if (!p) throw new BadRequestException(`Tidak ada periode fiskal yang memuat tanggal ${date}.`);
    return p.id;
  }

  private async genDocNumber(tx: Prisma.TransactionClient): Promise<string> {
    const n = await tx.erpDocumentNumbering.findFirst({ where: { documentCode: DOC_CODE, deletedAt: null } });
    if (n) { await tx.erpDocumentNumbering.update({ where: { id: n.id }, data: { nextNumber: n.nextNumber + 1 } }); return `${n.prefix}${String(n.nextNumber).padStart(n.digitCount, '0')}`; }
    return `BS${String((await tx.erpPurBidSelection.count()) + 1).padStart(6, '0')}`;
  }

  private async findRaw(id: bigint) {
    const bs = await this.prisma.erpPurBidSelection.findFirst({ where: { id, deletedAt: null }, include: { lines: { orderBy: { lineNo: 'asc' } } } });
    if (!bs) throw new NotFoundException('Bid selection tidak ditemukan');
    return bs;
  }

  private async one(id: bigint) { return { success: true, data: await this.findRaw(id) }; }

  async create(dto: CreatePurBidSelectionDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;
    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.docDate);
      const wantAuto = dto.auto !== false && !dto.docNumber;
      const docNumber = wantAuto ? await this.genDocNumber(tx) : dto.docNumber;
      if (!docNumber) throw new BadRequestException('No dokumen wajib diisi.');
      // BS header fields: no price-related cols (it's a comparison doc, no monetary totals).
      // We fill required not-null fields with sensible defaults (0 / IDR).
      const currency = await tx.erpCurrency.findFirst({ where: { code: 'IDR' } }) ?? await tx.erpCurrency.findFirst({});
      if (!currency) throw new BadRequestException('Tidak ada mata uang IDR terdaftar.');
      const row = await tx.erpPurBidSelection.create({
        data: {
          docNumber, autoNumber: wantAuto ? docNumber : null,
          branchId: BigInt(dto.branchId), locationId: bi(dto.locationId), warehouseId: null,
          docDate: new Date(dto.docDate), fiscalPeriodId,
          currencyId: currency.id, exchangeRate: new Prisma.Decimal(1),
          priceMode: 'TAX_EXCLUSIVE', subtotal: new Prisma.Decimal(0), grandTotal: new Prisma.Decimal(0),
          description: dto.description ?? null, notes: dto.notes ?? null, referenceNo: dto.referenceNo ?? null,
          status: 'DRAFT', postingStatus: 'UNPOSTED', legacyCode: dto.legacyCode ?? null,
          createdById: actor, updatedById: actor,
          lines: dto.lines.length ? {
            create: dto.lines.map((l) => ({
              quotationLineId: BigInt(l.quotationLineId),
              selected: l.selected ?? false,
              priceRank: l.priceRank,
              notes: l.notes ?? null,
              lineNo: l.lineNo,
            })),
          } : undefined,
        },
        select: { id: true },
      });
      return row;
    });
    return this.one(created.id);
  }

  async findAll(query: QueryPurBidSelectionsDto) {
    const page = query.page ?? 1; const limit = query.limit ?? 10;
    const where: Prisma.ErpPurBidSelectionWhereInput = { deletedAt: null };
    if (query.status) where.status = query.status as never;
    if (query.branchId) where.branchId = BigInt(query.branchId);
    if (query.createdById) where.createdById = BigInt(query.createdById);
    if (query.dateFrom || query.dateTo) where.docDate = { ...(query.dateFrom ? { gte: new Date(query.dateFrom) } : {}), ...(query.dateTo ? { lte: new Date(query.dateTo) } : {}) };
    if (query.search?.trim()) { const q = query.search.trim(); where.OR = [{ docNumber: { contains: q, mode: 'insensitive' } }, { description: { contains: q, mode: 'insensitive' } }]; }
    const sortBy = query.sortBy ?? 'docDate'; const sortDir = query.sortDir ?? 'desc';
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpPurBidSelection.findMany({ where, orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }], skip: (page - 1) * limit, take: limit, include: { lines: { orderBy: { lineNo: 'asc' } } } }),
      this.prisma.erpPurBidSelection.count({ where }),
    ]);
    return { success: true, data: items, meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 } };
  }

  findOne(id: bigint) { return this.one(id); }

  async update(id: bigint, dto: UpdatePurBidSelectionDto, actorId?: string) {
    const existing = await this.findRaw(id);
    if (!EDITABLE.has(existing.status)) throw new BadRequestException(`Dokumen berstatus ${existing.status} tidak bisa diedit.`);
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.$transaction(async (tx) => {
      const data: Prisma.ErpPurBidSelectionUpdateInput = { updatedById: actor };
      if (dto.docNumber !== undefined) data.docNumber = dto.docNumber;
      if (dto.branchId !== undefined) data.branchId = BigInt(dto.branchId);
      if (dto.locationId !== undefined) data.locationId = bi(dto.locationId);
      if (dto.description !== undefined) data.description = dto.description;
      if (dto.notes !== undefined) data.notes = dto.notes;
      if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;
      if (dto.docDate !== undefined) { data.docDate = new Date(dto.docDate); data.fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.docDate); }
      else if (dto.fiscalPeriodId !== undefined) data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);
      if (dto.lines !== undefined) {
        await tx.erpPurBidSelectionLine.deleteMany({ where: { bidSelectionId: id } });
        data.lines = { create: dto.lines.map((l) => ({ quotationLineId: BigInt(l.quotationLineId), selected: l.selected ?? false, priceRank: l.priceRank, notes: l.notes ?? null, lineNo: l.lineNo })) };
      }
      await tx.erpPurBidSelection.update({ where: { id }, data });
    });
    return this.one(id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status === 'POSTED') throw new BadRequestException('Dokumen POSTED tidak bisa dihapus.');
    await this.prisma.erpPurBidSelection.update({ where: { id }, data: { deletedAt: new Date(), updatedById: actorId ? BigInt(actorId) : null } });
    return { success: true, message: 'Bid selection dihapus' };
  }

  async transition(id: bigint, dto: TransitionPurBidSelectionDto, actorId?: string) {
    const bs = await this.findRaw(id);
    const actor = actorId ? BigInt(actorId) : null;
    const next = NEXT[bs.status]?.[dto.action];
    if (!next) throw new BadRequestException(`Aksi ${dto.action} tidak valid dari status ${bs.status}.`);
    if (dto.action === A.REJECT && !dto.reason?.trim()) throw new BadRequestException('Alasan reject wajib diisi.');
    const base = { previousStatus: bs.status as never, updatedById: actor };
    if (dto.action === A.POST) {
      await this.prisma.erpPurBidSelection.update({ where: { id }, data: { ...base, status: 'POSTED' as never, postingStatus: 'POSTED' as never, postedAt: new Date() } });
    } else if (dto.action === A.REOPEN) {
      await this.prisma.erpPurBidSelection.update({ where: { id }, data: { ...base, status: 'DRAFT' as never, postingStatus: 'UNPOSTED' as never, postedAt: null } });
    } else {
      await this.prisma.erpPurBidSelection.update({ where: { id }, data: { ...base, status: next as never, ...(dto.action === A.REJECT ? { metadata: { ...((bs.metadata as object) ?? {}), rejectReason: dto.reason } } : {}) } });
    }
    return this.one(id);
  }
}
