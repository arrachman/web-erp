import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpColorDto, BulkStatusErpColorDto } from './dto/bulk-color.dto';
import { CreateErpColorDto } from './dto/create-color.dto';
import { QueryErpColorDto } from './dto/query-color.dto';
import { UpdateErpColorDto } from './dto/update-color.dto';
import { ErpColorsService } from './erp-colors.service';

@ApiTags('ERP Colors')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/colors')
export class ErpColorsController {
  constructor(private readonly service: ErpColorsService) {}

  @Post()
  @ApiOperation({ summary: 'Create Color' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpColorDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Color' })
  findAll(@Query() query: QueryErpColorDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Color' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Color' })
  update(@Param('id') id: string, @Body() dto: UpdateErpColorDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpColorDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpColorDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Color' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
