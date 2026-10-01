import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  ArrayMinSize,
  IsArray,
  IsBoolean,
  IsDateString,
  IsEnum,
  IsNotEmpty,
  IsNumberString,
  IsOptional,
  IsString,
  ValidateNested,
} from 'class-validator';

export enum ErpGiroEntryKindDto {
  REGISTER = 'REGISTER',
  CLEAR = 'CLEAR',
}

export enum ErpGiroTypeDto {
  INCOMING = 'INCOMING',
  OUTGOING = 'OUTGOING',
}

export enum ErpDocumentStatusDto {
  DRAFT = 'DRAFT',
  POSTED = 'POSTED',
  VOID = 'VOID',
  CANCELLED = 'CANCELLED',
}

/**
 * One instrument row. Supports BOTH register rows (giroNumber/bankName/dueDate/
 * amount/notes/giroAccountId) AND clearing rows (giroId/clearedDate). All fields
 * are optional here; the service enforces the right shape per header.kind.
 */
export class GiroEntryRowDto {
  // ── REGISTER row fields ─────────────────────────────────────────────
  @ApiPropertyOptional({ description: 'REGISTER: nomor giro (unik, wajib)' })
  @IsOptional()
  @IsString()
  giroNumber?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() bankName?: string;

  @ApiPropertyOptional({ description: 'REGISTER: jatuh tempo (wajib)', example: '2026-06-30' })
  @IsOptional()
  @IsDateString()
  dueDate?: string;

  @ApiPropertyOptional({ description: 'REGISTER: nominal (wajib)', example: '5000000.0000' })
  @IsOptional()
  @IsNumberString()
  amount?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiPropertyOptional({ description: 'REGISTER: default ke giroAccountId header' })
  @IsOptional()
  @IsString()
  giroAccountId?: string;

  // ── CLEAR row fields ────────────────────────────────────────────────
  @ApiPropertyOptional({ description: 'CLEAR: id giro outstanding yang dikliring (wajib)' })
  @IsOptional()
  @IsString()
  giroId?: string;

  @ApiPropertyOptional({ description: 'CLEAR: tanggal kliring (wajib)', example: '2026-06-30' })
  @IsOptional()
  @IsDateString()
  clearedDate?: string;
}

export class CreateGiroEntryDto {
  @ApiPropertyOptional({ description: 'Kosongkan + auto=true untuk nomor otomatis' })
  @IsOptional()
  @IsString()
  docNumber?: string;

  @ApiPropertyOptional({ default: true })
  @IsOptional()
  @IsBoolean()
  auto?: boolean;

  @ApiProperty({ enum: ErpGiroEntryKindDto, example: ErpGiroEntryKindDto.REGISTER })
  @IsEnum(ErpGiroEntryKindDto)
  kind!: ErpGiroEntryKindDto;

  @ApiProperty({ enum: ErpGiroTypeDto, example: ErpGiroTypeDto.INCOMING })
  @IsEnum(ErpGiroTypeDto)
  type!: ErpGiroTypeDto;

  @ApiProperty({ example: '1' })
  @IsString()
  @IsNotEmpty()
  branchId!: string;

  @ApiPropertyOptional() @IsOptional() @IsString() partnerId?: string;

  @ApiProperty({ example: '2026-06-03' })
  @IsDateString()
  entryDate!: string;

  @ApiPropertyOptional({ description: 'Opsional — diturunkan dari entryDate bila kosong' })
  @IsOptional()
  @IsString()
  fiscalPeriodId?: string;

  @ApiPropertyOptional({ description: 'CLEAR: bank settlement (wajib untuk CLEAR)' })
  @IsOptional()
  @IsString()
  bankAccountId?: string;

  @ApiPropertyOptional({ description: 'REGISTER: default akun giro untuk tiap instrumen' })
  @IsOptional()
  @IsString()
  giroAccountId?: string;

  @ApiProperty({ example: '1' })
  @IsString()
  @IsNotEmpty()
  currencyId!: string;

  @ApiProperty({ example: '1.000000' })
  @IsNumberString()
  exchangeRate!: string;

  @ApiPropertyOptional() @IsOptional() @IsString() description?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() legacyCode?: string;

  @ApiProperty({ type: [GiroEntryRowDto] })
  @IsArray()
  @ArrayMinSize(0)
  @ValidateNested({ each: true })
  @Type(() => GiroEntryRowDto)
  rows!: GiroEntryRowDto[];
}
