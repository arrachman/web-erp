import { PartialType } from '@nestjs/swagger';
import { CreateErpPriceCategoryDto } from './create-price-categories.dto';

export class UpdateErpPriceCategoryDto extends PartialType(CreateErpPriceCategoryDto) {}
