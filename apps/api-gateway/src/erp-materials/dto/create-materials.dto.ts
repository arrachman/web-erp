import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsIn, IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpMaterialDto {
  @ApiProperty({ example: 'MAT-001' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Material Sample' })
  @IsString()
  @MaxLength(150)
  name!: string;



  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}
