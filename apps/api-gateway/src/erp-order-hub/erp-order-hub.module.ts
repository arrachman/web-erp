import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpSlsOrdersModule } from '../erp-sls-orders/erp-sls-orders.module';
import { ErpOrderHubController } from './erp-order-hub.controller';
import { ErpOrderHubImportService } from './erp-order-hub-import.service';
import { ErpOrderHubService } from './erp-order-hub.service';

@Module({
  imports: [PrismaModule, ErpSlsOrdersModule],
  controllers: [ErpOrderHubController],
  providers: [ErpOrderHubService, ErpOrderHubImportService],
  exports: [ErpOrderHubService],
})
export class ErpOrderHubModule {}
