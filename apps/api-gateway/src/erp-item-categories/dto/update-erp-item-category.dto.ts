import { PartialType } from '@nestjs/swagger';
import { CreateErpItemCategoryDto } from './create-erp-item-category.dto';

export class UpdateErpItemCategoryDto extends PartialType(CreateErpItemCategoryDto) {}
