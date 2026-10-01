import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpWorkEstimateDto, BulkStatusErpWorkEstimateDto } from './dto/bulk-work-estimates.dto';
import { CreateErpWorkEstimateDto } from './dto/create-work-estimates.dto';
import { QueryErpWorkEstimateDto } from './dto/query-work-estimates.dto';
import { UpdateErpWorkEstimateDto } from './dto/update-work-estimates.dto';
import { ErpWorkEstimatesService } from './work-estimates.service';

@ApiTags('ERP Work Estimates')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/work-estimates')
export class ErpWorkEstimatesController {
  constructor(private readonly service: ErpWorkEstimatesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Work Estimate' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpWorkEstimateDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Work Estimate' })
  findAll(@Query() query: QueryErpWorkEstimateDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Work Estimate' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpWorkEstimateDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Work Estimate' })
  update(@Param('id') id: string, @Body() dto: UpdateErpWorkEstimateDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpWorkEstimateDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Work Estimate' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
