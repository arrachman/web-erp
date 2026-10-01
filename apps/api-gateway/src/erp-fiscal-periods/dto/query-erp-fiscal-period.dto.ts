import { ApiPropertyOptional } from '@nestjs/swagger';
import { ErpFiscalPeriodStatus } from '@prisma/client';
import { Type } from 'class-transformer';
import { IsEnum, IsInt, IsOptional, Max, Min } from 'class-validator';

export class QueryErpFiscalPeriodDto {
  @ApiPropertyOptional({ example: 2025, description: 'Filter by fiscal year' })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(2000)
  @Max(2100)
  year?: number;

  @ApiPropertyOptional({ enum: ErpFiscalPeriodStatus, description: 'Filter by status' })
  @IsOptional()
  @IsEnum(ErpFiscalPeriodStatus)
  status?: ErpFiscalPeriodStatus;
}
