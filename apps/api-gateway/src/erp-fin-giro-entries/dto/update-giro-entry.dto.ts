import { PartialType } from '@nestjs/swagger';
import { CreateGiroEntryDto } from './create-giro-entry.dto';

export class UpdateGiroEntryDto extends PartialType(CreateGiroEntryDto) {}
