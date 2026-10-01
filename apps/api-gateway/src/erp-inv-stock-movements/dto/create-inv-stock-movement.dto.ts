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

/** Movement type discriminator — selects which M3 transaction (MR/TS/RS/RF). */
export enum ErpStockMovementTypeDto {
  REQUEST = 'REQUEST', // Material Request (MR)
  ISSUE = 'ISSUE', // Fuel Refill (RF) / generic issue
  TRANSFER = 'TRANSFER', // Stock Transfer (TS)
  TRANSFER_RECEIPT = 'TRANSFER_RECEIPT', // Transfer Receipt (RS)
  RETURN = 'RETURN',
}

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

/** One item line of a stock movement → inv_stock_movement_lines. */
export class InvStockMovementLineDto {
  @ApiProperty() @IsNotEmpty() @IsString() itemId!: string;
  @ApiProperty({ example: '1' }) @IsDecimalString() quantity!: string;
  @ApiProperty() @IsNotEmpty() @IsString() unitId!: string;

  @ApiPropertyOptional() @IsOptional() @IsDecimalString() unitCost?: string;
  @ApiPropertyOptional() @IsOptional() @IsDecimalString() salePrice?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() sourceWarehouseId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() destinationWarehouseId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() costCenterId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() divisionId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() subdivisionId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() projectId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiProperty({ example: 1 }) @IsInt() @Min(1) lineNo!: number;
}

/** Create payload for a stock movement (header + item lines). */
export class CreateInvStockMovementDto {
  @ApiPropertyOptional({ default: true }) @IsOptional() @IsBoolean() auto?: boolean;
  @ApiPropertyOptional() @IsOptional() @IsString() docNumber?: string;

  @ApiProperty({ enum: ErpStockMovementTypeDto })
  @IsEnum(ErpStockMovementTypeDto)
  movementType!: ErpStockMovementTypeDto;

  @ApiProperty({ description: 'YYYY-MM-DD' }) @IsDateString() movementDate!: string;
  @ApiPropertyOptional({ description: 'Derived from movementDate if omitted' })
  @IsOptional()
  @IsString()
  fiscalPeriodId?: string;

  @ApiProperty() @IsNotEmpty() @IsString() branchId!: string;
  @ApiPropertyOptional() @IsOptional() @IsString() locationId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() sourceWarehouseId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() transitWarehouseId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() destinationWarehouseId?: string;

  @ApiPropertyOptional({ description: 'Partner peminta (requested_partner_id)' })
  @IsOptional()
  @IsString()
  requestedPartnerId?: string;
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

  @ApiProperty({ type: [InvStockMovementLineDto] })
  @IsArray()
  @ArrayMinSize(0)
  @ValidateNested({ each: true })
  @Type(() => InvStockMovementLineDto)
  lines!: InvStockMovementLineDto[];
}
