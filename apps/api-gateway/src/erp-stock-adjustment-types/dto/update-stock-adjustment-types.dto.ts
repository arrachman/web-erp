import { PartialType } from '@nestjs/swagger';
import { CreateErpStockAdjustmentTypeDto } from './create-stock-adjustment-types.dto';

export class UpdateErpStockAdjustmentTypeDto extends PartialType(CreateErpStockAdjustmentTypeDto) {}
