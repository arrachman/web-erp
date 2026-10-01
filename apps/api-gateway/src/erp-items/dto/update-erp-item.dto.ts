import { PartialType } from '@nestjs/swagger';
import { CreateErpItemDto } from './create-erp-item.dto';

export class UpdateErpItemDto extends PartialType(CreateErpItemDto) {}
