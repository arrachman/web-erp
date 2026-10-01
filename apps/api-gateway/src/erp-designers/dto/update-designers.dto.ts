import { PartialType } from '@nestjs/swagger';
import { CreateErpDesignerDto } from './create-designers.dto';

export class UpdateErpDesignerDto extends PartialType(CreateErpDesignerDto) {}
