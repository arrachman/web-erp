import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsDateString, IsEnum, IsNotEmpty, IsOptional, IsString } from 'class-validator';
import { IsDecimalString } from '../../erp-common/decorators/is-decimal-string.decorator';

/** Senti 7-state document status (§2.7). */
export enum ErpDocumentStatusWbDto {
  DRAFT = 'DRAFT',
  NEED_APPROVE = 'NEED_APPROVE',
  APPROVED = 'APPROVED',
  REJECTED = 'REJECTED',
  POSTED = 'POSTED',
  VOID = 'VOID',
  CANCELLED = 'CANCELLED',
}

/**
 * Create payload for a weighbridge ticket (RW). Header-only document.
 * netWeight is computed server-side: grossWeight - tareWeight.
 * docNumber: user enters manually; system falls back to sys_document_numberings
 * code 'RW' if omitted.
 */
export class CreateInvWeighbridgeTicketDto {
  @ApiPropertyOptional({ description: 'Doc number (RW-XXXXXX). Falls back to sys_document_numberings code RW if omitted.' })
  @IsOptional()
  @IsString()
  docNumber?: string;

  @ApiProperty({ description: 'Ticket date (YYYY-MM-DD)' })
  @IsDateString()
  ticketDate!: string;

  @ApiPropertyOptional({ description: 'Derived from ticketDate if omitted' })
  @IsOptional()
  @IsString()
  fiscalPeriodId?: string;

  @ApiProperty({ description: 'Branch id (required)' })
  @IsNotEmpty()
  @IsString()
  branchId!: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  locationId?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  vehiclePlate?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  driverName?: string;

  @ApiPropertyOptional({ description: 'Partner id (supplier/customer)' })
  @IsOptional()
  @IsString()
  partnerId?: string;

  @ApiPropertyOptional({ description: 'Item id' })
  @IsOptional()
  @IsString()
  itemId?: string;

  @ApiProperty({ description: 'Gross weight (decimal)', default: '0' })
  @IsDecimalString()
  grossWeight!: string;

  @ApiProperty({ description: 'Tare weight (decimal)', default: '0' })
  @IsDecimalString()
  tareWeight!: string;

  @ApiPropertyOptional({ description: 'Unit price per kg/ton' })
  @IsOptional()
  @IsDecimalString()
  unitPrice?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  description?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  notes?: string;

  @ApiPropertyOptional({ enum: ErpDocumentStatusWbDto })
  @IsOptional()
  @IsEnum(ErpDocumentStatusWbDto)
  status?: ErpDocumentStatusWbDto;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  legacyCode?: string;
}
