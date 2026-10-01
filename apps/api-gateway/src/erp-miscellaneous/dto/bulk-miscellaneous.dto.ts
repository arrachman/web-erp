import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsArray, IsBoolean, IsOptional, IsString, ArrayMinSize } from 'class-validator';

export class BulkErpMiscellaneousDto {
  @ApiProperty({ type: [String] }) @IsArray() @ArrayMinSize(1) @IsString({ each: true })
  ids!: string[];
}

export class BulkStatusErpMiscellaneousDto {
  @ApiProperty({ type: [String] }) @IsArray() @ArrayMinSize(1) @IsString({ each: true })
  ids!: string[];

  @ApiPropertyOptional() @IsBoolean() @IsOptional()
  isActive!: boolean;
}
