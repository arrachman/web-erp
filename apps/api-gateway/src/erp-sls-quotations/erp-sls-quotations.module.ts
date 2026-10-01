import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { SlsQuotationPostingService } from './sls-quotation-posting.service';
import { ErpSlsQuotationsController } from './erp-sls-quotations.controller';
import { ErpSlsQuotationsService } from './erp-sls-quotations.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpSlsQuotationsController],
  providers: [ErpSlsQuotationsService, SlsQuotationPostingService],
  exports: [ErpSlsQuotationsService],
})
export class ErpSlsQuotationsModule {}
