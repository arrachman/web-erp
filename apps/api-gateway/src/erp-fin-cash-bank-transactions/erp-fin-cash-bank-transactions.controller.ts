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
import { CreateCashBankTransactionDto } from './dto/create-cash-bank-transaction.dto';
import { QueryCashBankTransactionDto } from './dto/query-cash-bank-transaction.dto';
import { TransitionCashBankTransactionDto } from './dto/transition-cash-bank-transaction.dto';
import { UpdateCashBankTransactionDto } from './dto/update-cash-bank-transaction.dto';
import { ErpFinCashBankTransactionsService } from './erp-fin-cash-bank-transactions.service';

@ApiTags('ERP Fin Cash/Bank Transactions')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/fin/cash-bank-transactions')
export class ErpFinCashBankTransactionsController {
  constructor(private readonly service: ErpFinCashBankTransactionsService) {}

  @Post()
  @ApiOperation({ summary: 'Create cash/bank transaction (CR/CD) with contra lines' })
  create(@Body() dto: CreateCashBankTransactionDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List cash/bank transactions (filter by direction/status/date)' })
  findAll(@Query() query: QueryCashBankTransactionDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get cash/bank transaction by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update cash/bank transaction (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(
    @Param('id') id: string,
    @Body() dto: UpdateCashBankTransactionDto,
    @Request() req: any,
  ) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN' })
  transition(
    @Param('id') id: string,
    @Body() dto: TransitionCashBankTransactionDto,
    @Request() req: any,
  ) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete cash/bank transaction (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
