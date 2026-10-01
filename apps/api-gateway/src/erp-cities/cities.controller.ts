import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpCityDto, BulkStatusErpCityDto } from './dto/bulk-cities.dto';
import { CreateErpCityDto } from './dto/create-cities.dto';
import { QueryErpCityDto } from './dto/query-cities.dto';
import { UpdateErpCityDto } from './dto/update-cities.dto';
import { ErpCitiesService } from './cities.service';

@ApiTags('ERP Cities')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/cities')
export class ErpCitiesController {
  constructor(private readonly service: ErpCitiesService) {}

  @Post()
  @ApiOperation({ summary: 'Create City' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpCityDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List City' })
  findAll(@Query() query: QueryErpCityDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get City' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update City' })
  update(@Param('id') id: string, @Body() dto: UpdateErpCityDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpCityDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpCityDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete City' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
