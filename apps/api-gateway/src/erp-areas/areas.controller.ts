import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpAreaDto, BulkStatusErpAreaDto } from './dto/bulk-areas.dto';
import { CreateErpAreaDto } from './dto/create-areas.dto';
import { QueryErpAreaDto } from './dto/query-areas.dto';
import { UpdateErpAreaDto } from './dto/update-areas.dto';
import { ErpAreasService } from './areas.service';

@ApiTags('ERP Areas')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/areas')
export class ErpAreasController {
  constructor(private readonly service: ErpAreasService) {}

  @Post()
  @ApiOperation({ summary: 'Create Area' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpAreaDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Area' })
  findAll(@Query() query: QueryErpAreaDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Area' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Area' })
  update(@Param('id') id: string, @Body() dto: UpdateErpAreaDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpAreaDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpAreaDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Area' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
