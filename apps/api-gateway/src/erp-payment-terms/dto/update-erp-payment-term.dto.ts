import { PartialType } from '@nestjs/swagger';
import { CreateErpPaymentTermDto } from './create-erp-payment-term.dto';

export class UpdateErpPaymentTermDto extends PartialType(CreateErpPaymentTermDto) {}
