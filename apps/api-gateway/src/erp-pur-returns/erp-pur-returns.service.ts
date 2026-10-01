import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { PurReturnPostingService } from './pur-return-posting.service';
import { enrichReturns } from './pur-return-enrich';
import { CreatePurReturnDto } from './dto/create-pur-return.dto';
import { QueryPurReturnsDto } from './dto/query-pur-returns.dto';
import { UpdatePurReturnDto } from './dto/update-pur-return.dto';
import {
  PurReturnTransitionAction as A,
  TransitionPurReturnDto,
} from './dto/transition-pur-return.dto';
import {
  toBigInt,
  EDITABLE,
  NEXT,
  buildPurReturnWhere,
  mapReturnLine,
  computeTotals,
} from './pur-return.helpers';

/** Doc numbering differs per returnType: DEBIT_NOTE → DNR, RETURN_TO_VENDOR → PRT. */
const DOC_CODE: Record<string, string> = { DEBIT_NOTE: 'DNR', RETURN_TO_VENDOR: 'PRT' };

@Injectable()
export class ErpPurReturnsService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly posting: PurReturnPostingService,
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

  private async genDocNumber(tx: Prisma.TransactionClient, returnType: string): Promise<string> {
    const code = DOC_CODE[returnType] ?? 'DNR';
    const numbering = await tx.erpDocumentNumbering.findFirst({
      where: { documentCode: code, deletedAt: null },
    });
    if (numbering) {
      const seq = numbering.nextNumber;
      await tx.erpDocumentNumbering.update({
        where: { id: numbering.id },
        data: { nextNumber: seq + 1 },
      });
      return `${numbering.prefix}${String(seq).padStart(numbering.digitCount, '0')}`;
    }
    const count = await tx.erpPurReturn.count({ where: { returnType: returnType as never } });
    return `${code}${String(count + 1).padStart(6, '0')}`;
  }

  private async findRaw(id: bigint) {
    const purReturn = await this.prisma.erpPurReturn.findFirst({
      where: { id, deletedAt: null },
      include: { lines: { orderBy: { lineNo: 'asc' } } },
    });
    if (!purReturn) throw new NotFoundException('Purchase return tidak ditemukan');
    return purReturn;
  }

  private async one(id: bigint) {
    const purReturn = await this.findRaw(id);
    const [enriched] = await enrichReturns(this.prisma, [purReturn]);
    return { success: true, data: enriched };
  }

  // ── CRUD ────────────────────────────────────────────────────────────────────
  async create(dto: CreatePurReturnDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;
    const header = { currencyId: dto.currencyId, exchangeRate: dto.exchangeRate };
    const { subtotal, grandTotal } = computeTotals(dto.lines, dto);

    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.docDate);
      const wantAuto = dto.auto !== false && !dto.docNumber;
      const docNumber = wantAuto ? await this.genDocNumber(tx, dto.returnType) : dto.docNumber;
      if (!docNumber) throw new BadRequestException('No dokumen wajib diisi.');

      const row = await tx.erpPurReturn.create({
        data: {
          docNumber,
          autoNumber: wantAuto ? docNumber : null,
          returnType: dto.returnType as never,
          branchId: BigInt(dto.branchId),
          locationId: toBigInt(dto.locationId),
          warehouseId: toBigInt(dto.warehouseId),
          docDate: new Date(dto.docDate),
          fiscalPeriodId,
          supplierId: toBigInt(dto.supplierId),
          paymentTermId: toBigInt(dto.paymentTermId),
          dueDate: dto.dueDate ? new Date(dto.dueDate) : null,
          currencyId: BigInt(dto.currencyId),
          exchangeRate: new Prisma.Decimal(dto.exchangeRate),
          priceMode: (dto.priceMode ?? 'TAX_EXCLUSIVE') as never,
          subtotal,
          discountPercent: dto.discountPercent != null ? new Prisma.Decimal(dto.discountPercent) : null,
          discountAmount: dto.discountAmount != null ? new Prisma.Decimal(dto.discountAmount) : null,
          tax1Amount: dto.tax1Amount != null ? new Prisma.Decimal(dto.tax1Amount) : null,
          tax2Amount: dto.tax2Amount != null ? new Prisma.Decimal(dto.tax2Amount) : null,
          otherCostAmount: dto.otherCostAmount != null ? new Prisma.Decimal(dto.otherCostAmount) : null,
          grandTotal,
          description: dto.description ?? null,
          notes: dto.notes ?? null,
          referenceNo: dto.referenceNo ?? null,
          referenceDate: dto.referenceDate ? new Date(dto.referenceDate) : null,
          payableAccountId: toBigInt(dto.payableAccountId),
          settlementStatus: 'UNPAID' as never,
          taxInvoiceNo: dto.taxInvoiceNo ?? null,
          orderId: toBigInt(dto.orderId),
          goodsReceiptId: toBigInt(dto.goodsReceiptId),
          invoiceId: toBigInt(dto.invoiceId),
          status: 'DRAFT',
          postingStatus: 'UNPOSTED',
          legacyCode: dto.legacyCode ?? null,
          createdById: actor,
          updatedById: actor,
          lines: dto.lines.length
            ? { create: dto.lines.map((l) => mapReturnLine(l, header)) }
            : undefined,
        },
        select: { id: true },
      });
      return row;
    });
    return this.one(created.id);
  }

  async findAll(query: QueryPurReturnsDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const where = buildPurReturnWhere(query);

    const sortBy = query.sortBy ?? 'docDate';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpPurReturn.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }],
        skip: (page - 1) * limit,
        take: limit,
        include: { lines: { orderBy: { lineNo: 'asc' } } },
      }),
      this.prisma.erpPurReturn.count({ where }),
    ]);

    return {
      success: true,
      data: await enrichReturns(this.prisma, items),
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  findOne(id: bigint) {
    return this.one(id);
  }

  async update(id: bigint, dto: UpdatePurReturnDto, actorId?: string) {
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

    await this.prisma.$transaction(async (tx) => {
      const data: Prisma.ErpPurReturnUpdateInput = { updatedById: actor };
      if (dto.returnType !== undefined) data.returnType = dto.returnType as never;
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
      if (dto.referenceDate !== undefined) {
        data.referenceDate = dto.referenceDate ? new Date(dto.referenceDate) : null;
      }
      if (dto.payableAccountId !== undefined) data.payableAccountId = toBigInt(dto.payableAccountId);
      if (dto.taxInvoiceNo !== undefined) data.taxInvoiceNo = dto.taxInvoiceNo;
      // Relation FKs use connect/disconnect (Prisma relation fields, not scalar setters).
      if (dto.orderId !== undefined) {
        const oid = toBigInt(dto.orderId);
        data.order = oid ? { connect: { id: oid } } : { disconnect: true };
      }
      if (dto.goodsReceiptId !== undefined) {
        const gid = toBigInt(dto.goodsReceiptId);
        data.goodsReceipt = gid ? { connect: { id: gid } } : { disconnect: true };
      }
      if (dto.invoiceId !== undefined) {
        const iid = toBigInt(dto.invoiceId);
        data.invoice = iid ? { connect: { id: iid } } : { disconnect: true };
      }
      if (dto.discountPercent !== undefined) {
        data.discountPercent = dto.discountPercent != null ? new Prisma.Decimal(dto.discountPercent) : null;
      }
      if (dto.legacyCode !== undefined) data.legacyCode = dto.legacyCode;
      if (dto.docDate !== undefined) {
        data.docDate = new Date(dto.docDate);
        data.fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.docDate);
      } else if (dto.fiscalPeriodId !== undefined) {
        data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);
      }

      // Header money fields participate in totals — recompute against merged values.
      const discountAmount =
        dto.discountAmount !== undefined ? dto.discountAmount : existing.discountAmount?.toString();
      const tax1Amount =
        dto.tax1Amount !== undefined ? dto.tax1Amount : existing.tax1Amount?.toString();
      const tax2Amount =
        dto.tax2Amount !== undefined ? dto.tax2Amount : existing.tax2Amount?.toString();
      const otherCostAmount =
        dto.otherCostAmount !== undefined
          ? dto.otherCostAmount
          : existing.otherCostAmount?.toString();
      if (dto.discountAmount !== undefined) {
        data.discountAmount = dto.discountAmount != null ? new Prisma.Decimal(dto.discountAmount) : null;
      }
      if (dto.tax1Amount !== undefined) {
        data.tax1Amount = dto.tax1Amount != null ? new Prisma.Decimal(dto.tax1Amount) : null;
      }
      if (dto.tax2Amount !== undefined) {
        data.tax2Amount = dto.tax2Amount != null ? new Prisma.Decimal(dto.tax2Amount) : null;
      }
      if (dto.otherCostAmount !== undefined) {
        data.otherCostAmount = dto.otherCostAmount != null ? new Prisma.Decimal(dto.otherCostAmount) : null;
      }

      const lines = dto.lines ?? existing.lines.map((l) => ({
        itemId: l.itemId.toString(),
        quantity: l.quantity.toString(),
        unitId: l.unitId.toString(),
        unitPrice: l.unitPrice.toString(),
        discountPercent: l.discountPercent?.toString(),
        discountAmount: l.discountAmount?.toString(),
        tax1Amount: l.tax1Amount?.toString(),
        tax2Amount: l.tax2Amount?.toString(),
        lineNo: l.lineNo,
      }));
      const { subtotal, grandTotal } = computeTotals(lines as never, {
        discountAmount,
        tax1Amount,
        tax2Amount,
        otherCostAmount,
      });
      data.subtotal = subtotal;
      data.grandTotal = grandTotal;

      if (dto.lines !== undefined) {
        await tx.erpPurReturnLine.deleteMany({ where: { returnId: id } });
        data.lines = { create: dto.lines.map((l) => mapReturnLine(l, header)) };
      }
      await tx.erpPurReturn.update({ where: { id }, data });
    });
    return this.one(id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status === 'POSTED') {
      throw new BadRequestException('Dokumen POSTED tidak bisa dihapus. Reopen dulu.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.erpPurReturn.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actor },
    });
    return { success: true, message: 'Purchase return dihapus' };
  }

  // ── workflow (§2.7 state machine) ────────────────────────────────────────────
  async transition(id: bigint, dto: TransitionPurReturnDto, actorId?: string) {
    const purReturn = await this.findRaw(id);
    const actor = actorId ? BigInt(actorId) : null;
    const next = NEXT[purReturn.status]?.[dto.action];
    if (!next) {
      throw new BadRequestException(`Aksi ${dto.action} tidak valid dari status ${purReturn.status}.`);
    }
    if (dto.action === A.REJECT && !dto.reason?.trim()) {
      throw new BadRequestException('Alasan reject wajib diisi.');
    }

    if (dto.action === A.POST) {
      const period = await this.prisma.erpFiscalPeriod.findUnique({
        where: { id: purReturn.fiscalPeriodId },
        select: { status: true },
      });
      if (period?.status === 'CLOSED') {
        throw new BadRequestException('Periode fiskal sudah ditutup — tidak bisa posting.');
      }
      await this.prisma.$transaction(async (tx) => {
        await this.posting.reverseLedger(tx, purReturn.id);
        await this.posting.postToLedger(tx, purReturn, actor);
        await tx.erpPurReturn.update({
          where: { id },
          data: {
            status: 'POSTED',
            previousStatus: purReturn.status as never,
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
        await this.posting.reverseLedger(tx, purReturn.id);
        await tx.erpPurReturn.update({
          where: { id },
          data: {
            status: 'DRAFT',
            previousStatus: purReturn.status as never,
            postingStatus: 'UNPOSTED',
            postedAt: null,
            updatedById: actor,
          },
        });
      });
      return this.one(id);
    }

    await this.prisma.erpPurReturn.update({
      where: { id },
      data: {
        status: next as never,
        previousStatus: purReturn.status as never,
        updatedById: actor,
        ...(dto.action === A.REJECT
          ? {
              metadata: {
                ...((purReturn.metadata as object) ?? {}),
                rejectReason: dto.reason,
              },
            }
          : {}),
      },
    });
    return this.one(id);
  }
}
