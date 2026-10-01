/**
 * Warehouse (M3) reports API. One generic endpoint set serves every report:
 *   GET /erp/inv/reports                  → catalog (key, title, group)
 *   GET /erp/inv/reports/:key             → ReportDataset JSON (on-screen table)
 *   GET /erp/inv/reports/:key/export      → file download (?format=xlsx|pdf|docx)
 * Filters are passed as query params (dateFrom, dateTo, asOfDate, warehouseId,
 * itemId, status, search, page, limit) and forwarded verbatim to the resolver.
 */

import {
  Controller,
  Get,
  Param,
  Query,
  Res,
  UseGuards,
} from '@nestjs/common';
import type { Response } from 'express';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { InvReportsService } from './inv-reports.service';
import { ReportExportService } from './report-export.service';
import { ReportFilters, ReportFormat } from './report-types';

const FORMATS: ReportFormat[] = ['xlsx', 'pdf', 'docx'];

/** Extract the resolver filters from raw query params (strings). */
function pickFilters(q: Record<string, string | undefined>): ReportFilters {
  const num = (v?: string) => (v != null && v !== '' ? Number(v) : undefined);
  return {
    dateFrom: q.dateFrom || undefined,
    dateTo: q.dateTo || undefined,
    asOfDate: q.asOfDate || undefined,
    warehouseId: q.warehouseId || undefined,
    branchId: q.branchId || undefined,
    itemId: q.itemId || undefined,
    partnerId: q.partnerId || undefined,
    status: q.status || undefined,
    search: q.search || undefined,
    page: num(q.page),
    limit: num(q.limit),
  };
}

@UseGuards(ErpJwtAuthGuard)
@Controller('erp/inv/reports')
export class ErpInvReportsController {
  constructor(
    private readonly reports: InvReportsService,
    private readonly exporter: ReportExportService,
  ) {}

  @Get()
  catalog() {
    return this.reports.list();
  }

  @Get(':key')
  data(@Param('key') key: string, @Query() q: Record<string, string>) {
    return this.reports.getDataset(key, pickFilters(q));
  }

  @Get(':key/export')
  async export(
    @Param('key') key: string,
    @Query() q: Record<string, string>,
    @Res() res: Response,
  ): Promise<void> {
    const format = (FORMATS.includes(q.format as ReportFormat)
      ? q.format
      : 'xlsx') as ReportFormat;
    const dataset = await this.reports.getDataset(key, pickFilters(q));
    const out = await this.exporter.render(dataset, format);
    res.setHeader('Content-Type', out.contentType);
    res.setHeader(
      'Content-Disposition',
      `attachment; filename="${out.filename}"`,
    );
    res.end(out.buffer);
  }
}
