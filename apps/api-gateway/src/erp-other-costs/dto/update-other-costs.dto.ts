import { PartialType } from '@nestjs/swagger';
import { CreateErpOtherCostDto } from './create-other-costs.dto';

export class UpdateErpOtherCostDto extends PartialType(CreateErpOtherCostDto) {}
