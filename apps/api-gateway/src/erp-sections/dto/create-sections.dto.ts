import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsIn, IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpSectionDto {
  @ApiProperty({ example: 'SEC-001' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Section Sample' })
  @IsString()
  @MaxLength(150)
  name!: string;



  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}
