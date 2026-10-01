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
import { CreateSlsInvoiceSwapDto } from './dto/create-sls-invoice-swap.dto';
import { QuerySlsInvoiceSwapsDto } from './dto/query-sls-invoice-swaps.dto';
import { TransitionSlsInvoiceSwapDto } from './dto/transition-sls-invoice-swap.dto';
import { UpdateSlsInvoiceSwapDto } from './dto/update-sls-invoice-swap.dto';
import { ErpSlsInvoiceSwapsService } from './erp-sls-invoice-swaps.service';

@ApiTags('ERP Sls Invoice Swaps')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sls/invoice-swaps')
export class ErpSlsInvoiceSwapsController {
  constructor(private readonly service: ErpSlsInvoiceSwapsService) {}

  @Post()
  @ApiOperation({ summary: 'Create invoice swap (SIE — swap antar invoice)' })
  create(@Body() dto: CreateSlsInvoiceSwapDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List invoice swaps (filter by status/date/customer)' })
  findAll(@Query() query: QuerySlsInvoiceSwapsDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get invoice swap by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update invoice swap (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateSlsInvoiceSwapDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN' })
  transition(
    @Param('id') id: string,
    @Body() dto: TransitionSlsInvoiceSwapDto,
    @Request() req: any,
  ) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete invoice swap (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
