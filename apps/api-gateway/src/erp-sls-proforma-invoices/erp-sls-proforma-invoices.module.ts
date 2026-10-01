import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { SlsProformaInvoicePostingService } from './sls-proforma-invoice-posting.service';
import { ErpSlsProformaInvoicesController } from './erp-sls-proforma-invoices.controller';
import { ErpSlsProformaInvoicesService } from './erp-sls-proforma-invoices.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpSlsProformaInvoicesController],
  providers: [ErpSlsProformaInvoicesService, SlsProformaInvoicePostingService],
  exports: [ErpSlsProformaInvoicesService],
})
export class ErpSlsProformaInvoicesModule {}
