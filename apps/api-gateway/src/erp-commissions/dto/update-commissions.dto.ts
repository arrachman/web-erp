import { PartialType } from '@nestjs/swagger';
import { CreateErpCommissionDto } from './create-commissions.dto';

export class UpdateErpCommissionDto extends PartialType(CreateErpCommissionDto) {}
