import { ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsDateString, IsEnum, IsIn, IsInt, IsOptional, IsString, Max, Min } from 'class-validator';
import { ErpDocumentStatusDto } from './create-pur-goods-receipt.dto';

export const PUR_GRN_SORTABLE = ['docNumber', 'docDate', 'grandTotal', 'status', 'createdAt'] as const;

export class QueryPurGoodsReceiptsDto {
  @IsOptional() @Type(() => Number) @IsInt() @Min(1) page?: number = 1;
  @IsOptional() @Type(() => Number) @IsInt() @Min(1) @Max(100) limit?: number = 10;
  @IsOptional() @IsString() search?: string;
  @IsOptional() @IsIn(PUR_GRN_SORTABLE as unknown as string[]) sortBy?: string = 'docDate';
  @IsOptional() @IsIn(['asc', 'desc']) sortDir?: 'asc' | 'desc' = 'desc';
  @IsOptional() @IsEnum(ErpDocumentStatusDto) status?: ErpDocumentStatusDto;
  @IsOptional() @IsString() branchId?: string;
  @IsOptional() @IsString() supplierId?: string;
  @IsOptional() @IsString() locationId?: string;
  @IsOptional() @IsDateString() dateFrom?: string;
  @IsOptional() @IsDateString() dateTo?: string;
  @IsOptional() @IsString() docNumberFrom?: string;
  @IsOptional() @IsString() docNumberTo?: string;
  @IsOptional() @IsString() description?: string;
  @IsOptional() @IsString() createdById?: string;
}
