import { ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsDateString, IsEnum, IsIn, IsInt, IsOptional, IsString, Max, Min } from 'class-validator';

export enum SlsCustomerAdvanceSettlementStatusDto {
  UNPAID = 'UNPAID',
  PARTIAL = 'PARTIAL',
  PAID = 'PAID',
}

export enum SlsCustomerAdvanceStatusDto {
  DRAFT = 'DRAFT',
  NEED_APPROVE = 'NEED_APPROVE',
  APPROVED = 'APPROVED',
  REJECTED = 'REJECTED',
  POSTED = 'POSTED',
  VOID = 'VOID',
  CANCELLED = 'CANCELLED',
}

export const SLS_CUSTOMER_ADVANCE_SORTABLE = [
  'docNumber',
  'docDate',
  'amount',
  'status',
  'createdAt',
] as const;

export class QuerySlsCustomerAdvancesDto {
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

  @ApiPropertyOptional({ enum: SLS_CUSTOMER_ADVANCE_SORTABLE, default: 'docDate' })
  @IsOptional()
  @IsIn(SLS_CUSTOMER_ADVANCE_SORTABLE as unknown as string[])
  sortBy?: string = 'docDate';

  @ApiPropertyOptional({ enum: ['asc', 'desc'], default: 'desc' })
  @IsOptional()
  @IsIn(['asc', 'desc'])
  sortDir?: 'asc' | 'desc' = 'desc';

  @ApiPropertyOptional({ enum: SlsCustomerAdvanceStatusDto })
  @IsOptional()
  @IsEnum(SlsCustomerAdvanceStatusDto)
  status?: SlsCustomerAdvanceStatusDto;

  @ApiPropertyOptional({ enum: SlsCustomerAdvanceSettlementStatusDto })
  @IsOptional()
  @IsEnum(SlsCustomerAdvanceSettlementStatusDto)
  settlementStatus?: SlsCustomerAdvanceSettlementStatusDto;

  @ApiPropertyOptional({ description: 'Pelanggan (customer id)' })
  @IsOptional()
  @IsString()
  customerId?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  branchId?: string;

  @ApiPropertyOptional({ description: 'docDate >= (YYYY-MM-DD)' })
  @IsOptional()
  @IsDateString()
  dateFrom?: string;

  @ApiPropertyOptional({ description: 'docDate <= (YYYY-MM-DD)' })
  @IsOptional()
  @IsDateString()
  dateTo?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  createdById?: string;
}
