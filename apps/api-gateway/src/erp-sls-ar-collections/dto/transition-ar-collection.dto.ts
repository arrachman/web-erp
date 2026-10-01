import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsEnum, IsOptional, IsString } from 'class-validator';

/** Workflow actions for AR Collection state machine (DRAFT→NEED_APPROVE→APPROVED/REJECTED→POSTED). */
export enum ArCollectionTransitionAction {
  SUBMIT = 'SUBMIT',   // DRAFT|REJECTED -> NEED_APPROVE
  APPROVE = 'APPROVE', // NEED_APPROVE -> APPROVED
  REJECT = 'REJECT',   // NEED_APPROVE -> REJECTED
  POST = 'POST',       // APPROVED -> POSTED
  REOPEN = 'REOPEN',  // APPROVED -> DRAFT
}

export class TransitionArCollectionDto {
  @ApiProperty({ enum: ArCollectionTransitionAction })
  @IsEnum(ArCollectionTransitionAction)
  action!: ArCollectionTransitionAction;

  @ApiPropertyOptional({ description: 'Required when action is REJECT' })
  @IsOptional()
  @IsString()
  reason?: string;
}
