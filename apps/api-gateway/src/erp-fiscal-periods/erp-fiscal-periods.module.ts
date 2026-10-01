import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpFiscalPeriodsController } from './erp-fiscal-periods.controller';
import { ErpFiscalPeriodsService } from './erp-fiscal-periods.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpFiscalPeriodsController],
  providers: [ErpFiscalPeriodsService],
  exports: [ErpFiscalPeriodsService],
})
export class ErpFiscalPeriodsModule {}
