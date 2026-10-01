import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpPriceCategoryDto, BulkStatusErpPriceCategoryDto } from './dto/bulk-price-categories.dto';
import { CreateErpPriceCategoryDto } from './dto/create-price-categories.dto';
import { QueryErpPriceCategoryDto } from './dto/query-price-categories.dto';
import { UpdateErpPriceCategoryDto } from './dto/update-price-categories.dto';
import { ErpPriceCategoriesService } from './price-categories.service';

@ApiTags('ERP Price Categories')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/price-categories')
export class ErpPriceCategoriesController {
  constructor(private readonly service: ErpPriceCategoriesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Price Category' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpPriceCategoryDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Price Category' })
  findAll(@Query() query: QueryErpPriceCategoryDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Price Category' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Price Category' })
  update(@Param('id') id: string, @Body() dto: UpdateErpPriceCategoryDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpPriceCategoryDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpPriceCategoryDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Price Category' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
