import { ApiPropertyOptional } from '@nestjs/swagger';
import { IsOptional, IsString } from 'class-validator';

export class RecalcCogsDto {
  @ApiPropertyOptional() @IsOptional() @IsString()
  fiscalPeriodId?: string;

  @ApiPropertyOptional() @IsOptional() @IsString()
  fromDate?: string;

  @ApiPropertyOptional() @IsOptional() @IsString()
  toDate?: string;
}
