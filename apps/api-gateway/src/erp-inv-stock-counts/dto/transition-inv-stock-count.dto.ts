import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsEnum, IsOptional, IsString } from 'class-validator';

/** Workflow actions over the Senti state machine (§2.7). */
export enum InvStockCountTransitionAction {
  SUBMIT = 'SUBMIT', // DRAFT|REJECTED -> NEED_APPROVE
  APPROVE = 'APPROVE', // NEED_APPROVE -> APPROVED
  REJECT = 'REJECT', // NEED_APPROVE -> REJECTED
  POST = 'POST', // APPROVED -> POSTED (count finalized; adjustments produced separately)
  REOPEN = 'REOPEN', // APPROVED|POSTED -> DRAFT
}

export class TransitionInvStockCountDto {
  @ApiProperty({ enum: InvStockCountTransitionAction })
  @IsEnum(InvStockCountTransitionAction)
  action!: InvStockCountTransitionAction;

  @ApiPropertyOptional({ description: 'Required for REJECT' })
  @IsOptional()
  @IsString()
  reason?: string;
}
