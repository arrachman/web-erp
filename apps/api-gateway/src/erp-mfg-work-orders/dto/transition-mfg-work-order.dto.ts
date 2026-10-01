import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsEnum, IsOptional, IsString } from 'class-validator';

/**
 * Workflow actions for Manufacturing Work Order state machine.
 * WO is a planning doc — no POST/GL action.
 *
 * DRAFT → SUBMIT → NEED_APPROVE → APPROVE → APPROVED
 *                              → REJECT  → REJECTED
 * APPROVED/REJECTED → REOPEN → DRAFT
 */
export enum MfgWoTransitionAction {
  SUBMIT = 'SUBMIT',   // DRAFT | REJECTED -> NEED_APPROVE
  APPROVE = 'APPROVE', // NEED_APPROVE -> APPROVED
  REJECT = 'REJECT',   // NEED_APPROVE -> REJECTED
  REOPEN = 'REOPEN',   // APPROVED | REJECTED -> DRAFT
}

export class TransitionMfgWorkOrderDto {
  @ApiProperty({ enum: MfgWoTransitionAction })
  @IsEnum(MfgWoTransitionAction)
  action!: MfgWoTransitionAction;

  @ApiPropertyOptional({ description: 'Required for REJECT' })
  @IsOptional()
  @IsString()
  reason?: string;
}
