import { PartialType } from '@nestjs/swagger';
import { CreateErpPartnerDto } from './create-erp-partner.dto';

export class UpdateErpPartnerDto extends PartialType(CreateErpPartnerDto) {}
