import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { SlsProformaInvoicePostingService } from './sls-proforma-invoice-posting.service';
import { enrichProformaInvoices } from './sls-proforma-invoice-enrich';
import { CreateSlsProformaInvoiceDto } from './dto/create-sls-proforma-invoice.dto';
import { QuerySlsProformaInvoicesDto } from './dto/query-sls-proforma-invoices.dto';
import { UpdateSlsProformaInvoiceDto } from './dto/update-sls-proforma-invoice.dto';
import {
  SlsProformaInvoiceTransitionAction as A,
  TransitionSlsProformaInvoiceDto,
} from './dto/transition-sls-proforma-invoice.dto';
import {
  toBigInt,
  EDITABLE,
  NEXT,
  buildSlsProformaInvoiceWhere,
  mapProformaInvoiceLine,
  computeOrderTotals,
} from './sls-proforma-invoice.helpers';
import {
  buildSlsProformaInvoiceCreateData,
  buildSlsProformaInvoiceUpdatePatch,
  mapExistingSlsProformaInvoiceLines,
  buildSlsProformaInvoiceTotalsInput,
} from './sls-proforma-invoice-persistence.mapper';

const DOC_CODE = 'PI';
const FALLBACK_PREFIX = 'PI';

@Injectable()
export class ErpSlsProformaInvoicesService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly posting: SlsProformaInvoicePostingService,
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
    const count = await tx.erpSlsProformaInvoice.count();
    return `${FALLBACK_PREFIX}${String(count + 1).padStart(6, '0')}`;
  }

  private async findRaw(id: bigint) {
    const proformaInvoice = await this.prisma.erpSlsProformaInvoice.findFirst({
      where: { id, deletedAt: null },
      include: { lines: { orderBy: { lineNo: 'asc' } } },
    });
    if (!proformaInvoice) throw new NotFoundException('Proforma invoice tidak ditemukan');
    return proformaInvoice;
  }

  private async one(id: bigint) {
    const proformaInvoice = await this.findRaw(id);
    const [enriched] = await enrichProformaInvoices(this.prisma, [proformaInvoice]);
    return { success: true, data: enriched };
  }

  /** Fetch rate map for all tax ids referenced by a line set. */
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

  /** Derive dueDate from payment term netDays if caller did not supply it. */
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
  async create(dto: CreateSlsProformaInvoiceDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;
    const priceMode = (dto.priceMode ?? 'TAX_EXCLUSIVE') as 'TAX_EXCLUSIVE' | 'TAX_INCLUSIVE';
    const header = { currencyId: dto.currencyId, exchangeRate: dto.exchangeRate };

    const taxIds = dto.lines.flatMap((l) => [l.tax1Id, l.tax2Id]);
    const rateById = await this.taxRateMap(taxIds);
    const totals = computeOrderTotals(dto.lines, dto, rateById, priceMode);

    const dueDate = await this.resolveDueDate(dto.paymentTermId, dto.docDate, dto.dueDate);

    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.docDate);
      const wantAuto = dto.auto !== false && !dto.docNumber;
      const docNumber = wantAuto ? await this.genDocNumber(tx) : dto.docNumber;
      if (!docNumber) throw new BadRequestException('No dokumen wajib diisi.');

      const data = buildSlsProformaInvoiceCreateData(dto, {
        docNumber,
        wantAuto,
        fiscalPeriodId,
        actor,
        priceMode,
        dueDate,
        header,
        totals,
      });
      const row = await tx.erpSlsProformaInvoice.create({ data, select: { id: true } });
      return row;
    });
    return this.one(created.id);
  }

  async findAll(query: QuerySlsProformaInvoicesDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const where = buildSlsProformaInvoiceWhere(query);

    const sortBy = query.sortBy ?? 'docDate';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total, agg] = await this.prisma.$transaction([
      this.prisma.erpSlsProformaInvoice.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }],
        skip: (page - 1) * limit,
        take: limit,
        include: { lines: { orderBy: { lineNo: 'asc' } } },
      }),
      this.prisma.erpSlsProformaInvoice.count({ where }),
      this.prisma.erpSlsProformaInvoice.aggregate({ where, _sum: { grandTotal: true } }),
    ]);

    return {
      success: true,
      data: await enrichProformaInvoices(this.prisma, items),
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

  async update(id: bigint, dto: UpdateSlsProformaInvoiceDto, actorId?: string) {
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
      const data = buildSlsProformaInvoiceUpdatePatch(dto, actor);

      // tax1/tax2 explicit amount fields (kept here — they feed totals recompute)
      if (dto.tax1Amount !== undefined) {
        data.tax1Amount = dto.tax1Amount != null ? new Prisma.Decimal(dto.tax1Amount) : null;
      }
      if (dto.tax2Amount !== undefined) {
        data.tax2Amount = dto.tax2Amount != null ? new Prisma.Decimal(dto.tax2Amount) : null;
      }

      // fiscal period (async — needs tx; cannot live in the pure mapper)
      if (dto.docDate !== undefined) {
        data.docDate = new Date(dto.docDate);
        data.fiscalPeriodId = await this.resolvePeriod(tx, dto.fiscalPeriodId, dto.docDate);
      } else if (dto.fiscalPeriodId !== undefined) {
        data.fiscalPeriodId = BigInt(dto.fiscalPeriodId);
      }

      // ── totals recompute ────────────────────────────────────────────────────
      const totalsInput = buildSlsProformaInvoiceTotalsInput(dto, existing);
      const mergedLines: typeof dto.lines =
        dto.lines ?? mapExistingSlsProformaInvoiceLines(existing.lines);

      const priceMode = ((dto.priceMode ?? existing.priceMode) as string) as
        | 'TAX_EXCLUSIVE'
        | 'TAX_INCLUSIVE';
      const taxIds = mergedLines.flatMap((l) => [l.tax1Id, l.tax2Id]);
      const rateById = await this.taxRateMap(taxIds);
      const totals = computeOrderTotals(
        mergedLines,
        { ...totalsInput, discountPercent: dto.discountPercent },
        rateById,
        priceMode,
      );
      data.subtotal = totals.subtotal;
      data.grandTotal = totals.grandTotal;
      if (totals.discountAmount !== null) data.discountAmount = totals.discountAmount;
      if (totals.otherCostAmount !== null) data.otherCostAmount = totals.otherCostAmount;

      // dueDate recompute when payment term changes without explicit dueDate
      if (dto.paymentTermId !== undefined && dto.dueDate === undefined) {
        const termId = dto.paymentTermId ?? null;
        const docDateStr = dto.docDate ?? existing.docDate.toISOString().slice(0, 10);
        data.dueDate = await this.resolveDueDate(termId ?? undefined, docDateStr, undefined);
      }

      if (dto.lines !== undefined) {
        await tx.erpSlsProformaInvoiceLine.deleteMany({ where: { proformaInvoiceId: id } });
        data.lines = { create: totals.lines.map((l) => mapProformaInvoiceLine(l, header)) };
      }
      await tx.erpSlsProformaInvoice.update({ where: { id }, data });
    });
    return this.one(id);
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.findRaw(id);
    if (existing.status === 'POSTED') {
      throw new BadRequestException('Dokumen POSTED tidak bisa dihapus. Reopen dulu.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.erpSlsProformaInvoice.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: actor },
    });
    return { success: true, message: 'Proforma invoice dihapus' };
  }

  // ── workflow (§2.7 state machine) ────────────────────────────────────────────
  async transition(id: bigint, dto: TransitionSlsProformaInvoiceDto, actorId?: string) {
    const proformaInvoice = await this.findRaw(id);
    const actor = actorId ? BigInt(actorId) : null;
    const next = NEXT[proformaInvoice.status]?.[dto.action];
    if (!next) {
      throw new BadRequestException(`Aksi ${dto.action} tidak valid dari status ${proformaInvoice.status}.`);
    }
    if (dto.action === A.REJECT && !dto.reason?.trim()) {
      throw new BadRequestException('Alasan reject wajib diisi.');
    }

    if (dto.action === A.POST) {
      const period = await this.prisma.erpFiscalPeriod.findUnique({
        where: { id: proformaInvoice.fiscalPeriodId },
        select: { status: true },
      });
      if (period?.status === 'CLOSED') {
        throw new BadRequestException('Periode fiskal sudah ditutup — tidak bisa posting.');
      }
      await this.prisma.$transaction(async (tx) => {
        await this.posting.reverseLedger(tx, proformaInvoice.id);
        await this.posting.postToLedger(tx, proformaInvoice, actor);
        await tx.erpSlsProformaInvoice.update({
          where: { id },
          data: {
            status: 'POSTED',
            previousStatus: proformaInvoice.status as never,
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
        await this.posting.reverseLedger(tx, proformaInvoice.id);
        await tx.erpSlsProformaInvoice.update({
          where: { id },
          data: {
            status: 'DRAFT',
            previousStatus: proformaInvoice.status as never,
            postingStatus: 'UNPOSTED',
            postedAt: null,
            updatedById: actor,
          },
        });
      });
      return this.one(id);
    }

    await this.prisma.erpSlsProformaInvoice.update({
      where: { id },
      data: {
        status: next as never,
        previousStatus: proformaInvoice.status as never,
        updatedById: actor,
        ...(dto.action === A.REJECT
          ? {
              metadata: {
                ...((proformaInvoice.metadata as object) ?? {}),
                rejectReason: dto.reason,
              },
            }
          : {}),
      },
    });
    return this.one(id);
  }
}