import { PartialType } from '@nestjs/swagger';
import { CreateErpItemModelDto } from './create-item-models.dto';

export class UpdateErpItemModelDto extends PartialType(CreateErpItemModelDto) {}
