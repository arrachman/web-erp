import { PartialType } from '@nestjs/swagger';
import { CreateErpPartnerSubCategoryDto } from './create-partner-sub-categories.dto';

export class UpdateErpPartnerSubCategoryDto extends PartialType(CreateErpPartnerSubCategoryDto) {}
