import { PartialType } from '@nestjs/swagger';
import { CreateErpAreaDto } from './create-areas.dto';

export class UpdateErpAreaDto extends PartialType(CreateErpAreaDto) {}
