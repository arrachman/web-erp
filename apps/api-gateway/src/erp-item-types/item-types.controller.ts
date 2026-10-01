import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpItemKindDto, BulkStatusErpItemKindDto } from './dto/bulk-item-types.dto';
import { CreateErpItemKindDto } from './dto/create-item-types.dto';
import { QueryErpItemKindDto } from './dto/query-item-types.dto';
import { UpdateErpItemKindDto } from './dto/update-item-types.dto';
import { ErpItemKindsService } from './item-types.service';

@ApiTags('ERP Item Types')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/item-types')
export class ErpItemKindsController {
  constructor(private readonly service: ErpItemKindsService) {}

  @Post()
  @ApiOperation({ summary: 'Create Item Type' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpItemKindDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Item Type' })
  findAll(@Query() query: QueryErpItemKindDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Item Type' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Item Type' })
  update(@Param('id') id: string, @Body() dto: UpdateErpItemKindDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpItemKindDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpItemKindDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Item Type' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
