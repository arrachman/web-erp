import { ApiProperty } from '@nestjs/swagger';
import { ArrayMinSize, IsArray, IsBoolean, IsString } from 'class-validator';

export class BulkErpSysMenuDto {
  @ApiProperty({ type: [String], example: ['1', '2'] })
  @IsArray()
  @ArrayMinSize(1)
  @IsString({ each: true })
  ids!: string[];
}

export class BulkStatusErpSysMenuDto {
  @ApiProperty({ type: [String], example: ['1', '2'] })
  @IsArray()
  @ArrayMinSize(1)
  @IsString({ each: true })
  ids!: string[];

  @ApiProperty({ example: true })
  @IsBoolean()
  isActive!: boolean;
}
