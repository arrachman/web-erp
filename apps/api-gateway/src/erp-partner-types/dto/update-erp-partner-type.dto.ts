import { PartialType } from '@nestjs/swagger';
import { CreateErpPartnerTypeDto } from './create-erp-partner-type.dto';

export class UpdateErpPartnerTypeDto extends PartialType(CreateErpPartnerTypeDto) {}
