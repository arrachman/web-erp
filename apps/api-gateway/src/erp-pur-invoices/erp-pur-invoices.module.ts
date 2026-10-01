import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { PurInvoicePostingService } from './pur-invoice-posting.service';
import { ErpPurInvoicesController } from './erp-pur-invoices.controller';
import { ErpPurInvoicesService } from './erp-pur-invoices.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpPurInvoicesController],
  providers: [ErpPurInvoicesService, PurInvoicePostingService],
  exports: [ErpPurInvoicesService],
})
export class ErpPurInvoicesModule {}
