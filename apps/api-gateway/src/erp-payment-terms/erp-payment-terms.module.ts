import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpPaymentTermsController } from './erp-payment-terms.controller';
import { ErpPaymentTermsService } from './erp-payment-terms.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpPaymentTermsController],
  providers: [ErpPaymentTermsService],
  exports: [ErpPaymentTermsService],
})
export class ErpPaymentTermsModule {}
