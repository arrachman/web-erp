import {
  Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards,
} from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpDivisionDto, BulkStatusErpDivisionDto } from './dto/bulk-erp-division.dto';
import { CreateErpDivisionDto } from './dto/create-erp-division.dto';
import { QueryErpDivisionDto } from './dto/query-erp-division.dto';
import { UpdateErpDivisionDto } from './dto/update-erp-division.dto';
import { ErpDivisionsService } from './erp-divisions.service';

@ApiTags('ERP Divisions')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/divisions')
export class ErpDivisionsController {
  constructor(private readonly service: ErpDivisionsService) {}

  @Post()
  @ApiOperation({ summary: 'Create ERP division' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpDivisionDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List ERP divisions' })
  findAll(@Query() query: QueryErpDivisionDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP division' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ERP division' })
  update(@Param('id') id: string, @Body() dto: UpdateErpDivisionDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate ERP divisions' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpDivisionDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete ERP divisions' })
  bulkDelete(@Body() dto: BulkErpDivisionDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete ERP division' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
