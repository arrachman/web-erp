import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { ErpMfgWorkOrdersService } from '../erp-mfg-work-orders/erp-mfg-work-orders.service';
import type { CreateMfgWorkOrderDto } from '../erp-mfg-work-orders/dto/create-mfg-work-order.dto';
import {
  AdvancePrintJobDto,
  ChecklistPrintJobDto,
  CreatePrintJobDto,
  QueryPrintJobsDto,
  UpdatePrintJobDto,
} from './dto/print-job.dto';

export interface ChecklistItem {
  key: string;
  label: string;
  required: boolean;
  done: boolean;
  doneById?: string | null;
  doneAt?: string | null;
}

export const DEFAULT_CHECKLIST: ChecklistItem[] = [
  { key: 'FILE_MASTER', label: 'File master diterima & lengkap', required: true, done: false },
  { key: 'PROOF', label: 'Proof warna disetujui', required: true, done: false },
  { key: 'PLAT', label: 'Plat siap', required: true, done: false },
  { key: 'KERTAS', label: 'Kertas tersedia di gudang produksi', required: true, done: false },
  { key: 'DATA_VARIABEL', label: 'Data variabel terverifikasi (bila ada)', required: false, done: false },
];

const TRANSITIONS: Record<string, string[]> = {
  PRE_PRESS: ['CETAK', 'CANCELLED'],
  CETAK: ['FINISHING', 'CANCELLED'],
  FINISHING: ['QC', 'CANCELLED'],
  QC: ['SELESAI', 'FINISHING', 'CANCELLED'],
  SELESAI: [],
  CANCELLED: [],
};

/**
 * Fase 2 P2 — Job cetak & pre-press: lapisan profil cetak di atas Work Order
 * generik (WO tetap dibuat/dikelola oleh modul mfg-work-orders). Job membawa
 * spesifikasi cetak, checklist pre-press (gerbang wajib sebelum tahap CETAK),
 * dan mesin tahap PRE_PRESS -> CETAK -> FINISHING -> QC -> SELESAI dengan
 * log perpindahan tahap.
 */
