import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpNozzlesController } from './nozzles.controller';
import { ErpNozzlesService } from './nozzles.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpNozzlesController],
  providers: [ErpNozzlesService],
  exports: [ErpNozzlesService],
})
export class ErpNozzlesModule {}
