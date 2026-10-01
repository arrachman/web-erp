import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpStockAdjustmentTypeDto, BulkStatusErpStockAdjustmentTypeDto } from './dto/bulk-stock-adjustment-types.dto';
import { CreateErpStockAdjustmentTypeDto } from './dto/create-stock-adjustment-types.dto';
import { QueryErpStockAdjustmentTypeDto } from './dto/query-stock-adjustment-types.dto';
import { UpdateErpStockAdjustmentTypeDto } from './dto/update-stock-adjustment-types.dto';
import { ErpStockAdjustmentTypesService } from './stock-adjustment-types.service';

@ApiTags('ERP Stock Adjustment Types')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/stock-adjustment-types')
export class ErpStockAdjustmentTypesController {
  constructor(private readonly service: ErpStockAdjustmentTypesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Stock Adjustment Type' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpStockAdjustmentTypeDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List Stock Adjustment Type' })
  findAll(@Query() query: QueryErpStockAdjustmentTypeDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get Stock Adjustment Type' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Stock Adjustment Type' })
  update(@Param('id') id: string, @Body() dto: UpdateErpStockAdjustmentTypeDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpStockAdjustmentTypeDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpStockAdjustmentTypeDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Stock Adjustment Type' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
