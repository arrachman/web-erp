import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpTxnNoteDetailDto } from './dto/bulk-txn-note-detail.dto';
import { CreateErpTxnNoteDetailDto } from './dto/create-txn-note-detail.dto';
import { QueryErpTxnNoteDetailDto } from './dto/query-txn-note-detail.dto';
import { UpdateErpTxnNoteDetailDto } from './dto/update-txn-note-detail.dto';
import { ErpTxnNoteDetailsService } from './txn-note-details.service';

@ApiTags('ERP Transaction Note Details')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/txn-note-details')
export class ErpTxnNoteDetailsController {
  constructor(private readonly service: ErpTxnNoteDetailsService) {}

  @Post()
  @ApiOperation({ summary: 'Create Transaction Note Detail' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpTxnNoteDetailDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Transaction Note Details' })
  findAll(@Query() query: QueryErpTxnNoteDetailDto) { return this.service.findAll(query); }

  @Get('by-note/:noteId')
  @ApiOperation({ summary: 'Get details by Transaction Note ID' })
  findByNoteId(@Param('noteId') noteId: string) { return this.service.findByNoteId(BigInt(noteId)); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Transaction Note Detail by ID' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Transaction Note Detail' })
  update(@Param('id') id: string, @Body() dto: UpdateErpTxnNoteDetailDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpTxnNoteDetailDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Transaction Note Detail' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
