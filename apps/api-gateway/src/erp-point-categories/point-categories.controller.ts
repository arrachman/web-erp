import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpPointCategoryDto, BulkStatusErpPointCategoryDto } from './dto/bulk-point-categories.dto';
import { CreateErpPointCategoryDto } from './dto/create-point-categories.dto';
import { QueryErpPointCategoryDto } from './dto/query-point-categories.dto';
import { UpdateErpPointCategoryDto } from './dto/update-point-categories.dto';
import { ErpPointCategoriesService } from './point-categories.service';

@ApiTags('ERP Point Category')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/point-categories')
export class ErpPointCategoriesController {
  constructor(private readonly service: ErpPointCategoriesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Point Category' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpPointCategoryDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Point Category' })
  findAll(@Query() query: QueryErpPointCategoryDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Point Category' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpPointCategoryDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Point Category' })
  update(@Param('id') id: string, @Body() dto: UpdateErpPointCategoryDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpPointCategoryDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Point Category' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
