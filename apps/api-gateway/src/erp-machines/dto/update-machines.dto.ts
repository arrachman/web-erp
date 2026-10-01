import { PartialType } from '@nestjs/swagger';
import { CreateErpMachineDto } from './create-machines.dto';

export class UpdateErpMachineDto extends PartialType(CreateErpMachineDto) {}
