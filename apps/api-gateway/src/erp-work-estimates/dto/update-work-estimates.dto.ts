import { PartialType } from '@nestjs/swagger';
import { CreateErpWorkEstimateDto } from './create-work-estimates.dto';

export class UpdateErpWorkEstimateDto extends PartialType(CreateErpWorkEstimateDto) {}
