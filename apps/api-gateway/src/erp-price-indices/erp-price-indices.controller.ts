import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpPriceIndexDto, BulkStatusErpPriceIndexDto } from './dto/bulk-price-index.dto';
import { CreateErpPriceIndexDto } from './dto/create-price-index.dto';
import { QueryErpPriceIndexDto } from './dto/query-price-index.dto';
import { UpdateErpPriceIndexDto } from './dto/update-price-index.dto';
import { ErpPriceIndicesService } from './erp-price-indices.service';

@ApiTags('ERP Price Indices')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/price-indices')
export class ErpPriceIndicesController {
  constructor(private readonly service: ErpPriceIndicesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Price Index' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpPriceIndexDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Price Index' })
  findAll(@Query() query: QueryErpPriceIndexDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Price Index' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Price Index' })
  update(@Param('id') id: string, @Body() dto: UpdateErpPriceIndexDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpPriceIndexDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpPriceIndexDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Price Index' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
