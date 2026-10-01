import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpLaborsController } from './labors.controller';
import { ErpLaborsService } from './labors.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpLaborsController],
  providers: [ErpLaborsService],
  exports: [ErpLaborsService],
})
export class ErpLaborsModule {}
