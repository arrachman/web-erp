import { ApiProperty } from '@nestjs/swagger';
import { IsInt, IsString, Max, Min } from 'class-validator';

export class UpdateNumberFormatDto {
  @ApiProperty({ example: '.', description: 'Pemisah ribuan ("", ".", ",", " ", "\'")' })
  @IsString()
  thousandsSep!: string;

  @ApiProperty({ example: ',', description: 'Pemisah desimal (".", ",")' })
  @IsString()
  decimalSep!: string;

  @ApiProperty({ example: 0, description: 'Jumlah digit desimal default (0-6)' })
  @IsInt()
  @Min(0)
  @Max(6)
  decimals!: number;
}
