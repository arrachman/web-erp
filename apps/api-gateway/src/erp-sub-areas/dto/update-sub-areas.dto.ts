import { PartialType } from '@nestjs/swagger';
import { CreateErpSubAreaDto } from './create-sub-areas.dto';

export class UpdateErpSubAreaDto extends PartialType(CreateErpSubAreaDto) {}
