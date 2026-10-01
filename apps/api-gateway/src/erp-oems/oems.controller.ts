import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpOemDto, BulkStatusErpOemDto } from './dto/bulk-oems.dto';
import { CreateErpOemDto } from './dto/create-oems.dto';
import { QueryErpOemDto } from './dto/query-oems.dto';
import { UpdateErpOemDto } from './dto/update-oems.dto';
import { ErpOemsService } from './oems.service';

@ApiTags('ERP Oems')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/oems')
export class ErpOemsController {
  constructor(private readonly service: ErpOemsService) {}

  @Post()
  @ApiOperation({ summary: 'Create OEM' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpOemDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List OEM' })
  findAll(@Query() query: QueryErpOemDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get OEM' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update OEM' })
  update(@Param('id') id: string, @Body() dto: UpdateErpOemDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpOemDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpOemDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete OEM' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
