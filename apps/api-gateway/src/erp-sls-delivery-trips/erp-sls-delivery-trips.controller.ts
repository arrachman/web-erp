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
  AddTripStopDto,
  ArriveTripStopDto,
  CreateDeliveryTripDto,
  CreateVehicleDto,
  FailTripStopDto,
  QueryDeliveryTripsDto,
  SetDeliveryTripStatusDto,
  SetTripCostsDto,
  UpdateDeliveryTripDto,
  UpdateVehicleDto,
} from './dto/delivery-trip.dto';
import { ErpSlsDeliveryTripsService } from './erp-sls-delivery-trips.service';

@ApiTags('ERP Sls Vehicles')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sls/vehicles')
export class ErpSlsVehiclesController {
  constructor(private readonly service: ErpSlsDeliveryTripsService) {}

  @Get()
  @ApiOperation({ summary: 'Daftar kendaraan' })
  list() {
    return this.service.listVehicles();
  }

  @Post()
  @ApiOperation({ summary: 'Tambah kendaraan' })
  create(@Body() dto: CreateVehicleDto, @Request() req: any) {
    return this.service.createVehicle(dto, req.user?.id);
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Ubah kendaraan (termasuk status)' })
  update(@Param('id') id: string, @Body() dto: UpdateVehicleDto, @Request() req: any) {
    return this.service.updateVehicle(id, dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Hapus kendaraan (soft delete)' })
  remove(@Param('id') id: string) {
    return this.service.deleteVehicle(id);
  }
}

@ApiTags('ERP Sls Delivery Trips')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sls/delivery-trips')
export class ErpSlsDeliveryTripsController {
  constructor(private readonly service: ErpSlsDeliveryTripsService) {}

  @Get()
  @ApiOperation({ summary: 'Daftar trip pengiriman (server-driven)' })
  list(@Query() query: QueryDeliveryTripsDto) {
    return this.service.listTrips(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Detail trip + stops' })
  one(@Param('id') id: string) {
    return this.service.getTrip(id);
  }

  @Post()
  @ApiOperation({ summary: 'Buat trip (nomor TRP otomatis)' })
  create(@Body() dto: CreateDeliveryTripDto, @Request() req: any) {
    return this.service.createTrip(dto, req.user?.id);
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Ubah trip (DRAFT/MUAT)' })
  update(@Param('id') id: string, @Body() dto: UpdateDeliveryTripDto, @Request() req: any) {
    return this.service.updateTrip(id, dto, req.user?.id);
  }

  @Post(':id/costs')
  @ApiOperation({ summary: 'Catat biaya trip (BBM/tol/lain) — dasar Freight Payable' })
  costs(@Param('id') id: string, @Body() dto: SetTripCostsDto) {
    return this.service.setCosts(id, dto);
  }

  @Post(':id/status')
  @ApiOperation({ summary: 'Transisi status trip DRAFT/MUAT/BERANGKAT/SELESAI/BATAL' })
  status(@Param('id') id: string, @Body() dto: SetDeliveryTripStatusDto) {
    return this.service.setTripStatus(id, dto.status);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Hapus trip DRAFT (soft delete)' })
  remove(@Param('id') id: string) {
    return this.service.deleteTrip(id);
  }

  @Post(':id/stops')
  @ApiOperation({ summary: 'Tambah stop (Delivery Order) ke trip' })
  addStop(@Param('id') id: string, @Body() dto: AddTripStopDto) {
    return this.service.addStop(id, dto.deliveryOrderId);
  }

  @Delete(':id/stops/:stopId')
  @ApiOperation({ summary: 'Hapus stop dari trip' })
  removeStop(@Param('id') id: string, @Param('stopId') stopId: string) {
    return this.service.removeStop(id, stopId);
  }

  @Post(':id/stops/:stopId/arrive')
  @ApiOperation({ summary: 'Tandai stop TIBA + penerima — menulis acceptance BAST (hub → DITERIMA)' })
  arrive(@Param('id') id: string, @Param('stopId') stopId: string, @Body() dto: ArriveTripStopDto) {
    return this.service.arriveStop(id, stopId, dto);
  }

  @Post(':id/stops/:stopId/fail')
  @ApiOperation({ summary: 'Tandai stop GAGAL dengan catatan' })
  fail(@Param('id') id: string, @Param('stopId') stopId: string, @Body() dto: FailTripStopDto) {
    return this.service.failStop(id, stopId, dto);
  }
}
