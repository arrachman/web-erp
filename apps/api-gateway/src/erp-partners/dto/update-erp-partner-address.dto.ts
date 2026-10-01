import { PartialType } from '@nestjs/swagger';
import { CreateErpPartnerAddressDto } from './create-erp-partner-address.dto';

export class UpdateErpPartnerAddressDto extends PartialType(CreateErpPartnerAddressDto) {}
