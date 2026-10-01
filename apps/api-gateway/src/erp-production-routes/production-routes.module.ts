import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpProductionRoutesController } from './production-routes.controller';
import { ErpProductionRoutesService } from './production-routes.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpProductionRoutesController],
  providers: [ErpProductionRoutesService],
  exports: [ErpProductionRoutesService],
})
export class ErpProductionRoutesModule {}
