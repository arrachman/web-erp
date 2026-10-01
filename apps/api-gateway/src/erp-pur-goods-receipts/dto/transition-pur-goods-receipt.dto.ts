import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsEnum, IsOptional, IsString } from 'class-validator';

export enum PurGoodsReceiptTransitionAction {
  SUBMIT = 'SUBMIT', APPROVE = 'APPROVE', REJECT = 'REJECT', POST = 'POST', REOPEN = 'REOPEN',
}

export class TransitionPurGoodsReceiptDto {
  @ApiProperty({ enum: PurGoodsReceiptTransitionAction })
  @IsEnum(PurGoodsReceiptTransitionAction) action!: PurGoodsReceiptTransitionAction;

  @ApiPropertyOptional({ description: 'Required for REJECT' })
  @IsOptional() @IsString() reason?: string;
}
