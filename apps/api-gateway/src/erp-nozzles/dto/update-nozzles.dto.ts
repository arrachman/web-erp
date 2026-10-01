import { PartialType } from '@nestjs/swagger';
import { CreateErpNozzleDto } from './create-nozzles.dto';

export class UpdateErpNozzleDto extends PartialType(CreateErpNozzleDto) {}
