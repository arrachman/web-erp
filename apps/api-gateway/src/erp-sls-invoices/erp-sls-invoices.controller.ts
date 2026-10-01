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
import { CreateSlsInvoiceDto } from './dto/create-sls-invoice.dto';
import { QuerySlsInvoicesDto } from './dto/query-sls-invoices.dto';
import { TransitionSlsInvoiceDto } from './dto/transition-sls-invoice.dto';
import { UpdateSlsInvoiceDto } from './dto/update-sls-invoice.dto';
import { ErpSlsInvoicesService } from './erp-sls-invoices.service';

@ApiTags('ERP Sls Invoices')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sls/invoices')
export class ErpSlsInvoicesController {
  constructor(private readonly service: ErpSlsInvoicesService) {}

  @Post()
  @ApiOperation({ summary: 'Create sales invoice (header + item lines)' })
  create(@Body() dto: CreateSlsInvoiceDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List sales invoices (filter by status/date/customer/orderId/deliveryOrderId)' })
  findAll(@Query() query: QuerySlsInvoicesDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get sales invoice by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update sales invoice (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateSlsInvoiceDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN — POST creates AR entry' })
  transition(@Param('id') id: string, @Body() dto: TransitionSlsInvoiceDto, @Request() req: any) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete sales invoice (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
