import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpProductionCategoryDto, BulkStatusErpProductionCategoryDto } from './dto/bulk-production-categories.dto';
import { CreateErpProductionCategoryDto } from './dto/create-production-categories.dto';
import { QueryErpProductionCategoryDto } from './dto/query-production-categories.dto';
import { UpdateErpProductionCategoryDto } from './dto/update-production-categories.dto';
import { ErpProductionCategoriesService } from './production-categories.service';

@ApiTags('ERP Production Category')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/production-categories')
export class ErpProductionCategoriesController {
  constructor(private readonly service: ErpProductionCategoriesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Production Category' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpProductionCategoryDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Production Category' })
  findAll(@Query() query: QueryErpProductionCategoryDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Production Category' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpProductionCategoryDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Production Category' })
  update(@Param('id') id: string, @Body() dto: UpdateErpProductionCategoryDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpProductionCategoryDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Production Category' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
