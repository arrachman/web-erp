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
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { CreateErpFiscalPeriodDto } from './dto/create-erp-fiscal-period.dto';
import { QueryErpFiscalPeriodDto } from './dto/query-erp-fiscal-period.dto';
import { UpdateErpFiscalPeriodDto } from './dto/update-erp-fiscal-period.dto';
import { ErpFiscalPeriodsService } from './erp-fiscal-periods.service';

@ApiTags('ERP Fiscal Periods')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/fiscal-periods')
export class ErpFiscalPeriodsController {
  constructor(private readonly service: ErpFiscalPeriodsService) {}

  @Post()
  @ApiOperation({ summary: 'Create fiscal period' })
  @ApiResponse({ status: 201, description: 'Fiscal period created' })
  create(@Body() dto: CreateErpFiscalPeriodDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List fiscal periods' })
  @ApiResponse({ status: 200, description: 'List of fiscal periods' })
  findAll(@Query() query: QueryErpFiscalPeriodDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one fiscal period' })
  @ApiResponse({ status: 200, description: 'Fiscal period detail' })
  @ApiResponse({ status: 404, description: 'Fiscal period not found' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update fiscal period' })
  @ApiResponse({ status: 200, description: 'Fiscal period updated' })
  update(@Param('id') id: string, @Body() dto: UpdateErpFiscalPeriodDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete fiscal period' })
  @ApiResponse({ status: 200, description: 'Fiscal period deleted' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }

  @Patch(':id/open')
  @ApiOperation({ summary: 'Reopen a closed fiscal period' })
  @ApiResponse({ status: 200, description: 'Fiscal period reopened' })
  openPeriod(@Param('id') id: string, @Request() req: any) {
    return this.service.openPeriod(BigInt(id), req.user?.id);
  }

  @Patch(':id/close')
  @ApiOperation({ summary: 'Close a fiscal period' })
  @ApiResponse({ status: 200, description: 'Fiscal period closed' })
  closePeriod(@Param('id') id: string, @Request() req: any) {
    return this.service.closePeriod(BigInt(id), req.user?.id);
  }
}
