import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpProjectsController } from './erp-projects.controller';
import { ErpProjectsService } from './erp-projects.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpProjectsController],
  providers: [ErpProjectsService],
  exports: [ErpProjectsService],
})
export class ErpProjectsModule {}
