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
import { CreateJournalEntryDto } from './dto/create-journal-entry.dto';
import { QueryJournalEntryDto } from './dto/query-journal-entry.dto';
import { TransitionJournalEntryDto } from './dto/transition-journal-entry.dto';
import { UpdateJournalEntryDto } from './dto/update-journal-entry.dto';
import { ErpFinJournalEntriesService } from './erp-fin-journal-entries.service';

@ApiTags('ERP Fin Journal Entries')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/fin/journal-entries')
export class ErpFinJournalEntriesController {
  constructor(private readonly service: ErpFinJournalEntriesService) {}

  @Post()
  @ApiOperation({ summary: 'Create journal entry with lines' })
  create(@Body() dto: CreateJournalEntryDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List journal entries' })
  findAll(@Query() query: QueryJournalEntryDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get journal entry by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update journal entry' })
  update(
    @Param('id') id: string,
    @Body() dto: UpdateJournalEntryDto,
    @Request() req: any,
  ) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow transition (SUBMIT/APPROVE/REJECT/POST/REOPEN)' })
  transition(
    @Param('id') id: string,
    @Body() dto: TransitionJournalEntryDto,
    @Request() req: any,
  ) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete journal entry (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
