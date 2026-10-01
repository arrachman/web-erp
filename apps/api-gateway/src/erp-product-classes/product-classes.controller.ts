import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpProductClassDto, BulkStatusErpProductClassDto } from './dto/bulk-product-classes.dto';
import { CreateErpProductClassDto } from './dto/create-product-classes.dto';
import { QueryErpProductClassDto } from './dto/query-product-classes.dto';
import { UpdateErpProductClassDto } from './dto/update-product-classes.dto';
import { ErpProductClassesService } from './product-classes.service';

@ApiTags('ERP Product Classes')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/product-classes')
export class ErpProductClassesController {
  constructor(private readonly service: ErpProductClassesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Product Class' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpProductClassDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Product Class' })
  findAll(@Query() query: QueryErpProductClassDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Product Class' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Product Class' })
  update(@Param('id') id: string, @Body() dto: UpdateErpProductClassDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpProductClassDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpProductClassDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Product Class' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
