import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpWarehousesController } from './erp-warehouses.controller';
import { ErpWarehousesService } from './erp-warehouses.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpWarehousesController],
  providers: [ErpWarehousesService],
  exports: [ErpWarehousesService],
})
export class ErpWarehousesModule {}
