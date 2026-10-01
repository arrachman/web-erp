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
import { CreateMfgWorkOrderDto } from './dto/create-mfg-work-order.dto';
import { QueryMfgWorkOrdersDto } from './dto/query-mfg-work-orders.dto';
import { TransitionMfgWorkOrderDto } from './dto/transition-mfg-work-order.dto';
import { UpdateMfgWorkOrderDto } from './dto/update-mfg-work-order.dto';
import { ErpMfgWorkOrdersService } from './erp-mfg-work-orders.service';

@ApiTags('ERP Mfg Work Orders')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/mfg/work-orders')
export class ErpMfgWorkOrdersController {
  constructor(private readonly service: ErpMfgWorkOrdersService) {}

  @Post()
  @ApiOperation({ summary: 'Create manufacturing work order (header + input/output lines)' })
  create(@Body() dto: CreateMfgWorkOrderDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List work orders (filter by status/date/bomId/branch)' })
  findAll(@Query() query: QueryMfgWorkOrdersDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get work order by id (with inputs + outputs)' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update work order (only DRAFT/NEED_APPROVE/REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateMfgWorkOrderDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow action: SUBMIT / APPROVE / REJECT / REOPEN' })
  transition(@Param('id') id: string, @Body() dto: TransitionMfgWorkOrderDto, @Request() req: any) {
    return this.service.transition(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete work order (soft delete; APPROVED status blocked)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
