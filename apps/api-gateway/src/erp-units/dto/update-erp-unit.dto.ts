import { PartialType } from '@nestjs/swagger';
import { CreateErpUnitDto } from './create-erp-unit.dto';

export class UpdateErpUnitDto extends PartialType(CreateErpUnitDto) {}
