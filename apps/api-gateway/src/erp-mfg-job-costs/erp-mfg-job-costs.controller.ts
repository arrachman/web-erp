import {
  Body,
  Controller,
  Delete,
  Get,
  Param,
  Patch,
  Post,
  Query,
  Request,
  UseGuards,
} from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import {
  CreateJobCostEntryDto,
  QueryJobCostEntriesDto,
  UpdateJobCostEntryDto,
} from './dto/job-cost.dto';
import { ErpMfgJobCostsService } from './erp-mfg-job-costs.service';

@ApiTags('ERP Mfg Job Costs')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/mfg/print-jobs/:jobId/costs')
export class ErpMfgPrintJobCostsController {
  constructor(private readonly service: ErpMfgJobCostsService) {}

  @Get()
  @ApiOperation({ summary: 'Entri biaya aktual sebuah job' })
  list(@Param('jobId') jobId: string, @Query() query: QueryJobCostEntriesDto) {
    return this.service.listEntries(jobId, query);
  }

  @Post()
  @ApiOperation({ summary: 'Tambah entri biaya aktual job' })
  create(
    @Param('jobId') jobId: string,
    @Body() dto: CreateJobCostEntryDto,
    @Request() req: any,
  ) {
    return this.service.createEntry(jobId, dto, req.user?.id);
  }
}

@ApiTags('ERP Mfg Job Costs')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/mfg/print-jobs/:jobId')
export class ErpMfgPrintJobCostActionsController {
  constructor(private readonly service: ErpMfgJobCostsService) {}

  @Get('cost-summary')
  @ApiOperation({ summary: 'Ringkasan HPP: aktual vs estimasi, varians, margin' })
  summary(@Param('jobId') jobId: string) {
    return this.service.summary(jobId);
  }

  @Post('post-cost-journal')
  @ApiOperation({ summary: 'Posting jurnal HPP: Dr Barang Jadi / Cr Barang Dalam Proses' })
  post(@Param('jobId') jobId: string, @Request() req: any) {
    return this.service.postJournal(jobId, req.user?.id);
  }

  @Post('void-cost-journal')
  @ApiOperation({ summary: 'Batalkan jurnal HPP (hapus baris ledger + buka kunci entri)' })
  void(@Param('jobId') jobId: string) {
    return this.service.voidJournal(jobId);
  }
}

@ApiTags('ERP Mfg Job Costs')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/mfg/job-costs')
export class ErpMfgJobCostsController {
  constructor(private readonly service: ErpMfgJobCostsService) {}

  @Get('report')
  @ApiOperation({ summary: 'Laporan HPP & margin semua job' })
  report() {
    return this.service.report();
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Ubah entri biaya' })
  update(@Param('id') id: string, @Body() dto: UpdateJobCostEntryDto, @Request() req: any) {
    return this.service.updateEntry(id, dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Hapus entri biaya (soft delete)' })
  remove(@Param('id') id: string) {
    return this.service.deleteEntry(id);
  }
}
