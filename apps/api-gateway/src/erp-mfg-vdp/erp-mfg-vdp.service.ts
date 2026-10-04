import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import {
  CreateVdpDatasetDto,
  ImportVdpRowsDto,
  QueryVdpRowsDto,
} from './dto/vdp.dto';

/** Parser CSV sederhana: dukung pemisah , atau ; dan nilai berpetik ganda. */
function parseCsv(text: string): { columns: string[]; rows: Record<string, string>[] } {
  const lines = text.split(/\r?\n/).filter((l) => l.trim().length > 0);
  if (!lines.length) return { columns: [], rows: [] };
  const delim = lines[0].includes(';') && !lines[0].includes(',') ? ';' : ',';
  const splitLine = (line: string): string[] => {
    const out: string[] = [];
    let cur = '';
    let quoted = false;
    for (let i = 0; i < line.length; i++) {
      const ch = line[i];
      if (quoted) {
        if (ch === '"') {
          if (line[i + 1] === '"') {
            cur += '"';
            i++;
          } else quoted = false;
        } else cur += ch;
      } else if (ch === '"') quoted = true;
      else if (ch === delim) {
        out.push(cur.trim());
        cur = '';
      } else cur += ch;
    }
    out.push(cur.trim());
    return out;
  };
  const columns = splitLine(lines[0]).map((c) => c.trim()).filter(Boolean);
  const rows = lines.slice(1).map((line) => {
    const cells = splitLine(line);
    const obj: Record<string, string> = {};
    columns.forEach((c, i) => (obj[c] = cells[i] ?? ''));
    return obj;
  });
  return { columns, rows };
}

/**
 * Fase 2 P4 — Variable Data Printing: dataset data variabel per job cetak
 * (impor CSV, validasi kolom wajib per baris, hitungan baris valid = jumlah
 * cetak variabel), teraudit per versi. Dataset MEMBEKU (TERKUNCI) saat job
 * memasuki tahap CETAK (kait di layanan job) atau dikunci manual; dataset
 * terkunci tidak bisa diimpor ulang / diubah / dihapus. ERP hanya mengelola
 * data & hitungannya — file hasil cetak tidak dibuat di sini.
 */
@Injectable()
export class ErpMfgVdpService {
  constructor(private readonly prisma: PrismaService) {}

  private present(row: any) {
    return {
      id: row.id?.toString(),
      jobId: row.jobId?.toString(),
      name: row.name,
      sourceFilename: row.sourceFilename,
      version: row.version,
      columns: row.columns ?? [],
      requiredColumns: row.requiredColumns ?? [],
      rowCount: row.rowCount,
      validCount: row.validCount,
      invalidCount: (row.rowCount ?? 0) - (row.validCount ?? 0),
      status: row.status,
      lockedAt: row.lockedAt,
      notes: row.notes,
      createdAt: row.createdAt,
      updatedAt: row.updatedAt,
    };
  }

  private async jobOrThrow(jobId: bigint) {
    const job = await this.prisma.erpMfgPrintJob.findFirst({
      where: { id: jobId, deletedAt: null },
      select: { id: true, stage: true, printQuantity: true },
    });
    if (!job) throw new NotFoundException('Job cetak tidak ditemukan.');
    return job;
  }

  private async datasetOrThrow(id: bigint) {
    const ds = await this.prisma.erpMfgVdpDataset.findFirst({
      where: { id, deletedAt: null },
    });
    if (!ds) throw new NotFoundException('Dataset VDP tidak ditemukan.');
    return ds;
  }

  /** Kunci efektif: status TERKUNCI atau job sudah melewati gerbang cetak. */
  private async effectiveLocked(ds: any): Promise<boolean> {
    if (ds.status === 'TERKUNCI') return true;
    const job = await this.jobOrThrow(ds.jobId);
    return ['CETAK', 'FINISHING', 'QC', 'SELESAI'].includes(job.stage as string);
  }

  private async assertUnlocked(ds: any) {
    if (await this.effectiveLocked(ds)) {
      throw new BadRequestException(
        'Dataset sudah terkunci (job memasuki tahap cetak) — data variabel tidak dapat diubah.',
      );
    }
  }

  async listForJob(jobId: string) {
    const job = await this.jobOrThrow(BigInt(jobId));
    const rows = await this.prisma.erpMfgVdpDataset.findMany({
      where: { jobId: job.id, deletedAt: null },
      orderBy: { createdAt: 'desc' },
    });
    return {
      data: rows.map((r) => this.present(r)),
      jobStage: job.stage,
      printQuantity: job.printQuantity.toString(),
    };
  }

