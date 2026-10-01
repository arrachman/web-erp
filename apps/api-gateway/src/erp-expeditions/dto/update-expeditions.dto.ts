import { PartialType } from '@nestjs/swagger';
import { CreateErpExpeditionDto } from './create-expeditions.dto';

export class UpdateErpExpeditionDto extends PartialType(CreateErpExpeditionDto) {}
