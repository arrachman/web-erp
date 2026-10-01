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
  ErpStockMovementTypeDto,
  InvStockMovementLineDto,
} from './create-inv-stock-movement.dto';

/**
 * All-optional update payload. Declared explicitly (not PartialType) so the
 * required header fields (movementType/movementDate/branchId) become optional
 * for partial edits — mirrors update-sls-order style.
 */
export class UpdateInvStockMovementDto {
  @ApiPropertyOptional() @IsOptional() @IsString() docNumber?: string;

  @ApiPropertyOptional({ enum: ErpStockMovementTypeDto })
  @IsOptional()
  @IsEnum(ErpStockMovementTypeDto)
  movementType?: ErpStockMovementTypeDto;

  @ApiPropertyOptional() @IsOptional() @IsDateString() movementDate?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() fiscalPeriodId?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() branchId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() locationId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() sourceWarehouseId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() transitWarehouseId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() destinationWarehouseId?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() requestedPartnerId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() requestedTo?: string;
  @ApiPropertyOptional() @IsOptional() @IsDateString() neededDate?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() referenceNo?: string;
  @ApiPropertyOptional() @IsOptional() @IsDateString() referenceDate?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() description?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiPropertyOptional({ enum: ErpDocumentStatusDto })
  @IsOptional()
  @IsEnum(ErpDocumentStatusDto)
  status?: ErpDocumentStatusDto;

  @ApiPropertyOptional() @IsOptional() @IsString() legacyCode?: string;

  @ApiPropertyOptional({ type: [InvStockMovementLineDto] })
  @IsOptional()
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => InvStockMovementLineDto)
  lines?: InvStockMovementLineDto[];
}
