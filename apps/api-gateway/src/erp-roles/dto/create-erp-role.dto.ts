import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpRoleDto {
  @ApiProperty({ example: 'ADMIN', description: 'Unique role code' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Administrator' })
  @IsString()
  @MaxLength(150)
  name!: string;

  @ApiPropertyOptional({ example: 'Full access administrator role' })
  @IsOptional()
  @IsString()
  @MaxLength(500)
  description?: string;
}
