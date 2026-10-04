import { PartialType } from '@nestjs/swagger';
import { CreateErpSchoolDto } from './create-erp-school.dto';

export class UpdateErpSchoolDto extends PartialType(CreateErpSchoolDto) {}