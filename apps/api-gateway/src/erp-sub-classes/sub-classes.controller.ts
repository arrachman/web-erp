import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpSubClassDto, BulkStatusErpSubClassDto } from './dto/bulk-sub-classes.dto';
import { CreateErpSubClassDto } from './dto/create-sub-classes.dto';
import { QueryErpSubClassDto } from './dto/query-sub-classes.dto';
import { UpdateErpSubClassDto } from './dto/update-sub-classes.dto';
import { ErpSubClassesService } from './sub-classes.service';

@ApiTags('ERP Sub Classes')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sub-classes')
export class ErpSubClassesController {
  constructor(private readonly service: ErpSubClassesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Sub Class' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpSubClassDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Sub Class' })
  findAll(@Query() query: QueryErpSubClassDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Sub Class' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpSubClassDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Sub Class' })
  update(@Param('id') id: string, @Body() dto: UpdateErpSubClassDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpSubClassDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Sub Class' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
