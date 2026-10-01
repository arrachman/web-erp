import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpTransactionNoteDto, BulkStatusErpTransactionNoteDto } from './dto/bulk-transaction-notes.dto';
import { CreateErpTransactionNoteDto } from './dto/create-transaction-notes.dto';
import { QueryErpTransactionNoteDto } from './dto/query-transaction-notes.dto';
import { UpdateErpTransactionNoteDto } from './dto/update-transaction-notes.dto';
import { ErpTransactionNotesService } from './transaction-notes.service';

@ApiTags('ERP Transaction Notes')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/transaction-notes')
export class ErpTransactionNotesController {
  constructor(private readonly service: ErpTransactionNotesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Transaction Note' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpTransactionNoteDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Transaction Note' })
  findAll(@Query() query: QueryErpTransactionNoteDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Transaction Note' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Transaction Note' })
  update(@Param('id') id: string, @Body() dto: UpdateErpTransactionNoteDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpTransactionNoteDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpTransactionNoteDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Transaction Note' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
