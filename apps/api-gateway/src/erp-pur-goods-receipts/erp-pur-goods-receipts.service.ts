import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { PurGoodsReceiptPostingService } from './pur-goods-receipt-posting.service';
import { enrichGoodsReceipts } from './pur-goods-receipt-enrich';
import { CreatePurGoodsReceiptDto } from './dto/create-pur-goods-receipt.dto';
import { QueryPurGoodsReceiptsDto } from './dto/query-pur-goods-receipts.dto';
import { UpdatePurGoodsReceiptDto } from './dto/update-pur-goods-receipt.dto';
import { PurGoodsReceiptTransitionAction as A, TransitionPurGoodsReceiptDto } from './dto/transition-pur-goods-receipt.dto';
import { toBigInt, EDITABLE, NEXT, buildPurGrnWhere, mapGrnLine, computeTotals } from './pur-goods-receipt.helpers';

const DOC_CODE = 'GRN';
const FALLBACK_PREFIX = 'GRN';

@Injectable()
export class ErpPurGoodsReceiptsService {
  constructor(private readonly prisma: PrismaService, private readonly posting: PurGoodsReceiptPostingService) {}

  private async resolvePeriod(tx: Prisma.TransactionClient, fiscalPeriodId: string | undefined, date: string): Promise<bigint> {
    if (fiscalPeriodId) return BigInt(fiscalPeriodId);
    const period = await tx.erpFiscalPeriod.findFirst({ where: { deletedAt: null, startDate: { lte: new Date(date) }, endDate: { gte: new Date(date) } }, select: { id: true } });
    if (!period) throw new BadRequestException(`Tidak ada periode fiskal yang memuat tanggal ${date}.`);
    return period.id;
  }

  private async genDocNumber(tx: Prisma.TransactionClient): Promise<string> {
    const n = await tx.erpDocumentNumbering.findFirst({ where: { documentCode: DOC_CODE, deletedAt: null } });
    if (n) {
      await tx.erpDocumentNumbering.update({ where: { id: n.id }, data: { nextNumber: n.nextNumber + 1 } });
      return `${n.prefix}${String(n.nextNumber).padStart(n.digitCount, '0')}`;
    }
    const count = await tx.erpPurGoodsReceipt.count();
    return `${FALLBACK_PREFIX}${String(count + 1).padStart(6, '0')}`;
  }

  private async findRaw(id: bigint) {
    const grn = await this.prisma.erpPurGoodsReceipt.findFirst({ where: { id, deletedAt: null }, include: { lines: { orderBy: { lineNo: 'asc' } } } });
    if (!grn) throw new NotFoundException('Goods receipt tidak ditemukan');
    return grn;
  }

  private async one(id: bigint) {
    const grn = await this.findRaw(id);
    const [enriched] = await enrichGoodsReceipts(this.prisma, [grn]);
    return { success: true, data: enriched };
  }

  async create(dto: CreatePurGoodsReceiptDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;
    const header = { currencyId: dto.currencyId, exchangeRate: dto.exchangeRate };
    const { subtotal, grandTotal } = computeTotals(dto.lines, dto);

    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.docDate);
      const wantAuto = dto.auto !== false && !dto.docNumber;
      const docNumber = wantAuto ? await this.genDocNumber(tx) : dto.docNumber;
      if (!docNumber) throw new BadRequestException('No dokumen wajib diisi.');

