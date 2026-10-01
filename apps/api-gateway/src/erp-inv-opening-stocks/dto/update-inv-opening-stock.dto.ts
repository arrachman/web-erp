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
import { IsDecimalString } from '../../erp-common/decorators/is-decimal-string.decorator';
import {
  ErpDocumentStatusDto,
  InvOpeningStockLineDto,
} from './create-inv-opening-stock.dto';

/**
 * All-optional update payload. Declared explicitly (not PartialType) so the
 * required header fields (openingDate/branchId/warehouseId/currencyId/exchangeRate)
 * become optional for partial edits — mirrors update-inv-stock-movement style.
 */
export class UpdateInvOpeningStockDto {
  @ApiPropertyOptional() @IsOptional() @IsString() docNumber?: string;

  @ApiPropertyOptional() @IsOptional() @IsDateString() openingDate?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() fiscalPeriodId?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() branchId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() warehouseId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() locationId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() kind?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() currencyId?: string;
  @ApiPropertyOptional() @IsOptional() @IsDecimalString() exchangeRate?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() description?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiPropertyOptional({ enum: ErpDocumentStatusDto })
  @IsOptional()
  @IsEnum(ErpDocumentStatusDto)
  status?: ErpDocumentStatusDto;

  @ApiPropertyOptional() @IsOptional() @IsString() legacyCode?: string;

  @ApiPropertyOptional({ type: [InvOpeningStockLineDto] })
  @IsOptional()
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => InvOpeningStockLineDto)
  lines?: InvOpeningStockLineDto[];
}
