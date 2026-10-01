import { PartialType } from '@nestjs/swagger';
import { CreateErpCostCenterDto } from './create-erp-cost-center.dto';

export class UpdateErpCostCenterDto extends PartialType(CreateErpCostCenterDto) {}
