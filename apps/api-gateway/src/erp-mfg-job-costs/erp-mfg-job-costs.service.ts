import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { buildLedgerRows } from '../erp-inv-gl/inv-gl-posting.helpers';
import {
  assertLedgerRowsPeriodOpen,
  assertSourceLedgerPeriodOpen,
} from '../erp-common/utils/ledger-period-guard';
import {
  CreateJobCostEntryDto,
  QueryJobCostEntriesDto,
  UpdateJobCostEntryDto,
} from './dto/job-cost.dto';

const FG_ACCOUNT_CODE = '1133.01.001'; // Persediaan Barang Jadi
const WIP_ACCOUNT_CODE = '1132.01.001'; // Persediaan Barang Dalam Proses
const LEDGER_SOURCE = 'PRODUCTION';
const LEDGER_DOCTYPE = 'mfg_print_jobs';

/**
 * Fase 2 P5 — HPP per job: entri biaya aktual job cetak (material, tenaga
 * kerja, overhead, makloon, lain), ringkasan aktual vs estimasi (P1) dengan
 * varians + margin, dan jurnal penyelesaian Dr Persediaan Barang Jadi /
 * Cr Persediaan Barang Dalam Proses sebesar total aktual saat job SELESAI.
 * Entri terkunci setelah jurnal diposting (batalkan jurnal untuk mengubah).
 */
@Injectable()
export class ErpMfgJobCostsService {
  constructor(private readonly prisma: PrismaService) {}

  private present(row: any) {
    return {
      id: row.id?.toString(),
      jobId: row.jobId?.toString(),
      entryDate: row.entryDate ? new Date(row.entryDate).toISOString().slice(0, 10) : null,
      costType: row.costType,
      stage: row.stage,
      description: row.description,
      itemId: row.itemId?.toString() ?? null,
      quantity: row.quantity?.toString(),
      unitId: row.unitId?.toString() ?? null,
      unitCost: row.unitCost?.toString(),
      amount: row.amount?.toString(),
      sourceType: row.sourceType,
      sourceId: row.sourceId?.toString() ?? null,
      createdAt: row.createdAt,
      updatedAt: row.updatedAt,
    };
  }

  private async jobOrThrow(jobId: bigint) {
    const job = await this.prisma.erpMfgPrintJob.findFirst({
      where: { id: jobId, deletedAt: null },
    });
    if (!job) throw new NotFoundException('Job cetak tidak ditemukan.');
    return job;
  }

  private assertEntriesUnlocked(job: any) {
    if (job.costPostedAt) {
      throw new BadRequestException(
        'HPP job ini sudah diposting ke jurnal — batalkan jurnal terlebih dahulu untuk mengubah biaya.',
      );
    }
    if (job.stage === 'CANCELLED') {
      throw new BadRequestException('Job CANCELLED tidak dapat diubah biayanya.');
    }
  }

  // ── Entri biaya ─────────────────────────────────────────────────────────────
  async listEntries(jobId: string, query: QueryJobCostEntriesDto) {
    await this.jobOrThrow(BigInt(jobId));
    const page = query.page ?? 1;
    const limit = query.limit ?? 50;
    const where: Prisma.ErpMfgJobCostEntryWhereInput = {
      jobId: BigInt(jobId),
      deletedAt: null,
    };
    if (query.costType) where.costType = query.costType as any;
    const [total, rows] = await this.prisma.$transaction([
      this.prisma.erpMfgJobCostEntry.count({ where }),
      this.prisma.erpMfgJobCostEntry.findMany({
        where,
        orderBy: [{ entryDate: 'asc' }, { id: 'asc' }],
        skip: (page - 1) * limit,
        take: limit,
      }),
    ]);
    return {
      data: rows.map((r) => this.present(r)),
      meta: { page, limit, total, totalPages: Math.max(1, Math.ceil(total / limit)) },
    };
  }

  async createEntry(jobId: string, dto: CreateJobCostEntryDto, actorId?: string) {
    const job = await this.jobOrThrow(BigInt(jobId));
    this.assertEntriesUnlocked(job);
    const qty = new Prisma.Decimal(dto.quantity ?? '1');
    const amount = qty.mul(new Prisma.Decimal(dto.unitCost));
    const row = await this.prisma.erpMfgJobCostEntry.create({
      data: {
        jobId: job.id,
        entryDate: new Date(dto.entryDate),
        costType: dto.costType as any,
        stage: dto.stage ?? null,
        description: dto.description ?? null,
        itemId: dto.itemId ? BigInt(dto.itemId) : null,
        quantity: qty,
        unitId: dto.unitId ? BigInt(dto.unitId) : null,
        unitCost: new Prisma.Decimal(dto.unitCost),
        amount,
        createdById: actorId ? BigInt(actorId) : null,
        updatedById: actorId ? BigInt(actorId) : null,
      },
    });
    return this.present(row);
  }

