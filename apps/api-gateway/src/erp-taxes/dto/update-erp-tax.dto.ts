import { PartialType } from '@nestjs/swagger';
import { CreateErpTaxDto } from './create-erp-tax.dto';

export class UpdateErpTaxDto extends PartialType(CreateErpTaxDto) {}
