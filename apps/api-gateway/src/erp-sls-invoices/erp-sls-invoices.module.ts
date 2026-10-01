import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { SlsInvoicePostingService } from './sls-invoice-posting.service';
import { ErpSlsInvoicesController } from './erp-sls-invoices.controller';
import { ErpSlsInvoicesService } from './erp-sls-invoices.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpSlsInvoicesController],
  providers: [ErpSlsInvoicesService, SlsInvoicePostingService],
  exports: [ErpSlsInvoicesService],
})
export class ErpSlsInvoicesModule {}
