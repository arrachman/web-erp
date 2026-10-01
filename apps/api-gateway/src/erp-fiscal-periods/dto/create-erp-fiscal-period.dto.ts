import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { ErpFiscalPeriodStatus } from '@prisma/client';
import { Type } from 'class-transformer';
import { IsDate, IsEnum, IsInt, IsOptional, IsString, Max, MaxLength, Min } from 'class-validator';

export class CreateErpFiscalPeriodDto {
  @ApiProperty({ example: 2025, description: 'Fiscal year' })
  @IsInt()
  @Min(2000)
  @Max(2100)
  year!: number;

  @ApiProperty({ example: 1, description: 'Period number within the year (1–12 for monthly)' })
  @IsInt()
  @Min(1)
  @Max(12)
  periodNo!: number;

  @ApiProperty({ example: 'Jan 2025', description: 'Human-readable period name' })
  @IsString()
  @MaxLength(100)
  name!: string;

  @ApiProperty({ example: '2025-01-01', description: 'Period start date (YYYY-MM-DD)' })
  @Type(() => Date)
  @IsDate()
  startDate!: Date;

  @ApiProperty({ example: '2025-01-31', description: 'Period end date (YYYY-MM-DD)' })
  @Type(() => Date)
  @IsDate()
  endDate!: Date;

  @ApiPropertyOptional({
    enum: ErpFiscalPeriodStatus,
    default: ErpFiscalPeriodStatus.OPEN,
    description: 'Initial period status',
  })
  @IsOptional()
  @IsEnum(ErpFiscalPeriodStatus)
  status?: ErpFiscalPeriodStatus;
}
