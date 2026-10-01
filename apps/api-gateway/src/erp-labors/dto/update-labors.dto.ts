import { PartialType } from '@nestjs/swagger';
import { CreateErpLaborDto } from './create-labors.dto';

export class UpdateErpLaborDto extends PartialType(CreateErpLaborDto) {}