  async create(jobId: string, dto: CreateVdpDatasetDto, actorId?: string) {
    const job = await this.jobOrThrow(BigInt(jobId));
    if (job.stage === 'CANCELLED') {
      throw new BadRequestException('Job CANCELLED tidak dapat diberi dataset.');
    }
    const row = await this.prisma.erpMfgVdpDataset.create({
      data: {
        jobId: job.id,
        name: dto.name,
        sourceFilename: dto.sourceFilename ?? null,
        requiredColumns: (dto.requiredColumns ?? []) as unknown as Prisma.InputJsonValue,
        notes: dto.notes ?? null,
        createdById: actorId ? BigInt(actorId) : null,
        updatedById: actorId ? BigInt(actorId) : null,
      },
    });
    return this.present(row);
  }

  async importRows(id: string, dto: ImportVdpRowsDto) {
    const ds = await this.datasetOrThrow(BigInt(id));
    await this.assertUnlocked(ds);
    let columns: string[] = [];
    let rawRows: Record<string, unknown>[] = [];
    if (dto.csvText?.trim()) {
      const parsed = parseCsv(dto.csvText);
      columns = parsed.columns;
      rawRows = parsed.rows;
    } else if (dto.rows?.length) {
      rawRows = dto.rows;
      columns = [...new Set(rawRows.flatMap((r) => Object.keys(r)))];
    }
    if (!columns.length || !rawRows.length) {
      throw new BadRequestException('Tidak ada baris data terbaca — kirim csvText (dengan header) atau rows[].');
    }
    const required =
      ((ds.requiredColumns as unknown as string[]) ?? []).length > 0
        ? (ds.requiredColumns as unknown as string[])
        : columns;
    const missingRequired = required.filter((c) => !columns.includes(c));
    if (missingRequired.length) {
      throw new BadRequestException(
        `Kolom wajib tidak ada di data: ${missingRequired.join(', ')}.`,
      );
    }
    const prepared = rawRows.map((r, i) => {
      const data: Record<string, string> = {};
      for (const c of columns) data[c] = r[c] == null ? '' : String(r[c]);
      const errors = required.filter((c) => !data[c]?.trim());
      return {
        datasetId: ds.id,
        rowNo: i + 1,
        data: data as unknown as Prisma.InputJsonValue,
        isValid: errors.length === 0,
        errorNote: errors.length ? `Kolom kosong: ${errors.join(', ')}` : null,
      };
    });
    const validCount = prepared.filter((p) => p.isValid).length;
    await this.prisma.$transaction(async (tx) => {
      await tx.erpMfgVdpRow.deleteMany({ where: { datasetId: ds.id } });
      await tx.erpMfgVdpRow.createMany({ data: prepared });
      await tx.erpMfgVdpDataset.update({
        where: { id: ds.id },
        data: {
          columns: columns as unknown as Prisma.InputJsonValue,
          requiredColumns: required as unknown as Prisma.InputJsonValue,
          rowCount: prepared.length,
          validCount,
          version: ds.rowCount > 0 ? ds.version + 1 : ds.version,
          sourceFilename: dto.sourceFilename ?? ds.sourceFilename,
        },
      });
    });
    const after = await this.datasetOrThrow(ds.id);
    return this.present(after);
  }

  async listRows(id: string, query: QueryVdpRowsDto) {
    const ds = await this.datasetOrThrow(BigInt(id));
    const page = query.page ?? 1;
    const limit = query.limit ?? 50;
    const where: Prisma.ErpMfgVdpRowWhereInput = { datasetId: ds.id };
    if (query.onlyInvalid) where.isValid = false;
    const [total, rows] = await this.prisma.$transaction([
      this.prisma.erpMfgVdpRow.count({ where }),
      this.prisma.erpMfgVdpRow.findMany({
        where,
        orderBy: { rowNo: 'asc' },
        skip: (page - 1) * limit,
        take: limit,
      }),
    ]);
    return {
      data: rows.map((r) => ({
        id: r.id.toString(),
        rowNo: r.rowNo,
        data: r.data ?? {},
        isValid: r.isValid,
        errorNote: r.errorNote,
      })),
      meta: { page, limit, total, totalPages: Math.max(1, Math.ceil(total / limit)) },
    };
  }

  async lock(id: string) {
    const ds = await this.datasetOrThrow(BigInt(id));
    if (ds.status === 'TERKUNCI') return this.present(ds);
    const updated = await this.prisma.erpMfgVdpDataset.update({
      where: { id: ds.id },
      data: { status: 'TERKUNCI', lockedAt: new Date() },
    });
    return this.present(updated);
  }

  async remove(id: string) {
    const ds = await this.datasetOrThrow(BigInt(id));
    await this.assertUnlocked(ds);
    await this.prisma.erpMfgVdpDataset.update({
      where: { id: ds.id },
      data: { deletedAt: new Date() },
    });
    return { success: true };
  }
}
