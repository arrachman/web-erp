import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import {
  ErpSlsDeliveryTripsController,
  ErpSlsVehiclesController,
} from './erp-sls-delivery-trips.controller';
import { ErpSlsDeliveryTripsService } from './erp-sls-delivery-trips.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpSlsVehiclesController, ErpSlsDeliveryTripsController],
  providers: [ErpSlsDeliveryTripsService],
  exports: [ErpSlsDeliveryTripsService],
})
export class ErpSlsDeliveryTripsModule {}
