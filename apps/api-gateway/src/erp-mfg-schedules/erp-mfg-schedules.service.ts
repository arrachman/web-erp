import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import {
  CreateJobScheduleDto,
  CreateMachineDto,
  QueryJobSchedulesDto,
  UpdateJobScheduleDto,
  UpdateMachineDto,
} from './dto/schedule.dto';

const TRANSITIONS: Record<string, string[]> = {
  TERJADWAL: ['BERJALAN', 'BATAL'],
  BERJALAN: ['SELESAI', 'BATAL'],
  SELESAI: [],
  BATAL: ['TERJADWAL'],
};

/**
 * Fase 2 P3 — Penjadwalan produksi: master mesin cetak (2 unit contoh
 * bertanda provisional sampai daftar asli klien tersedia) + jadwal job per
 * mesin dengan validasi bentrok: dua jadwal aktif pada mesin yang sama
 * dengan rentang waktu tumpang tindih DITOLAK (400) beserta rinciannya.
 */
@Injectable()
export class ErpMfgSchedulesService {
  constructor(private readonly prisma: PrismaService) {}

  private machinePresent(row: any) {
    return {
      id: row.id?.toString(),
      code: row.code,
      name: row.name,
      machineType: row.machineType,
      capacityPerHour: row.capacityPerHour?.toString(),
      capacityUnit: row.capacityUnit,
      workStart: row.workStart,
      workEnd: row.workEnd,
      status: row.status,
      legacyCode: row.legacyCode,
      notes: row.notes,
    };
  }

  private schedulePresent(row: any, extra?: any) {
    return {
      id: row.id?.toString(),
      jobId: row.jobId?.toString(),
      machineId: row.machineId?.toString(),
      stage: row.stage,
      plannedStart: row.plannedStart,
      plannedEnd: row.plannedEnd,
      actualStart: row.actualStart,
      actualEnd: row.actualEnd,
      status: row.status,
      sequenceNo: row.sequenceNo,
      notes: row.notes,
      ...(extra ?? {}),
    };
  }

  // ── Mesin ───────────────────────────────────────────────────────────────────
  async listMachines() {
    const rows = await this.prisma.erpMfgMachine.findMany({
      where: { deletedAt: null },
      orderBy: { code: 'asc' },
    });
    return { data: rows.map((r) => this.machinePresent(r)) };
  }

  async createMachine(dto: CreateMachineDto, actorId?: string) {
    const dup = await this.prisma.erpMfgMachine.findFirst({
      where: { code: dto.code, deletedAt: null },
    });
    if (dup) throw new BadRequestException(`Kode mesin ${dto.code} sudah dipakai.`);
    const row = await this.prisma.erpMfgMachine.create({
      data: {
        code: dto.code,
        name: dto.name,
        machineType: dto.machineType as any,
        capacityPerHour: new Prisma.Decimal(dto.capacityPerHour ?? '0'),
        capacityUnit: dto.capacityUnit ?? null,
        workStart: dto.workStart ?? '08:00',
        workEnd: dto.workEnd ?? '17:00',
        notes: dto.notes ?? null,
        createdById: actorId ? BigInt(actorId) : null,
        updatedById: actorId ? BigInt(actorId) : null,
      },
    });
    return this.machinePresent(row);
  }

  async updateMachine(id: string, dto: UpdateMachineDto, actorId?: string) {
    const row = await this.prisma.erpMfgMachine.findFirst({
      where: { id: BigInt(id), deletedAt: null },
    });
    if (!row) throw new NotFoundException('Mesin tidak ditemukan.');
    const updated = await this.prisma.erpMfgMachine.update({
      where: { id: row.id },
      data: {
        name: dto.name ?? row.name,
        machineType: (dto.machineType as any) ?? row.machineType,
        capacityPerHour:
          dto.capacityPerHour !== undefined
            ? new Prisma.Decimal(dto.capacityPerHour)
            : row.capacityPerHour,
        capacityUnit: dto.capacityUnit !== undefined ? dto.capacityUnit : row.capacityUnit,
        workStart: dto.workStart ?? row.workStart,
        workEnd: dto.workEnd ?? row.workEnd,
        status: (dto.status as any) ?? row.status,
        notes: dto.notes !== undefined ? dto.notes : row.notes,
        updatedById: actorId ? BigInt(actorId) : null,
      },
    });
    return this.machinePresent(updated);
  }

