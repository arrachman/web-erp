import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpItemModelDto, BulkStatusErpItemModelDto } from './dto/bulk-item-models.dto';
import { CreateErpItemModelDto } from './dto/create-item-models.dto';
import { QueryErpItemModelDto } from './dto/query-item-models.dto';
import { UpdateErpItemModelDto } from './dto/update-item-models.dto';
import { ErpItemModelsService } from './item-models.service';

@ApiTags('ERP Models')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/item-models')
export class ErpItemModelsController {
  constructor(private readonly service: ErpItemModelsService) {}

  @Post()
  @ApiOperation({ summary: 'Create Model' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpItemModelDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Model' })
  findAll(@Query() query: QueryErpItemModelDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Model' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Model' })
  update(@Param('id') id: string, @Body() dto: UpdateErpItemModelDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpItemModelDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpItemModelDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Model' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
