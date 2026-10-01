import { PartialType } from '@nestjs/swagger';
import { CreateErpWarehouseDto } from './create-erp-warehouse.dto';

export class UpdateErpWarehouseDto extends PartialType(CreateErpWarehouseDto) {}
