import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpWorkEstimatesController } from './work-estimates.controller';
import { ErpWorkEstimatesService } from './work-estimates.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpWorkEstimatesController],
  providers: [ErpWorkEstimatesService],
  exports: [ErpWorkEstimatesService],
})
export class ErpWorkEstimatesModule {}
