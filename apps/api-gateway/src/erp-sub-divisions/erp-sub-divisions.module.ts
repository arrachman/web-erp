import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpSubDivisionsController } from './erp-sub-divisions.controller';
import { ErpSubDivisionsService } from './erp-sub-divisions.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpSubDivisionsController],
  providers: [ErpSubDivisionsService],
  exports: [ErpSubDivisionsService],
})
export class ErpSubDivisionsModule {}
