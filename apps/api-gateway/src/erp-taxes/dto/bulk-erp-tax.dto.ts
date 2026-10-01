import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsArray, IsBoolean, IsOptional, IsString, ArrayMinSize } from 'class-validator';

export class BulkErpTaxDto {
  @ApiProperty({ type: [String], example: ['1', '2'] })
  @IsArray()
  @ArrayMinSize(1)
  @IsString({ each: true })
  ids!: string[];
}

export class BulkStatusErpTaxDto {
  @ApiProperty({ type: [String], example: ['1', '2'] })
  @IsArray()
  @ArrayMinSize(1)
  @IsString({ each: true })
  ids!: string[];

  @ApiPropertyOptional({ example: true })
  @IsBoolean()
  @IsOptional()
  isActive!: boolean;
}
