import { ApiProperty } from '@nestjs/swagger';
import { IsArray, IsString } from 'class-validator';

export class AssignMenusDto {
  @ApiProperty({
    example: ['1', '2', '3'],
    description: 'Array of menu IDs as numeric strings (BigInt)',
    type: [String],
  })
  @IsArray()
  @IsString({ each: true })
  menuIds!: string[];
}
