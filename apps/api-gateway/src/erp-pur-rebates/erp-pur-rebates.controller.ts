import {
  Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards,
} from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { CreatePurRebateDto } from './dto/create-pur-rebate.dto';
import { BulkRebateIdsDto, BulkRebateStatusDto } from './dto/bulk-pur-rebate.dto';
import { QueryPurRebateDto } from './dto/query-pur-rebate.dto';
import { UpdatePurRebateDto } from './dto/update-pur-rebate.dto';
import { ErpPurRebatesService } from './erp-pur-rebates.service';

@ApiTags('ERP Pur Rebates')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/pur/rebates')
export class ErpPurRebatesController {
  constructor(private readonly service: ErpPurRebatesService) {}

  @Get('accrual')
  @ApiOperation({ summary: 'Accrued rebate per agreement for a year' })
  accrual(@Query('year') year?: string, @Query('supplierId') supplierId?: string) {
    return this.service.accrual(year ? Number(year) : undefined, supplierId);
  }

  @Get()
  @ApiOperation({ summary: 'List supplier rebate agreements' })
  list(@Query() q: QueryPurRebateDto) {
    return this.service.list(q);
  }

  @Post()
  @ApiOperation({ summary: 'Create a supplier rebate agreement' })
  create(@Body() dto: CreatePurRebateDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Patch('bulk-status')
  @ApiOperation({ summary: 'Bulk activate/deactivate rebate agreements' })
  bulkStatus(@Body() dto: BulkRebateStatusDto) {
    return this.service.bulkStatus(dto.ids, dto.isActive);
  }

  @Post('bulk-delete')
  @ApiOperation({ summary: 'Bulk delete rebate agreements (soft)' })
  bulkRemove(@Body() dto: BulkRebateIdsDto) {
    return this.service.bulkRemove(dto.ids);
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update a supplier rebate agreement' })
  update(@Param('id') id: string, @Body() dto: UpdatePurRebateDto, @Request() req: any) {
    return this.service.update(id, dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete a supplier rebate agreement (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(id, req.user?.id);
  }
}
