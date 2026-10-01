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
import { CreateSlsProformaInvoiceDto } from './dto/create-sls-proforma-invoice.dto';
import { QuerySlsProformaInvoicesDto } from './dto/query-sls-proforma-invoices.dto';
import { TransitionSlsProformaInvoiceDto } from './dto/transition-sls-proforma-invoice.dto';
import { UpdateSlsProformaInvoiceDto } from './dto/update-sls-proforma-invoice.dto';
import { ErpSlsProformaInvoicesService } from './erp-sls-proforma-invoices.service';

@ApiTags('ERP Sls Proforma Invoices')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sls/proforma-invoices')
export class ErpSlsProformaInvoicesController {
  constructor(private readonly service: ErpSlsProformaInvoicesService) {}

  @Post()
  @ApiOperation({ summary: 'Create proforma invoice (header + item lines)' })
  create(@Body() dto: CreateSlsProformaInvoiceDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List proforma invoices (filter by status/date/customer)' })
  findAll(@Query() query: QuerySlsProformaInvoicesDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get proforma invoice by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update proforma invoice (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateSlsProformaInvoiceDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN' })
  transition(
    @Param('id') id: string,
    @Body() dto: TransitionSlsProformaInvoiceDto,
    @Request() req: any,
  ) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete proforma invoice (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
