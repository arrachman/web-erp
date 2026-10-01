import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpMaterialDto, BulkStatusErpMaterialDto } from './dto/bulk-materials.dto';
import { CreateErpMaterialDto } from './dto/create-materials.dto';
import { QueryErpMaterialDto } from './dto/query-materials.dto';
import { UpdateErpMaterialDto } from './dto/update-materials.dto';
import { ErpMaterialsService } from './materials.service';

@ApiTags('ERP Materials')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/materials')
export class ErpMaterialsController {
  constructor(private readonly service: ErpMaterialsService) {}

  @Post()
  @ApiOperation({ summary: 'Create Material' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpMaterialDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Material' })
  findAll(@Query() query: QueryErpMaterialDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Material' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Material' })
  update(@Param('id') id: string, @Body() dto: UpdateErpMaterialDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpMaterialDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpMaterialDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Material' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
