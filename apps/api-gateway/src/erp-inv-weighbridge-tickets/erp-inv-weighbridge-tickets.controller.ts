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
import { CreateInvWeighbridgeTicketDto } from './dto/create-inv-weighbridge-ticket.dto';
import { QueryInvWeighbridgeTicketsDto } from './dto/query-inv-weighbridge-tickets.dto';
import { TransitionInvWeighbridgeTicketDto } from './dto/transition-inv-weighbridge-ticket.dto';
import { UpdateInvWeighbridgeTicketDto } from './dto/update-inv-weighbridge-ticket.dto';
import { ErpInvWeighbridgeTicketsService } from './erp-inv-weighbridge-tickets.service';

@ApiTags('ERP Inv Weighbridge Tickets')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/inv/weighbridge-tickets')
export class ErpInvWeighbridgeTicketsController {
  constructor(private readonly service: ErpInvWeighbridgeTicketsService) {}

  @Post()
  @ApiOperation({ summary: 'Create weighbridge ticket (RW) — header-only document' })
  create(@Body() dto: CreateInvWeighbridgeTicketDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List weighbridge tickets (filter by status/branch/date/partner)' })
  findAll(@Query() query: QueryInvWeighbridgeTicketsDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get weighbridge ticket by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update weighbridge ticket (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateInvWeighbridgeTicketDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT/APPROVE/REJECT/POST/REOPEN' })
  transition(
    @Param('id') id: string,
    @Body() dto: TransitionInvWeighbridgeTicketDto,
    @Request() req: any,
  ) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete weighbridge ticket (soft-delete)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
