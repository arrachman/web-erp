import { PartialType } from '@nestjs/swagger';
import { CreateGiroDto } from './create-giro.dto';

export class UpdateGiroDto extends PartialType(CreateGiroDto) {}
