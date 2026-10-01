import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpCommissionDto, BulkStatusErpCommissionDto } from './dto/bulk-commissions.dto';
import { CreateErpCommissionDto } from './dto/create-commissions.dto';
import { QueryErpCommissionDto } from './dto/query-commissions.dto';
import { UpdateErpCommissionDto } from './dto/update-commissions.dto';
import { ErpCommissionsService } from './commissions.service';

@ApiTags('ERP Commissions')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/commissions')
export class ErpCommissionsController {
  constructor(private readonly service: ErpCommissionsService) {}

  @Post()
  @ApiOperation({ summary: 'Create Commission' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpCommissionDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Commission' })
  findAll(@Query() query: QueryErpCommissionDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Commission' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Commission' })
  update(@Param('id') id: string, @Body() dto: UpdateErpCommissionDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpCommissionDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpCommissionDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Commission' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