  async updateEntry(id: string, dto: UpdateJobCostEntryDto, actorId?: string) {
    const entry = await this.prisma.erpMfgJobCostEntry.findFirst({
      where: { id: BigInt(id), deletedAt: null },
    });
    if (!entry) throw new NotFoundException('Entri biaya tidak ditemukan.');
    const job = await this.jobOrThrow(entry.jobId);
    this.assertEntriesUnlocked(job);
    const qty = dto.quantity !== undefined ? new Prisma.Decimal(dto.quantity) : entry.quantity;
    const unitCost =
      dto.unitCost !== undefined ? new Prisma.Decimal(dto.unitCost) : entry.unitCost;
    const updated = await this.prisma.erpMfgJobCostEntry.update({
      where: { id: entry.id },
      data: {
        entryDate: dto.entryDate ? new Date(dto.entryDate) : entry.entryDate,
        costType: (dto.costType as any) ?? entry.costType,
        stage: dto.stage !== undefined ? dto.stage : entry.stage,
        description: dto.description !== undefined ? dto.description : entry.description,
        itemId:
          dto.itemId !== undefined ? (dto.itemId ? BigInt(dto.itemId) : null) : entry.itemId,
        quantity: qty,
        unitId:
          dto.unitId !== undefined ? (dto.unitId ? BigInt(dto.unitId) : null) : entry.unitId,
        unitCost,
        amount: qty.mul(unitCost),
        updatedById: actorId ? BigInt(actorId) : null,
      },
    });
    return this.present(updated);
  }

  async deleteEntry(id: string) {
    const entry = await this.prisma.erpMfgJobCostEntry.findFirst({
      where: { id: BigInt(id), deletedAt: null },
    });
    if (!entry) throw new NotFoundException('Entri biaya tidak ditemukan.');
    const job = await this.jobOrThrow(entry.jobId);
    this.assertEntriesUnlocked(job);
    await this.prisma.erpMfgJobCostEntry.update({
      where: { id: entry.id },
      data: { deletedAt: new Date() },
    });
    return { success: true };
  }

  // ── Ringkasan & laporan ─────────────────────────────────────────────────────
  async summary(jobId: string) {
    const job = await this.jobOrThrow(BigInt(jobId));
    const entries = await this.prisma.erpMfgJobCostEntry.findMany({
      where: { jobId: job.id, deletedAt: null },
      select: { costType: true, stage: true, amount: true },
    });
    const zero = new Prisma.Decimal(0);
    const actualTotal = entries.reduce((s, e) => s.add(e.amount), zero);
    const byType: Record<string, Prisma.Decimal> = {};
    const byStage: Record<string, Prisma.Decimal> = {};
    for (const e of entries) {
      byType[e.costType] = (byType[e.costType] ?? zero).add(e.amount);
      const st = e.stage ?? 'TANPA_TAHAP';
      byStage[st] = (byStage[st] ?? zero).add(e.amount);
    }
    let estimate: { docNumber: string; totalCost: Prisma.Decimal; totalPrice: Prisma.Decimal } | null = null;
    if (job.estimateId) {
      const est = await this.prisma.erpMfgPrintEstimate.findUnique({
        where: { id: job.estimateId },
        select: { docNumber: true, totalCost: true, totalPrice: true },
      });
      if (est) estimate = est;
    }
    const qty = new Prisma.Decimal(job.printQuantity);
    const variance = estimate ? actualTotal.sub(estimate.totalCost) : null;
    return {
      jobId: job.id.toString(),
      stage: job.stage,
      printQuantity: job.printQuantity.toString(),
      entryCount: entries.length,
      estimateDocNumber: estimate?.docNumber ?? null,
      estimateTotalCost: estimate?.totalCost.toString() ?? null,
      estimateTotalPrice: estimate?.totalPrice.toString() ?? null,
      actualTotal: actualTotal.toString(),
      actualUnitCost: qty.gt(0) ? actualTotal.div(qty).toString() : '0',
      varianceVsEstimate: variance?.toString() ?? null,
      variancePercent:
        estimate && estimate.totalCost.gt(0)
          ? variance!.div(estimate.totalCost).mul(100).toString()
          : null,
      marginVsEstimatePrice: estimate
        ? estimate.totalPrice.sub(actualTotal).toString()
        : null,
      byType: Object.fromEntries(Object.entries(byType).map(([k, v]) => [k, v.toString()])),
      byStage: Object.fromEntries(Object.entries(byStage).map(([k, v]) => [k, v.toString()])),
      costPostedAt: job.costPostedAt,
      costJournalDoc: job.costJournalDoc,
    };
  }

