import { PartialType } from '@nestjs/swagger';
import { CreateErpSubDivisionDto } from './create-erp-sub-division.dto';

export class UpdateErpSubDivisionDto extends PartialType(CreateErpSubDivisionDto) {}
