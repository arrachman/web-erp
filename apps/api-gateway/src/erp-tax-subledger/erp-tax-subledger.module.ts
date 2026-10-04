import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpTaxSubledgerController } from './erp-tax-subledger.controller';
import { ErpTaxSubledgerService } from './erp-tax-subledger.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpTaxSubledgerController],
  providers: [ErpTaxSubledgerService],
  exports: [ErpTaxSubledgerService],
})
export class ErpTaxSubledgerModule {}
