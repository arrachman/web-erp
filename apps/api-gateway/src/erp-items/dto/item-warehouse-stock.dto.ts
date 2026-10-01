import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsNotEmpty, IsOptional, IsString } from 'class-validator';

/** One per-warehouse stock level override (Stok Min/Maks + Min Order per Gudang).
 *  Global defaults live on md_items; a row here overrides them for one warehouse. */
export class ItemWarehouseStockDto {
  @ApiProperty({ example: '1', description: 'Warehouse ID (Gudang, BigInt as string)' })
  @IsString()
  @IsNotEmpty()
  warehouseId!: string;

  @ApiPropertyOptional({ example: '10' })
  @IsOptional()
  @IsString()
  minStock?: string;

  @ApiPropertyOptional({ example: '500' })
  @IsOptional()
  @IsString()
  maxStock?: string;

  @ApiPropertyOptional({ example: '5' })
  @IsOptional()
  @IsString()
  minOrderQty?: string;
}
