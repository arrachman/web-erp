import { PartialType } from '@nestjs/swagger';
import { CreateArReceiptDto } from './create-ar-receipt.dto';

export class UpdateArReceiptDto extends PartialType(CreateArReceiptDto) {}
