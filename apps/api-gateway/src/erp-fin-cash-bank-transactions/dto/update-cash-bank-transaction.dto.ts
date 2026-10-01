import { PartialType } from '@nestjs/swagger';
import { CreateCashBankTransactionDto } from './create-cash-bank-transaction.dto';

export class UpdateCashBankTransactionDto extends PartialType(
  CreateCashBankTransactionDto,
) {}
