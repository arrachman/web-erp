import {
  IsArray,
  IsBoolean,
  IsOptional,
  IsString,
  ValidateNested,
} from 'class-validator';
import { Type } from 'class-transformer';
import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsDecimalString } from '../../erp-common/decorators/is-decimal-string.decorator';

/** Fase 3 W7 — harga kontrak per sekolah (berlapis) + paket/bundle. */

export class CreateContractPriceDto {
  @ApiProperty({ description: 'Partner sekolah (md_partners) id' })
  @IsString()
  partnerId!: string;

  @ApiPropertyOptional({ description: 'Item untuk harga tetap per item' })
  @IsOptional() @IsString()
  itemId?: string;

  @ApiPropertyOptional({ description: 'Kategori untuk diskon per kategori' })
  @IsOptional() @IsString()
  categoryId?: string;

  @ApiPropertyOptional({ description: 'Harga tetap (wajib bila itemId diisi)' })
  @IsOptional() @IsDecimalString()
  price?: string;

  @ApiPropertyOptional({ description: 'Diskon persen 0-100 (untuk baris kategori / seluruh sekolah)' })
  @IsOptional() @IsDecimalString()
  discountPercent?: string;

  @ApiPropertyOptional({ example: '2026-01-01' })
  @IsOptional() @IsString()
  validFrom?: string;

  @ApiPropertyOptional({ example: '2026-12-31' })
  @IsOptional() @IsString()
  validTo?: string;

  @ApiPropertyOptional()
  @IsOptional() @IsString()
  notes?: string;
}

export class UpdateContractPriceDto {
  @ApiPropertyOptional() @IsOptional() @IsDecimalString() price?: string;
  @ApiPropertyOptional() @IsOptional() @IsDecimalString() discountPercent?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() validFrom?: string | null;
  @ApiPropertyOptional() @IsOptional() @IsString() validTo?: string | null;
  @ApiPropertyOptional() @IsOptional() @IsBoolean() isActive?: boolean;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string | null;
}

export class QueryContractPricesDto {
  @ApiPropertyOptional() @IsOptional() @IsString() partnerId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() itemId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() page?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() limit?: string;
}

export class BundleLineDto {
  @ApiProperty() @IsString() componentItemId!: string;
  @ApiProperty({ example: '1' }) @IsDecimalString() quantity!: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;
}

export class UpsertBundleDto {
  @ApiProperty({ description: 'Item paket (md_items) id' })
  @IsString()
  itemId!: string;

  @ApiPropertyOptional() @IsOptional() @IsString() name?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;
  @ApiPropertyOptional() @IsOptional() @IsBoolean() isActive?: boolean;

  @ApiProperty({ type: [BundleLineDto] })
  @IsArray() @ValidateNested({ each: true }) @Type(() => BundleLineDto)
  lines!: BundleLineDto[];
}
