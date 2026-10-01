import { ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsDateString, IsEnum, IsIn, IsInt, IsOptional, IsString, Max, Min } from 'class-validator';
import { ErpDocumentStatusDto } from './create-pur-rfq.dto';

export const PUR_RFQ_SORTABLE = ['docNumber', 'docDate', 'status', 'createdAt'] as const;

export class QueryPurRfqsDto {
  @IsOptional() @Type(() => Number) @IsInt() @Min(1) page?: number = 1;
  @IsOptional() @Type(() => Number) @IsInt() @Min(1) @Max(100) limit?: number = 10;
  @IsOptional() @IsString() search?: string;
  @IsOptional() @IsIn(PUR_RFQ_SORTABLE as unknown as string[]) sortBy?: string = 'docDate';
  @IsOptional() @IsIn(['asc', 'desc']) sortDir?: 'asc' | 'desc' = 'desc';
  @IsOptional() @IsEnum(ErpDocumentStatusDto) status?: ErpDocumentStatusDto;
  @IsOptional() @IsString() branchId?: string;
  @IsOptional() @IsDateString() dateFrom?: string;
  @IsOptional() @IsDateString() dateTo?: string;
  @IsOptional() @IsString() docNumberFrom?: string;
  @IsOptional() @IsString() docNumberTo?: string;
  @IsOptional() @IsString() createdById?: string;
}
