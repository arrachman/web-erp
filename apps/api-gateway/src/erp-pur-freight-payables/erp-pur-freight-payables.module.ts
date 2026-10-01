import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpPurFreightPayablesController } from './erp-pur-freight-payables.controller';
import { ErpPurFreightPayablesService } from './erp-pur-freight-payables.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpPurFreightPayablesController],
  providers: [ErpPurFreightPayablesService],
  exports: [ErpPurFreightPayablesService],
})
export class ErpPurFreightPayablesModule {}
