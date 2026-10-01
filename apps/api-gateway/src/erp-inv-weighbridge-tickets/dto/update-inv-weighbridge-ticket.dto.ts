import { ApiPropertyOptional } from '@nestjs/swagger';
import { IsDateString, IsEnum, IsOptional, IsString } from 'class-validator';
import { IsDecimalString } from '../../erp-common/decorators/is-decimal-string.decorator';
import { ErpDocumentStatusWbDto } from './create-inv-weighbridge-ticket.dto';

/**
 * All-optional update payload for a weighbridge ticket.
 * netWeight is recomputed server-side whenever grossWeight or tareWeight changes.
 */
export class UpdateInvWeighbridgeTicketDto {
  @ApiPropertyOptional() @IsOptional() @IsString() docNumber?: string;

  @ApiPropertyOptional({ description: 'YYYY-MM-DD' })
  @IsOptional()
  @IsDateString()
  ticketDate?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() fiscalPeriodId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() branchId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() locationId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() vehiclePlate?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() driverName?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() partnerId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() itemId?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsDecimalString()
  grossWeight?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsDecimalString()
  tareWeight?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsDecimalString()
  unitPrice?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() description?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiPropertyOptional({ enum: ErpDocumentStatusWbDto })
  @IsOptional()
  @IsEnum(ErpDocumentStatusWbDto)
  status?: ErpDocumentStatusWbDto;

  @ApiPropertyOptional() @IsOptional() @IsString() legacyCode?: string;
}
