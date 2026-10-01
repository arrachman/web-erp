import { ApiProperty } from '@nestjs/swagger';
import { IsNotEmpty, IsString } from 'class-validator';

/** One item distributor (legacy "Distributor" tab: a supplier/distributor partner). */
export class ItemDistributorDto {
  @ApiProperty({ example: '1', description: 'Partner ID (md_partners, supplier/distributor, BigInt as string)' })
  @IsString()
  @IsNotEmpty()
  partnerId!: string;
}
