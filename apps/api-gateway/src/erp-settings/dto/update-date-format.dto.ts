import { ApiProperty } from '@nestjs/swagger';
import { IsString } from 'class-validator';

export class UpdateDateFormatDto {
  @ApiProperty({
    example: 'DD/MM/YYYY',
    description:
      'Token format tanggal (DD/MM/YYYY, DD-MM-YYYY, MM/DD/YYYY, YYYY-MM-DD, DD MMMM YYYY, D MMM YYYY)',
  })
  @IsString()
  format!: string;
}
