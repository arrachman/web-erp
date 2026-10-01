import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpPartnerSubCategoryDto, BulkStatusErpPartnerSubCategoryDto } from './dto/bulk-partner-sub-categories.dto';
import { CreateErpPartnerSubCategoryDto } from './dto/create-partner-sub-categories.dto';
import { QueryErpPartnerSubCategoryDto } from './dto/query-partner-sub-categories.dto';
import { UpdateErpPartnerSubCategoryDto } from './dto/update-partner-sub-categories.dto';
import { ErpPartnerSubCategoriesService } from './partner-sub-categories.service';

@ApiTags('ERP Partner Sub Categories')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/partner-sub-categories')
export class ErpPartnerSubCategoriesController {
  constructor(private readonly service: ErpPartnerSubCategoriesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Partner Sub Category' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpPartnerSubCategoryDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Partner Sub Category' })
  findAll(@Query() query: QueryErpPartnerSubCategoryDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Partner Sub Category' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Partner Sub Category' })
  update(@Param('id') id: string, @Body() dto: UpdateErpPartnerSubCategoryDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpPartnerSubCategoryDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpPartnerSubCategoryDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Partner Sub Category' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
