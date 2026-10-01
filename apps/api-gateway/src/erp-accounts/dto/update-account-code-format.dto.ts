import { ApiProperty } from '@nestjs/swagger';
import {
  ArrayMaxSize,
  ArrayMinSize,
  IsArray,
  IsIn,
  IsInt,
  IsString,
  Max,
  Min,
} from 'class-validator';

export class UpdateAccountCodeFormatDto {
  @ApiProperty({
    example: [4, 2, 3],
    description: 'Array of segment digit-lengths. 1–5 segments, each 1–12 digits.',
    type: [Number],
  })
  @IsArray()
  @ArrayMinSize(1)
  @ArrayMaxSize(5)
  @IsInt({ each: true })
  @Min(1, { each: true })
  @Max(12, { each: true })
  segments!: number[];

  @ApiProperty({
    example: '.',
    description: 'Separator antar segmen. Pilihan: "" (kosong = tanpa grouping), ".", "-", "/".',
    enum: ['', '.', '-', '/'],
  })
  @IsString()
  @IsIn(['', '.', '-', '/'])
  separator!: string;
}
