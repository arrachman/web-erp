import { PartialType } from '@nestjs/swagger';
import { CreateErpSubClassDto } from './create-sub-classes.dto';

export class UpdateErpSubClassDto extends PartialType(CreateErpSubClassDto) {}
