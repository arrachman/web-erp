import { PartialType } from '@nestjs/swagger';
import { CreateErpProvinceDto } from './create-provinces.dto';

export class UpdateErpProvinceDto extends PartialType(CreateErpProvinceDto) {}
