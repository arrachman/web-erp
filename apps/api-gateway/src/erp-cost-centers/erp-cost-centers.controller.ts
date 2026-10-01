import {
  Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards,
} from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpCostCenterDto, BulkStatusErpCostCenterDto } from './dto/bulk-erp-cost-center.dto';
import { CreateErpCostCenterDto } from './dto/create-erp-cost-center.dto';
import { QueryErpCostCenterDto } from './dto/query-erp-cost-center.dto';
import { UpdateErpCostCenterDto } from './dto/update-erp-cost-center.dto';
import { ErpCostCentersService } from './erp-cost-centers.service';

@ApiTags('ERP Cost centerons')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/cost-centers')
export class ErpCostCentersController {
  constructor(private readonly service: ErpCostCentersService) {}

  @Post()
  @ApiOperation({ summary: 'Create ERP CostCenter' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpCostCenterDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List ERP CostCenters' })
  findAll(@Query() query: QueryErpCostCenterDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP CostCenter' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ERP CostCenter' })
  update(@Param('id') id: string, @Body() dto: UpdateErpCostCenterDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate ERP CostCenters' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpCostCenterDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete ERP CostCenters' })
  bulkDelete(@Body() dto: BulkErpCostCenterDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete ERP CostCenter' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
