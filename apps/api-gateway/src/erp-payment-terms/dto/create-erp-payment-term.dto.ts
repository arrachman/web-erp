import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsInt, IsNotEmpty, IsOptional, IsString, MaxLength, Min } from 'class-validator';
import { Type } from 'class-transformer';

export class CreateErpPaymentTermDto {
  @ApiProperty({ example: 'NET30', description: 'Unique payment term code' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Net 30 Days' })
  @IsString()
  @MaxLength(120)
  name!: string;

  @ApiProperty({ example: 30, description: 'Net due days' })
  @Type(() => Number)
  @IsInt()
  @Min(0)
  netDays!: number;

  @ApiPropertyOptional({ example: 10, description: 'Early payment discount days (tier 1)' })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(0)
  discountDays1?: number;

  @ApiPropertyOptional({ example: '2.00', description: 'Early payment discount percent (tier 1), Decimal' })
  @IsOptional()
  @IsString()
  discountPercent1?: string;

  @ApiPropertyOptional({ example: 5, description: 'Early payment discount days (tier 2)' })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(0)
  discountDays2?: number;

  @ApiPropertyOptional({ example: '1.00', description: 'Early payment discount percent (tier 2), Decimal' })
  @IsOptional()
  @IsString()
  discountPercent2?: string;

  @ApiPropertyOptional({ example: '1.50', description: 'Late payment penalty percent, Decimal' })
  @IsOptional()
  @IsString()
  penaltyPercent?: string;

  @ApiPropertyOptional({ example: 'monthly', description: 'Penalty period descriptor' })
  @IsOptional()
  @IsString()
  @MaxLength(50)
  penaltyPeriod?: string;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}
