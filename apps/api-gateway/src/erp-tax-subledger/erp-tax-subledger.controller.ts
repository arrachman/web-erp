import {
  Body,
  Controller,
  Get,
  Param,
  Post,
  Query,
  Request,
  Res,
  UseGuards,
} from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { Response } from 'express';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import {
  AssignFakturDto,
  CreateTaxEntryDto,
  CreateWhtCertificateDto,
  MonthQueryDto,
  QueryTaxEntriesDto,
  SetTaxEntryStatusDto,
} from './dto/tax-subledger.dto';
import { ErpTaxSubledgerService } from './erp-tax-subledger.service';

@ApiTags('ERP Tax Subledger')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/tax-subledger')
export class ErpTaxSubledgerController {
  constructor(private readonly service: ErpTaxSubledgerService) {}

  @Get('entries')
  @ApiOperation({ summary: 'PPN/PPh subledger entries (per transaction)' })
  entries(@Query() query: QueryTaxEntriesDto) {
    return this.service.findEntries(query);
  }

  @Post('sync')
  @ApiOperation({ summary: 'Build subledger entries from posted sales/purchase invoices (idempotent)' })
  sync(@Request() req: any) {
    return this.service.sync(req.user?.id);
  }

  @Post('entries')
  @ApiOperation({ summary: 'Record a manual tax entry (e.g. expected PPh withholding by bendahara)' })
  createEntry(@Body() dto: CreateTaxEntryDto, @Request() req: any) {
    return this.service.createEntry(dto, req.user?.id);
  }

  @Post('entries/:id/faktur')
  @ApiOperation({ summary: 'Assign faktur pajak number/date (propagates to the source invoice)' })
  faktur(@Param('id') id: string, @Body() dto: AssignFakturDto) {
    return this.service.assignFaktur(BigInt(id), dto);
  }

  @Post('entries/:id/status')
  @ApiOperation({ summary: 'Confirm / report / cancel a subledger entry' })
  status(@Param('id') id: string, @Body() dto: SetTaxEntryStatusDto) {
    return this.service.setStatus(BigInt(id), dto);
  }

  @Get('coretax-export')
  @ApiOperation({ summary: 'Coretax CSV export of PPN Keluaran for one month' })
  async coretax(@Query() query: MonthQueryDto, @Res() res: Response) {
    const { csv, fileName } = await this.service.coretaxCsv(query.year, query.month);
    res.setHeader('Content-Type', 'text/csv; charset=utf-8');
    res.setHeader('Content-Disposition', `attachment; filename="${fileName}"`);
    res.send(csv);
  }

  @Get('withholding')
  @ApiOperation({ summary: 'PPh 22/23 expected vs bukti potong received (reconciliation)' })
  withholding() {
    return this.service.withholding();
  }

  @Get('certificates')
  @ApiOperation({ summary: 'Bukti potong received from bendahara' })
  certificates() {
    return this.service.listCertificates();
  }

  @Post('certificates')
  @ApiOperation({ summary: 'Record a bukti potong (withholding certificate)' })
  createCertificate(@Body() dto: CreateWhtCertificateDto, @Request() req: any) {
    return this.service.createCertificate(dto, req.user?.id);
  }

  @Post('certificates/:id/cancel')
  @ApiOperation({ summary: 'Cancel a bukti potong' })
  cancelCertificate(@Param('id') id: string) {
    return this.service.cancelCertificate(BigInt(id));
  }

  @Get('monthly-report')
  @ApiOperation({ summary: 'Monthly tax report for the tax consultant' })
  monthly(@Query() query: MonthQueryDto) {
    return this.service.monthlyReport(query.year, query.month);
  }

  @Get('monthly-report/export')
  @ApiOperation({ summary: 'Monthly tax report as CSV' })
  async monthlyExport(@Query() query: MonthQueryDto, @Res() res: Response) {
    const { csv, fileName } = await this.service.monthlyCsv(query.year, query.month);
    res.setHeader('Content-Type', 'text/csv; charset=utf-8');
    res.setHeader('Content-Disposition', `attachment; filename="${fileName}"`);
    res.send(csv);
  }
}
