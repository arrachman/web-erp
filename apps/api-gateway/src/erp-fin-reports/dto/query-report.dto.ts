import { IsDateString, IsEnum, IsOptional, IsString } from 'class-validator';
import { ReportFormat } from '../report-types';

export class QueryReportDto {
  @IsOptional()
  @IsDateString()
  from?: string;

  @IsOptional()
  @IsDateString()
  to?: string;

  @IsOptional()
  @IsDateString()
  asOf?: string;

  @IsOptional()
  @IsString()
  accountId?: string;

  @IsOptional()
  @IsString()
  branchId?: string;

  @IsOptional()
  @IsString()
  partnerId?: string;

  @IsOptional()
  @IsString()
  fiscalPeriodId?: string;

  @IsOptional()
  @IsEnum(['json', 'xlsx', 'pdf', 'docx'] as ReportFormat[])
  format?: ReportFormat = 'json';
}
