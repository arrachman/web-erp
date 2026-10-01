import { PartialType } from '@nestjs/swagger';
import { CreateErpProductionActivityDto } from './create-production-activities.dto';

export class UpdateErpProductionActivityDto extends PartialType(CreateErpProductionActivityDto) {}
