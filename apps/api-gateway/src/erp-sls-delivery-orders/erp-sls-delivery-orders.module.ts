import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpInvStockMovementsModule } from '../erp-inv-stock-movements/erp-inv-stock-movements.module';
import { SlsDeliveryOrderPostingService } from './sls-delivery-order-posting.service';
import { ErpSlsDeliveryOrdersController } from './erp-sls-delivery-orders.controller';
import { ErpSlsDeliveryOrdersService } from './erp-sls-delivery-orders.service';

@Module({
  imports: [PrismaModule, ErpInvStockMovementsModule],
  controllers: [ErpSlsDeliveryOrdersController],
  providers: [ErpSlsDeliveryOrdersService, SlsDeliveryOrderPostingService],
  exports: [ErpSlsDeliveryOrdersService],
})
export class ErpSlsDeliveryOrdersModule {}
