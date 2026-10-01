import { PartialType } from '@nestjs/swagger';
import { CreateErpOemDto } from './create-oems.dto';

export class UpdateErpOemDto extends PartialType(CreateErpOemDto) {}
