import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  ArrayMinSize,
  IsArray,
  IsBoolean,
  IsDateString,
  IsInt,
  IsNotEmpty,
  IsOptional,
  IsString,
  Min,
  ValidateNested,
} from 'class-validator';
import { IsDecimalString } from '../../erp-common/decorators/is-decimal-string.decorator';

/** One BOM input or output line (material component or produced item). */
export class MfgBomLineDto {
  @ApiProperty({ example: '1001', description: 'Item (md_items) id' })
  @IsString()
  @IsNotEmpty()
  itemId!: string;

  @ApiProperty({ example: '10.0000', description: 'Quantity in transaction unit' })
  @IsDecimalString()
  quantity!: string;

  @ApiProperty({ example: '5', description: 'Unit (md_units) id' })
  @IsString()
  @IsNotEmpty()
  unitId!: string;

  @ApiProperty({ example: '150000.0000', description: 'Unit price' })
  @IsDecimalString()
  unitPrice!: string;

  @ApiProperty({ example: '150000.0000', description: 'Unit cost' })
  @IsDecimalString()
  unitCost!: string;

  @ApiPropertyOptional({ example: '100.0000', description: 'Cost allocation percent' })
  @IsOptional()
  @IsDecimalString()
  costPercent?: string;

  @ApiPropertyOptional({ description: 'Source warehouse (md_warehouses) id' })
  @IsOptional()
  @IsString()
  sourceWarehouseId?: string;

  @ApiPropertyOptional({ description: 'Production warehouse (md_warehouses) id' })
  @IsOptional()
  @IsString()
  productionWarehouseId?: string;

  @ApiPropertyOptional({ description: 'Destination warehouse (md_warehouses) id' })
  @IsOptional()
  @IsString()
  destinationWarehouseId?: string;

  @ApiPropertyOptional({ description: 'Inventory account (md_accounts) id' })
  @IsOptional()
  @IsString()
  inventoryAccountId?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() costCenterId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() divisionId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() subdivisionId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() projectId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiProperty({ example: 1, description: 'Line sequence number' })
  @IsInt()
  @Min(1)
  lineNo!: number;
}

export class CreateMfgBomDto {
  @ApiPropertyOptional({
    description: 'Auto-generate docNumber via sys_document_numberings',
    default: true,
  })
  @IsOptional()
  @IsBoolean()
  auto?: boolean;

  @ApiPropertyOptional({
    description: 'Manual doc number; omit (or set auto=true) to server-generate',
  })
  @IsOptional()
  @IsString()
  docNumber?: string;

  @ApiProperty({ example: '2026-06-02', description: 'Tanggal dokumen (YYYY-MM-DD)' })
  @IsDateString()
  docDate!: string;

  @ApiPropertyOptional({ description: 'Fiscal period id; derived from docDate when omitted' })
  @IsOptional()
  @IsString()
  fiscalPeriodId?: string;

  @ApiProperty({ example: '1', description: 'Branch (md_branches) id — Cabang' })
  @IsString()
  @IsNotEmpty()
  branchId!: string;

  @ApiPropertyOptional({ description: 'Location (md_locations) id — Lokasi' })
  @IsOptional()
  @IsString()
  locationId?: string;

  @ApiPropertyOptional({ description: 'Source warehouse (md_warehouses) id' })
  @IsOptional()
  @IsString()
  sourceWarehouseId?: string;

  @ApiPropertyOptional({ description: 'Production warehouse (md_warehouses) id' })
  @IsOptional()
  @IsString()
  productionWarehouseId?: string;

  @ApiPropertyOptional({ description: 'Destination warehouse (md_warehouses) id' })
  @IsOptional()
  @IsString()
  destinationWarehouseId?: string;

  @ApiProperty({ example: '1', description: 'Currency (md_currencies) id' })
  @IsString()
  @IsNotEmpty()
  currencyId!: string;

  @ApiProperty({ example: '1.000000', description: 'Exchange rate' })
  @IsDecimalString()
  exchangeRate!: string;

  @ApiPropertyOptional({ example: '2026-07-01', description: 'Needed date (YYYY-MM-DD)' })
  @IsOptional()
  @IsDateString()
  neededDate?: string;

  @ApiPropertyOptional({ example: '8.0000', description: 'Work estimate (hours/days)' })
  @IsOptional()
  @IsDecimalString()
  workEstimate?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() description?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() referenceNo?: string;

  @ApiPropertyOptional({ example: '2026-06-01', description: 'Reference date (YYYY-MM-DD)' })
  @IsOptional()
  @IsDateString()
  referenceDate?: string;

  @ApiPropertyOptional({ description: 'Requester user (adm_users) id' })
  @IsOptional()
  @IsString()
  requestedById?: string;

  @ApiPropertyOptional({ description: 'Requester partner (md_partners) id' })
  @IsOptional()
  @IsString()
  requestedPartnerId?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() legacyCode?: string;

  @ApiProperty({ type: [MfgBomLineDto], description: 'Input materials (components consumed)' })
  @IsArray()
  @ArrayMinSize(0)
  @ValidateNested({ each: true })
  @Type(() => MfgBomLineDto)
  inputs!: MfgBomLineDto[];

  @ApiProperty({ type: [MfgBomLineDto], description: 'Output products (items produced)' })
  @IsArray()
  @ArrayMinSize(0)
  @ValidateNested({ each: true })
  @Type(() => MfgBomLineDto)
  outputs!: MfgBomLineDto[];
}
