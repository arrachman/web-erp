import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpClassDto, BulkStatusErpClassDto } from './dto/bulk-classes.dto';
import { CreateErpClassDto } from './dto/create-classes.dto';
import { QueryErpClassDto } from './dto/query-classes.dto';
import { UpdateErpClassDto } from './dto/update-classes.dto';
import { ErpClassesService } from './classes.service';

@ApiTags('ERP Classes')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/classes')
export class ErpClassesController {
  constructor(private readonly service: ErpClassesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Class' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpClassDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Classes' })
  findAll(@Query() query: QueryErpClassDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Class' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Class' })
  update(@Param('id') id: string, @Body() dto: UpdateErpClassDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpClassDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpClassDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Class' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
