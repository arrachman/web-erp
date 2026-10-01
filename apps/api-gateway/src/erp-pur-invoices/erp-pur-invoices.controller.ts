import {
  Body,
  Controller,
  Delete,
  Get,
  Param,
  Patch,
  Post,
  Query,
  Request,
  UseGuards,
} from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { CreatePurInvoiceDto } from './dto/create-pur-invoice.dto';
import { QueryPurInvoicesDto } from './dto/query-pur-invoices.dto';
import { TransitionPurInvoiceDto } from './dto/transition-pur-invoice.dto';
import { UpdatePurInvoiceDto } from './dto/update-pur-invoice.dto';
import { ErpPurInvoicesService } from './erp-pur-invoices.service';

@ApiTags('ERP Pur Invoices')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/pur/invoices')
export class ErpPurInvoicesController {
  constructor(private readonly service: ErpPurInvoicesService) {}

  @Post()
  @ApiOperation({ summary: 'Create purchase invoice (header + item lines)' })
  create(@Body() dto: CreatePurInvoiceDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List purchase invoices (filter by status/date/supplier)' })
  findAll(@Query() query: QueryPurInvoicesDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get purchase invoice by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update purchase invoice (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdatePurInvoiceDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN' })
  transition(@Param('id') id: string, @Body() dto: TransitionPurInvoiceDto, @Request() req: any) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete purchase invoice (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
