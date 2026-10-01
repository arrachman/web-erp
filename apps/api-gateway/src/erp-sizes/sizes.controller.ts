import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpSizeDto, BulkStatusErpSizeDto } from './dto/bulk-sizes.dto';
import { CreateErpSizeDto } from './dto/create-sizes.dto';
import { QueryErpSizeDto } from './dto/query-sizes.dto';
import { UpdateErpSizeDto } from './dto/update-sizes.dto';
import { ErpSizesService } from './sizes.service';

@ApiTags('ERP Sizes')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sizes')
export class ErpSizesController {
  constructor(private readonly service: ErpSizesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Size' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpSizeDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Size' })
  findAll(@Query() query: QueryErpSizeDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Size' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Size' })
  update(@Param('id') id: string, @Body() dto: UpdateErpSizeDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpSizeDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpSizeDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Size' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
