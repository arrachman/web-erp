import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditController } from './erp-audit.controller';
import { ErpAuditService } from './erp-audit.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpAuditController],
  providers: [ErpAuditService],
  exports: [ErpAuditService],
})
export class ErpAuditModule {}
