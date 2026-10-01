import { PartialType } from '@nestjs/swagger';
import { CreateErpCurrencyDto } from './create-erp-currency.dto';

export class UpdateErpCurrencyDto extends PartialType(CreateErpCurrencyDto) {}
