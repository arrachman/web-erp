import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpDivisionsController } from './erp-divisions.controller';
import { ErpDivisionsService } from './erp-divisions.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpDivisionsController],
  providers: [ErpDivisionsService],
  exports: [ErpDivisionsService],
})
export class ErpDivisionsModule {}
