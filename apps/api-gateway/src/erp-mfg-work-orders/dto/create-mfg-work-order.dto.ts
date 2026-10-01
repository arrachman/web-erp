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

/** Senti document status — mirrors DB ErpDocumentStatus. */
export enum MfgWoDocumentStatusDto {
  DRAFT = 'DRAFT',
  NEED_APPROVE = 'NEED_APPROVE',
  APPROVED = 'APPROVED',
  REJECTED = 'REJECTED',
}

/** One input or output line of a Manufacturing Work Order. */
export class MfgWorkOrderLineDto {
  @ApiProperty({ example: '1001', description: 'Item (md_items) id' })
  @IsString()
  @IsNotEmpty()
  itemId!: string;

  @ApiProperty({ example: '10.0000', description: 'Quantity (transaction unit)' })
  @IsDecimalString()
  quantity!: string;

  @ApiProperty({ example: '5', description: 'Unit (md_units) id' })
  @IsString()
  @IsNotEmpty()
  unitId!: string;

  @ApiPropertyOptional({ example: '150000.0000', description: 'Unit price; defaults to 0' })
  @IsOptional()
  @IsDecimalString()
  unitPrice?: string;

  @ApiPropertyOptional({ example: '0.0000', description: 'Unit cost; defaults to 0' })
  @IsOptional()
  @IsDecimalString()
  unitCost?: string;

  @ApiPropertyOptional({ description: 'Inventory account (md_accounts) id' })
  @IsOptional()
  @IsString()
  inventoryAccountId?: string;

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

  @ApiPropertyOptional() @IsOptional() @IsString() costCenterId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() divisionId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() subdivisionId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() projectId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiProperty({ example: 1 })
  @IsInt()
  @Min(1)
  lineNo!: number;
}

export class CreateMfgWorkOrderDto {
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

  @ApiPropertyOptional({ description: 'BOM (mfg_boms) id — link ke Bill of Materials' })
  @IsOptional()
  @IsString()
  bomId?: string;

  @ApiPropertyOptional({ description: 'Requested by (adm_users) id' })
  @IsOptional()
  @IsString()
  requestedById?: string;

  @ApiPropertyOptional({ description: 'Requested partner (md_partners) id' })
  @IsOptional()
  @IsString()
  requestedPartnerId?: string;

  @ApiPropertyOptional({ example: '2026-07-01', description: 'Needed date (YYYY-MM-DD)' })
  @IsOptional()
  @IsDateString()
  neededDate?: string;

  @ApiPropertyOptional({ example: '8.0000', description: 'Estimated work hours' })
  @IsOptional()
  @IsDecimalString()
  workEstimate?: string;

  @ApiProperty({ example: '1', description: 'Currency (md_currencies) id' })
  @IsString()
  @IsNotEmpty()
  currencyId!: string;

  @ApiProperty({ example: '1.000000' })
  @IsDecimalString()
  exchangeRate!: string;

  @ApiPropertyOptional() @IsOptional() @IsString() description?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() referenceNo?: string;

  @ApiPropertyOptional({ example: '2026-06-01', description: 'Reference date (YYYY-MM-DD)' })
  @IsOptional()
  @IsDateString()
  referenceDate?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() legacyCode?: string;

  @ApiPropertyOptional({ enum: MfgWoDocumentStatusDto, default: MfgWoDocumentStatusDto.DRAFT })
  @IsOptional()
  @IsEnum(MfgWoDocumentStatusDto)
  status?: MfgWoDocumentStatusDto;

  @ApiProperty({ type: [MfgWorkOrderLineDto], description: 'Input material lines' })
  @IsArray()
  @ArrayMinSize(0)
  @ValidateNested({ each: true })
  @Type(() => MfgWorkOrderLineDto)
  inputs!: MfgWorkOrderLineDto[];

  @ApiProperty({ type: [MfgWorkOrderLineDto], description: 'Output product lines' })
  @IsArray()
  @ArrayMinSize(0)
  @ValidateNested({ each: true })
  @Type(() => MfgWorkOrderLineDto)
  outputs!: MfgWorkOrderLineDto[];
}
