import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsArray, IsBoolean, IsOptional, IsString, ArrayMinSize } from 'class-validator';

export class BulkErpProductClassDto {
  @ApiProperty({ type: [String] }) @IsArray() @ArrayMinSize(1) @IsString({ each: true })
  ids!: string[];
}

export class BulkStatusErpProductClassDto {
  @ApiProperty({ type: [String] }) @IsArray() @ArrayMinSize(1) @IsString({ each: true })
  ids!: string[];

  @ApiPropertyOptional() @IsBoolean() @IsOptional()
  isActive!: boolean;
}
