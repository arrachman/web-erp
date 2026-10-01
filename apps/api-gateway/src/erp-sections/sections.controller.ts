import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpSectionDto, BulkStatusErpSectionDto } from './dto/bulk-sections.dto';
import { CreateErpSectionDto } from './dto/create-sections.dto';
import { QueryErpSectionDto } from './dto/query-sections.dto';
import { UpdateErpSectionDto } from './dto/update-sections.dto';
import { ErpSectionsService } from './sections.service';

@ApiTags('ERP Sections')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sections')
export class ErpSectionsController {
  constructor(private readonly service: ErpSectionsService) {}

  @Post()
  @ApiOperation({ summary: 'Create Section' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpSectionDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Section' })
  findAll(@Query() query: QueryErpSectionDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Section' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Section' })
  update(@Param('id') id: string, @Body() dto: UpdateErpSectionDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpSectionDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpSectionDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Section' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
