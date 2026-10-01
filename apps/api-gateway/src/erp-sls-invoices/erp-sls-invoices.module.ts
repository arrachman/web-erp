import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpInvStockMovementsModule } from '../erp-inv-stock-movements/erp-inv-stock-movements.module';
import { SlsInvoicePostingService } from './sls-invoice-posting.service';
import { ErpSlsInvoicesController } from './erp-sls-invoices.controller';
import { ErpSlsInvoicesService } from './erp-sls-invoices.service';

@Module({
  imports: [PrismaModule, ErpInvStockMovementsModule],
  controllers: [ErpSlsInvoicesController],
  providers: [ErpSlsInvoicesService, SlsInvoicePostingService],
  exports: [ErpSlsInvoicesService],
})
export class ErpSlsInvoicesModule {}