  async deleteMachine(id: string) {
    const row = await this.prisma.erpMfgMachine.findFirst({
      where: { id: BigInt(id), deletedAt: null },
    });
    if (!row) throw new NotFoundException('Mesin tidak ditemukan.');
    await this.prisma.erpMfgMachine.update({
      where: { id: row.id },
      data: { deletedAt: new Date() },
    });
    return { success: true };
  }

  // ── Jadwal ──────────────────────────────────────────────────────────────────
  private async assertNoConflict(
    machineId: bigint,
    start: Date,
    end: Date,
    excludeId?: bigint,
  ) {
    const conflict = await this.prisma.erpMfgJobSchedule.findFirst({
      where: {
        machineId,
        deletedAt: null,
        status: { in: ['TERJADWAL', 'BERJALAN'] as any },
        plannedStart: { lt: end },
        plannedEnd: { gt: start },
        ...(excludeId ? { id: { not: excludeId } } : {}),
      },
      select: { id: true, jobId: true, plannedStart: true, plannedEnd: true },
    });
    if (conflict) {
      const job = await this.prisma.erpMfgPrintJob.findUnique({
        where: { id: conflict.jobId },
        select: { workOrderId: true },
      });
      const wo = job
        ? await this.prisma.erpMfgWorkOrder.findUnique({
            where: { id: job.workOrderId },
            select: { docNumber: true },
          })
        : null;
      throw new BadRequestException(
        `Bentrok jadwal pada mesin ini dengan job ${wo?.docNumber ?? conflict.jobId.toString()} ` +
          `(${new Date(conflict.plannedStart).toLocaleString('id-ID')} – ${new Date(conflict.plannedEnd).toLocaleString('id-ID')}). ` +
          'Geser waktu atau pilih mesin lain.',
      );
    }
  }

  private async enrich(row: any) {
    const [job, machine] = await Promise.all([
      this.prisma.erpMfgPrintJob.findUnique({
        where: { id: row.jobId },
        select: { workOrderId: true, stage: true, printQuantity: true },
      }),
      this.prisma.erpMfgMachine.findUnique({
        where: { id: row.machineId },
        select: { code: true, name: true },
      }),
    ]);
    let woDoc: string | null = null;
    let title: string | null = null;
    if (job) {
      const wo = await this.prisma.erpMfgWorkOrder.findUnique({
        where: { id: job.workOrderId },
        select: { docNumber: true, description: true },
      });
      woDoc = wo?.docNumber ?? null;
      title = wo?.description ?? null;
    }
    return this.schedulePresent(row, {
      workOrderDocNumber: woDoc,
      jobTitle: title,
      jobStage: job?.stage ?? null,
      printQuantity: job?.printQuantity?.toString() ?? null,
      machineCode: machine?.code ?? null,
      machineName: machine?.name ?? null,
    });
  }

  async listSchedules(query: QueryJobSchedulesDto) {
    const where: Prisma.ErpMfgJobScheduleWhereInput = { deletedAt: null };
    if (query.machineId) where.machineId = BigInt(query.machineId);
    if (query.jobId) where.jobId = BigInt(query.jobId);
    if (query.from || query.to) {
      where.plannedStart = {
        ...(query.from ? { gte: new Date(query.from) } : {}),
        ...(query.to ? { lte: new Date(query.to) } : {}),
      };
    }
    const rows = await this.prisma.erpMfgJobSchedule.findMany({
      where,
      orderBy: [{ plannedStart: 'asc' }, { sequenceNo: 'asc' }],
      take: query.limit ?? 300,
    });
    const data = [] as any[];
    for (const r of rows) data.push(await this.enrich(r));
    return { data };
  }

