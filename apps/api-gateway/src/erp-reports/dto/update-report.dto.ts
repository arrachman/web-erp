import { PartialType } from '@nestjs/swagger';
import { CreateErpReportDto } from './create-report.dto';

export class UpdateErpReportDto extends PartialType(CreateErpReportDto) {}
