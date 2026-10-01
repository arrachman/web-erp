import { PartialType } from '@nestjs/swagger';
import { CreateApPaymentDto } from './create-ap-payment.dto';

export class UpdateApPaymentDto extends PartialType(CreateApPaymentDto) {}
