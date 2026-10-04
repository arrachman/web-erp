import {
  Body,
  Controller,
  Delete,
  Get,
  Param,
  Post,
  Query,
  Request,
  UseGuards,
} from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import {
  CreateVdpDatasetDto,
  ImportVdpRowsDto,
  QueryVdpRowsDto,
} from './dto/vdp.dto';
import { ErpMfgVdpService } from './erp-mfg-vdp.service';

@ApiTags('ERP Mfg VDP')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/mfg/print-jobs/:jobId/vdp-datasets')
export class ErpMfgJobVdpController {
  constructor(private readonly service: ErpMfgVdpService) {}

  @Get()
  @ApiOperation({ summary: 'Dataset VDP sebuah job + tahap job & oplah' })
  list(@Param('jobId') jobId: string) {
    return this.service.listForJob(jobId);
  }

  @Post()
  @ApiOperation({ summary: 'Buat dataset VDP untuk job' })
  create(@Param('jobId') jobId: string, @Body() dto: CreateVdpDatasetDto, @Request() req: any) {
    return this.service.create(jobId, dto, req.user?.id);
  }
}

@ApiTags('ERP Mfg VDP')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/mfg/vdp-datasets')
export class ErpMfgVdpController {
  constructor(private readonly service: ErpMfgVdpService) {}

  @Post(':id/import')
  @ApiOperation({ summary: 'Impor baris (CSV / rows[]) — menggantikan baris lama, versi naik' })
  importRows(@Param('id') id: string, @Body() dto: ImportVdpRowsDto) {
    return this.service.importRows(id, dto);
  }

  @Get(':id/rows')
  @ApiOperation({ summary: 'Baris dataset (paginasi; onlyInvalid untuk yang gagal validasi)' })
  rows(@Param('id') id: string, @Query() query: QueryVdpRowsDto) {
    return this.service.listRows(id, query);
  }

  @Post(':id/lock')
  @ApiOperation({ summary: 'Kunci dataset secara manual' })
  lock(@Param('id') id: string) {
    return this.service.lock(id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Hapus dataset (soft delete; ditolak bila terkunci)' })
  remove(@Param('id') id: string) {
    return this.service.remove(id);
  }
}