  async createSchedule(dto: CreateJobScheduleDto, actorId?: string) {
    const job = await this.prisma.erpMfgPrintJob.findFirst({
      where: { id: BigInt(dto.jobId), deletedAt: null },
      select: { id: true, stage: true },
    });
    if (!job) throw new NotFoundException('Job cetak tidak ditemukan.');
    if (job.stage === 'CANCELLED' || job.stage === 'SELESAI') {
      throw new BadRequestException('Job yang sudah SELESAI/CANCELLED tidak dijadwalkan.');
    }
    const machine = await this.prisma.erpMfgMachine.findFirst({
      where: { id: BigInt(dto.machineId), deletedAt: null },
    });
    if (!machine) throw new NotFoundException('Mesin tidak ditemukan.');
    if (machine.status !== 'ACTIVE') {
      throw new BadRequestException(`Mesin ${machine.code} berstatus ${machine.status} — tidak dapat dijadwalkan.`);
    }
    const start = new Date(dto.plannedStart);
    const end = new Date(dto.plannedEnd);
    if (!(end > start)) throw new BadRequestException('Waktu selesai harus setelah waktu mulai.');
    await this.assertNoConflict(machine.id, start, end);
    const row = await this.prisma.erpMfgJobSchedule.create({
      data: {
        jobId: job.id,
        machineId: machine.id,
        stage: dto.stage ?? null,
        plannedStart: start,
        plannedEnd: end,
        sequenceNo: dto.sequenceNo ?? 0,
        notes: dto.notes ?? null,
        createdById: actorId ? BigInt(actorId) : null,
        updatedById: actorId ? BigInt(actorId) : null,
      },
    });
    return this.enrich(row);
  }

  async updateSchedule(id: string, dto: UpdateJobScheduleDto, actorId?: string) {
    const row = await this.prisma.erpMfgJobSchedule.findFirst({
      where: { id: BigInt(id), deletedAt: null },
    });
    if (!row) throw new NotFoundException('Jadwal tidak ditemukan.');
    if (row.status === 'SELESAI' || row.status === 'BATAL') {
      throw new BadRequestException(`Jadwal berstatus ${row.status} tidak dapat digeser.`);
    }
    const machineId = dto.machineId ? BigInt(dto.machineId) : row.machineId;
    const start = dto.plannedStart ? new Date(dto.plannedStart) : row.plannedStart;
    const end = dto.plannedEnd ? new Date(dto.plannedEnd) : row.plannedEnd;
    if (!(end > start)) throw new BadRequestException('Waktu selesai harus setelah waktu mulai.');
    await this.assertNoConflict(machineId, start, end, row.id);
    const updated = await this.prisma.erpMfgJobSchedule.update({
      where: { id: row.id },
      data: {
        machineId,
        stage: dto.stage !== undefined ? dto.stage : row.stage,
        plannedStart: start,
        plannedEnd: end,
        sequenceNo: dto.sequenceNo ?? row.sequenceNo,
        notes: dto.notes !== undefined ? dto.notes : row.notes,
        updatedById: actorId ? BigInt(actorId) : null,
      },
    });
    return this.enrich(updated);
  }

  async setStatus(id: string, status: string) {
    const row = await this.prisma.erpMfgJobSchedule.findFirst({
      where: { id: BigInt(id), deletedAt: null },
    });
    if (!row) throw new NotFoundException('Jadwal tidak ditemukan.');
    const allowed = TRANSITIONS[row.status] ?? [];
    if (!allowed.includes(status)) {
      throw new BadRequestException(
        `Transisi jadwal ${row.status} → ${status} tidak valid (pilihan: ${allowed.join(', ') || 'tidak ada'}).`,
      );
    }
    const now = new Date();
    const updated = await this.prisma.erpMfgJobSchedule.update({
      where: { id: row.id },
      data: {
        status: status as any,
        actualStart: status === 'BERJALAN' ? (row.actualStart ?? now) : row.actualStart,
        actualEnd: status === 'SELESAI' ? now : row.actualEnd,
      },
    });
    return this.enrich(updated);
  }

  async deleteSchedule(id: string) {
    const row = await this.prisma.erpMfgJobSchedule.findFirst({
      where: { id: BigInt(id), deletedAt: null },
    });
    if (!row) throw new NotFoundException('Jadwal tidak ditemukan.');
    await this.prisma.erpMfgJobSchedule.update({
      where: { id: row.id },
      data: { deletedAt: new Date() },
    });
    return { success: true };
  }
}
