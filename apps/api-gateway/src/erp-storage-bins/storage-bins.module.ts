import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpStorageBinsController } from './storage-bins.controller';
import { ErpStorageBinsService } from './storage-bins.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpStorageBinsController],
  providers: [ErpStorageBinsService],
  exports: [ErpStorageBinsService],
})
export class ErpStorageBinsModule {}
