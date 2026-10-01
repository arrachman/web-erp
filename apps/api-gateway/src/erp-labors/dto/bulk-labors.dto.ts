import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsArray, IsBoolean, IsOptional, IsString, ArrayMinSize } from 'class-validator';

export class BulkErpLaborDto {
  @ApiProperty({ type: [String] }) @IsArray() @ArrayMinSize(1) @IsString({ each: true })
  ids!: string[];
}

export class BulkStatusErpLaborDto {
  @ApiProperty({ type: [String] }) @IsArray() @ArrayMinSize(1) @IsString({ each: true })
  ids!: string[];

  @ApiPropertyOptional() @IsBoolean() @IsOptional()
  isActive!: boolean;
}
