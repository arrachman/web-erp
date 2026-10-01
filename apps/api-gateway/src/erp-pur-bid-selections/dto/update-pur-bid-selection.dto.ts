import { ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsArray, IsDateString, IsEnum, IsOptional, IsString, ValidateNested } from 'class-validator';
import { ErpDocumentStatusDto, PurBidSelectionLineDto } from './create-pur-bid-selection.dto';

export class UpdatePurBidSelectionDto {
  @IsOptional() @IsString() docNumber?: string;
  @IsOptional() @IsDateString() docDate?: string;
  @IsOptional() @IsString() fiscalPeriodId?: string;
  @IsOptional() @IsString() branchId?: string;
  @IsOptional() @IsString() locationId?: string;
  @IsOptional() @IsString() description?: string;
  @IsOptional() @IsString() notes?: string;
  @IsOptional() @IsString() referenceNo?: string;
  @IsOptional() @IsEnum(ErpDocumentStatusDto) status?: ErpDocumentStatusDto;
  @IsOptional() @IsString() legacyCode?: string;

  @ApiPropertyOptional({ type: [PurBidSelectionLineDto] })
  @IsOptional() @IsArray() @ValidateNested({ each: true }) @Type(() => PurBidSelectionLineDto)
  lines?: PurBidSelectionLineDto[];
}
