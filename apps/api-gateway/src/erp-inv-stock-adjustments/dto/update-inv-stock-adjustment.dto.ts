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
import {
  ErpDocumentStatusDto,
  InvStockAdjustmentLineDto,
} from './create-inv-stock-adjustment.dto';

/**
 * All-optional update payload. Declared explicitly (not PartialType) so the
 * required header fields (adjustmentDate/branchId/warehouseId) become optional
 * for partial edits — mirrors update-inv-stock-movement style.
 */
export class UpdateInvStockAdjustmentDto {
  @ApiPropertyOptional() @IsOptional() @IsString() docNumber?: string;

  @ApiPropertyOptional() @IsOptional() @IsDateString() adjustmentDate?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() fiscalPeriodId?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() branchId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() warehouseId?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() kind?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() stockCountId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() description?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiPropertyOptional({ enum: ErpDocumentStatusDto })
  @IsOptional()
  @IsEnum(ErpDocumentStatusDto)
  status?: ErpDocumentStatusDto;

  @ApiPropertyOptional() @IsOptional() @IsString() legacyCode?: string;

  @ApiPropertyOptional({ type: [InvStockAdjustmentLineDto] })
  @IsOptional()
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => InvStockAdjustmentLineDto)
  lines?: InvStockAdjustmentLineDto[];
}
