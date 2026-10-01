import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpSubDepartmentDto {
  @ApiProperty({ example: 'SUB-DEPT-A' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Sub Department A' })
  @IsString()
  @MaxLength(150)
  name!: string;

  @ApiProperty({ description: 'Parent Department id (required)' })
  @IsString()
  departmentId!: string;

  @ApiPropertyOptional({ description: 'Parent sub-division id (for nesting)' })
  @IsOptional()
  @IsString()
  parentId?: string;

  @ApiPropertyOptional({ default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}
