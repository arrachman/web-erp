import { ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  IsArray,
  IsDateString,
  IsEnum,
  IsInt,
  IsOptional,
  IsString,
  Min,
  ValidateNested,
} from 'class-validator';
import {
  ErpDocumentStatusDto,
  ErpStockCountTypeDto,
  InvStockCountLineDto,
} from './create-inv-stock-count.dto';

/**
 * All-optional update payload. Declared explicitly (not PartialType) so the
 * required header fields (countDate/branchId/warehouseId) become optional for
 * partial edits — mirrors update-inv-stock-movement style.
 */
export class UpdateInvStockCountDto {
  @ApiPropertyOptional() @IsOptional() @IsString() docNumber?: string;

  @ApiPropertyOptional() @IsOptional() @IsDateString() countDate?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() fiscalPeriodId?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() branchId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() warehouseId?: string;

  @ApiPropertyOptional({ enum: ErpStockCountTypeDto })
  @IsOptional()
  @IsEnum(ErpStockCountTypeDto)
  countType?: ErpStockCountTypeDto;

  @ApiPropertyOptional() @IsOptional() @IsInt() @Min(0) stepNo?: number;
  @ApiPropertyOptional() @IsOptional() @IsString() description?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiPropertyOptional({ enum: ErpDocumentStatusDto })
  @IsOptional()
  @IsEnum(ErpDocumentStatusDto)
  status?: ErpDocumentStatusDto;

  @ApiPropertyOptional() @IsOptional() @IsString() legacyCode?: string;

  @ApiPropertyOptional({ type: [InvStockCountLineDto] })
  @IsOptional()
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => InvStockCountLineDto)
  lines?: InvStockCountLineDto[];
}
