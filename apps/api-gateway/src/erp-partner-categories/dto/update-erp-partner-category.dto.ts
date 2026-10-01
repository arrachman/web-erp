import { PartialType } from '@nestjs/swagger';
import { CreateErpPartnerCategoryDto } from './create-erp-partner-category.dto';

export class UpdateErpPartnerCategoryDto extends PartialType(CreateErpPartnerCategoryDto) {}
