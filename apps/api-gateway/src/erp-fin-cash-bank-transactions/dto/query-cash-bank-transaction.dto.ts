import { ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsDateString, IsEnum, IsIn, IsInt, IsOptional, IsString, Max, Min } from 'class-validator';
import {
  ErpCashBankDirectionDto,
  ErpCashBankKindDto,
  ErpPaymentMethodDto,
  ErpDocumentStatusDto,
} from './create-cash-bank-transaction.dto';

export const CASH_BANK_SORTABLE = [
  'docNumber',
  'transactionDate',
  'amount',
  'status',
  'createdAt',
] as const;

export class QueryCashBankTransactionDto {
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

  @ApiPropertyOptional({ description: 'docNumber / description / contactPerson' })
  @IsOptional()
  @IsString()
  search?: string;

  @ApiPropertyOptional({ enum: CASH_BANK_SORTABLE, default: 'transactionDate' })
  @IsOptional()
  @IsIn(CASH_BANK_SORTABLE as unknown as string[])
  sortBy?: string = 'transactionDate';

  @ApiPropertyOptional({ enum: ['asc', 'desc'], default: 'desc' })
  @IsOptional()
  @IsIn(['asc', 'desc'])
  sortDir?: 'asc' | 'desc' = 'desc';

  @ApiPropertyOptional({ enum: ErpCashBankDirectionDto })
  @IsOptional()
  @IsEnum(ErpCashBankDirectionDto)
  direction?: ErpCashBankDirectionDto;

  @ApiPropertyOptional({ enum: ErpCashBankKindDto, description: 'CASH (Kas) vs BANK (Bank)' })
  @IsOptional()
  @IsEnum(ErpCashBankKindDto)
  kind?: ErpCashBankKindDto;

  @ApiPropertyOptional({ enum: ErpPaymentMethodDto, description: 'Cara Bayar (bank txns)' })
  @IsOptional()
  @IsEnum(ErpPaymentMethodDto)
  paymentMethod?: ErpPaymentMethodDto;

  @ApiPropertyOptional({ enum: ErpDocumentStatusDto })
  @IsOptional()
  @IsEnum(ErpDocumentStatusDto)
  status?: ErpDocumentStatusDto;

  @ApiPropertyOptional({ description: 'transactionDate >= (YYYY-MM-DD)' })
  @IsOptional()
  @IsDateString()
  dateFrom?: string;

  @ApiPropertyOptional({ description: 'transactionDate <= (YYYY-MM-DD)' })
  @IsOptional()
  @IsDateString()
  dateTo?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() branchId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() locationId?: string;
  @ApiPropertyOptional({ description: 'Terima Dari (partner id)' })
  @IsOptional()
  @IsString()
  partnerId?: string;

  @ApiPropertyOptional({ description: 'No Transaksi >= (range start)' })
  @IsOptional()
  @IsString()
  docNumberFrom?: string;

  @ApiPropertyOptional({ description: 'No Transaksi <= (range end)' })
  @IsOptional()
  @IsString()
  docNumberTo?: string;

  @ApiPropertyOptional({ description: 'Uraian (description contains)' })
  @IsOptional()
  @IsString()
  description?: string;

  @ApiPropertyOptional({ description: 'Catatan (notes contains)' })
  @IsOptional()
  @IsString()
  notes?: string;

  @ApiPropertyOptional({ description: 'User input (createdById)' })
  @IsOptional()
  @IsString()
  createdById?: string;
}
