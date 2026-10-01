import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpProductionRouteDto, BulkStatusErpProductionRouteDto } from './dto/bulk-production-routes.dto';
import { CreateErpProductionRouteDto } from './dto/create-production-routes.dto';
import { QueryErpProductionRouteDto } from './dto/query-production-routes.dto';
import { UpdateErpProductionRouteDto } from './dto/update-production-routes.dto';
import { ErpProductionRoutesService } from './production-routes.service';

@ApiTags('ERP Production Routes')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/production-routes')
export class ErpProductionRoutesController {
  constructor(private readonly service: ErpProductionRoutesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Production Route' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpProductionRouteDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Production Route' })
  findAll(@Query() query: QueryErpProductionRouteDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Production Route' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpProductionRouteDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Production Route' })
  update(@Param('id') id: string, @Body() dto: UpdateErpProductionRouteDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpProductionRouteDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Production Route' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
