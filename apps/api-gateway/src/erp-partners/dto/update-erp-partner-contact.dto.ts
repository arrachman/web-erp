import { PartialType } from '@nestjs/swagger';
import { CreateErpPartnerContactDto } from './create-erp-partner-contact.dto';

export class UpdateErpPartnerContactDto extends PartialType(CreateErpPartnerContactDto) {}
