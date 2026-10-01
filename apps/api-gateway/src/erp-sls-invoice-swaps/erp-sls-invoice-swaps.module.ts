import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { SlsInvoiceSwapPostingService } from './sls-invoice-swap-posting.service';
import { ErpSlsInvoiceSwapsController } from './erp-sls-invoice-swaps.controller';
import { ErpSlsInvoiceSwapsService } from './erp-sls-invoice-swaps.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpSlsInvoiceSwapsController],
  providers: [ErpSlsInvoiceSwapsService, SlsInvoiceSwapPostingService],
  exports: [ErpSlsInvoiceSwapsService],
})
export class ErpSlsInvoiceSwapsModule {}
