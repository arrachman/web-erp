import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpExpeditionDto, BulkStatusErpExpeditionDto } from './dto/bulk-expeditions.dto';
import { CreateErpExpeditionDto } from './dto/create-expeditions.dto';
import { QueryErpExpeditionDto } from './dto/query-expeditions.dto';
import { UpdateErpExpeditionDto } from './dto/update-expeditions.dto';
import { ErpExpeditionsService } from './expeditions.service';

@ApiTags('ERP Expeditions')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/expeditions')
export class ErpExpeditionsController {
  constructor(private readonly service: ErpExpeditionsService) {}

  @Post()
  @ApiOperation({ summary: 'Create Expedition' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpExpeditionDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Expedition' })
  findAll(@Query() query: QueryErpExpeditionDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Expedition' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Expedition' })
  update(@Param('id') id: string, @Body() dto: UpdateErpExpeditionDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpExpeditionDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpExpeditionDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Expedition' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
