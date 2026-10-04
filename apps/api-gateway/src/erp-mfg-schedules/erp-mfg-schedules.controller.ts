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
import {
  CreateJobScheduleDto,
  CreateMachineDto,
  QueryJobSchedulesDto,
  SetJobScheduleStatusDto,
  UpdateJobScheduleDto,
  UpdateMachineDto,
} from './dto/schedule.dto';
import { ErpMfgSchedulesService } from './erp-mfg-schedules.service';

@ApiTags('ERP Mfg Machines')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/mfg/machines')
export class ErpMfgMachinesController {
  constructor(private readonly service: ErpMfgSchedulesService) {}

  @Get()
  @ApiOperation({ summary: 'Daftar mesin cetak' })
  list() {
    return this.service.listMachines();
  }

  @Post()
  @ApiOperation({ summary: 'Tambah mesin' })
  create(@Body() dto: CreateMachineDto, @Request() req: any) {
    return this.service.createMachine(dto, req.user?.id);
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Ubah mesin (termasuk status)' })
  update(@Param('id') id: string, @Body() dto: UpdateMachineDto, @Request() req: any) {
    return this.service.updateMachine(id, dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Hapus mesin (soft delete)' })
  remove(@Param('id') id: string) {
    return this.service.deleteMachine(id);
  }
}

@ApiTags('ERP Mfg Job Schedules')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/mfg/job-schedules')
export class ErpMfgJobSchedulesController {
  constructor(private readonly service: ErpMfgSchedulesService) {}

  @Get()
  @ApiOperation({ summary: 'Papan jadwal: filter rentang tanggal / mesin / job' })
  list(@Query() query: QueryJobSchedulesDto) {
    return this.service.listSchedules(query);
  }

  @Post()
  @ApiOperation({ summary: 'Jadwalkan job ke mesin (bentrok ditolak)' })
  create(@Body() dto: CreateJobScheduleDto, @Request() req: any) {
    return this.service.createSchedule(dto, req.user?.id);
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Geser jadwal (cek bentrok ulang)' })
  update(@Param('id') id: string, @Body() dto: UpdateJobScheduleDto, @Request() req: any) {
    return this.service.updateSchedule(id, dto, req.user?.id);
  }

  @Post(':id/status')
  @ApiOperation({ summary: 'Transisi status jadwal TERJADWAL/BERJALAN/SELESAI/BATAL' })
  status(@Param('id') id: string, @Body() dto: SetJobScheduleStatusDto) {
    return this.service.setStatus(id, dto.status);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Hapus jadwal (soft delete)' })
  remove(@Param('id') id: string) {
    return this.service.deleteSchedule(id);
  }
}
