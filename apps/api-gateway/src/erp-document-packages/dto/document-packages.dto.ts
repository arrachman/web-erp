import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  IsArray,
  IsBoolean,
  IsDateString,
  IsEnum,
  IsInt,
  IsOptional,
  IsString,
  Max,
  Min,
} from 'class-validator';

export enum DocKindDto {
  PENAWARAN = 'PENAWARAN',
  SURAT_PESANAN = 'SURAT_PESANAN',
  INVOICE = 'INVOICE',
  KUITANSI = 'KUITANSI',
  SURAT_JALAN = 'SURAT_JALAN',
  BAST = 'BAST',
}

export enum DocVariantDto {
  BOS = 'BOS',
  NON_BOS = 'NON_BOS',
}

export class QueryDocumentPackagesDto {
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

  @ApiPropertyOptional({ description: 'Nomor dokumen sumber / nomor order' })
  @IsOptional()
  @IsString()
  search?: string;

  @ApiPropertyOptional({ description: 'School (md_partners) id' })
  @IsOptional()
  @IsString()
  schoolId?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  budgetYear?: number;

  @ApiPropertyOptional({ enum: DocKindDto })
  @IsOptional()
  @IsString()
  docType?: string;

  @ApiPropertyOptional({ enum: ['GENERATED', 'SIGNED', 'SUPERSEDED'] })
  @IsOptional()
  @IsString()
  status?: string;

  @ApiPropertyOptional({ description: 'Sales order id' })
  @IsOptional()
  @IsString()
  orderId?: string;
}

export class GenerateDocumentsDto {
  @ApiProperty({ description: 'Sales order id' })
  @IsString()
  orderId!: string;

  @ApiProperty({ enum: DocKindDto, isArray: true })
  @IsArray()
  @IsEnum(DocKindDto, { each: true })
  docTypes!: DocKindDto[];

  @ApiPropertyOptional({ enum: DocVariantDto, description: 'Default follows the order funding source' })
  @IsOptional()
  @IsEnum(DocVariantDto)
  variant?: DocVariantDto;

  @ApiPropertyOptional({ description: 'Render all selected documents into one package PDF' })
  @IsOptional()
  @IsBoolean()
  package?: boolean;
}

export class SignDocumentDto {
  @ApiProperty({ description: 'Nama penanda tangan (sesuai stempel/tanda tangan digital)' })
  @IsString()
  signerName!: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  note?: string;
}

export class RecordAcceptanceDto {
  @ApiPropertyOptional({ description: 'Waktu penerimaan (ISO); default sekarang' })
  @IsOptional()
  @IsDateString()
  acceptedAt?: string;

  @ApiProperty({ description: 'Nama penerima barang di sekolah' })
  @IsString()
  acceptedByName!: string;

  @ApiPropertyOptional({ description: 'Jabatan penerima (Kepala Sekolah/Bendahara/dll.)' })
  @IsOptional()
  @IsString()
  acceptedByTitle?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  notes?: string;
}
