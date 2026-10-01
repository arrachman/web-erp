import {
  Body,
  Controller,
  Delete,
  Get,
  Param,
  Patch,
  Post,
  Query,
  Request,
  UseGuards,
} from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { CreateInvDailyCheckDto } from './dto/create-inv-daily-check.dto';
import { QueryInvDailyChecksDto } from './dto/query-inv-daily-checks.dto';
import { TransitionInvDailyCheckDto } from './dto/transition-inv-daily-check.dto';
import { UpdateInvDailyCheckDto } from './dto/update-inv-daily-check.dto';
import { ErpInvDailyChecksService } from './erp-inv-daily-checks.service';

@ApiTags('ERP Inv Daily Checks')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/inv/daily-checks')
export class ErpInvDailyChecksController {
  constructor(private readonly service: ErpInvDailyChecksService) {}

  @Post()
  @ApiOperation({ summary: 'Create daily check / time sheet (header + item lines) — DC' })
  create(@Body() dto: CreateInvDailyCheckDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List daily checks (filter by status/date/branch)' })
  findAll(@Query() query: QueryInvDailyChecksDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get daily check by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update daily check (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateInvDailyCheckDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN' })
  transition(
    @Param('id') id: string,
    @Body() dto: TransitionInvDailyCheckDto,
    @Request() req: any,
  ) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete daily check (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
