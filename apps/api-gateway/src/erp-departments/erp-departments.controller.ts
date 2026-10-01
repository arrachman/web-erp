import {
  Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards,
} from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpDepartmentDto, BulkStatusErpDepartmentDto } from './dto/bulk-erp-department.dto';
import { CreateErpDepartmentDto } from './dto/create-erp-department.dto';
import { QueryErpDepartmentDto } from './dto/query-erp-department.dto';
import { UpdateErpDepartmentDto } from './dto/update-erp-department.dto';
import { ErpDepartmentsService } from './erp-departments.service';

@ApiTags('ERP Departemenons')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/departments')
export class ErpDepartmentsController {
  constructor(private readonly service: ErpDepartmentsService) {}

  @Post()
  @ApiOperation({ summary: 'Create ERP Department' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpDepartmentDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List ERP Departments' })
  findAll(@Query() query: QueryErpDepartmentDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP Department' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ERP Department' })
  update(@Param('id') id: string, @Body() dto: UpdateErpDepartmentDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate ERP Departments' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpDepartmentDto, @Request() req: any) {
    return this.service.bulkUpdateStatus(dto, req.user?.id);
  }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete ERP Departments' })
  bulkDelete(@Body() dto: BulkErpDepartmentDto, @Request() req: any) {
    return this.service.bulkDelete(dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete ERP Department' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
