import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { ErpSlsQuotationsService } from '../erp-sls-quotations/erp-sls-quotations.service';
import type { CreateSlsQuotationDto } from '../erp-sls-quotations/dto/create-sls-quotation.dto';
import {
  CreatePrintEstimateDto,
  PrintEstimateLineDto,
  QueryPrintEstimatesDto,
  UpdatePrintEstimateDto,
} from './dto/print-estimate.dto';

const DOC_CODE = 'EST';
const FALLBACK_PREFIX = 'EST';

const SORTABLE: Record<string, string> = {
  docNumber: 'docNumber',
  docDate: 'docDate',
  totalPrice: 'totalPrice',
  totalCost: 'totalCost',
  createdAt: 'createdAt',
};

/**
 * Fase 2 P1 — Estimasi cetak: rincian komponen biaya (kertas, tinta, plat,
 * pre-press, cetak, finishing, makloon, overhead) per pekerjaan, margin ->
 * harga jual + harga satuan, dan konversi menjadi Penawaran (quotation SQ)
 * sebagai snapshot. Total selalu dihitung server; klien tidak mengirim total.
 */
@Injectable()
export class ErpMfgPrintEstimatesService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly quotations: ErpSlsQuotationsService,
  ) {}

  // ── helpers (pola mfg work orders) ──────────────────────────────────────────
  private async resolvePeriod(
    tx: Prisma.TransactionClient,
    date: string,
  ): Promise<bigint> {
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
      const bumped = await tx.erpDocumentNumbering.update({
        where: { id: numbering.id },
        data: { nextNumber: { increment: 1 } },
        select: { nextNumber: true },
      });
      const seq = bumped.nextNumber - 1;
      return `${numbering.prefix}${String(seq).padStart(numbering.digitCount, '0')}`;
    }
    const count = await tx.erpMfgPrintEstimate.count();
    return `${FALLBACK_PREFIX}${String(count + 1).padStart(6, '0')}`;
  }

  private compute(lines: PrintEstimateLineDto[], marginPercent: string, printQuantity: string) {
    const computed = lines.map((l) => {
      const amount = new Prisma.Decimal(l.quantity).mul(new Prisma.Decimal(l.unitCost));
      return { line: l, amount };
    });
    const totalCost = computed.reduce((s, c) => s.add(c.amount), new Prisma.Decimal(0));
    const margin = new Prisma.Decimal(marginPercent || '0');
    const totalPrice = totalCost.mul(margin.add(100)).div(100);
    const qty = new Prisma.Decimal(printQuantity);
    const unitPrice = qty.gt(0) ? totalPrice.div(qty) : new Prisma.Decimal(0);
    return { computed, totalCost, totalPrice, unitPrice };
  }

  private present(row: any) {
    if (!row) return row;
    const { lines, ...rest } = row;
    return {
      ...rest,
      id: row.id?.toString(),
      branchId: row.branchId?.toString(),
      fiscalPeriodId: row.fiscalPeriodId?.toString(),
      customerId: row.customerId?.toString() ?? null,
      itemId: row.itemId?.toString() ?? null,
      quotationId: row.quotationId?.toString() ?? null,
      createdById: row.createdById?.toString() ?? null,
      updatedById: row.updatedById?.toString() ?? null,
      printQuantity: row.printQuantity?.toString(),
      marginPercent: row.marginPercent?.toString(),
      totalCost: row.totalCost?.toString(),
      totalPrice: row.totalPrice?.toString(),
      unitPrice: row.unitPrice?.toString(),
      docDate: row.docDate ? new Date(row.docDate).toISOString().slice(0, 10) : null,
      ...(lines
        ? {
            lines: lines.map((l: any) => ({
              ...l,
              id: l.id?.toString(),
              estimateId: l.estimateId?.toString(),
              itemId: l.itemId?.toString() ?? null,
              unitId: l.unitId?.toString() ?? null,
              quantity: l.quantity?.toString(),
              unitCost: l.unitCost?.toString(),
              amount: l.amount?.toString(),
            })),
          }
        : {}),
    };
  }

  private async getOrThrow(id: bigint) {
    const row = await this.prisma.erpMfgPrintEstimate.findFirst({
      where: { id, deletedAt: null },
      include: { lines: { orderBy: { lineNo: 'asc' } } },
    });
    if (!row) throw new NotFoundException('Estimasi cetak tidak ditemukan.');
    return row;
  }

  // ── CRUD ────────────────────────────────────────────────────────────────────
  async create(dto: CreatePrintEstimateDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;
    if (!dto.lines?.length) throw new BadRequestException('Estimasi wajib punya minimal 1 baris komponen biaya.');
    const { computed, totalCost, totalPrice, unitPrice } = this.compute(
      dto.lines,
      dto.marginPercent ?? '0',
      dto.printQuantity,
    );
    const created = await this.prisma.$transaction(async (tx) => {
      const fiscalPeriodId = await this.resolvePeriod(tx, dto.docDate);
      const docNumber = await this.genDocNumber(tx);
      return tx.erpMfgPrintEstimate.create({
        data: {
          docNumber,
          branchId: BigInt(dto.branchId),
          docDate: new Date(dto.docDate),
          fiscalPeriodId,
          customerId: dto.customerId ? BigInt(dto.customerId) : null,
          itemId: dto.itemId ? BigInt(dto.itemId) : null,
          title: dto.title,
          paperSize: dto.paperSize ?? null,
          pageCount: dto.pageCount ?? null,
          printQuantity: new Prisma.Decimal(dto.printQuantity),
          colorSpec: dto.colorSpec ?? null,
          finishing: dto.finishing ?? null,
          marginPercent: new Prisma.Decimal(dto.marginPercent ?? '0'),
          totalCost,
          totalPrice,
          unitPrice,
          notes: dto.notes ?? null,
          createdById: actor,
          updatedById: actor,
          lines: {
            create: computed.map(({ line, amount }) => ({
              lineNo: line.lineNo,
              component: line.component as any,
              description: line.description ?? null,
              itemId: line.itemId ? BigInt(line.itemId) : null,
              quantity: new Prisma.Decimal(line.quantity),
              unitId: line.unitId ? BigInt(line.unitId) : null,
              unitCost: new Prisma.Decimal(line.unitCost),
              amount,
            })),
          },
        },
        select: { id: true },
      });
    });
    return this.present(await this.getOrThrow(created.id));
  }

  async findAll(query: QueryPrintEstimatesDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 25;
    const where: Prisma.ErpMfgPrintEstimateWhereInput = { deletedAt: null };
    if (query.status) where.status = query.status as any;
    if (query.search) {
      where.OR = [
        { docNumber: { contains: query.search, mode: 'insensitive' } },
        { title: { contains: query.search, mode: 'insensitive' } },
      ];
    }
    const sortField = SORTABLE[query.sortBy ?? 'docNumber'] ?? 'docNumber';
    const sortDir = query.sortDir === 'asc' ? 'asc' : 'desc';
    const [total, rows] = await this.prisma.$transaction([
      this.prisma.erpMfgPrintEstimate.count({ where }),
      this.prisma.erpMfgPrintEstimate.findMany({
        where,
        orderBy: { [sortField]: sortDir } as any,
        skip: (page - 1) * limit,
        take: limit,
      }),
    ]);
    const customerIds = [...new Set(rows.map((r) => r.customerId).filter(Boolean))] as bigint[];
    const customers = customerIds.length
      ? await this.prisma.erpPartner.findMany({
          where: { id: { in: customerIds } },
          select: { id: true, name: true },
        })
      : [];
    const nameById = new Map(customers.map((c) => [c.id.toString(), c.name]));
    return {
      data: rows.map((r) => ({
        ...this.present(r),
        customerName: r.customerId ? (nameById.get(r.customerId.toString()) ?? null) : null,
      })),
      meta: { page, limit, total, totalPages: Math.max(1, Math.ceil(total / limit)) },
    };
  }

  async findOne(id: string) {
    const row = await this.getOrThrow(BigInt(id));
    const [customer, item] = await Promise.all([
      row.customerId
        ? this.prisma.erpPartner.findUnique({ where: { id: row.customerId }, select: { name: true, code: true } })
        : null,
      row.itemId
        ? this.prisma.erpItem.findUnique({ where: { id: row.itemId }, select: { name: true, code: true } })
        : null,
    ]);
    return { ...this.present(row), customerName: customer?.name ?? null, itemName: item?.name ?? null };
  }

  async update(id: string, dto: UpdatePrintEstimateDto, actorId?: string) {
    const existing = await this.getOrThrow(BigInt(id));
    if (existing.status !== 'DRAFT') {
      throw new BadRequestException('Hanya estimasi DRAFT yang dapat diubah.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    const lines = dto.lines ?? existing.lines.map((l) => ({
      lineNo: l.lineNo,
      component: l.component as string,
      description: l.description ?? undefined,
      itemId: l.itemId?.toString(),
      quantity: l.quantity.toString(),
      unitId: l.unitId?.toString(),
      unitCost: l.unitCost.toString(),
    }));
    const margin = dto.marginPercent ?? existing.marginPercent.toString();
    const qty = dto.printQuantity ?? existing.printQuantity.toString();
    const { computed, totalCost, totalPrice, unitPrice } = this.compute(lines, margin, qty);
    await this.prisma.$transaction(async (tx) => {
      if (dto.docDate && dto.docDate !== existing.docDate.toISOString().slice(0, 10)) {
        await this.resolvePeriod(tx, dto.docDate);
      }
      await tx.erpMfgPrintEstimateLine.deleteMany({ where: { estimateId: existing.id } });
      await tx.erpMfgPrintEstimate.update({
        where: { id: existing.id },
        data: {
          docDate: dto.docDate ? new Date(dto.docDate) : existing.docDate,
          fiscalPeriodId: dto.docDate ? await this.resolvePeriod(tx, dto.docDate) : existing.fiscalPeriodId,
          customerId: dto.customerId !== undefined ? (dto.customerId ? BigInt(dto.customerId) : null) : existing.customerId,
          itemId: dto.itemId !== undefined ? (dto.itemId ? BigInt(dto.itemId) : null) : existing.itemId,
          title: dto.title ?? existing.title,
          paperSize: dto.paperSize !== undefined ? dto.paperSize : existing.paperSize,
          pageCount: dto.pageCount !== undefined ? dto.pageCount : existing.pageCount,
          printQuantity: new Prisma.Decimal(qty),
          colorSpec: dto.colorSpec !== undefined ? dto.colorSpec : existing.colorSpec,
          finishing: dto.finishing !== undefined ? dto.finishing : existing.finishing,
          marginPercent: new Prisma.Decimal(margin),
          totalCost,
          totalPrice,
          unitPrice,
          notes: dto.notes !== undefined ? dto.notes : existing.notes,
          updatedById: actor,
          lines: {
            create: computed.map(({ line, amount }) => ({
              lineNo: line.lineNo,
              component: line.component as any,
              description: line.description ?? null,
              itemId: line.itemId ? BigInt(line.itemId) : null,
              quantity: new Prisma.Decimal(line.quantity),
              unitId: line.unitId ? BigInt(line.unitId) : null,
              unitCost: new Prisma.Decimal(line.unitCost),
              amount,
            })),
          },
        },
      });
    });
    return this.present(await this.getOrThrow(existing.id));
  }

  async remove(id: string) {
    const existing = await this.getOrThrow(BigInt(id));
    if (existing.status !== 'DRAFT') {
      throw new BadRequestException('Hanya estimasi DRAFT yang dapat dihapus.');
    }
    await this.prisma.erpMfgPrintEstimate.update({
      where: { id: existing.id },
      data: { deletedAt: new Date() },
    });
    return { success: true };
  }

  async setStatus(id: string, status: string) {
    const existing = await this.getOrThrow(BigInt(id));
    const allowed: Record<string, string[]> = {
      DRAFT: ['ISSUED', 'CANCELLED'],
      ISSUED: ['CANCELLED', 'DRAFT'],
      CONVERTED: [],
      CANCELLED: [],
    };
    if (!allowed[existing.status]?.includes(status)) {
      throw new BadRequestException(`Transisi status ${existing.status} -> ${status} tidak diizinkan.`);
    }
    const updated = await this.prisma.erpMfgPrintEstimate.update({
      where: { id: existing.id },
      data: { status: status as any },
    });
    return this.present(updated);
  }

  // ── Konversi ke Penawaran ───────────────────────────────────────────────────
  async convertToQuotation(id: string, actorId?: string) {
    const est = await this.getOrThrow(BigInt(id));
    if (est.status === 'CONVERTED') throw new BadRequestException('Estimasi ini sudah menjadi penawaran.');
    if (est.status === 'CANCELLED') throw new BadRequestException('Estimasi CANCELLED tidak dapat dikonversi.');
    if (!est.customerId) throw new BadRequestException('Estimasi belum punya pelanggan — wajib diisi sebelum jadi penawaran.');
    if (!est.itemId) throw new BadRequestException('Estimasi belum punya item hasil cetak — wajib diisi sebelum jadi penawaran.');
    const item = await this.prisma.erpItem.findUnique({
      where: { id: est.itemId },
      select: { baseUnitId: true, name: true },
    });
    if (!item) throw new BadRequestException('Item hasil cetak tidak ditemukan.');
    const idr = await this.prisma.erpCurrency.findFirst({ where: { code: 'IDR' }, select: { id: true } });
    if (!idr) throw new BadRequestException('Mata uang IDR tidak ditemukan.');
    const today = new Date().toISOString().slice(0, 10);
    const quotation = await this.quotations.create(
      {
        branchId: est.branchId.toString(),
        docDate: today,
        customerId: est.customerId.toString(),
        currencyId: idr.id.toString(),
        exchangeRate: '1',
        description: `Estimasi cetak ${est.docNumber} — ${est.title}`,
        lines: [
          {
            lineNo: 1,
            itemId: est.itemId.toString(),
            quantity: est.printQuantity.toString(),
            unitId: item.baseUnitId.toString(),
            unitPrice: est.unitPrice.toString(),
          },
        ],
      } as unknown as CreateSlsQuotationDto,
      actorId,
    );
    const q = (quotation as any)?.data ?? quotation;
    const updated = await this.prisma.erpMfgPrintEstimate.update({
      where: { id: est.id },
      data: { status: 'CONVERTED', quotationId: q?.id ? BigInt(q.id) : null },
      include: { lines: { orderBy: { lineNo: 'asc' } } },
    });
    return {
      ...this.present(updated),
      quotationDocNumber: q?.docNumber ?? null,
    };
  }
}
