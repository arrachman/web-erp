import { ApiPropertyOptional } from '@nestjs/swagger';
import { IsOptional, IsString } from 'class-validator';

export class RepostJournalDto {
  @ApiPropertyOptional() @IsOptional() @IsString()
  fiscalPeriodId?: string;

  @ApiPropertyOptional() @IsOptional() @IsString()
  fromDate?: string;

  @ApiPropertyOptional() @IsOptional() @IsString()
  toDate?: string;

  @ApiPropertyOptional({ description: 'Which module to repost (e.g. AR, AP, GL)' })
  @IsOptional() @IsString()
  module?: string;
}
