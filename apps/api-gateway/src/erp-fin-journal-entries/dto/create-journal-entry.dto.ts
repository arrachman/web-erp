import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  ArrayMinSize,
  IsArray,
  IsBoolean,
  IsDateString,
  IsEnum,
  IsInt,
  IsNotEmpty,
  IsNumberString,
  IsOptional,
  IsString,
  Min,
  ValidateNested,
} from 'class-validator';

export enum ErpJournalTypeDto {
  GENERAL = 'GENERAL',
  MEMORIAL = 'MEMORIAL',
  ADJUSTMENT = 'ADJUSTMENT',
  OPENING_BALANCE = 'OPENING_BALANCE',
  REVALUATION = 'REVALUATION',
  CLOSING = 'CLOSING',
}

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

export class CreateJournalLineDto {
  @ApiProperty({ example: '101' })
  @IsString()
  @IsNotEmpty()
  accountId!: string;

  @ApiPropertyOptional({ description: 'Default ke mata uang header' })
  @IsOptional()
  @IsString()
  currencyId?: string;

  @ApiPropertyOptional({ description: 'Default ke kurs header' })
  @IsOptional()
  @IsNumberString()
  exchangeRate?: string;

  @ApiProperty({ example: '100000.0000' })
  @IsNumberString()
  debit!: string;

  @ApiProperty({ example: '0.0000' })
  @IsNumberString()
  credit!: string;

  @ApiPropertyOptional() @IsOptional() @IsNumberString() debitFx?: string;
  @ApiPropertyOptional() @IsOptional() @IsNumberString() creditFx?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() costCenterId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() divisionId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() subdivisionId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() projectId?: string;

  @ApiProperty({ example: 1 })
  @IsInt()
  @Min(1)
  lineNo!: number;
}

export class CreateJournalEntryDto {
  @ApiPropertyOptional({ description: 'Kosongkan + auto=true untuk nomor otomatis' })
  @IsOptional()
  @IsString()
  docNumber?: string;

  @ApiPropertyOptional({ default: true, description: 'Generate nomor otomatis bila docNumber kosong' })
  @IsOptional()
  @IsBoolean()
  auto?: boolean;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  autoNumber?: string;

  @ApiProperty({ enum: ErpJournalTypeDto, example: ErpJournalTypeDto.GENERAL })
  @IsEnum(ErpJournalTypeDto)
  journalType!: ErpJournalTypeDto;

  @ApiProperty({ example: '1' })
  @IsString()
  @IsNotEmpty()
  branchId!: string;

  @ApiPropertyOptional() @IsOptional() @IsString() locationId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() source?: string;

  @ApiProperty({ example: '2026-05-20' })
  @IsDateString()
  entryDate!: string;

  @ApiPropertyOptional({ description: 'Opsional — diturunkan dari entryDate bila kosong' })
  @IsOptional()
  @IsString()
  fiscalPeriodId?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() partnerId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() contactPerson?: string;

  @ApiProperty({ example: 'Pencatatan jurnal umum' })
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

  @ApiPropertyOptional({ enum: ErpDocumentStatusDto, default: ErpDocumentStatusDto.DRAFT })
  @IsOptional()
  @IsEnum(ErpDocumentStatusDto)
  status?: ErpDocumentStatusDto;

  @ApiPropertyOptional({ enum: ErpPostingStatusDto, default: ErpPostingStatusDto.UNPOSTED })
  @IsOptional()
  @IsEnum(ErpPostingStatusDto)
  postingStatus?: ErpPostingStatusDto;

  @ApiPropertyOptional() @IsOptional() @IsString() legacyCode?: string;

  @ApiProperty({ type: [CreateJournalLineDto] })
  @IsArray()
  @ArrayMinSize(0)
  @ValidateNested({ each: true })
  @Type(() => CreateJournalLineDto)
  lines!: CreateJournalLineDto[];
}
