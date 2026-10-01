import { PartialType } from '@nestjs/swagger';
import { CreateErpSectionDto } from './create-sections.dto';

export class UpdateErpSectionDto extends PartialType(CreateErpSectionDto) {}
