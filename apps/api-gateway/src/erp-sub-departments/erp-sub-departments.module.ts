import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpSubDepartmentsController } from './erp-sub-departments.controller';
import { ErpSubDepartmentsService } from './erp-sub-departments.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpSubDepartmentsController],
  providers: [ErpSubDepartmentsService],
  exports: [ErpSubDepartmentsService],
})
export class ErpSubDepartmentsModule {}
