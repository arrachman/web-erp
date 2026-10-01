import { Body, Controller, Get, Post, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { RecalcCogsDto } from './dto/recalc-cogs.dto';
import { RepostJournalDto } from './dto/repost-journal.dto';
import { ErpToolsService } from './erp-tools.service';

@ApiTags('ERP Tools')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/tools')
export class ErpToolsController {
  constructor(private readonly service: ErpToolsService) {}

  @Post('recalc-cogs')
  @ApiOperation({ summary: 'Run COGS recalculation' })
  recalcCogs(@Body() dto: RecalcCogsDto, @Request() req: any) {
    return this.service.recalcCogs(dto, req.user?.id);
  }

  @Post('repost-journal')
  @ApiOperation({ summary: 'Repost journals for a period or module' })
  repostJournal(@Body() dto: RepostJournalDto, @Request() req: any) {
    return this.service.repostJournal(dto, req.user?.id);
  }

  @Get('data-validity')
  @ApiOperation({ summary: 'Run data validity checks' })
  checkDataValidity(@Request() req: any) {
    return this.service.checkDataValidity(req.user?.id);
  }
}
