import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpDesignerDto, BulkStatusErpDesignerDto } from './dto/bulk-designers.dto';
import { CreateErpDesignerDto } from './dto/create-designers.dto';
import { QueryErpDesignerDto } from './dto/query-designers.dto';
import { UpdateErpDesignerDto } from './dto/update-designers.dto';
import { ErpDesignersService } from './designers.service';

@ApiTags('ERP Designers')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/designers')
export class ErpDesignersController {
  constructor(private readonly service: ErpDesignersService) {}

  @Post()
  @ApiOperation({ summary: 'Create Designer' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpDesignerDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Designer' })
  findAll(@Query() query: QueryErpDesignerDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Designer' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpDesignerDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Designer' })
  update(@Param('id') id: string, @Body() dto: UpdateErpDesignerDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpDesignerDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Designer' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
