import { ApiPropertyOptional } from '@nestjs/swagger';
import { IsOptional, IsString, MaxLength } from 'class-validator';

/**
 * Legacy "Lain-lain" tab — alias names + free-text notes.
 * Persisted under `md_items.metadata.others` (no dedicated columns, §2.38).
 */
export class ItemOthersDto {
  @ApiPropertyOptional({ description: 'Nama Alias 1' })
  @IsOptional() @IsString() @MaxLength(255) aliasName1?: string;

  @ApiPropertyOptional({ description: 'Nama Alias 2' })
  @IsOptional() @IsString() @MaxLength(255) aliasName2?: string;

  @ApiPropertyOptional({ description: 'Nama Alias 3' })
  @IsOptional() @IsString() @MaxLength(255) aliasName3?: string;

  @ApiPropertyOptional({ description: 'Nama Alias 4' })
  @IsOptional() @IsString() @MaxLength(255) aliasName4?: string;

  @ApiPropertyOptional({ description: 'Notes RC' })
  @IsOptional() @IsString() @MaxLength(500) notesRc?: string;

  @ApiPropertyOptional({ description: 'Catatan' })
  @IsOptional() @IsString() @MaxLength(1000) catatan?: string;
}

/**
 * Legacy "Custom" tab — production/moulding attributes.
 * Persisted under `md_items.metadata.custom` (no dedicated columns, §2.38).
 * Numeric values kept as strings for parity with the other Decimal-as-string
 * item fields; the form formats them via `lib/format.ts`.
 */
export class ItemCustomDto {
  @ApiPropertyOptional({ description: 'Kategori Produksi' })
  @IsOptional() @IsString() @MaxLength(255) productionCategory?: string;

  @ApiPropertyOptional({ description: 'Kelompok Produksi' })
  @IsOptional() @IsString() @MaxLength(255) productionGroup?: string;

  @ApiPropertyOptional({ description: 'Max Qty SO' })
  @IsOptional() @IsString() @MaxLength(50) maxQtySo?: string;

  @ApiPropertyOptional({ description: 'Kapasitas Per Jam' })
  @IsOptional() @IsString() @MaxLength(50) capacityPerHour?: string;

  @ApiPropertyOptional({ description: 'Max Qty RC' })
  @IsOptional() @IsString() @MaxLength(50) maxQtyRc?: string;

  @ApiPropertyOptional({ description: 'Allowance' })
  @IsOptional() @IsString() @MaxLength(50) allowance?: string;

  @ApiPropertyOptional({ description: 'WIP 1' })
  @IsOptional() @IsString() @MaxLength(255) wip1?: string;

  @ApiPropertyOptional({ description: 'WIP 2' })
  @IsOptional() @IsString() @MaxLength(255) wip2?: string;

  @ApiPropertyOptional({ description: 'WIP 3' })
  @IsOptional() @IsString() @MaxLength(255) wip3?: string;

  @ApiPropertyOptional({ description: 'Mould Finish' })
  @IsOptional() @IsString() @MaxLength(255) mouldFinish?: string;

  @ApiPropertyOptional({ description: 'Mold Semi 1' })
  @IsOptional() @IsString() @MaxLength(255) moldSemi1?: string;

  @ApiPropertyOptional({ description: 'Mold Semi 2' })
  @IsOptional() @IsString() @MaxLength(255) moldSemi2?: string;

  @ApiPropertyOptional({ description: 'MIN1' })
  @IsOptional() @IsString() @MaxLength(50) min1?: string;

  @ApiPropertyOptional({ description: 'MAX1' })
  @IsOptional() @IsString() @MaxLength(50) max1?: string;

  @ApiPropertyOptional({ description: 'MIN2' })
  @IsOptional() @IsString() @MaxLength(50) min2?: string;

  @ApiPropertyOptional({ description: 'MAX2' })
  @IsOptional() @IsString() @MaxLength(50) max2?: string;
}
