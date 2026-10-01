import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ApPaymentPostingService } from './ap-payment-posting.service';
import { ErpFinApPaymentsController } from './erp-fin-ap-payments.controller';
import { ErpFinApPaymentsService } from './erp-fin-ap-payments.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpFinApPaymentsController],
  providers: [ErpFinApPaymentsService, ApPaymentPostingService],
  exports: [ErpFinApPaymentsService],
})
export class ErpFinApPaymentsModule {}
