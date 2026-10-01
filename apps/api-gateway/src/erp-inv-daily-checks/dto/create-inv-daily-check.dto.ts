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
  IsOptional,
  IsString,
  Min,
  ValidateNested,
} from 'class-validator';
import { IsDecimalString } from '../../erp-common/decorators/is-decimal-string.decorator';

/** Senti 7-state document status (§2.7). */
export enum ErpDocumentStatusDto {
  DRAFT = 'DRAFT',
  NEED_APPROVE = 'NEED_APPROVE',
  APPROVED = 'APPROVED',
  REJECTED = 'REJECTED',
  POSTED = 'POSTED',
  VOID = 'VOID',
  CANCELLED = 'CANCELLED',
}

/** One item line of a daily check → inv_daily_check_lines. */
export class InvDailyCheckLineDto {
  @ApiProperty() @IsNotEmpty() @IsString() itemId!: string;
  @ApiProperty({ example: '1' }) @IsDecimalString() quantity!: string;
  @ApiProperty() @IsNotEmpty() @IsString() unitId!: string;

  @ApiPropertyOptional() @IsOptional() @IsString() warehouseId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() costCenterId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() machineRef?: string;
  @ApiPropertyOptional({ example: '8' }) @IsOptional() @IsDecimalString() workHours?: string;

  @ApiProperty({ example: 1 }) @IsInt() @Min(1) lineNo!: number;
}

/** Create payload for a daily check / time sheet (header + item lines). */
export class CreateInvDailyCheckDto {
  @ApiPropertyOptional({ default: true }) @IsOptional() @IsBoolean() auto?: boolean;
  @ApiPropertyOptional() @IsOptional() @IsString() docNumber?: string;

  @ApiProperty({ description: 'YYYY-MM-DD' }) @IsDateString() checkDate!: string;
  @ApiPropertyOptional({ description: 'Derived from checkDate if omitted' })
  @IsOptional()
  @IsString()
  fiscalPeriodId?: string;

  @ApiProperty() @IsNotEmpty() @IsString() branchId!: string;
  @ApiPropertyOptional() @IsOptional() @IsString() locationId?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() machineRef?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() operatorRef?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() description?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiPropertyOptional({ enum: ErpDocumentStatusDto })
  @IsOptional()
  @IsEnum(ErpDocumentStatusDto)
  status?: ErpDocumentStatusDto;

  @ApiPropertyOptional() @IsOptional() @IsString() legacyCode?: string;

  @ApiProperty({ type: [InvDailyCheckLineDto] })
  @IsArray()
  @ArrayMinSize(0)
  @ValidateNested({ each: true })
  @Type(() => InvDailyCheckLineDto)
  lines!: InvDailyCheckLineDto[];
}
