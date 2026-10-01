import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { CreatePurGoodsReceiptDto } from './dto/create-pur-goods-receipt.dto';
import { QueryPurGoodsReceiptsDto } from './dto/query-pur-goods-receipts.dto';
import { TransitionPurGoodsReceiptDto } from './dto/transition-pur-goods-receipt.dto';
import { UpdatePurGoodsReceiptDto } from './dto/update-pur-goods-receipt.dto';
import { ErpPurGoodsReceiptsService } from './erp-pur-goods-receipts.service';

@ApiTags('ERP Pur Goods Receipts')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/pur/goods-receipts')
export class ErpPurGoodsReceiptsController {
  constructor(private readonly service: ErpPurGoodsReceiptsService) {}

  @Post() @ApiOperation({ summary: 'Create goods receipt (item lines + QC delta)' })
  create(@Body() dto: CreatePurGoodsReceiptDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get() @ApiOperation({ summary: 'List goods receipts' })
  findAll(@Query() query: QueryPurGoodsReceiptsDto) { return this.service.findAll(query); }

  @Get(':id') @ApiOperation({ summary: 'Get goods receipt by id' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id') @ApiOperation({ summary: 'Update goods receipt (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdatePurGoodsReceiptDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Post(':id/transition') @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN' })
  transition(@Param('id') id: string, @Body() dto: TransitionPurGoodsReceiptDto, @Request() req: any) { return this.service.transition(BigInt(id), dto, req.user?.id); }

  @Delete(':id') @ApiOperation({ summary: 'Delete goods receipt (soft)' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
