import { ApiProperty } from '@nestjs/swagger';
import { IsArray, IsString } from 'class-validator';

export class AssignPermissionsDto {
  @ApiProperty({
    example: ['1', '2', '3'],
    description: 'Array of permission IDs as numeric strings (BigInt)',
    type: [String],
  })
  @IsArray()
  @IsString({ each: true })
  permissionIds!: string[];
}
