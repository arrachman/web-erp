import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsEnum, IsOptional, IsString } from 'class-validator';

/**
 * Workflow actions for Manufacturing BOM (planning document — no POST step).
 * State machine: DRAFT → SUBMIT → NEED_APPROVE → APPROVE → APPROVED
 *                                               → REJECT  → REJECTED
 *                APPROVED | REJECTED → REOPEN → DRAFT
 */
export enum MfgBomTransitionAction {
  SUBMIT = 'SUBMIT',   // DRAFT | REJECTED → NEED_APPROVE
  APPROVE = 'APPROVE', // NEED_APPROVE → APPROVED
  REJECT = 'REJECT',   // NEED_APPROVE → REJECTED (reason required)
  REOPEN = 'REOPEN',   // APPROVED | REJECTED → DRAFT
}

export class TransitionMfgBomDto {
  @ApiProperty({ enum: MfgBomTransitionAction })
  @IsEnum(MfgBomTransitionAction)
  action!: MfgBomTransitionAction;

  @ApiPropertyOptional({ description: 'Required when action is REJECT' })
  @IsOptional()
  @IsString()
  reason?: string;
}
