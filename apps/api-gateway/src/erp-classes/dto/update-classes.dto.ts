import { PartialType } from '@nestjs/swagger';
import { CreateErpClassDto } from './create-classes.dto';

export class UpdateErpClassDto extends PartialType(CreateErpClassDto) {}
