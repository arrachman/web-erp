import { PartialType } from '@nestjs/swagger';
import { CreateErpBrandDto } from './create-brands.dto';

export class UpdateErpBrandDto extends PartialType(CreateErpBrandDto) {}
