import { ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  IsArray,
  IsDateString,
  IsEnum,
  IsOptional,
  IsString,
  ValidateNested,
} from 'class-validator';
import { ErpDocumentStatusDto, InvDailyCheckLineDto } from './create-inv-daily-check.dto';

/**
 * All-optional update payload. Declared explicitly (not PartialType) so required
 * header fields become optional for partial edits.
 */
export class UpdateInvDailyCheckDto {
  @ApiPropertyOptional() @IsOptional() @IsString() docNumber?: string;
  @ApiPropertyOptional() @IsOptional() @IsDateString() checkDate?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() fiscalPeriodId?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() branchId?: string;
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

  @ApiPropertyOptional({ type: [InvDailyCheckLineDto] })
  @IsOptional()
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => InvDailyCheckLineDto)
  lines?: InvDailyCheckLineDto[];
}
