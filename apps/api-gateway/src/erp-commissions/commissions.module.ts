import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpCommissionsController } from './commissions.controller';
import { ErpCommissionsService } from './commissions.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpCommissionsController],
  providers: [ErpCommissionsService],
  exports: [ErpCommissionsService],
})
export class ErpCommissionsModule {}
