import { ApiProperty } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  ArrayMinSize,
  IsArray,
  IsInt,
  IsOptional,
  IsString,
  ValidateNested,
} from 'class-validator';

export class ReorderErpSysMenuItemDto {
  @ApiProperty({ example: '12' })
  @IsString()
  id!: string;

  @ApiProperty({ example: '5', nullable: true, required: false })
  @IsOptional()
  @IsString()
  parentId?: string | null;

  @ApiProperty({ example: 0 })
  @IsInt()
  sortOrder!: number;
}

export class ReorderErpSysMenuDto {
  @ApiProperty({ type: [ReorderErpSysMenuItemDto] })
  @IsArray()
  @ArrayMinSize(1)
  @ValidateNested({ each: true })
  @Type(() => ReorderErpSysMenuItemDto)
  items!: ReorderErpSysMenuItemDto[];
}
