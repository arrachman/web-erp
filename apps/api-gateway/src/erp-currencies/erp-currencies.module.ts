import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpCurrenciesController } from './erp-currencies.controller';
import { ErpCurrenciesService } from './erp-currencies.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpCurrenciesController],
  providers: [ErpCurrenciesService],
  exports: [ErpCurrenciesService],
})
export class ErpCurrenciesModule {}
