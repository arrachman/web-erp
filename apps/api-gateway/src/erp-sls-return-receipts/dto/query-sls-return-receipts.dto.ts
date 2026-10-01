import { ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsDateString, IsEnum, IsIn, IsInt, IsOptional, IsString, Max, Min } from 'class-validator';
import { ErpDocumentStatusDto } from './create-sls-return-receipt.dto';

export const SLS_RETURN_RECEIPT_SORTABLE = [
  'docNumber',
  'docDate',
  'grandTotal',
  'status',
  'createdAt',
] as const;

export class QuerySlsReturnReceiptsDto {
  @ApiPropertyOptional({ default: 1 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  page?: number = 1;

  @ApiPropertyOptional({ default: 10 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  @Max(100)
  limit?: number = 10;

  @ApiPropertyOptional({ description: 'docNumber / code / description' })
  @IsOptional()
  @IsString()
  search?: string;

  @ApiPropertyOptional({ enum: SLS_RETURN_RECEIPT_SORTABLE, default: 'docDate' })
  @IsOptional()
  @IsIn(SLS_RETURN_RECEIPT_SORTABLE as unknown as string[])
  sortBy?: string = 'docDate';

  @ApiPropertyOptional({ enum: ['asc', 'desc'], default: 'desc' })
  @IsOptional()
  @IsIn(['asc', 'desc'])
  sortDir?: 'asc' | 'desc' = 'desc';

  @ApiPropertyOptional({ enum: ErpDocumentStatusDto })
  @IsOptional()
  @IsEnum(ErpDocumentStatusDto)
  status?: ErpDocumentStatusDto;

  @ApiPropertyOptional() @IsOptional() @IsString() branchId?: string;

  @ApiPropertyOptional({ description: 'Pelanggan (customer id)' })
  @IsOptional()
  @IsString()
  customerId?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() locationId?: string;

  @ApiPropertyOptional({ description: 'Return (sls_returns) id' })
  @IsOptional()
  @IsString()
  returnId?: string;

  @ApiPropertyOptional({ description: 'Invoice (sls_invoices) id' })
  @IsOptional()
  @IsString()
  invoiceId?: string;

  @ApiPropertyOptional({ description: 'Settlement status filter' })
  @IsOptional()
  @IsString()
  settlementStatus?: string;

  @ApiPropertyOptional({ description: 'docDate >= (YYYY-MM-DD)' })
  @IsOptional()
  @IsDateString()
  dateFrom?: string;

  @ApiPropertyOptional({ description: 'docDate <= (YYYY-MM-DD)' })
  @IsOptional()
  @IsDateString()
  dateTo?: string;

  @ApiPropertyOptional({ description: 'No Dokumen >= (range start)' })
  @IsOptional()
  @IsString()
  docNumberFrom?: string;

  @ApiPropertyOptional({ description: 'No Dokumen <= (range end)' })
  @IsOptional()
  @IsString()
  docNumberTo?: string;

  @ApiPropertyOptional({ description: 'Uraian (description contains)' })
  @IsOptional()
  @IsString()
  description?: string;

  @ApiPropertyOptional({ description: 'User input (createdById)' })
  @IsOptional()
  @IsString()
  createdById?: string;
}
