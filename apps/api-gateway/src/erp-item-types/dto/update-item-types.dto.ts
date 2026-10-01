import { PartialType } from '@nestjs/swagger';
import { CreateErpItemKindDto } from './create-item-types.dto';

export class UpdateErpItemKindDto extends PartialType(CreateErpItemKindDto) {}
