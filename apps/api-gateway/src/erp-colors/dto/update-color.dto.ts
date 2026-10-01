import { PartialType } from '@nestjs/swagger';
import { CreateErpColorDto } from './create-color.dto';

export class UpdateErpColorDto extends PartialType(CreateErpColorDto) {}
