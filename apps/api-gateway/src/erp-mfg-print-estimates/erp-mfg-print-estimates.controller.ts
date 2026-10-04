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
  CreatePrintEstimateDto,
  QueryPrintEstimatesDto,
  SetPrintEstimateStatusDto,
  UpdatePrintEstimateDto,
} from './dto/print-estimate.dto';
import { ErpMfgPrintEstimatesService } from './erp-mfg-print-estimates.service';

@ApiTags('ERP Mfg Print Estimates')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/mfg/print-estimates')
export class ErpMfgPrintEstimatesController {
  constructor(private readonly service: ErpMfgPrintEstimatesService) {}

  @Get()
  @ApiOperation({ summary: 'Daftar estimasi cetak (server-driven)' })
  findAll(@Query() query: QueryPrintEstimatesDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Detail estimasi cetak + baris komponen' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(id);
  }

  @Post()
  @ApiOperation({ summary: 'Buat estimasi cetak (total dihitung server)' })
  create(@Body() dto: CreatePrintEstimateDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Ubah estimasi (hanya DRAFT)' })
  update(@Param('id') id: string, @Body() dto: UpdatePrintEstimateDto, @Request() req: any) {
    return this.service.update(id, dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Hapus estimasi (soft delete, hanya DRAFT)' })
  remove(@Param('id') id: string) {
    return this.service.remove(id);
  }

  @Post(':id/status')
  @ApiOperation({ summary: 'Ubah status: ISSUED / CANCELLED / kembali DRAFT' })
  setStatus(@Param('id') id: string, @Body() dto: SetPrintEstimateStatusDto) {
    return this.service.setStatus(id, dto.status);
  }

  @Post(':id/convert-to-quotation')
  @ApiOperation({ summary: 'Jadikan Penawaran (quotation SQ) dari estimasi' })
  convert(@Param('id') id: string, @Request() req: any) {
    return this.service.convertToQuotation(id, req.user?.id);
  }
}
