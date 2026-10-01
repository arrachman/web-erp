import { ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsDateString, IsEnum, IsIn, IsInt, IsOptional, IsString, Max, Min } from 'class-validator';
import { ErpDocumentStatusDto, ErpPurchaseReturnTypeDto } from './create-pur-return.dto';

export const PUR_RETURN_SORTABLE = [
  'docNumber',
  'docDate',
  'grandTotal',
  'status',
  'createdAt',
] as const;

export class QueryPurReturnsDto {
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

  @ApiPropertyOptional({ description: 'docNumber / description' })
  @IsOptional()
  @IsString()
  search?: string;

  @ApiPropertyOptional({ enum: PUR_RETURN_SORTABLE, default: 'docDate' })
  @IsOptional()
  @IsIn(PUR_RETURN_SORTABLE as unknown as string[])
  sortBy?: string = 'docDate';

  @ApiPropertyOptional({ enum: ['asc', 'desc'], default: 'desc' })
  @IsOptional()
  @IsIn(['asc', 'desc'])
  sortDir?: 'asc' | 'desc' = 'desc';

  @ApiPropertyOptional({
    enum: ErpPurchaseReturnTypeDto,
    description: 'Filter DEBIT_NOTE (DNR page) vs RETURN_TO_VENDOR (PRT page)',
  })
  @IsOptional()
  @IsEnum(ErpPurchaseReturnTypeDto)
  returnType?: ErpPurchaseReturnTypeDto;

  @ApiPropertyOptional({ enum: ErpDocumentStatusDto })
  @IsOptional()
  @IsEnum(ErpDocumentStatusDto)
  status?: ErpDocumentStatusDto;

  @ApiPropertyOptional() @IsOptional() @IsString() branchId?: string;

  @ApiPropertyOptional({ description: 'Supplier (partner id)' })
  @IsOptional()
  @IsString()
  supplierId?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() locationId?: string;

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
