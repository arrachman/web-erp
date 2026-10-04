import { Type } from 'class-transformer';
import {
  IsArray,
  IsBoolean,
  IsInt,
  IsOptional,
  IsString,
  MaxLength,
  Min,
} from 'class-validator';

export class CreateVdpDatasetDto {
  @IsString()
  @MaxLength(200)
  name!: string;

  @IsOptional()
  @IsString()
  @MaxLength(255)
  sourceFilename?: string;

  /** Kolom yang wajib terisi di setiap baris; bawaan: semua kolom CSV. */
  @IsOptional()
  @IsArray()
  @IsString({ each: true })
  requiredColumns?: string[];

  @IsOptional()
  @IsString()
  @MaxLength(500)
  notes?: string;
}

export class ImportVdpRowsDto {
  /** Teks CSV: baris pertama = header kolom. */
  @IsOptional()
  @IsString()
  csvText?: string;

  /** Alternatif impor: array objek per baris. */
  @IsOptional()
  @IsArray()
  rows?: Record<string, unknown>[];

  @IsOptional()
  @IsString()
  @MaxLength(255)
  sourceFilename?: string;
}

export class QueryVdpRowsDto {
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  page?: number;

  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  limit?: number;

  @IsOptional()
  @IsBoolean()
  @Type(() => Boolean)
  onlyInvalid?: boolean;
}
