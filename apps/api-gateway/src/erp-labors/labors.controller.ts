import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpLaborDto, BulkStatusErpLaborDto } from './dto/bulk-labors.dto';
import { CreateErpLaborDto } from './dto/create-labors.dto';
import { QueryErpLaborDto } from './dto/query-labors.dto';
import { UpdateErpLaborDto } from './dto/update-labors.dto';
import { ErpLaborsService } from './labors.service';

@ApiTags('ERP Labors')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/labors')
export class ErpLaborsController {
  constructor(private readonly service: ErpLaborsService) {}

  @Post()
  @ApiOperation({ summary: 'Create Labor' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpLaborDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Labor' })
  findAll(@Query() query: QueryErpLaborDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Labor' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpLaborDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Labor' })
  update(@Param('id') id: string, @Body() dto: UpdateErpLaborDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpLaborDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Labor' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
