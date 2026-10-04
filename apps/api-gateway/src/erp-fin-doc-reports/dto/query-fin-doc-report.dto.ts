import { Type } from 'class-transformer';
import { IsDateString, IsIn, IsInt, IsOptional, IsString, Max, Min } from 'class-validator';
import { ReportFilters } from '../report-types';

/** Whitelisted query parameters for Finance document reports. */
export class QueryFinDocReportDto {
  @IsOptional()
  @IsDateString()
  dateFrom?: string;

  @IsOptional()
  @IsDateString()
  dateTo?: string;

  @IsOptional()
  @IsDateString()
  asOfDate?: string;

  @IsOptional()
  @IsString()
  partnerId?: string;

  @IsOptional()
  @IsString()
  status?: string;

  @IsOptional()
  @IsString()
  search?: string;

  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  page?: number;

  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  @Max(500)
  limit?: number;

  @IsOptional()
  @IsIn(['xlsx', 'pdf', 'docx'])
  format?: 'xlsx' | 'pdf' | 'docx';

  toFilters(): ReportFilters {
    return {
      dateFrom: this.dateFrom,
      dateTo: this.dateTo,
      asOfDate: this.asOfDate,
      partnerId: this.partnerId,
      status: this.status,
      search: this.search,
      page: this.page,
      limit: this.limit,
    };
  }
}
