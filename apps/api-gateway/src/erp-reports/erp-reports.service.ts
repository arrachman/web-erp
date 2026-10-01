import {
  BadRequestException,
  Injectable,
  NotFoundException,
} from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { ReportEngineService } from '../erp-report-engine/report-engine.service';
import { datasetColumns, datasetContext } from '../erp-report-engine/dataset-adapter';
import { buildTableTemplate } from '../erp-report-engine/template-builder';
import { sampleDataset } from './report-preview-sample';
import { ReportColumnsResolver } from './report-columns-resolver';
import { CreateErpReportDto } from './dto/create-report.dto';
import { ExecuteSqlDto } from './dto/execute-sql.dto';
import { QueryErpReportDto } from './dto/query-report.dto';
import { UpdateErpReportDto } from './dto/update-report.dto';

const FORBIDDEN_KEYWORDS = /\b(insert|update|delete|drop|truncate|alter|create|grant|revoke|execute|exec|pg_read_file|pg_ls_dir|copy)\b/i;

@Injectable()
export class ErpReportsService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly engine: ReportEngineService,
    private readonly columnsResolver: ReportColumnsResolver,
  ) {}

  /**
   * Materialize an auto-template into explicit, editable bands built from the bound
   * report's REAL columns — so the Report Designer can band-edit it and the live
   * render matches. Requires the template to have a `reportKey`.
   */
  async materializeTemplate(id: bigint, userId?: bigint) {
    const rec = await this.findOne(id);
    if (!rec.reportKey) {
      throw new BadRequestException('Template belum terikat ke laporan (reportKey kosong)');
    }
    const columns = await this.columnsResolver.resolve(rec.reportKey);
    const stored = (rec.templateJson ?? {}) as Record<string, unknown>;
    const template = buildTableTemplate({
      name: (stored.name as string) ?? rec.name,
      module: rec.module,
      columns,
      pageSize: stored.pageSize as never,
      orientation: stored.orientation as never,
      margins: stored.margins as never,
    });
    return this.update(id, { templateJson: template as unknown as Record<string, unknown> }, userId);
  }

  /**
   * Render a template (as currently edited in the designer) to a PDF with sample
   * data — backs the "Preview PDF" button. Throws if the template can't render.
   */
  async previewTemplate(templateJson: Record<string, unknown>): Promise<Buffer> {
    const ds = sampleDataset();
    const buffer = await this.engine.renderStoredTemplate(
      templateJson,
      datasetColumns(ds),
      datasetContext(ds),
    );
    if (!buffer) throw new BadRequestException('Template tidak dapat dirender');
    return buffer;
  }

  async findAll(query: QueryErpReportDto) {
    const { page = 1, limit = 20, search, module, reportKey, isActive, sortBy = 'createdAt', sortDir = 'desc' } = query;
    const skip = (page - 1) * limit;
    const where: any = { deletedAt: null };
    if (search) where.OR = [{ name: { contains: search, mode: 'insensitive' } }, { code: { equals: search, mode: 'insensitive' } }];
    if (module) where.module = module;
    if (reportKey) where.reportKey = reportKey;
    if (isActive !== undefined) where.isActive = isActive;

    const [items, total] = await Promise.all([
      this.prisma.erpRptTemplate.findMany({ where, skip, take: limit, orderBy: { [sortBy]: sortDir } }),
      this.prisma.erpRptTemplate.count({ where }),
    ]);
    return { success: true, data: items, meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 } };
  }

  async findOne(id: bigint) {
    const rec = await this.prisma.erpRptTemplate.findFirst({ where: { id, deletedAt: null } });
    if (!rec) throw new NotFoundException(`Report template ${id} not found`);
    return rec;
  }

  /**
   * Resolve the active template bound to a report (`<module>.<report>`). Used by the
   * report export services to drive template-based PDF rendering. Most-recently
   * updated active template wins if several share a reportKey.
   */
  async findActiveByReportKey(reportKey: string) {
    return this.prisma.erpRptTemplate.findFirst({
      where: { reportKey, isActive: true, deletedAt: null },
      orderBy: { updatedAt: 'desc' },
    });
  }

  async create(dto: CreateErpReportDto, userId?: bigint) {
    return this.prisma.erpRptTemplate.create({
      data: {
        code: dto.code,
        name: dto.name,
        module: dto.module,
        description: dto.description,
        templateJson: (dto.templateJson ?? {}) as Prisma.InputJsonValue,
        reportKey: dto.reportKey,
        isActive: dto.isActive ?? true,
        createdById: userId,
        updatedById: userId,
      },
    });
  }

  async update(id: bigint, dto: UpdateErpReportDto, userId?: bigint) {
    await this.findOne(id);
    return this.prisma.erpRptTemplate.update({
      where: { id },
      data: {
        ...(dto.code !== undefined && { code: dto.code }),
        ...(dto.name !== undefined && { name: dto.name }),
        ...(dto.module !== undefined && { module: dto.module }),
        ...(dto.description !== undefined && { description: dto.description }),
        ...(dto.templateJson !== undefined && { templateJson: dto.templateJson as Prisma.InputJsonValue }),
        ...(dto.reportKey !== undefined && { reportKey: dto.reportKey }),
        ...(dto.isActive !== undefined && { isActive: dto.isActive }),
        updatedById: userId,
      },
    });
  }

  async remove(id: bigint, userId?: bigint) {
    await this.findOne(id);
    return this.prisma.erpRptTemplate.update({
      where: { id },
      data: { deletedAt: new Date(), updatedById: userId },
    });
  }

  async executeSql(dto: ExecuteSqlDto) {
    const { sql, params = {}, limit = 200 } = dto;
    const normalized = sql.trim().toLowerCase().replace(/\s+/g, ' ');

    if (!normalized.startsWith('select') && !normalized.startsWith('with')) {
      throw new BadRequestException('Only SELECT queries are allowed');
    }
    if (FORBIDDEN_KEYWORDS.test(sql)) {
      throw new BadRequestException('Query contains forbidden SQL keywords');
    }

    // Replace named params :name → positional $1, $2, ...
    const paramNames: string[] = [];
    const paramValues: unknown[] = [];
    // Negative lookbehind (?<!:) prevents matching :name inside ::casttype (PostgreSQL cast syntax)
    const paramPattern = /(?<!:):([a-zA-Z_][a-zA-Z0-9_]*)/g;
    let sqlWithPositional = sql.replace(paramPattern, (_match, name: string) => {
      if (!paramNames.includes(name)) {
        paramNames.push(name);
        paramValues.push(params[name] ?? null);
      }
      return `$${paramNames.indexOf(name) + 1}`;
    });

    // Add LIMIT guard so a heavy query can't DOS the server
    if (!/\blimit\b/i.test(sqlWithPositional)) {
      sqlWithPositional += ` LIMIT ${limit}`;
    }

    try {
      const rows = await this.prisma.$queryRawUnsafe(sqlWithPositional, ...paramValues);
      const data = rows as Record<string, unknown>[];
      // Convert bigint → string for JSON serialization
      const serialized = data.map(row =>
        Object.fromEntries(
          Object.entries(row).map(([k, v]) => [k, typeof v === 'bigint' ? v.toString() : v]),
        ),
      );
      return { rows: serialized, count: serialized.length, columns: serialized[0] ? Object.keys(serialized[0]) : [] };
    } catch (err: any) {
      throw new BadRequestException(`SQL error: ${err?.message ?? 'Unknown error'}`);
    }
  }
}
