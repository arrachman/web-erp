import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpPurPaymentSchedulesController } from './erp-pur-payment-schedules.controller';
import { ErpPurPaymentSchedulesService } from './erp-pur-payment-schedules.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpPurPaymentSchedulesController],
  providers: [ErpPurPaymentSchedulesService],
  exports: [ErpPurPaymentSchedulesService],
})
export class ErpPurPaymentSchedulesModule {}
