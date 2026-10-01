import { PartialType } from '@nestjs/swagger';
import { CreateErpFiscalPeriodDto } from './create-erp-fiscal-period.dto';

export class UpdateErpFiscalPeriodDto extends PartialType(CreateErpFiscalPeriodDto) {}
