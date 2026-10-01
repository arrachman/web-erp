import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsEnum, IsOptional, IsString } from 'class-validator';

/** Workflow actions over the Senti state machine (§2.7). */
export enum InvOpeningStockTransitionAction {
  SUBMIT = 'SUBMIT', // DRAFT|REJECTED -> NEED_APPROVE
  APPROVE = 'APPROVE', // NEED_APPROVE -> APPROVED
  REJECT = 'REJECT', // NEED_APPROVE -> REJECTED
  POST = 'POST', // APPROVED -> POSTED (counts toward derived stock balance)
  REOPEN = 'REOPEN', // APPROVED|POSTED -> DRAFT
}

export class TransitionInvOpeningStockDto {
  @ApiProperty({ enum: InvOpeningStockTransitionAction })
  @IsEnum(InvOpeningStockTransitionAction)
  action!: InvOpeningStockTransitionAction;

  @ApiPropertyOptional({ description: 'Required for REJECT' })
  @IsOptional()
  @IsString()
  reason?: string;
}
