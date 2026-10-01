import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpMachinesController } from './machines.controller';
import { ErpMachinesService } from './machines.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpMachinesController],
  providers: [ErpMachinesService],
  exports: [ErpMachinesService],
})
export class ErpMachinesModule {}
