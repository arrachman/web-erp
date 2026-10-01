import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { SlsCustomerAdvancePostingService } from './sls-customer-advance-posting.service';
import { ErpSlsCustomerAdvancesController } from './erp-sls-customer-advances.controller';
import { ErpSlsCustomerAdvancesService } from './erp-sls-customer-advances.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpSlsCustomerAdvancesController],
  providers: [ErpSlsCustomerAdvancesService, SlsCustomerAdvancePostingService],
  exports: [ErpSlsCustomerAdvancesService],
})
export class ErpSlsCustomerAdvancesModule {}
