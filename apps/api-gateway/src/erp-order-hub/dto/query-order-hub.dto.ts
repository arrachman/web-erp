import { ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsEnum, IsIn, IsInt, IsOptional, IsString, Max, Min } from 'class-validator';
import { ErpFundingSourceDto, ErpSalesChannelDto } from '../../erp-sls-orders/dto/create-sls-order.dto';

export enum ErpOrderHubStatusDto {
  BARU = 'BARU',
  DIKONFIRMASI = 'DIKONFIRMASI',
  DISIAPKAN = 'DISIAPKAN',
  DIKIRIM = 'DIKIRIM',
  DITERIMA = 'DITERIMA',
  DITAGIH = 'DITAGIH',
  LUNAS = 'LUNAS',
}

export const ORDER_HUB_SORTABLE = ['docNumber', 'docDate', 'grandTotal', 'createdAt'] as const;

export class QueryOrderHubDto {
  @ApiPropertyOptional({ default: 1 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  page?: number = 1;

  @ApiPropertyOptional({ default: 25 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  @Max(100)
  limit?: number = 25;

  @ApiPropertyOptional({ description: 'docNumber / external order id / customer' })
  @IsOptional()
  @IsString()
  search?: string;

  @ApiPropertyOptional({ enum: ORDER_HUB_SORTABLE, default: 'docNumber' })
  @IsOptional()
  @IsIn(ORDER_HUB_SORTABLE as unknown as string[])
  sortBy?: string = 'docNumber';

  @ApiPropertyOptional({ enum: ['asc', 'desc'], default: 'desc' })
  @IsOptional()
  @IsIn(['asc', 'desc'])
  sortDir?: 'asc' | 'desc' = 'desc';

  @ApiPropertyOptional({ enum: ErpSalesChannelDto })
  @IsOptional()
  @IsEnum(ErpSalesChannelDto)
  channel?: ErpSalesChannelDto;

  @ApiPropertyOptional({ enum: ErpOrderHubStatusDto, description: 'Derived stage filter (computed)' })
  @IsOptional()
  @IsEnum(ErpOrderHubStatusDto)
  hubStatus?: ErpOrderHubStatusDto;

  @ApiPropertyOptional({ enum: ErpFundingSourceDto })
  @IsOptional()
  @IsEnum(ErpFundingSourceDto)
  fundingSource?: ErpFundingSourceDto;

  @ApiPropertyOptional({ description: 'Budget year (tahun anggaran), e.g. 2026' })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  budgetYear?: number;

  @ApiPropertyOptional({ description: 'Customer (md_partners) id' })
  @IsOptional()
  @IsString()
  customerId?: string;
}
