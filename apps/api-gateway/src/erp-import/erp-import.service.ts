import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { toAuditUserId } from '../common/utils/audit-user.util';
import { getAdapter, listEntities } from './erp-import.adapters';
import { buildTemplate, parseFile } from './erp-import.parse';

export interface UploadedFileLike {
  buffer: Buffer;
  originalname: string;
  mimetype: string;
}

export interface RowError {
  row: number;
  message: string;
}

export interface ImportSummary {
  jobId: string;
  total: number;
  ok: number;
  failed: number;
  errors: RowError[];
  /** Present when processing continues after the HTTP response. */
  status?: string;
  async?: boolean;
}

const MAX_REPORTED_ERRORS = 200;

@Injectable()
export class ErpImportService {
  constructor(private prisma: PrismaService) {}

  getEntities() {
    return { success: true, data: listEntities() };
  }

  async buildTemplate(entity: string): Promise<{ buffer: Buffer; fileName: string }> {
    const adapter = getAdapter(entity);
    if (!adapter) throw new NotFoundException(`Entitas "${entity}" tidak didukung`);
    const buffer = await buildTemplate(
      adapter.label,
      adapter.requiredHeaders,
      adapter.optionalHeaders,
    );
    return { buffer, fileName: `template-${entity}.xlsx` };
  }

  async listJobs() {
    const jobs = await this.prisma.erpImportJob.findMany({
      where: { deletedAt: null },
      orderBy: { createdAt: 'desc' },
      take: 50,
    });
    return {
      success: true,
      data: jobs.map((j) => ({
        id: j.id.toString(),
        entity: j.entity,
        fileName: j.fileName,
        status: j.status,
        rowsTotal: j.rowsTotal,
        rowsOk: j.rowsOk,
        rowsFailed: j.rowsFailed,
        errors: (j.errors ?? []) as unknown as RowError[],
        createdAt: j.createdAt,
      })),
    };
  }

  async import(
    entity: string,
    file: UploadedFileLike | undefined,
    actorId?: string,
  ): Promise<{ success: true; data: ImportSummary }> {
    const adapter = getAdapter(entity);
    if (!adapter) throw new NotFoundException(`Entitas "${entity}" tidak didukung`);
    if (!file || !file.buffer || file.buffer.length === 0) {
      throw new BadRequestException('File tidak ditemukan atau kosong');
    }

    // Cap rows to avoid unbounded sync/async work on one request payload.
    const MAX_IMPORT_ROWS = 20_000;
    const { headers, rows: parsedRows } = await parseFile(file.buffer, file.originalname);
    if (parsedRows.length > MAX_IMPORT_ROWS) {
      throw new BadRequestException(
        `Maksimal ${MAX_IMPORT_ROWS} baris per impor (file punya ${parsedRows.length}).`,
      );
    }

    const missing = adapter.requiredHeaders.filter((h) => !headers.includes(h));
    if (missing.length > 0) {
      throw new BadRequestException(
        `Kolom wajib hilang: ${missing.join(', ')}. Unduh template untuk format yang benar.`,
      );
    }

    // Create PENDING job immediately so the client can poll /import/jobs.
    const job = await this.prisma.erpImportJob.create({
      data: {
        entity,
        fileName: file.originalname,
        status: 'PENDING',
        rowsTotal: parsedRows.length,
        rowsOk: 0,
        rowsFailed: 0,
        createdById: toAuditUserId(actorId),
        updatedById: toAuditUserId(actorId),
      },
    });

    const jobId = job.id;
    const rows = parsedRows;

    // Fire-and-forget worker (same process). For multi-instance, swap to BullMQ.
    setImmediate(() => {
      void this.runImportJob(jobId, entity, rows, actorId).catch(async (err) => {
        await this.prisma.erpImportJob.update({
          where: { id: jobId },
          data: {
            status: 'FAILED',
            errors: [
              { row: 0, message: err instanceof Error ? err.message : 'Import gagal' },
            ] as unknown as Prisma.InputJsonValue,
          },
        });
      });
    });

    return {
      success: true,
      data: {
        jobId: jobId.toString(),
        total: rows.length,
        ok: 0,
        failed: 0,
        errors: [],
        status: 'PENDING',
        async: true,
      } as ImportSummary & { status: string; async: boolean },
    };
  }

  private async runImportJob(
    jobId: bigint,
    entity: string,
    rows: Record<string, string>[],
    actorId?: string,
  ): Promise<void> {
    const adapter = getAdapter(entity);
    if (!adapter) return;

    await this.prisma.erpImportJob.update({
      where: { id: jobId },
      data: { status: 'RUNNING' },
    });

    const errors: RowError[] = [];
    let ok = 0;
    const CHUNK = 100;

    for (let i = 0; i < rows.length; i += 1) {
      const rowNumber = i + 2;
      const row = rows[i];
      try {
        const emptyRequired = adapter.requiredHeaders.filter(
          (h) => (row[h] ?? '').trim() === '',
        );
        if (emptyRequired.length > 0) {
          throw new Error(`Kolom wajib kosong: ${emptyRequired.join(', ')}`);
        }
        const data = adapter.rowToData(row);
        await adapter.insert(this.prisma, data, actorId);
        ok += 1;
      } catch (err) {
        errors.push({ row: rowNumber, message: this.toRowMessage(err) });
      }

      // Progress every CHUNK rows
      if ((i + 1) % CHUNK === 0 || i === rows.length - 1) {
        await this.prisma.erpImportJob.update({
          where: { id: jobId },
          data: {
            rowsOk: ok,
            rowsFailed: errors.length,
            errors: errors.slice(0, MAX_REPORTED_ERRORS) as unknown as Prisma.InputJsonValue,
          },
        });
      }
    }

    const failed = errors.length;
    const status = failed === 0 ? 'COMPLETED' : ok === 0 ? 'FAILED' : 'PARTIAL';
    await this.prisma.erpImportJob.update({
      where: { id: jobId },
      data: {
        status,
        rowsOk: ok,
        rowsFailed: failed,
        errors: errors.slice(0, MAX_REPORTED_ERRORS) as unknown as Prisma.InputJsonValue,
      },
    });
  }

  private toRowMessage(err: unknown): string {
    if (err instanceof Prisma.PrismaClientKnownRequestError) {
      if (err.code === 'P2002') {
        const target = (err.meta?.target as string[] | undefined)?.join(', ') ?? 'kode';
        return `Duplikat: ${target} sudah ada`;
      }
      if (err.code === 'P2003') return 'Referensi (foreign key) tidak valid';
      return `Galat database (${err.code})`;
    }
    if (err instanceof Error) return err.message;
    return 'Galat tidak diketahui';
  }
}
