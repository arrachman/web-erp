import { PartialType } from '@nestjs/swagger';
import { CreateErpDivisionDto } from './create-erp-division.dto';

export class UpdateErpDivisionDto extends PartialType(CreateErpDivisionDto) {}
