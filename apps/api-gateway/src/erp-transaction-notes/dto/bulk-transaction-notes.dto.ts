import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsArray, IsBoolean, IsOptional, IsString, ArrayMinSize } from 'class-validator';

export class BulkErpTransactionNoteDto {
  @ApiProperty({ type: [String] }) @IsArray() @ArrayMinSize(1) @IsString({ each: true })
  ids!: string[];
}

export class BulkStatusErpTransactionNoteDto {
  @ApiProperty({ type: [String] }) @IsArray() @ArrayMinSize(1) @IsString({ each: true })
  ids!: string[];

  @ApiPropertyOptional() @IsBoolean() @IsOptional()
  isActive!: boolean;
}
