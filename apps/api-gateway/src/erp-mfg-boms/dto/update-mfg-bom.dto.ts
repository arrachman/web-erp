import { PartialType } from '@nestjs/swagger';
import { CreateMfgBomDto } from './create-mfg-bom.dto';

export class UpdateMfgBomDto extends PartialType(CreateMfgBomDto) {}
