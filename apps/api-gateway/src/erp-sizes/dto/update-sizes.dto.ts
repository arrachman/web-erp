import { PartialType } from '@nestjs/swagger';
import { CreateErpSizeDto } from './create-sizes.dto';

export class UpdateErpSizeDto extends PartialType(CreateErpSizeDto) {}
