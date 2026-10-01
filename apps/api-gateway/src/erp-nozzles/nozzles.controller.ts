import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpNozzleDto, BulkStatusErpNozzleDto } from './dto/bulk-nozzles.dto';
import { CreateErpNozzleDto } from './dto/create-nozzles.dto';
import { QueryErpNozzleDto } from './dto/query-nozzles.dto';
import { UpdateErpNozzleDto } from './dto/update-nozzles.dto';
import { ErpNozzlesService } from './nozzles.service';

@ApiTags('ERP Nozzles')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/nozzles')
export class ErpNozzlesController {
  constructor(private readonly service: ErpNozzlesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Nozzle' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpNozzleDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Nozzle' })
  findAll(@Query() query: QueryErpNozzleDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Nozzle' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Nozzle' })
  update(@Param('id') id: string, @Body() dto: UpdateErpNozzleDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpNozzleDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpNozzleDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Nozzle' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
