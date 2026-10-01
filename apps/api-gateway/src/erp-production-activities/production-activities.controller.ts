import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpProductionActivityDto, BulkStatusErpProductionActivityDto } from './dto/bulk-production-activities.dto';
import { CreateErpProductionActivityDto } from './dto/create-production-activities.dto';
import { QueryErpProductionActivityDto } from './dto/query-production-activities.dto';
import { UpdateErpProductionActivityDto } from './dto/update-production-activities.dto';
import { ErpProductionActivitiesService } from './production-activities.service';

@ApiTags('ERP Production Activities')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/production-activities')
export class ErpProductionActivitiesController {
  constructor(private readonly service: ErpProductionActivitiesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Production Activity' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpProductionActivityDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Production Activity' })
  findAll(@Query() query: QueryErpProductionActivityDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Production Activity' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpProductionActivityDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Production Activity' })
  update(@Param('id') id: string, @Body() dto: UpdateErpProductionActivityDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpProductionActivityDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Production Activity' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
