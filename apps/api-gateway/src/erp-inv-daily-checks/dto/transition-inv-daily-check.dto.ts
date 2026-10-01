import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsEnum, IsOptional, IsString } from 'class-validator';

/** Workflow actions over the Senti state machine (§2.7). */
export enum InvDailyCheckTransitionAction {
  SUBMIT = 'SUBMIT',   // DRAFT|REJECTED -> NEED_APPROVE
  APPROVE = 'APPROVE', // NEED_APPROVE -> APPROVED
  REJECT = 'REJECT',   // NEED_APPROVE -> REJECTED
  POST = 'POST',       // APPROVED -> POSTED
  REOPEN = 'REOPEN',   // APPROVED|POSTED -> DRAFT
}

export class TransitionInvDailyCheckDto {
  @ApiProperty({ enum: InvDailyCheckTransitionAction })
  @IsEnum(InvDailyCheckTransitionAction)
  action!: InvDailyCheckTransitionAction;

  @ApiPropertyOptional({ description: 'Required for REJECT' })
  @IsOptional()
  @IsString()
  reason?: string;
}
