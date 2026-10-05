import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpInvLotsModule } from '../erp-inv-lots/erp-inv-lots.module';
import { ErpOutboundNotificationsModule } from '../erp-outbound-notifications/erp-outbound-notifications.module';
import { ErpInvStockMovementsModule } from '../erp-inv-stock-movements/erp-inv-stock-movements.module';
import { SlsDeliveryOrderPostingService } from './sls-delivery-order-posting.service';
import { ErpSlsDeliveryOrdersController } from './erp-sls-delivery-orders.controller';
import { ErpSlsDeliveryOrdersService } from './erp-sls-delivery-orders.service';

@Module({
  imports: [PrismaModule, ErpInvStockMovementsModule, ErpInvLotsModule, ErpOutboundNotificationsModule],
  controllers: [ErpSlsDeliveryOrdersController],
  providers: [ErpSlsDeliveryOrdersService, SlsDeliveryOrderPostingService],
  exports: [ErpSlsDeliveryOrdersService],
})
export class ErpSlsDeliveryOrdersModule {}
