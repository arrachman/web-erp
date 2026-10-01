import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpOtherCostsController } from './other-costs.controller';
import { ErpOtherCostsService } from './other-costs.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpOtherCostsController],
  providers: [ErpOtherCostsService],
  exports: [ErpOtherCostsService],
})
export class ErpOtherCostsModule {}
