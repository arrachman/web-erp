import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsIn, IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpAreaDto {
  @ApiProperty({ example: 'AREA-001' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Area Sample' })
  @IsString()
  @MaxLength(150)
  name!: string;

  @ApiProperty({ example: "1", description: "City id (required)" })
  @IsString()
  cityId!: string;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}
