import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpProvinceDto, BulkStatusErpProvinceDto } from './dto/bulk-provinces.dto';
import { CreateErpProvinceDto } from './dto/create-provinces.dto';
import { QueryErpProvinceDto } from './dto/query-provinces.dto';
import { UpdateErpProvinceDto } from './dto/update-provinces.dto';
import { ErpProvincesService } from './provinces.service';

@ApiTags('ERP Provinces')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/provinces')
export class ErpProvincesController {
  constructor(private readonly service: ErpProvincesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Province' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpProvinceDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Province' })
  findAll(@Query() query: QueryErpProvinceDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Province' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Province' })
  update(@Param('id') id: string, @Body() dto: UpdateErpProvinceDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpProvinceDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpProvinceDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Province' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
