import { PartialType } from '@nestjs/swagger';
import { CreateMfgWorkOrderDto } from './create-mfg-work-order.dto';

export class UpdateMfgWorkOrderDto extends PartialType(CreateMfgWorkOrderDto) {}
