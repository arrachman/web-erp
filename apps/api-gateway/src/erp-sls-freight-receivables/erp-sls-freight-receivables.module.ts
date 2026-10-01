import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { SlsFreightReceivablePostingService } from './sls-freight-receivable-posting.service';
import { ErpSlsFreightReceivablesController } from './erp-sls-freight-receivables.controller';
import { ErpSlsFreightReceivablesService } from './erp-sls-freight-receivables.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpSlsFreightReceivablesController],
  providers: [ErpSlsFreightReceivablesService, SlsFreightReceivablePostingService],
  exports: [ErpSlsFreightReceivablesService],
})
export class ErpSlsFreightReceivablesModule {}
