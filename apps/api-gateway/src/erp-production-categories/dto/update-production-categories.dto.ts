import { PartialType } from '@nestjs/swagger';
import { CreateErpProductionCategoryDto } from './create-production-categories.dto';

export class UpdateErpProductionCategoryDto extends PartialType(CreateErpProductionCategoryDto) {}
