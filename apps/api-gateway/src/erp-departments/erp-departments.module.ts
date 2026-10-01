import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpDepartmentsController } from './erp-departments.controller';
import { ErpDepartmentsService } from './erp-departments.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpDepartmentsController],
  providers: [ErpDepartmentsService],
  exports: [ErpDepartmentsService],
})
export class ErpDepartmentsModule {}
