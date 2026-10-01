import { PartialType, OmitType } from '@nestjs/swagger';
import { CreateErpItemInformationDto } from './create-item-information.dto';

export class UpdateErpItemInformationDto extends PartialType(OmitType(CreateErpItemInformationDto, ['itemId'] as const)) {}