      const row = await tx.erpPurGoodsReceipt.create({
        data: {
          docNumber, autoNumber: wantAuto ? docNumber : null,
          branchId: BigInt(dto.branchId), locationId: toBigInt(dto.locationId),
          warehouseId: toBigInt(dto.warehouseId), docDate: new Date(dto.docDate), fiscalPeriodId,
          supplierId: toBigInt(dto.supplierId), paymentTermId: toBigInt(dto.paymentTermId),
          dueDate: dto.dueDate ? new Date(dto.dueDate) : null,
          currencyId: BigInt(dto.currencyId), exchangeRate: new Prisma.Decimal(dto.exchangeRate),
          priceMode: (dto.priceMode ?? 'TAX_EXCLUSIVE') as never, subtotal, grandTotal,
          discountPercent: dto.discountPercent != null ? new Prisma.Decimal(dto.discountPercent) : null,
          discountAmount: dto.discountAmount != null ? new Prisma.Decimal(dto.discountAmount) : null,
          tax1Amount: dto.tax1Amount != null ? new Prisma.Decimal(dto.tax1Amount) : null,
          tax2Amount: dto.tax2Amount != null ? new Prisma.Decimal(dto.tax2Amount) : null,
          otherCostAmount: dto.otherCostAmount != null ? new Prisma.Decimal(dto.otherCostAmount) : null,
          description: dto.description ?? null, notes: dto.notes ?? null,
          referenceNo: dto.referenceNo ?? null, referenceDate: dto.referenceDate ? new Date(dto.referenceDate) : null,
          payableAccountId: toBigInt(dto.payableAccountId),
          orderId: toBigInt(dto.orderId),
          status: 'DRAFT', postingStatus: 'UNPOSTED', legacyCode: dto.legacyCode ?? null,
          createdById: actor, updatedById: actor,
          lines: dto.lines.length ? { create: dto.lines.map((l) => mapGrnLine(l, header)) } : undefined,
        },
        select: { id: true },
      });
      return row;
    });
    return this.one(created.id);
  }

  async findAll(query: QueryPurGoodsReceiptsDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const where = buildPurGrnWhere(query);
    const sortBy = query.sortBy ?? 'docDate';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpPurGoodsReceipt.findMany({ where, orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }], skip: (page - 1) * limit, take: limit, include: { lines: { orderBy: { lineNo: 'asc' } } } }),
      this.prisma.erpPurGoodsReceipt.count({ where }),
    ]);
    return { success: true, data: await enrichGoodsReceipts(this.prisma, items), meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 } };
  }

  findOne(id: bigint) { return this.one(id); }

  async update(id: bigint, dto: UpdatePurGoodsReceiptDto, actorId?: string) {
    const existing = await this.findRaw(id);
    if (!EDITABLE.has(existing.status)) throw new BadRequestException(`Dokumen berstatus ${existing.status} tidak bisa diedit.`);
    const actor = actorId ? BigInt(actorId) : null;
    const header = { currencyId: dto.currencyId ?? existing.currencyId.toString(), exchangeRate: dto.exchangeRate ?? existing.exchangeRate.toString() };

    await this.prisma.$transaction(async (tx) => {
      const data: Prisma.ErpPurGoodsReceiptUpdateInput = { updatedById: actor };
      if (dto.docNumber !== undefined) data.docNumber = dto.docNumber;
      if (dto.branchId !== undefined) data.branchId = BigInt(dto.branchId);
      if (dto.locationId !== undefined) data.locationId = toBigInt(dto.locationId);
      if (dto.warehouseId !== undefined) data.warehouseId = toBigInt(dto.warehouseId);
      if (dto.supplierId !== undefined) data.supplierId = toBigInt(dto.supplierId);
      if (dto.paymentTermId !== undefined) data.paymentTermId = toBigInt(dto.paymentTermId);
      if (dto.dueDate !== undefined) data.dueDate = dto.dueDate ? new Date(dto.dueDate) : null;
      if (dto.currencyId !== undefined) data.currencyId = BigInt(dto.currencyId);
      if (dto.exchangeRate !== undefined) data.exchangeRate = new Prisma.Decimal(dto.exchangeRate);
      if (dto.priceMode !== undefined) data.priceMode = dto.priceMode as never;
      if (dto.description !== undefined) data.description = dto.description;
      if (dto.notes !== undefined) data.notes = dto.notes;
      if (dto.referenceNo !== undefined) data.referenceNo = dto.referenceNo;
      if (dto.referenceDate !== undefined) data.referenceDate = dto.referenceDate ? new Date(dto.referenceDate) : null;
      if (dto.payableAccountId !== undefined) data.payableAccountId = toBigInt(dto.payableAccountId);
      // orderId has a @relation in the schema — use connect/disconnect.
      if (dto.orderId !== undefined) {
        const oid = toBigInt(dto.orderId);
        data.order = oid ? { connect: { id: oid } } : { disconnect: true };
      }
      if (dto.discountPercent !== undefined) data.discountPercent = dto.discountPercent != null ? new Prisma.Decimal(dto.discountPercent) : null;
      if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;
      if (dto.docDate !== undefined) { data.docDate = new Date(dto.docDate); data.fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.docDate); }
      else if (dto.fiscalPeriodId !== undefined) data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);

      const discountAmount = dto.discountAmount !== undefined ? dto.discountAmount : existing.discountAmount?.toString();
      const tax1Amount = dto.tax1Amount !== undefined ? dto.tax1Amount : existing.tax1Amount?.toString();
      const tax2Amount = dto.tax2Amount !== undefined ? dto.tax2Amount : existing.tax2Amount?.toString();
      const otherCostAmount = dto.otherCostAmount !== undefined ? dto.otherCostAmount : existing.otherCostAmount?.toString();
      if (dto.discountAmount !== undefined) data.discountAmount = dto.discountAmount != null ? new Prisma.Decimal(dto.discountAmount) : null;
      if (dto.tax1Amount !== undefined) data.tax1Amount = dto.tax1Amount != null ? new Prisma.Decimal(dto.tax1Amount) : null;
      if (dto.tax2Amount !== undefined) data.tax2Amount = dto.tax2Amount != null ? new Prisma.Decimal(dto.tax2Amount) : null;
      if (dto.otherCostAmount !== undefined) data.otherCostAmount = dto.otherCostAmount != null ? new Prisma.Decimal(dto.otherCostAmount) : null;

      const lines = dto.lines ?? existing.lines.map((l) => ({ itemId: l.itemId.toString(), quantity: l.quantity.toString(), unitId: l.unitId.toString(), unitPrice: l.unitPrice.toString(), lineNo: l.lineNo }));
      const { subtotal, grandTotal } = computeTotals(lines as never, { discountAmount, tax1Amount, tax2Amount, otherCostAmount });
      data.subtotal = subtotal; data.grandTotal = grandTotal;

      if (dto.lines !== undefined) {
        await tx.erpPurGoodsReceiptLine.deleteMany({ where: { goodsReceiptId: id } });
        data.lines = { create: dto.lines.map((l) => mapGrnLine(l, header)) };
      }
      await tx.erpPurGoodsReceipt.update({ where: { id }, data });
    });
    return this.one(id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status === 'POSTED') throw new BadRequestException('Dokumen POSTED tidak bisa dihapus.');
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.erpPurGoodsReceipt.update({ where: { id }, data: { deletedAt: new Date(), updatedById: actor } });
    return { success: true, message: 'Goods receipt dihapus' };
  }

  async transition(id: bigint, dto: TransitionPurGoodsReceiptDto, actorId?: string) {
    const grn = await this.findRaw(id);
    const actor = actorId ? BigInt(actorId) : null;
    const next = NEXT[grn.status]?.[dto.action];
    if (!next) throw new BadRequestException(`Aksi ${dto.action} tidak valid dari status ${grn.status}.`);
    if (dto.action === A.REJECT && !dto.reason?.trim()) throw new BadRequestException('Alasan reject wajib diisi.');

    if (dto.action === A.POST) {
      const period = await this.prisma.erpFiscalPeriod.findUnique({ where: { id: grn.fiscalPeriodId }, select: { status: true } });
      if (period?.status === 'CLOSED') throw new BadRequestException('Periode fiskal sudah ditutup.');
      await this.prisma.$transaction(async (tx) => {
        await this.posting.reverseLedger(tx, grn.id);
        await this.posting.postToLedger(tx, grn, actor);
        await tx.erpPurGoodsReceipt.update({ where: { id }, data: { status: 'POSTED', previousStatus: grn.status as never, postingStatus: 'POSTED', postedAt: new Date(), updatedById: actor } });
      });
      return this.one(id);
    }
    if (dto.action === A.REOPEN) {
      await this.prisma.$transaction(async (tx) => {
        await this.posting.reverseLedger(tx, grn.id);
        await tx.erpPurGoodsReceipt.update({ where: { id }, data: { status: 'DRAFT', previousStatus: grn.status as never, postingStatus: 'UNPOSTED', postedAt: null, updatedById: actor } });
      });
      return this.one(id);
    }
    await this.prisma.erpPurGoodsReceipt.update({ where: { id }, data: { status: next as never, previousStatus: grn.status as never, updatedById: actor, ...(dto.action === A.REJECT ? { metadata: { ...((grn.metadata as object) ?? {}), rejectReason: dto.reason } } : {}) } });
    return this.one(id);
  }
}
