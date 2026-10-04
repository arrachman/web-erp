import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsArray, IsBoolean, IsOptional, IsString } from 'class-validator';

export class BulkSchoolIdsDto {
  @ApiProperty({ type: [String], example: ['1', '2'] })
  @IsArray()
  @IsString({ each: true })
  ids!: string[];
}

export class BulkSchoolStatusDto extends BulkSchoolIdsDto {
  @ApiPropertyOptional({ example: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}
