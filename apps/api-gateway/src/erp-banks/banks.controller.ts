import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpBankDto, BulkStatusErpBankDto } from './dto/bulk-banks.dto';
import { CreateErpBankDto } from './dto/create-banks.dto';
import { QueryErpBankDto } from './dto/query-banks.dto';
import { UpdateErpBankDto } from './dto/update-banks.dto';
import { ErpBanksService } from './banks.service';

@ApiTags('ERP Banks')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/banks')
export class ErpBanksController {
  constructor(private readonly service: ErpBanksService) {}

  @Post()
  @ApiOperation({ summary: 'Create Bank' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpBankDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Bank' })
  findAll(@Query() query: QueryErpBankDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Bank' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Bank' })
  update(@Param('id') id: string, @Body() dto: UpdateErpBankDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpBankDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpBankDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Bank' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
