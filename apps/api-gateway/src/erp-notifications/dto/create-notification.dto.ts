import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsDateString, IsInt, IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpNotificationDto {
  @ApiProperty({ description: 'User ID penerima (BigInt sebagai string/number)' })
  @Type(() => Number)
  @IsInt()
  recipientId!: number;

  @ApiProperty({ example: 'success', description: 'Tipe: success|info|warn|danger' })
  @IsString()
  @MaxLength(32)
  type!: string;

  @ApiProperty()
  @IsString()
  @MaxLength(160)
  title!: string;

  @ApiProperty()
  @IsString()
  body!: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  referenceEntity?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  referenceId?: number;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  actionUrl?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsDateString()
  expiresAt?: string;
}
