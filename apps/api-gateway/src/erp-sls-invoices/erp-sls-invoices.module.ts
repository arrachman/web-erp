import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpInvStockMovementsModule } from '../erp-inv-stock-movements/erp-inv-stock-movements.module';
import { ErpOutboundNotificationsModule } from '../erp-outbound-notifications/erp-outbound-notifications.module';
import { SlsInvoicePostingService } from './sls-invoice-posting.service';
import { ErpSlsInvoicesController } from './erp-sls-invoices.controller';
import { ErpSlsInvoicesService } from './erp-sls-invoices.service';

@Module({
  imports: [PrismaModule, ErpInvStockMovementsModule, ErpOutboundNotificationsModule],
  controllers: [ErpSlsInvoicesController],
  providers: [ErpSlsInvoicesService, SlsInvoicePostingService],
  exports: [ErpSlsInvoicesService],
})
export class ErpSlsInvoicesModule {}