@Injectable()
export class ErpMfgPrintJobsService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly workOrders: ErpMfgWorkOrdersService,
  ) {}

  private present(row: any, extra: Record<string, unknown> = {}) {
    const checklist = (row.checklist ?? []) as ChecklistItem[];
    return {
      id: row.id?.toString(),
      workOrderId: row.workOrderId?.toString(),
      estimateId: row.estimateId?.toString() ?? null,
      salesOrderId: row.salesOrderId?.toString() ?? null,
      stage: row.stage,
      paperSize: row.paperSize,
      pageCount: row.pageCount,
      printQuantity: row.printQuantity?.toString(),
      colorSpec: row.colorSpec,
      finishing: row.finishing,
      masterFileName: row.masterFileName,
      checklist,
      checklistProgress: {
        done: checklist.filter((c) => c.done).length,
        total: checklist.length,
        requiredDone: checklist.filter((c) => !c.required || c.done).length,
        requiredTotal: checklist.length,
      },
      stageLog: row.stageLog ?? [],
      notes: row.notes,
      createdAt: row.createdAt,
      updatedAt: row.updatedAt,
      ...extra,
    };
  }

  private async getOrThrow(id: bigint) {
    const row = await this.prisma.erpMfgPrintJob.findFirst({
      where: { id, deletedAt: null },
    });
    if (!row) throw new NotFoundException('Job cetak tidak ditemukan.');
    return row;
  }

  private async woInfo(workOrderId: bigint) {
    const wo = await this.prisma.erpMfgWorkOrder.findUnique({
      where: { id: workOrderId },
      select: { docNumber: true, docDate: true, status: true, description: true },
    });
    const out = await this.prisma.erpMfgWorkOrderOutput.findFirst({
      where: { workOrderId },
      orderBy: { lineNo: 'asc' },
      select: { itemId: true, quantity: true },
    });
    let itemName: string | null = null;
    if (out?.itemId) {
      const item = await this.prisma.erpItem.findUnique({
        where: { id: out.itemId },
        select: { name: true },
      });
      itemName = item?.name ?? null;
    }
    return { wo, outputItemId: out?.itemId ?? null, itemName };
  }

  // ── CRUD ────────────────────────────────────────────────────────────────────
  async create(dto: CreatePrintJobDto, actorId?: string) {
    const actor = actorId ? BigInt(actorId) : null;

    let estimate: any = null;
    if (dto.estimateId) {
      estimate = await this.prisma.erpMfgPrintEstimate.findFirst({
        where: { id: BigInt(dto.estimateId), deletedAt: null },
      });
      if (!estimate) throw new BadRequestException('Estimasi tidak ditemukan.');
    }
    const itemId = dto.itemId ?? estimate?.itemId?.toString();
    if (!itemId) {
      throw new BadRequestException('Item hasil cetak wajib diisi (langsung atau dari estimasi).');
    }
    const printQuantity = dto.printQuantity ?? estimate?.printQuantity?.toString();
    if (!printQuantity) {
      throw new BadRequestException('Oplah (printQuantity) wajib diisi (langsung atau dari estimasi).');
    }
    const item = await this.prisma.erpItem.findUnique({
      where: { id: BigInt(itemId) },
      select: { baseUnitId: true, name: true },
    });
    if (!item) throw new BadRequestException('Item hasil cetak tidak ditemukan.');
    const idr = await this.prisma.erpCurrency.findFirst({
      where: { code: 'IDR' },
      select: { id: true },
    });
    if (!idr) throw new BadRequestException('Mata uang IDR tidak ditemukan.');

    const unitCost =
      estimate && new Prisma.Decimal(estimate.printQuantity).gt(0)
        ? new Prisma.Decimal(estimate.totalCost).div(new Prisma.Decimal(estimate.printQuantity)).toString()
        : '0';

    const woRes: any = await this.workOrders.create(
      {
        branchId: dto.branchId,
        docDate: dto.docDate,
        currencyId: idr.id.toString(),
        exchangeRate: '1',
        description: dto.title ?? estimate?.title ?? `Job cetak ${item.name}`,
        inputs: [],
        outputs: [
          {
            lineNo: 1,
            itemId,
            quantity: printQuantity,
            unitId: item.baseUnitId.toString(),
            unitCost,
          },
        ],
      } as unknown as CreateMfgWorkOrderDto,
      actorId,
    );
    const wo = woRes?.data ?? woRes;
    if (!wo?.id) throw new BadRequestException('Gagal membuat Work Order untuk job ini.');

    try {
      const job = await this.prisma.erpMfgPrintJob.create({
        data: {
          workOrderId: BigInt(wo.id),
          estimateId: estimate ? estimate.id : null,
          stage: 'PRE_PRESS',
          paperSize: dto.paperSize ?? estimate?.paperSize ?? null,
          pageCount: dto.pageCount ?? estimate?.pageCount ?? null,
          printQuantity: new Prisma.Decimal(printQuantity),
          colorSpec: dto.colorSpec ?? estimate?.colorSpec ?? null,
          finishing: dto.finishing ?? estimate?.finishing ?? null,
          masterFileName: dto.masterFileName ?? null,
          checklist: DEFAULT_CHECKLIST as unknown as Prisma.InputJsonValue,
          stageLog: [
            { from: null, to: 'PRE_PRESS', at: new Date().toISOString(), byId: actor?.toString() ?? null },
          ] as unknown as Prisma.InputJsonValue,
          notes: dto.notes ?? null,
          createdById: actor,
          updatedById: actor,
        },
      });
      return this.findOne(job.id.toString());
    } catch (err) {
      // Kompensasi: jangan tinggalkan WO yatim bila profil job gagal dibuat.
      try {
        await this.workOrders.remove(BigInt(wo.id));
      } catch { /* abaikan */ }
      throw err;
    }
  }

  async findAll(query: QueryPrintJobsDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 25;
    const where: Prisma.ErpMfgPrintJobWhereInput = { deletedAt: null };
    if (query.stage) where.stage = query.stage as any;
    if (query.search) {
      const wos = await this.prisma.erpMfgWorkOrder.findMany({
        where: { docNumber: { contains: query.search, mode: 'insensitive' } },
        select: { id: true },
        take: 200,
      });
      where.workOrderId = { in: wos.map((w) => w.id) };
    }
    const sortField = query.sortBy === 'printQuantity' ? 'printQuantity' : 'createdAt';
    const sortDir = query.sortDir === 'asc' ? 'asc' : 'desc';
    const [total, rows] = await this.prisma.$transaction([
      this.prisma.erpMfgPrintJob.count({ where }),
      this.prisma.erpMfgPrintJob.findMany({
        where,
        orderBy: { [sortField]: sortDir } as any,
        skip: (page - 1) * limit,
        take: limit,
      }),
    ]);
    const data = [] as any[];
    for (const r of rows) {
      const { wo, itemName } = await this.woInfo(r.workOrderId);
      data.push(
        this.present(r, {
          workOrderDocNumber: wo?.docNumber ?? null,
          workOrderStatus: wo?.status ?? null,
          workOrderDocDate: wo?.docDate ? new Date(wo.docDate).toISOString().slice(0, 10) : null,
          title: wo?.description ?? null,
          itemName,
        }),
      );
    }
    return { data, meta: { page, limit, total, totalPages: Math.max(1, Math.ceil(total / limit)) } };
  }

  async findOne(id: string) {
    const row = await this.getOrThrow(BigInt(id));
    const { wo, outputItemId, itemName } = await this.woInfo(row.workOrderId);
    let estimateDocNumber: string | null = null;
    if (row.estimateId) {
      const est = await this.prisma.erpMfgPrintEstimate.findUnique({
        where: { id: row.estimateId },
        select: { docNumber: true },
      });
      estimateDocNumber = est?.docNumber ?? null;
    }
    return this.present(row, {
      workOrderDocNumber: wo?.docNumber ?? null,
      workOrderStatus: wo?.status ?? null,
      workOrderDocDate: wo?.docDate ? new Date(wo.docDate).toISOString().slice(0, 10) : null,
      title: wo?.description ?? null,
      outputItemId: outputItemId?.toString() ?? null,
      itemName,
      estimateDocNumber,
    });
  }

  async update(id: string, dto: UpdatePrintJobDto, actorId?: string) {
    const existing = await this.getOrThrow(BigInt(id));
    if (existing.stage === 'SELESAI' || existing.stage === 'CANCELLED') {
      throw new BadRequestException('Job yang sudah SELESAI/CANCELLED tidak dapat diubah.');
    }
    await this.prisma.erpMfgPrintJob.update({
      where: { id: existing.id },
      data: {
        paperSize: dto.paperSize !== undefined ? dto.paperSize : existing.paperSize,
        pageCount: dto.pageCount !== undefined ? dto.pageCount : existing.pageCount,
        colorSpec: dto.colorSpec !== undefined ? dto.colorSpec : existing.colorSpec,
        finishing: dto.finishing !== undefined ? dto.finishing : existing.finishing,
        masterFileName: dto.masterFileName !== undefined ? dto.masterFileName : existing.masterFileName,
        notes: dto.notes !== undefined ? dto.notes : existing.notes,
        updatedById: actorId ? BigInt(actorId) : null,
      },
    });
    return this.findOne(id);
  }

  async remove(id: string) {
    const existing = await this.getOrThrow(BigInt(id));
    if (existing.stage !== 'PRE_PRESS') {
      throw new BadRequestException('Hanya job tahap PRE_PRESS yang dapat dihapus.');
    }
    await this.prisma.erpMfgPrintJob.update({
      where: { id: existing.id },
      data: { deletedAt: new Date() },
    });
    return { success: true };
  }

  // ── Checklist & tahap ───────────────────────────────────────────────────────
  async setChecklist(id: string, dto: ChecklistPrintJobDto, actorId?: string) {
    const existing = await this.getOrThrow(BigInt(id));
    if (existing.stage !== 'PRE_PRESS') {
      throw new BadRequestException('Checklist pre-press hanya dapat diubah pada tahap PRE_PRESS.');
    }
    const checklist = (existing.checklist ?? []) as unknown as ChecklistItem[];
    const item = checklist.find((c) => c.key === dto.key);
    if (!item) throw new BadRequestException(`Item checklist ${dto.key} tidak dikenal.`);
    item.done = dto.done;
    item.doneById = dto.done ? (actorId ?? null) : null;
    item.doneAt = dto.done ? new Date().toISOString() : null;
    await this.prisma.erpMfgPrintJob.update({
      where: { id: existing.id },
      data: { checklist: checklist as unknown as Prisma.InputJsonValue },
    });
    return this.findOne(id);
  }

  async advance(id: string, dto: AdvancePrintJobDto, actorId?: string) {
    const existing = await this.getOrThrow(BigInt(id));
    const from = existing.stage as string;
    const to = dto.toStage;
    if (!TRANSITIONS[from]?.includes(to)) {
      throw new BadRequestException(`Transisi tahap ${from} -> ${to} tidak diizinkan.`);
    }
    if (from === 'PRE_PRESS' && to === 'CETAK') {
      const missing = ((existing.checklist ?? []) as unknown as ChecklistItem[])
        .filter((c) => c.required && !c.done)
        .map((c) => c.label);
      if (missing.length) {
        throw new BadRequestException(
          `Checklist pre-press belum lengkap: ${missing.join('; ')}.`,
        );
      }
    }
    const stageLog = [
      ...((existing.stageLog ?? []) as any[]),
      { from, to, at: new Date().toISOString(), byId: actorId ?? null },
    ];
    await this.prisma.erpMfgPrintJob.update({
      where: { id: existing.id },
      data: {
        stage: to as any,
        stageLog: stageLog as unknown as Prisma.InputJsonValue,
      },
    });
    return this.findOne(id);
  }
}
