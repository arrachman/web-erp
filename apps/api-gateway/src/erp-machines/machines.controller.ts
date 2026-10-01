import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkErpMachineDto, BulkStatusErpMachineDto } from './dto/bulk-machines.dto';
import { CreateErpMachineDto } from './dto/create-machines.dto';
import { QueryErpMachineDto } from './dto/query-machines.dto';
import { UpdateErpMachineDto } from './dto/update-machines.dto';
import { ErpMachinesService } from './machines.service';

@ApiTags('ERP Machines')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/machines')
export class ErpMachinesController {
  constructor(private readonly service: ErpMachinesService) {}

  @Post()
  @ApiOperation({ summary: 'Create Machine' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: CreateErpMachineDto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List Machine' })
  findAll(@Query() query: QueryErpMachineDto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get Machine' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatusErpMachineDto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update Machine' })
  update(@Param('id') id: string, @Body() dto: UpdateErpMachineDto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: BulkErpMachineDto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete Machine' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
