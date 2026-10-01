import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsEnum, IsOptional, IsString } from 'class-validator';

/** Workflow actions for the journal state machine (§2.7). */
export enum JournalTransitionAction {
  SUBMIT = 'SUBMIT',
  APPROVE = 'APPROVE',
  REJECT = 'REJECT',
  POST = 'POST',
  REOPEN = 'REOPEN',
}

export class TransitionJournalEntryDto {
  @ApiProperty({ enum: JournalTransitionAction })
  @IsEnum(JournalTransitionAction)
  action!: JournalTransitionAction;

  @ApiPropertyOptional({ description: 'Wajib untuk REJECT' })
  @IsOptional()
  @IsString()
  reason?: string;
}
