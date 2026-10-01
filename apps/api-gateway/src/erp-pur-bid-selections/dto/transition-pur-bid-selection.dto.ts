import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsEnum, IsOptional, IsString } from 'class-validator';

export enum PurBidSelectionTransitionAction {
  SUBMIT = 'SUBMIT', APPROVE = 'APPROVE', REJECT = 'REJECT', POST = 'POST', REOPEN = 'REOPEN',
}

export class TransitionPurBidSelectionDto {
  @ApiProperty({ enum: PurBidSelectionTransitionAction }) @IsEnum(PurBidSelectionTransitionAction) action!: PurBidSelectionTransitionAction;
  @ApiPropertyOptional() @IsOptional() @IsString() reason?: string;
}
