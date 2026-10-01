import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import {
  IsArray,
  IsBoolean,
  IsDateString,
  IsEmail,
  IsEnum,
  IsOptional,
  IsString,
  MaxLength,
  MinLength,
} from 'class-validator';
import { ErpUserLevel } from '@prisma/client';

export class CreateErpUserDto {
  @ApiProperty({ example: 'johndoe', description: 'Unique username (code)' })
  @IsString()
  @MaxLength(50)
  username!: string;

  @ApiPropertyOptional({ example: 'john@example.com' })
  @IsOptional()
  @IsEmail()
  @MaxLength(150)
  email?: string;

  @ApiProperty({ example: 'Secret@123', description: 'Plain password — will be hashed by service' })
  @IsString()
  @MinLength(8)
  @MaxLength(100)
  password!: string;

  @ApiProperty({ example: 'John Doe' })
  @IsString()
  @MaxLength(150)
  fullName!: string;

  @ApiProperty({ enum: ErpUserLevel, example: ErpUserLevel.CENTRAL })
  @IsEnum(ErpUserLevel)
  erpLevel!: ErpUserLevel;

  @ApiPropertyOptional({ example: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;

  @ApiPropertyOptional({
    example: '1',
    description: 'Branch ID as numeric string (BigInt)',
  })
  @IsOptional()
  @IsString()
  branchId?: string;

  @ApiPropertyOptional({ example: '1', description: 'Home warehouse ID (BigInt string)' })
  @IsOptional()
  @IsString()
  homeWarehouseId?: string;

  @ApiPropertyOptional({ example: 'id', description: 'Language code: id or en' })
  @IsOptional()
  @IsString()
  @MaxLength(5)
  language?: string;

  @ApiPropertyOptional({ example: '2100-12-31', description: 'Account expiry date (ISO 8601)' })
  @IsOptional()
  @IsDateString()
  expiresAt?: string;

  @ApiPropertyOptional({ example: ['1', '2'], description: 'Role IDs to assign' })
  @IsOptional()
  @IsArray()
  @IsString({ each: true })
  roleIds?: string[];
}