  async report() {
    const jobs = await this.prisma.erpMfgPrintJob.findMany({
      where: { deletedAt: null },
      orderBy: { createdAt: 'desc' },
      take: 200,
    });
    const rows = [] as any[];
    for (const job of jobs) {
      const [wo, sum] = await Promise.all([
        this.prisma.erpMfgWorkOrder.findUnique({
          where: { id: job.workOrderId },
          select: { docNumber: true, description: true },
        }),
        this.summary(job.id.toString()),
      ]);
      rows.push({
        jobId: job.id.toString(),
        workOrderDocNumber: wo?.docNumber ?? null,
        title: wo?.description ?? null,
        stage: job.stage,
        estimateDocNumber: sum.estimateDocNumber,
        estimateTotalCost: sum.estimateTotalCost,
        estimateTotalPrice: sum.estimateTotalPrice,
        actualTotal: sum.actualTotal,
        actualUnitCost: sum.actualUnitCost,
        varianceVsEstimate: sum.varianceVsEstimate,
        marginVsEstimatePrice: sum.marginVsEstimatePrice,
        costPostedAt: sum.costPostedAt,
        costJournalDoc: sum.costJournalDoc,
      });
    }
    return { data: rows };
  }

  // ── Jurnal penyelesaian ─────────────────────────────────────────────────────
  private async accountByCode(code: string) {
    const acc = await this.prisma.erpAccount.findFirst({
      where: { code, deletedAt: null },
      select: { id: true, name: true },
    });
    if (!acc) throw new BadRequestException(`Akun ${code} tidak ditemukan di CoA.`);
    return acc;
  }

  async postJournal(jobId: string, actorId?: string) {
    const job = await this.jobOrThrow(BigInt(jobId));
    if (job.stage !== 'SELESAI') {
      throw new BadRequestException('Jurnal HPP hanya dapat diposting untuk job tahap SELESAI.');
    }
    if (job.costPostedAt) {
      throw new BadRequestException('Jurnal HPP job ini sudah diposting.');
    }
    const sum = await this.summary(jobId);
    const total = new Prisma.Decimal(sum.actualTotal);
    if (total.lte(0)) {
      throw new BadRequestException('Total biaya aktual masih 0 — isi entri biaya terlebih dahulu.');
    }
    const wo = await this.prisma.erpMfgWorkOrder.findUnique({
      where: { id: job.workOrderId },
      select: { docNumber: true, branchId: true, locationId: true },
    });
    if (!wo) throw new BadRequestException('Work Order job tidak ditemukan.');
    const [fg, wip, idr] = await Promise.all([
      this.accountByCode(FG_ACCOUNT_CODE),
      this.accountByCode(WIP_ACCOUNT_CODE),
      this.prisma.erpCurrency.findFirst({ where: { code: 'IDR' }, select: { id: true } }),
    ]);
    if (!idr) throw new BadRequestException('Mata uang IDR tidak ditemukan.');
    const today = new Date();
    const period = await this.prisma.erpFiscalPeriod.findFirst({
      where: { deletedAt: null, startDate: { lte: today }, endDate: { gte: today } },
      select: { id: true },
    });
    if (!period) throw new BadRequestException('Tidak ada periode fiskal untuk tanggal hari ini.');
    const docNumber = `${wo.docNumber}-HPP`;
    const actor = actorId ? BigInt(actorId) : null;
    const rows = buildLedgerRows(
      {
        branchId: wo.branchId,
        locationId: wo.locationId ?? null,
        sourceDocType: LEDGER_DOCTYPE,
        sourceId: job.id,
        docNumber,
        entryDate: today,
        fiscalPeriodId: period.id,
        currencyId: idr.id,
        exchangeRate: new Prisma.Decimal(1),
        actorId: actor,
      },
      [
        { accountId: fg.id, debit: total, credit: new Prisma.Decimal(0), description: `HPP job ${wo.docNumber} — barang jadi` },
        { accountId: wip.id, debit: new Prisma.Decimal(0), credit: total, description: `HPP job ${wo.docNumber} — keluar dari barang dalam proses` },
      ],
    ).map((r) => ({ ...r, source: LEDGER_SOURCE }));
    await this.prisma.$transaction(async (tx) => {
      await assertLedgerRowsPeriodOpen(tx, rows);
      await tx.erpFinLedgerEntry.createMany({ data: rows });
      await tx.erpMfgPrintJob.update({
        where: { id: job.id },
        data: { costPostedAt: new Date(), costJournalDoc: docNumber },
      });
    });
    return this.summary(jobId);
  }

  async voidJournal(jobId: string) {
    const job = await this.jobOrThrow(BigInt(jobId));
    if (!job.costPostedAt) {
      throw new BadRequestException('Jurnal HPP job ini belum diposting.');
    }
    const where: Prisma.ErpFinLedgerEntryWhereInput = {
      sourceDocType: LEDGER_DOCTYPE,
      sourceId: job.id,
    };
    await this.prisma.$transaction(async (tx) => {
      await assertSourceLedgerPeriodOpen(tx, where);
      await tx.erpFinLedgerEntry.deleteMany({ where });
      await tx.erpMfgPrintJob.update({
        where: { id: job.id },
        data: { costPostedAt: null, costJournalDoc: null },
      });
    });
    return this.summary(jobId);
  }
}
