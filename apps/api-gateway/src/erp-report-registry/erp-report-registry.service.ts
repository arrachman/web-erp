import {
  BadRequestException,
  Injectable,
  NotFoundException,
} from '@nestjs/common';
import { PrismaService } from '../prisma/prisma.service';
import { ReportEngineService } from '../erp-report-engine/report-engine.service';
import { ReportFormatService } from '../erp-report-engine/report-format.service';
import { isTemplateV2, type ReportData } from '../erp-report-engine/engine-types-v2';
import { ReportProviderRegistry } from './report-data-provider';
import type { RenderRegistryReportDto } from './dto/render-report.dto';

export interface RegistryListItem {
  code: string;
  title: string;
  reportKey: string | null;
  legacyModule: string;
  erpModule: string | null;
  translationStatus: string;
  urutan: number;
}

interface ParamDef {
  name: string;
  label: string;
  source?: string;
}

export interface RegistryDetail extends RegistryListItem {
  params: ParamDef[];
  paramSchema: Array<{ name: string; label: string; type: 'date' | 'number' | 'text' }>;
  pageSetup: unknown;
  exportFormats: string[];
  hasTemplate: boolean;
  hasDataProvider: boolean;
}

@Injectable()
export class ErpReportRegistryService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly engine: ReportEngineService,
    private readonly formatService: ReportFormatService,
    private readonly providers: ReportProviderRegistry,
  ) {}

  /** Combo-box feed: active registry rows of one module, ordered by urutan. */
  async list(module: string): Promise<RegistryListItem[]> {
    const rows = await this.prisma.erpM0Report.findMany({
      where: {
        isActive: true,
        deletedAt: null,
        OR: [
          { legacyModule: module.toLowerCase() },
          { legacyModule: module.toUpperCase() },
          { erpModule: module.toUpperCase() },
        ],
      },
      orderBy: { urutan: 'asc' },
    });
    return rows.map((r) => ({
      code: r.code,
      title: r.title,
      reportKey: r.reportKey,
      legacyModule: r.legacyModule,
      erpModule: r.erpModule,
      translationStatus: r.translationStatus,
      urutan: r.urutan,
    }));
  }

  async detail(code: string): Promise<RegistryDetail> {
    const row = await this.findRow(code);
    const params = (Array.isArray(row.params) ? row.params : []) as unknown as ParamDef[];
    const exportFormats =
      row.exportFormats && typeof row.exportFormats === 'object'
        ? Object.entries(row.exportFormats as Record<string, unknown>)
            .filter(([, v]) => v === true)
            .map(([k]) => k.toLowerCase())
        : ['pdf', 'word', 'excel', 'html'];
    return {
      code: row.code,
      title: row.title,
      reportKey: row.reportKey,
      legacyModule: row.legacyModule,
      erpModule: row.erpModule,
      translationStatus: row.translationStatus,
      urutan: row.urutan,
      params,
      paramSchema: params.map((p) => ({
        name: p.name,
        label: p.label,
        type: p.name.startsWith('period_') ? 'date' : 'text',
      })),
      pageSetup: row.pageSetup ?? null,
      exportFormats,
      hasTemplate: !!row.rptTemplateId,
      hasDataProvider: row.reportKey ? !!this.providers.find(row.reportKey) : false,
    };
  }

  async render(code: string, dto: RenderRegistryReportDto, userName?: string) {
    const row = await this.findRow(code);
    if (!row.reportKey) {
      throw new BadRequestException(`Laporan ${code} belum dikonversi (tidak ada report_key)`);
    }
    const stored = await this.engine.resolveActiveTemplate(row.reportKey);
    if (!stored || !isTemplateV2(stored)) {
      throw new BadRequestException(`Template v2 untuk ${code} tidak ditemukan`);
    }
    const params = dto.params ?? {};
    const provider = this.providers.find(row.reportKey);
    let data: ReportData;
    if (provider) {
      data = await provider.build({
        code: row.code,
        reportKey: row.reportKey,
        title: row.title,
        params,
        userName,
        datasets: stored.datasets ?? [],
      });
    } else {
      // Generic fallback: declared datasets render empty (header-only).
      const datasets: Record<string, Array<Record<string, unknown>>> = {};
      for (const ds of stored.datasets ?? []) datasets[ds.name] = [];
      data = { datasets, paramLines: this.deriveParamLines(row.params, params) };
    }
    const [settings, company] = await Promise.all([
      this.formatService.getSettings(),
      this.formatService.getCompany(),
    ]);
    const paramLines = data.paramLines ?? this.deriveParamLines(row.params, params);
    const result = await this.engine.renderV2(
      stored,
      { ...data, paramLines },
      {
        company,
        report: { title: row.title, code: row.code, paramLines },
        params,
        userName,
        now: new Date(),
      },
      settings,
      dto.format,
      dto.mode ?? 'layout',
    );
    return { ...result, filename: `${row.code.toLowerCase()}.${result.ext}` };
  }

  private async findRow(code: string) {
    const row = await this.prisma.erpM0Report.findFirst({
      where: { code, isActive: true, deletedAt: null },
    });
    if (!row) throw new NotFoundException(`Laporan ${code} tidak ditemukan`);
    return row;
  }

  /** Human-readable parameter summary lines for report headers (param1..5). */
  deriveParamLines(paramsJson: unknown, params: Record<string, unknown>): string[] {
    const defs = (Array.isArray(paramsJson) ? paramsJson : []) as unknown as ParamDef[];
    const lines: string[] = [];
    for (const def of defs) {
      const v = params[def.name];
      if (v === undefined || v === null || v === '') continue;
      lines.push(`${def.label}: ${String(v)}`);
    }
    return lines.slice(0, 5);
  }
}
