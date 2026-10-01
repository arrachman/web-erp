import {
  Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards,
} from '@nestjs/common';
import { ApiBearerAuth, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpSubDivisionDto, BulkStatusErpSubDivisionDto } from './dto/bulk-erp-sub-division.dto';
import { CreateErpSubDivisionDto } from './dto/create-erp-sub-division.dto';
import { QueryErpSubDivisionDto } from './dto/query-erp-sub-division.dto';
import { UpdateErpSubDivisionDto } from './dto/update-erp-sub-division.dto';
import { ErpSubDivisionsService } from './erp-sub-divisions.service';

@ApiTags('ERP Sub Divisions')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sub-divisions')
export class ErpSubDivisionsController {
  constructor(private readonly service: ErpSubDivisionsService) {}

  @Post()
  create(@Body() dto: CreateErpSubDivisionDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }
  @Get()
  findAll(@Query() query: QueryErpSubDivisionDto) { return this.service.findAll(query); }
  @Get(':id')
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }
  @Patch(':id')
  update(@Param('id') id: string, @Body() dto: UpdateErpSubDivisionDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }
  @Patch('bulk/status')
  bulkUpdateStatus(@Body() dto: BulkStatusErpSubDivisionDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }
  @Delete('bulk')
  bulkDelete(@Body() dto: BulkErpSubDivisionDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }
  @Delete(':id')
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
