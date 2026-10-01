import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpLocationsController } from './erp-locations.controller';
import { ErpLocationsService } from './erp-locations.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpLocationsController],
  providers: [ErpLocationsService],
  exports: [ErpLocationsService],
})
export class ErpLocationsModule {}
