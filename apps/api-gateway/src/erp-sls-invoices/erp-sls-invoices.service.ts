import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { SlsInvoicePostingService } from './sls-invoice-posting.service';
import { enrichInvoices } from './sls-invoice-enrich';
import { CreateSlsInvoiceDto } from './dto/create-sls-invoice.dto';
import { QuerySlsInvoicesDto } from './dto/query-sls-invoices.dto';
import { UpdateSlsInvoiceDto } from './dto/update-sls-invoice.dto';
import {
  SlsInvoiceTransitionAction as A,
  TransitionSlsInvoiceDto,
} from './dto/transition-sls-invoice.dto';
import {
  EDITABLE,
  NEXT,
  buildSlsInvoiceWhere,
  mapInvoiceLine,
  computeInvoiceTotals,
} from './sls-invoice.helpers';
import {
  buildSlsInvoiceCreateData,
  buildSlsInvoiceUpdatePatch,
  mapExistingSlsInvoiceLines,
  buildSlsInvoiceTotalsInput,
} from './sls-invoice-persistence.mapper';

const DOC_CODE = 'SI';
const FALLBACK_PREFIX = 'SI';

@Injectable()
export class ErpSlsInvoicesService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly posting: SlsInvoicePostingService,
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
    const count = await tx.erpSlsInvoice.count();
    return `${FALLBACK_PREFIX}${String(count + 1).padStart(6, '0')}`;
  }

  private async findRaw(id: bigint) {
    const invoice = await this.prisma.erpSlsInvoice.findFirst({
      where: { id, deletedAt: null },
      include: { lines: { orderBy: { lineNo: 'asc' } } },
    });
    if (!invoice) throw new NotFoundException('Sales invoice tidak ditemukan');
    return invoice;
  }

  private async one(id: bigint) {
    const invoice = await this.findRaw(id);
    const [enriched] = await enrichInvoices(this.prisma, [invoice]);
    return { success: true, data: enriched };
  }

  private async taxRateMap(taxIds: (string | undefined)[]): Promise<Map<string, Prisma.Decimal>> {
    const ids = [...new Set(taxIds.filter((v): v is string => !!v))];
    if (!ids.length) return new Map();
    const taxes = await this.prisma.erpTax.findMany({
      where: { id: { in: ids.map(BigInt) }, deletedAt: null },
      select: { id: true, rate: true },
    });
    const m = new Map<string, Prisma.Decimal>();
    for (const t of taxes) m.set(t.id.toString(), t.rate);
    return m;
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
  async create(dto: CreateSlsInvoiceDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;
    const priceMode = (dto.priceMode ?? 'TAX_EXCLUSIVE') as 'TAX_EXCLUSIVE' | 'TAX_INCLUSIVE';
    const header = { currencyId: dto.currencyId, exchangeRate: dto.exchangeRate };

    const taxIds = dto.lines.flatMap((l) => [l.tax1Id, l.tax2Id]);
    const rateById = await this.taxRateMap(taxIds);
    const { subtotal, grandTotal, lines: computedLines, discountAmount, otherCostAmount } =
      computeInvoiceTotals(dto.lines, dto, rateById, priceMode);

    const dueDate = await this.resolveDueDate(dto.paymentTermId, dto.docDate, dto.dueDate);

    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.docDate);
      const wantAuto = dto.auto !== false && !dto.docNumber;
      const docNumber = wantAuto ? await this.genDocNumber(tx) : dto.docNumber;
      if (!docNumber) throw new BadRequestException('No dokumen wajib diisi.');

      const data = buildSlsInvoiceCreateData(dto, {
        docNumber,
        wantAuto,
        fiscalPeriodId,
        dueDate,
        actor,
        priceMode,
        header,
        subtotal,
        grandTotal,
        discountAmount,
        otherCostAmount,
        computedLines,
      });
      const row = await tx.erpSlsInvoice.create({ data, select: { id: true } });
      return row;
    });
    return this.one(created.id);
  }

  async findAll(query: QuerySlsInvoicesDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const where = buildSlsInvoiceWhere(query);

    const sortBy = query.sortBy ?? 'docDate';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total, agg] = await this.prisma.$transaction([
      this.prisma.erpSlsInvoice.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }],
        skip: (page - 1) * limit,
        take: limit,
        include: { lines: { orderBy: { lineNo: 'asc' } } },
      }),
      this.prisma.erpSlsInvoice.count({ where }),
      this.prisma.erpSlsInvoice.aggregate({ where, _sum: { grandTotal: true } }),
    ]);

    return {
      success: true,
      data: await enrichInvoices(this.prisma, items),
      meta: {
        page,
        limit,
        total,
        totalPages: Math.ceil(total / limit) || 1,
        sumGrandTotal: agg._sum.grandTotal?.toString() ?? '0',
      },
    };
  }

  findOne(id: bigint) {
    return this.one(id);
  }

  async update(id: bigint, dto: UpdateSlsInvoiceDto, actorId?: string) {
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
      const data = buildSlsInvoiceUpdatePatch(dto, actor);

      if (dto.docDate !== undefined) {
        data.docDate = new Date(dto.docDate);
        data.fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.docDate);
      } else if (dto.fiscalPeriodId !== undefined) {
        data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);
      }

      const mergedLines = dto.lines ?? mapExistingSlsInvoiceLines(existing.lines);

      const priceMode =
        ((dto.priceMode ?? existing.priceMode) as string) as 'TAX_EXCLUSIVE' | 'TAX_INCLUSIVE';
      const taxIds = mergedLines.flatMap((l) => [l.tax1Id, l.tax2Id]);
      const rateById = await this.taxRateMap(taxIds);
      const totals = computeInvoiceTotals(
        mergedLines,
        buildSlsInvoiceTotalsInput(dto, existing),
        rateById,
        priceMode,
      );
      data.subtotal = totals.subtotal;
      data.grandTotal = totals.grandTotal;
      if (totals.discountAmount !== null) data.discountAmount = totals.discountAmount;
      if (totals.otherCostAmount !== null) data.otherCostAmount = totals.otherCostAmount;

      if (dto.paymentTermId !== undefined && dto.dueDate === undefined) {
        const termId = dto.paymentTermId ?? null;
        const docDateStr = dto.docDate ?? existing.docDate.toISOString().slice(0, 10);
        data.dueDate = await this.resolveDueDate(termId ?? undefined, docDateStr, undefined);
      }

      if (dto.lines !== undefined) {
        await tx.erpSlsInvoiceLine.deleteMany({ where: { invoiceId: id } });
        data.lines = { create: totals.lines.map((l) => mapInvoiceLine(l, header)) };
      }
      await tx.erpSlsInvoice.update({ where: { id }, data });
    });
    return this.one(id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status === 'POSTED') {
      throw new BadRequestException('Dokumen POSTED tidak bisa dihapus. Reopen dulu.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.erpSlsInvoice.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actor },
    });
    return { success: true, message: 'Sales invoice dihapus' };
  }

  // ── workflow (§2.7 state machine) ────────────────────────────────────────────
  async transition(id: bigint, dto: TransitionSlsInvoiceDto, actorId?: string) {
    const invoice = await this.findRaw(id);
    const actor = actorId ? BigInt(actorId) : null;
    const next = NEXT[invoice.status]?.[dto.action];
    if (!next) {
      throw new BadRequestException(`Aksi ${dto.action} tidak valid dari status ${invoice.status}.`);
    }
    if (dto.action === A.REJECT && !dto.reason?.trim()) {
      throw new BadRequestException('Alasan reject wajib diisi.');
    }

    if (dto.action === A.POST) {
      const period = await this.prisma.erpFiscalPeriod.findUnique({
        where: { id: invoice.fiscalPeriodId },
        select: { status: true },
      });
      if (period?.status === 'CLOSED') {
        throw new BadRequestException('Periode fiskal sudah ditutup — tidak bisa posting.');
      }
      await this.prisma.$transaction(async (tx) => {
        await this.posting.reverseLedger(tx, invoice.id);
        // postToLedger validates lines; AR entry creation is noted in posting service
        await this.posting.postToLedger(tx, invoice, actor);
        await tx.erpSlsInvoice.update({
          where: { id },
          data: {
            status: 'POSTED',
            previousStatus: invoice.status as never,
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
        await this.posting.reverseLedger(tx, invoice.id);
        await tx.erpSlsInvoice.update({
          where: { id },
          data: {
            status: 'DRAFT',
            previousStatus: invoice.status as never,
            postingStatus: 'UNPOSTED',
            postedAt: null,
            updatedById: actor,
          },
        });
      });
      return this.one(id);
    }

    await this.prisma.erpSlsInvoice.update({
      where: { id },
      data: {
        status: next as never,
        previousStatus: invoice.status as never,
        updatedById: actor,
        ...(dto.action === A.REJECT
          ? {
              metadata: {
                ...((invoice.metadata as object) ?? {}),
                rejectReason: dto.reason,
              },
            }
          : {}),
      },
    });
    return this.one(id);
  }
}
