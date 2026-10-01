import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  ArrayMinSize, IsArray, IsBoolean, IsDateString, IsEnum,
  IsInt, IsNotEmpty, IsOptional, IsString, Min, ValidateNested,
} from 'class-validator';

export enum ErpDocumentStatusDto {
  DRAFT = 'DRAFT', NEED_APPROVE = 'NEED_APPROVE', APPROVED = 'APPROVED',
  REJECTED = 'REJECTED', POSTED = 'POSTED', VOID = 'VOID', CANCELLED = 'CANCELLED',
}

/**
 * BS "lines" = bid evaluation rows. Each row references a quotation line
 * (quotationLineId NOT NULL) and marks rank + winner flag.
 */
export class PurBidSelectionLineDto {
  @ApiProperty({ description: 'Quotation line (pur_quotation_lines) id — NOT NULL' })
  @IsString() @IsNotEmpty() quotationLineId!: string;

  @ApiPropertyOptional({ default: false, description: 'Is this line selected as winner?' })
  @IsOptional() @IsBoolean() selected?: boolean;

  @ApiProperty({ example: 1, description: 'Price rank (1 = cheapest)' })
  @IsInt() @Min(0) priceRank!: number;

  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiProperty({ example: 1 }) @IsInt() @Min(1) lineNo!: number;
}

export class CreatePurBidSelectionDto {
  @ApiPropertyOptional({ default: true }) @IsOptional() @IsBoolean() auto?: boolean;
  @ApiPropertyOptional() @IsOptional() @IsString() docNumber?: string;
  @ApiProperty({ example: '2026-06-02' }) @IsDateString() docDate!: string;
  @ApiPropertyOptional() @IsOptional() @IsString() fiscalPeriodId?: string;
  @ApiProperty({ example: '1' }) @IsString() @IsNotEmpty() branchId!: string;
  @ApiPropertyOptional() @IsOptional() @IsString() locationId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() description?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() referenceNo?: string;
  @ApiPropertyOptional({ enum: ErpDocumentStatusDto, default: ErpDocumentStatusDto.DRAFT })
  @IsOptional() @IsEnum(ErpDocumentStatusDto) status?: ErpDocumentStatusDto;
  @ApiPropertyOptional() @IsOptional() @IsString() legacyCode?: string;

  /** Bid evaluation lines (reference quotation lines). */
  @ApiProperty({ type: [PurBidSelectionLineDto] })
  @IsArray() @ArrayMinSize(0) @ValidateNested({ each: true }) @Type(() => PurBidSelectionLineDto)
  lines!: PurBidSelectionLineDto[];
}
