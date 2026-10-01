import {
  BadRequestException,
  Controller,
  Get,
  Query,
  Res,
  UseGuards,
} from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { Response } from 'express';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { QueryReportDto } from './dto/query-report.dto';
import { ErpFinReportsExtService } from './erp-fin-reports-ext.service';
import { ErpFinReportsService } from './erp-fin-reports.service';
import { ReportExportService } from './report-export.service';
import { ReportDocument, ReportFormat } from './report-types';

@ApiTags('ERP Fin Reports')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/fin/reports')
export class ErpFinReportsController {
  constructor(
    private readonly service: ErpFinReportsService,
    private readonly ext: ErpFinReportsExtService,
    private readonly exporter: ReportExportService,
  ) {}

  @Get('trial-balance')
  @ApiOperation({ summary: 'Trial balance (Neraca Saldo)' })
  async trialBalance(@Query() q: QueryReportDto, @Res({ passthrough: false }) res: Response) {
    if (!q.from || !q.to) throw new BadRequestException('from and to are required');
    const doc = await this.service.buildTrialBalance(q.from, q.to, q.branchId);
    return this.respond(doc, q.format, res);
  }

  @Get('income-statement')
  @ApiOperation({ summary: 'Income statement (Laba Rugi)' })
  async incomeStatement(@Query() q: QueryReportDto, @Res({ passthrough: false }) res: Response) {
    if (!q.from || !q.to) throw new BadRequestException('from and to are required');
    const doc = await this.service.buildIncomeStatement(q.from, q.to, q.branchId);
    return this.respond(doc, q.format, res);
  }

  @Get('balance-sheet')
  @ApiOperation({ summary: 'Balance sheet (Neraca)' })
  async balanceSheet(@Query() q: QueryReportDto, @Res({ passthrough: false }) res: Response) {
    if (!q.asOf) throw new BadRequestException('asOf is required');
    const doc = await this.service.buildBalanceSheet(q.asOf, q.branchId);
    return this.respond(doc, q.format, res);
  }

  @Get('movement-balance')
  @ApiOperation({ summary: 'Movement balance (Neraca Mutasi)' })
  async movementBalance(@Query() q: QueryReportDto, @Res({ passthrough: false }) res: Response) {
    if (!q.from || !q.to) throw new BadRequestException('from and to are required');
    const doc = await this.service.buildMovementBalance(q.from, q.to, q.branchId);
    return this.respond(doc, q.format, res);
  }

  @Get('equity-changes')
  @ApiOperation({ summary: 'Statement of changes in equity (Perubahan Modal)' })
  async equityChanges(@Query() q: QueryReportDto, @Res({ passthrough: false }) res: Response) {
    if (!q.from || !q.to) throw new BadRequestException('from and to are required');
    const doc = await this.service.buildEquityChanges(q.from, q.to, q.branchId);
    return this.respond(doc, q.format, res);
  }

  @Get('general-ledger')
  @ApiOperation({ summary: 'General ledger (Buku Besar)' })
  async generalLedger(@Query() q: QueryReportDto, @Res({ passthrough: false }) res: Response) {
    if (!q.from || !q.to) throw new BadRequestException('from and to are required');
    const doc = await this.service.buildGeneralLedger(q.from, q.to, q.accountId, q.branchId);
    return this.respond(doc, q.format, res);
  }

  @Get('cash-flow')
  @ApiOperation({ summary: 'Cash flow (Arus Kas)' })
  async cashFlow(@Query() q: QueryReportDto, @Res({ passthrough: false }) res: Response) {
    if (!q.from || !q.to) throw new BadRequestException('from and to are required');
    const doc = await this.ext.buildCashFlow(q.from, q.to, q.branchId);
    return this.respond(doc, q.format, res);
  }

  @Get('daily-cash-bank')
  @ApiOperation({ summary: 'Daily cash bank (Kas Bank Harian)' })
  async dailyCashBank(@Query() q: QueryReportDto, @Res({ passthrough: false }) res: Response) {
    if (!q.from || !q.to) throw new BadRequestException('from and to are required');
    const doc = await this.ext.buildDailyCashBank(q.from, q.to, q.branchId);
    return this.respond(doc, q.format, res);
  }

  @Get('ar-card')
  @ApiOperation({ summary: 'AR Card (Kartu Piutang)' })
  async arCard(@Query() q: QueryReportDto, @Res({ passthrough: false }) res: Response) {
    if (!q.from || !q.to) throw new BadRequestException('from and to are required');
    const doc = await this.ext.buildArCard(q.from, q.to, q.partnerId, q.branchId);
    return this.respond(doc, q.format, res);
  }

  @Get('ar-aging')
  @ApiOperation({ summary: 'AR Aging (Analisis Umur Piutang)' })
  async arAging(@Query() q: QueryReportDto, @Res({ passthrough: false }) res: Response) {
    const doc = await this.ext.buildArAging(q.asOf);
    return this.respond(doc, q.format, res);
  }

  @Get('ap-card')
  @ApiOperation({ summary: 'AP Card (Kartu Utang)' })
  async apCard(@Query() q: QueryReportDto, @Res({ passthrough: false }) res: Response) {
    if (!q.from || !q.to) throw new BadRequestException('from and to are required');
    const doc = await this.ext.buildApCard(q.from, q.to, q.partnerId, q.branchId);
    return this.respond(doc, q.format, res);
  }

  @Get('ap-aging')
  @ApiOperation({ summary: 'AP Aging (Analisis Umur Utang)' })
  async apAging(@Query() q: QueryReportDto, @Res({ passthrough: false }) res: Response) {
    const doc = await this.ext.buildApAging(q.asOf);
    return this.respond(doc, q.format, res);
  }

  @Get('giro-maturity')
  @ApiOperation({ summary: 'Giro maturity (Jatuh Tempo Giro)' })
  async giroMaturity(@Query() q: QueryReportDto, @Res({ passthrough: false }) res: Response) {
    const doc = await this.ext.buildGiroMaturity(q.from, q.to);
    return this.respond(doc, q.format, res);
  }

  @Get('budget-realization')
  @ApiOperation({ summary: 'Budget realization (Realisasi Anggaran)' })
  async budgetRealization(@Query() q: QueryReportDto, @Res({ passthrough: false }) res: Response) {
    const doc = await this.ext.buildBudgetRealization(q.from, q.to);
    return this.respond(doc, q.format, res);
  }

  private async respond(
    doc: ReportDocument,
    format: ReportFormat | undefined,
    res: Response,
  ): Promise<void> {
    if (!format || format === 'json') {
      res.json({ success: true, data: doc });
      return;
    }
    const { buffer, contentType, filename } = await this.exporter.render(doc, format);
    res.setHeader('Content-Type', contentType);
    res.setHeader('Content-Disposition', `attachment; filename="${filename}"`);
    res.end(buffer);
  }
}
