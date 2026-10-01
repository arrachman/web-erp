import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsEnum, IsOptional, IsString } from 'class-validator';

/** Workflow actions for the giro state machine (§2.7). */
export enum GiroTransitionAction {
  SUBMIT = 'SUBMIT',
  APPROVE = 'APPROVE',
  REJECT = 'REJECT',
  POST = 'POST',
  REOPEN = 'REOPEN',
}

export class TransitionGiroEntryDto {
  @ApiProperty({ enum: GiroTransitionAction })
  @IsEnum(GiroTransitionAction)
  action!: GiroTransitionAction;

  @ApiPropertyOptional({ description: 'Wajib untuk REJECT' })
  @IsOptional()
  @IsString()
  reason?: string;
}
