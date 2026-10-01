import { PartialType } from '@nestjs/swagger';
import { CreateErpBankDto } from './create-banks.dto';

export class UpdateErpBankDto extends PartialType(CreateErpBankDto) {}
