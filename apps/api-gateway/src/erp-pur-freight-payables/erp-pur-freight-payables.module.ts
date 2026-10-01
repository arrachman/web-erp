import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { PurFreightPayablePostingService } from './pur-freight-payable-posting.service';
import { ErpPurFreightPayablesController } from './erp-pur-freight-payables.controller';
import { ErpPurFreightPayablesService } from './erp-pur-freight-payables.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpPurFreightPayablesController],
  providers: [ErpPurFreightPayablesService, PurFreightPayablePostingService],
  exports: [ErpPurFreightPayablesService],
})
export class ErpPurFreightPayablesModule {}
