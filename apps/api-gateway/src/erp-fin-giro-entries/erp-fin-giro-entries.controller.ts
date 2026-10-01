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
import { ApiBearerAuth, ApiOperation, ApiQuery, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { CreateGiroEntryDto } from './dto/create-giro-entry.dto';
import { QueryGiroEntryDto } from './dto/query-giro-entry.dto';
import { TransitionGiroEntryDto } from './dto/transition-giro-entry.dto';
import { UpdateGiroEntryDto } from './dto/update-giro-entry.dto';
import { ErpFinGiroEntriesService } from './erp-fin-giro-entries.service';

@ApiTags('ERP Fin Giro Entries')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/fin/giro-entries')
export class ErpFinGiroEntriesController {
  constructor(private readonly service: ErpFinGiroEntriesService) {}

  @Post()
  @ApiOperation({ summary: 'Create giro entry (REGISTER instruments or CLEAR links)' })
  create(@Body() dto: CreateGiroEntryDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List giro entries' })
  findAll(@Query() query: QueryGiroEntryDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get giro entry by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update giro entry' })
  update(@Param('id') id: string, @Body() dto: UpdateGiroEntryDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow transition (SUBMIT/APPROVE/REJECT/POST/REOPEN)' })
  transition(@Param('id') id: string, @Body() dto: TransitionGiroEntryDto, @Request() req: any) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete giro entry (soft)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}

@ApiTags('ERP Fin Giro Entries')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/fin/giros')
export class ErpFinGirosLookupController {
  constructor(private readonly service: ErpFinGiroEntriesService) {}

  @Get('outstanding')
  @ApiOperation({ summary: 'List OUTSTANDING giros (CLEAR form row picker)' })
  @ApiQuery({ name: 'type', enum: ['INCOMING', 'OUTGOING'] })
  @ApiQuery({ name: 'search', required: false })
  @ApiQuery({ name: 'partnerId', required: false })
  outstanding(
    @Query('type') type: string,
    @Query('search') search?: string,
    @Query('partnerId') partnerId?: string,
  ) {
    return this.service.findOutstandingGiros(type, search, partnerId);
  }
}
