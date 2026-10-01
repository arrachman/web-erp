import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { SlsDeliveryOrderPostingService } from './sls-delivery-order-posting.service';
import { ErpSlsDeliveryOrdersController } from './erp-sls-delivery-orders.controller';
import { ErpSlsDeliveryOrdersService } from './erp-sls-delivery-orders.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpSlsDeliveryOrdersController],
  providers: [ErpSlsDeliveryOrdersService, SlsDeliveryOrderPostingService],
  exports: [ErpSlsDeliveryOrdersService],
})
export class ErpSlsDeliveryOrdersModule {}
