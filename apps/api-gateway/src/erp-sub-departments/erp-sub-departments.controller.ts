import {
  Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards,
} from '@nestjs/common';
import { ApiBearerAuth, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpSubDepartmentDto, BulkStatusErpSubDepartmentDto } from './dto/bulk-erp-sub-department.dto';
import { CreateErpSubDepartmentDto } from './dto/create-erp-sub-department.dto';
import { QueryErpSubDepartmentDto } from './dto/query-erp-sub-department.dto';
import { UpdateErpSubDepartmentDto } from './dto/update-erp-sub-department.dto';
import { ErpSubDepartmentsService } from './erp-sub-departments.service';

@ApiTags('ERP Sub Departments')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sub-departments')
export class ErpSubDepartmentsController {
  constructor(private readonly service: ErpSubDepartmentsService) {}

  @Post()
  create(@Body() dto: CreateErpSubDepartmentDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }
  @Get()
  findAll(@Query() query: QueryErpSubDepartmentDto) { return this.service.findAll(query); }
  @Get(':id')
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }
  @Patch(':id')
  update(@Param('id') id: string, @Body() dto: UpdateErpSubDepartmentDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }
  @Patch('bulk/status')
  bulkUpdateStatus(@Body() dto: BulkStatusErpSubDepartmentDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }
  @Delete('bulk')
  bulkDelete(@Body() dto: BulkErpSubDepartmentDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }
  @Delete(':id')
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
