import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import {
  IsDateString,
  IsEnum,
  IsNotEmpty,
  IsNumberString,
  IsOptional,
  IsString,
} from 'class-validator';

export enum ErpDocumentStatusDto {
  DRAFT = 'DRAFT',
  POSTED = 'POSTED',
  VOID = 'VOID',
  CANCELLED = 'CANCELLED',
}

export enum ErpPostingStatusDto {
  UNPOSTED = 'UNPOSTED',
  POSTED = 'POSTED',
}

export enum ErpSettlementStatusDto {
  UNPAID = 'UNPAID',
  PARTIAL = 'PARTIAL',
  PAID = 'PAID',
}

export class CreateArReceiptDto {
  @ApiProperty({ example: 'RCP-2026-000001' })
  @IsString()
  @IsNotEmpty()
  docNumber!: string;

  @ApiPropertyOptional() @IsOptional() @IsString() autoNumber?: string;

  @ApiProperty({ example: '1' })
  @IsString()
  @IsNotEmpty()
  branchId!: string;

  @ApiPropertyOptional() @IsOptional() @IsString() locationId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() source?: string;

  @ApiProperty({ example: '2026-05-20' })
  @IsDateString()
  transactionDate!: string;

  @ApiProperty({ example: '1' })
  @IsString()
  @IsNotEmpty()
  fiscalPeriodId!: string;

  @ApiProperty({ example: '1' })
  @IsString()
  @IsNotEmpty()
  partnerId!: string;

  @ApiPropertyOptional() @IsOptional() @IsString() contactPerson?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() bankAccountId?: string;

  @ApiProperty({ example: 'Penerimaan dari pelanggan' })
  @IsString()
  @IsNotEmpty()
  description!: string;

  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiProperty({ example: '1' })
  @IsString()
  @IsNotEmpty()
  currencyId!: string;

  @ApiProperty({ example: '1.000000' })
  @IsNumberString()
  exchangeRate!: string;

  @ApiProperty({ example: '500000.0000' })
  @IsNumberString()
  amount!: string;

  @ApiPropertyOptional() @IsOptional() @IsNumberString() amountFx?: string;

  @ApiProperty({ example: '0.0000' })
  @IsNumberString()
  allocatedAmount!: string;

  @ApiPropertyOptional({ enum: ErpSettlementStatusDto })
  @IsOptional()
  @IsEnum(ErpSettlementStatusDto)
  paymentStatus?: ErpSettlementStatusDto;

  @ApiPropertyOptional({ enum: ErpDocumentStatusDto })
  @IsOptional()
  @IsEnum(ErpDocumentStatusDto)
  status?: ErpDocumentStatusDto;

  @ApiPropertyOptional({ enum: ErpPostingStatusDto })
  @IsOptional()
  @IsEnum(ErpPostingStatusDto)
  postingStatus?: ErpPostingStatusDto;

  @ApiPropertyOptional() @IsOptional() @IsString() legacyCode?: string;
}
