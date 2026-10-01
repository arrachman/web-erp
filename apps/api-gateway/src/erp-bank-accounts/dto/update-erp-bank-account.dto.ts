import { PartialType } from '@nestjs/swagger';
import { CreateErpBankAccountDto } from './create-erp-bank-account.dto';

export class UpdateErpBankAccountDto extends PartialType(CreateErpBankAccountDto) {}
