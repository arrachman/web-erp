import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpOtherCostDto, BulkStatusErpOtherCostDto } from './dto/bulk-other-costs.dto';
import { CreateErpOtherCostDto } from './dto/create-other-costs.dto';
import { QueryErpOtherCostDto } from './dto/query-other-costs.dto';
import { UpdateErpOtherCostDto } from './dto/update-other-costs.dto';
import { ErpOtherCostsService } from './other-costs.service';

@ApiTags('ERP Other Costs')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/other-costs')
export class ErpOtherCostsController {
  constructor(private readonly service: ErpOtherCostsService) {}

  @Post()
  @ApiOperation({ summary: 'Create Other Cost' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpOtherCostDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Other Cost' })
  findAll(@Query() query: QueryErpOtherCostDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Other Cost' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Other Cost' })
  update(@Param('id') id: string, @Body() dto: UpdateErpOtherCostDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpOtherCostDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpOtherCostDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Other Cost' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
