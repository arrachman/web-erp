import { PartialType } from '@nestjs/swagger';
import { CreateErpMaterialDto } from './create-materials.dto';

export class UpdateErpMaterialDto extends PartialType(CreateErpMaterialDto) {}
