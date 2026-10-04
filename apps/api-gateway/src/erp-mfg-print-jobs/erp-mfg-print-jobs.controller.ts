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
  AdvancePrintJobDto,
  ChecklistPrintJobDto,
  CreatePrintJobDto,
  QueryPrintJobsDto,
  UpdatePrintJobDto,
} from './dto/print-job.dto';
import { ErpMfgPrintJobsService } from './erp-mfg-print-jobs.service';

@ApiTags('ERP Mfg Print Jobs')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/mfg/print-jobs')
export class ErpMfgPrintJobsController {
  constructor(private readonly service: ErpMfgPrintJobsService) {}

  @Get()
  @ApiOperation({ summary: 'Daftar job cetak (server-driven)' })
  findAll(@Query() query: QueryPrintJobsDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Detail job cetak + checklist + log tahap' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(id);
  }

  @Post()
  @ApiOperation({ summary: 'Buat job cetak (membuat Work Order + profil cetak)' })
  create(@Body() dto: CreatePrintJobDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Ubah spesifikasi job' })
  update(@Param('id') id: string, @Body() dto: UpdatePrintJobDto, @Request() req: any) {
    return this.service.update(id, dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Hapus job (soft delete, hanya PRE_PRESS)' })
  remove(@Param('id') id: string) {
    return this.service.remove(id);
  }

  @Post(':id/checklist')
  @ApiOperation({ summary: 'Centang/lepas item checklist pre-press' })
  checklist(@Param('id') id: string, @Body() dto: ChecklistPrintJobDto, @Request() req: any) {
    return this.service.setChecklist(id, dto, req.user?.id);
  }

  @Post(':id/advance')
  @ApiOperation({ summary: 'Pindah tahap (gerbang checklist untuk PRE_PRESS -> CETAK)' })
  advance(@Param('id') id: string, @Body() dto: AdvancePrintJobDto, @Request() req: any) {
    return this.service.advance(id, dto, req.user?.id);
  }
}
