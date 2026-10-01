import { PartialType } from '@nestjs/swagger';
import { CreateErpProductClassDto } from './create-product-classes.dto';

export class UpdateErpProductClassDto extends PartialType(CreateErpProductClassDto) {}
