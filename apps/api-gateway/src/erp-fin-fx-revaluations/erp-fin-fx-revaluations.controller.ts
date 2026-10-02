import { Body, Controller, Get, Post, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { IsNotEmpty, IsOptional, IsString } from 'class-validator';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { ErpFinFxRevaluationsService } from './erp-fin-fx-revaluations.service';

export class RunFxRevaluationDto {
  @IsString() @IsNotEmpty() fiscalPeriodId!: string;
  @IsString() @IsNotEmpty() branchId!: string;
  @IsOptional() @IsString() notes?: string;
}

@ApiTags('ERP Fin FX Revaluations')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/fin/fx-revaluations')
export class ErpFinFxRevaluationsController {
  constructor(private readonly service: ErpFinFxRevaluationsService) {}

  @Post('run')
  @ApiOperation({ summary: 'Jalankan revaluasi valas akhir periode (auto-reverse hari pertama periode berikutnya)' })
  run(@Body() dto: RunFxRevaluationDto, @Request() req: any) {
    return this.service.run(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'Daftar run revaluasi valas' })
  list() {
    return this.service.list();
  }
}
