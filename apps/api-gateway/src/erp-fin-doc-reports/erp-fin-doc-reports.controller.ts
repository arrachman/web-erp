import { Controller, Get, Param, Query, Res, UseGuards } from '@nestjs/common';
import type { Response } from 'express';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { QueryFinDocReportDto } from './dto/query-fin-doc-report.dto';
import { FinDocReportExportService } from './fin-doc-report-export.service';
import { FinDocReportsService } from './fin-doc-reports.service';
import { ReportFormat } from './report-types';

const FORMATS: ReportFormat[] = ['xlsx', 'pdf', 'docx'];

@UseGuards(ErpJwtAuthGuard)
@Controller('erp/fin/doc-reports')
export class ErpFinDocReportsController {
  constructor(
    private readonly reports: FinDocReportsService,
    private readonly exporter: FinDocReportExportService,
  ) {}

  @Get()
  catalog() {
    return this.reports.list();
  }

  @Get(':key/export')
  async export(
    @Param('key') key: string,
    @Query() query: QueryFinDocReportDto,
    @Res() res: Response,
  ): Promise<void> {
    const resolvedFormat = FORMATS.includes(query.format as ReportFormat)
      ? (query.format as ReportFormat)
      : 'xlsx';
    const document = await this.reports.getDataset(key, query.toFilters());
    const output = await this.exporter.render(document, resolvedFormat);
    res.setHeader('Content-Type', output.contentType);
    res.setHeader('Content-Disposition', `attachment; filename="${output.filename}"`);
    res.end(output.buffer);
  }

  @Get(':key')
  data(@Param('key') key: string, @Query() query: QueryFinDocReportDto) {
    return this.reports.getDataset(key, query.toFilters());
  }
}
