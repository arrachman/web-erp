import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpCountryDto, BulkStatusErpCountryDto } from './dto/bulk-countries.dto';
import { CreateErpCountryDto } from './dto/create-countries.dto';
import { QueryErpCountryDto } from './dto/query-countries.dto';
import { UpdateErpCountryDto } from './dto/update-countries.dto';
import { ErpCountriesService } from './countries.service';

@ApiTags('ERP Countries')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/countries')
export class ErpCountriesController {
  constructor(private readonly service: ErpCountriesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Country' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpCountryDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Country' })
  findAll(@Query() query: QueryErpCountryDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Country' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Country' })
  update(@Param('id') id: string, @Body() dto: UpdateErpCountryDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpCountryDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpCountryDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Country' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
