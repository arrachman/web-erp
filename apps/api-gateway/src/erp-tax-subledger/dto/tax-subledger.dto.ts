import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  IsDateString,
  IsEnum,
  IsInt,
  IsOptional,
  IsString,
  Max,
  Min,
} from 'class-validator';
import { IsDecimalString } from '../../erp-common/decorators/is-decimal-string.decorator';

export enum TaxEntryTypeDto {
  PPN_KELUARAN = 'PPN_KELUARAN',
  PPN_MASUKAN = 'PPN_MASUKAN',
  PPH_21 = 'PPH_21',
  PPH_22 = 'PPH_22',
  PPH_23 = 'PPH_23',
  PPH_4_2 = 'PPH_4_2',
  PPH_25 = 'PPH_25',
  PPH_26 = 'PPH_26',
  OTHER = 'OTHER',
}

export class QueryTaxEntriesDto {
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

  @ApiPropertyOptional({ description: 'Nomor dokumen / nama partner' })
  @IsOptional()
  @IsString()
  search?: string;

  @ApiPropertyOptional({ enum: TaxEntryTypeDto })
  @IsOptional()
  @IsEnum(TaxEntryTypeDto)
  taxEntryType?: TaxEntryTypeDto;

  @ApiPropertyOptional({ enum: ['DRAFT', 'CONFIRMED', 'REPORTED', 'CANCELLED'] })
  @IsOptional()
  @IsString()
  status?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  year?: number;

  @ApiPropertyOptional()
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  @Max(12)
  month?: number;
}

export class CreateTaxEntryDto {
  @ApiProperty({ description: 'Nomor dokumen sumber (mis. nomor invoice)' })
  @IsString()
  docNumber!: string;

  @ApiProperty({ description: 'Tanggal transaksi (YYYY-MM-DD)' })
  @IsDateString()
  transactionDate!: string;

  @ApiProperty({ description: 'Master pajak (md_taxes) id' })
  @IsString()
  taxId!: string;

  @ApiProperty({ enum: TaxEntryTypeDto })
  @IsEnum(TaxEntryTypeDto)
  taxEntryType!: TaxEntryTypeDto;

  @ApiProperty({ description: 'Dasar pengenaan pajak' })
  @IsDecimalString()
  dpp!: string;

  @ApiProperty({ description: 'Jumlah pajak / potongan yang diharapkan' })
  @IsDecimalString()
  taxAmount!: string;

  @ApiPropertyOptional({ description: 'Partner (sekolah/vendor) id' })
  @IsOptional()
  @IsString()
  partnerId?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  module?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  sourceDocType?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  sourceId?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  fakturNumber?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsDateString()
  fakturDate?: string;
}

export class AssignFakturDto {
  @ApiProperty({ description: 'Nomor faktur pajak (Coretax)' })
  @IsString()
  fakturNumber!: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsDateString()
  fakturDate?: string;
}

export class SetTaxEntryStatusDto {
  @ApiProperty({ enum: ['CONFIRMED', 'REPORTED', 'CANCELLED'] })
  @IsString()
  status!: string;
}

export class CreateWhtCertificateDto {
  @ApiProperty({ description: 'Nomor bukti potong dari bendahara' })
  @IsString()
  certNumber!: string;

  @ApiProperty({ enum: TaxEntryTypeDto, description: 'PPH_22 / PPH_23' })
  @IsEnum(TaxEntryTypeDto)
  pphType!: TaxEntryTypeDto;

  @ApiProperty({ description: 'Tanggal bukti potong (YYYY-MM-DD)' })
  @IsDateString()
  transactionDate!: string;

  @ApiProperty({ description: 'Partner (sekolah pemotong) id' })
  @IsString()
  partnerId!: string;

  @ApiProperty({ description: 'DPP pada bukti potong' })
  @IsDecimalString()
  dpp!: string;

  @ApiProperty({ description: 'Tarif %' })
  @IsDecimalString()
  rate!: string;

  @ApiProperty({ description: 'Jumlah yang dipotong' })
  @IsDecimalString()
  amountWithheld!: string;

  @ApiPropertyOptional({ description: 'Entri pajak (ekspektasi potongan) yang direkonsiliasi' })
  @IsOptional()
  @IsString()
  taxEntryId?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  notes?: string;
}

export class MonthQueryDto {
  @ApiProperty()
  @Type(() => Number)
  @IsInt()
  year!: number;

  @ApiProperty()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  @Max(12)
  month!: number;
}
