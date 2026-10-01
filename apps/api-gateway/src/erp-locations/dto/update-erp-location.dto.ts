import { PartialType } from '@nestjs/swagger';
import { CreateErpLocationDto } from './create-erp-location.dto';

export class UpdateErpLocationDto extends PartialType(CreateErpLocationDto) {}
