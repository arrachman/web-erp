import { PartialType } from '@nestjs/swagger';
import { CreateErpPointCategoryDto } from './create-point-categories.dto';

export class UpdateErpPointCategoryDto extends PartialType(CreateErpPointCategoryDto) {}
