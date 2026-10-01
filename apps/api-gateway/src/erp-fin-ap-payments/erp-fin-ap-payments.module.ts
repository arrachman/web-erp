import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpFinApPaymentsController } from './erp-fin-ap-payments.controller';
import { ErpFinApPaymentsService } from './erp-fin-ap-payments.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpFinApPaymentsController],
  providers: [ErpFinApPaymentsService],
  exports: [ErpFinApPaymentsService],
})
export class ErpFinApPaymentsModule {}
