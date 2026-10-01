import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpMiscellaneousDto, BulkStatusErpMiscellaneousDto } from './dto/bulk-miscellaneous.dto';
import { CreateErpMiscellaneousDto } from './dto/create-miscellaneous.dto';
import { QueryErpMiscellaneousDto } from './dto/query-miscellaneous.dto';
import { UpdateErpMiscellaneousDto } from './dto/update-miscellaneous.dto';
import { ErpMiscellaneousService } from './miscellaneous.service';

@ApiTags('ERP Miscellaneous')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/miscellaneous')
export class ErpMiscellaneousController {
  constructor(private readonly service: ErpMiscellaneousService) {}

  @Post()
  @ApiOperation({ summary: 'Create Miscellaneous' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpMiscellaneousDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Miscellaneous' })
  findAll(@Query() query: QueryErpMiscellaneousDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Miscellaneous' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpMiscellaneousDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Miscellaneous' })
  update(@Param('id') id: string, @Body() dto: UpdateErpMiscellaneousDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpMiscellaneousDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Miscellaneous' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
