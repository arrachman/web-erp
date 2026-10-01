import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpTaxesController } from './erp-taxes.controller';
import { ErpTaxesService } from './erp-taxes.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpTaxesController],
  providers: [ErpTaxesService],
  exports: [ErpTaxesService],
})
export class ErpTaxesModule {}
